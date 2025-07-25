/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

public partial class ClassicBot
{
    private ShipmentDecision DecidedShipmentAction { get; set; }
    private Shipment? DecidedShipment { get; set; }
    private Location? FinalDestination { get; set; }

    protected virtual Shipment DetermineShipment()
    {
        LogInfo("DetermineShipment()");

        DecidedShipmentAction = ShipmentDecision.None;
        DecidedShipment = null;
        FinalDestination = null;

        if (!Game.PreventedFromShipping(Faction) && Player.ForcesInReserve + Player.SpecialForcesInReserve > 0 && ResourcesIncludingAllyContribution > 0)
        {
            var winning = IAmWinning;
            var willDoEverythingToPreventNormalWin = !Game.Applicable(Rule.DisableOrangeSpecialVictory) && (
                Faction == Faction.Orange ||
                Ally == Faction.Orange ||
                (Faction == Faction.Yellow && !Game.IsPlaying(Faction.Orange)) ||
                (Ally == Faction.Yellow && !Game.IsPlaying(Faction.Orange)));

            var extraForces = LastTurn || (Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors)) ? 1 : D(1, Param.Shipment_DialForExtraForcesToShip);
            var minResourcesToKeep = Game.Applicable(Rule.AdvancedCombat) ? Param.Shipment_MinimumResourcesToKeepForBattle : 0;
            var inGreatNeedOfSpice = (Faction == Faction.Black || Faction == Faction.Green || Faction == Faction.White) && ResourcesIncludingAllyContribution <= 2;
            var stillNeedsResources = Faction != Faction.Red && ResourcesIncludingAllyContribution < 15 + D(1, 20);
            var hasWeapons = Player.TreacheryCards.Any(c => c.IsWeapon);
            var hasCards = Player.TreacheryCards.Any();
            var feelingConfident = Resources >= 20 && Player.ForcesKilled + Player.SpecialForcesKilled < 10;

            DetermineShipment_PreventNormalWin(LastTurn && willDoEverythingToPreventNormalWin ? 20 : Param.Shipment_MinimumOtherPlayersITrustToPreventAWin - NrOfNonWinningPlayersToShipAndMoveIncludingMe, extraForces, Param.Shipment_DialShortageToAccept, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (DecidedShipment == null && Faction == Faction.Yellow && Game.CurrentTurn < 3 && !winning) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow();
            if (DecidedShipment == null && Faction != Faction.Yellow && Ally != Faction.Yellow) DetermineShipment_PreventYellowWin();
            if (DecidedShipment == null && winning && hasCards) DetermineShipment_StrengthenWeakestStronghold(false, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
            if (DecidedShipment == null && !winning && !AlmostLastTurn && inGreatNeedOfSpice && !Player.Is(Faction.Red)) DetermineShipment_ShipToStrongholdNearSpice();
            if (DecidedShipment == null && !winning && hasCards) DetermineShipment_TakeVacantStronghold(OpponentsToShipAndMove.Count() + extraForces, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (DecidedShipment == null && !winning && hasCards) DetermineShipment_AttackEmptyHomeworld(minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (DecidedShipment == null && Faction == Faction.Grey && DecidedShipment == null && Player.AnyForcesIn(Game.Map.HiddenMobileStronghold) == 0) DetermineShipment_AttackWeakHMS(1, Param.Shipment_DialShortageToAccept, 0, LastTurn ? 99 : Param.Battle_MaximumUnsupportedForces);
            if (DecidedShipment == null && !winning && ((feelingConfident && hasCards) || hasWeapons)) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, feelingConfident || LastTurn ? 20 : 0);
            if (DecidedShipment == null && Faction != Faction.Yellow && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipToStrongholdNearSpice();
            if (DecidedShipment == null && Faction == Faction.Yellow && !winning && !LastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow();
            if (DecidedShipment == null && Shipment.MayShipWithDiscount(Game, Player) && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsOrangeOrOrangeAlly();
            if (DecidedShipment == null) DetermineShipment_UnlockMoveBonus(minResourcesToKeep);
            if (DecidedShipment == null && Faction == Faction.Orange && !LastTurn && Shipment.MayShipToReserves(Game, Player)) DetermineShipment_BackToReserves();
            if (DecidedShipment == null) DetermineShipment_DummyAttack(minResourcesToKeep);
            if (DecidedShipment == null) DetermineShipment_StrengthenWeakestStronghold(true, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
            if (DecidedShipment == null && Faction == Faction.Yellow && Player.AnyForcesIn(Game.Map.PolarSink) <= 2 && (AlmostLastTurn || LastTurn || Player.ForcesInReserve + Player.SpecialForcesInReserve * 2 >= 8)) DetermineShipment_PolarSinkAsYellow();
            if (DecidedShipment == null && !winning && hasCards) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, feelingConfident || LastTurn ? 20 : 0);
        }

        if (DecidedShipment == null)
        {
            DecidedShipmentAction = ShipmentDecision.None;
            DecidedShipment = new Shipment(Game, Faction) { Passed = true };
        }

        return DecidedShipment;
    }

    private bool WillLeaveMyHomeDefenseless(int shippedAmountOfForces, int shippedAmountOfSpecialForces)
    {
        if (Game.Applicable(Rule.Homeworlds))
            if ((Faction != Faction.Red && Player.ForcesInReserve + Player.SpecialForcesInReserve - shippedAmountOfForces - shippedAmountOfSpecialForces <= 3) ||
                (Faction == Faction.Red && (Player.ForcesInReserve - shippedAmountOfForces <= 3 || Player.SpecialForcesInReserve - shippedAmountOfSpecialForces <= 2)))
                return true;

        return false;
    }

    protected virtual void DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsOrangeOrOrangeAlly()
    {
        LogInfo("DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsOrangeOrOrangeAlly()");

        var bestLocation = Game.ResourcesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            IDontHaveAdvisorsIn(kvp.Key) &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            !StormWillProbablyHit(kvp.Key) &&
            ProbablySafeFromMonster(kvp.Key.Territory) &&
            !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
        ).HighestOrDefault(kvp => kvp.Value).Key;

        if (bestLocation != null)
        {
            var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min((float)Game.ResourcesOnPlanet[bestLocation] / Game.ResourceCollectionRate(Player), 4) + TotalMaxDialOfOpponents(bestLocation.Territory));

            if (DetermineShortageForShipment(forcesNeededForCollection, false, bestLocation, Faction.None, Player.ForcesInReserve, 0, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 0)
            {
                DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, true, true);
                return;
            }
        }

        bestLocation = Game.DiscoveriesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            IDontHaveAdvisorsIn(kvp.Key) &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            !StormWillProbablyHit(kvp.Key) &&
            ProbablySafeFromMonster(kvp.Key.Territory) &&
            !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
        ).Select(kvp => kvp.Key).FirstOrDefault();

        if (bestLocation != null)
        {
            var forcesNeeded = MakeEvenIfEfficientForShipping(1 + TotalMaxDialOfOpponents(bestLocation.Territory));

            if (DetermineShortageForShipment(forcesNeeded, false, bestLocation, Faction.None, Player.ForcesInReserve, 0, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 0) DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, true, true);
        }
    }

    protected virtual void DetermineShipment_BackToReserves()
    {
        LogInfo("DetermineShipment_BackToReserves()");

        var battalionToEvacuate = BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice;
        if (battalionToEvacuate != null) DoShipment(ShipmentDecision.BackToReserves, -battalionToEvacuate.Battalion.AmountOfForces, 
            -battalionToEvacuate.Battalion.AmountOfSpecialForces, -1, -1, battalionToEvacuate.Location, false, true);
    }

    protected virtual void DetermineShipment_AttackWeakHMS(int extraForces, float riskAppetite, int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_AttackWeakHMS()");

        var opponent = GetOpponentThatOccupies(Game.Map.HiddenMobileStronghold.Territory);

        if (opponent != null && ValidShipmentLocations(false).Contains(Game.Map.HiddenMobileStronghold) && IDontHaveAdvisorsIn(Game.Map.HiddenMobileStronghold))
        {
            var attack = ConstructAttack(Game.Map.HiddenMobileStronghold, extraForces, minResourcesToKeep, maxUnsupportedForces);
            if (attack.HasForces) DoShipment(ShipmentDecision.AttackWeakStronghold, attack, true, true);
        }
    }

    protected virtual void DetermineShipment_PolarSinkAsYellow()
    {
        LogInfo("DetermineShipment_PolarSinkAsYellow()");

        int nrOfForces;
        int nrOfSpecialForces;

        if (LastTurn)
        {
            nrOfSpecialForces = Player.SpecialForcesInReserve;
            nrOfForces = Player.ForcesInReserve;
        }
        else
        {
            nrOfSpecialForces = Math.Min(1, Player.SpecialForcesInReserve);
            nrOfForces = Math.Min(Player.ForcesInReserve, 8 - Math.Min(8, 2 * nrOfSpecialForces));
        }

        DoShipment(ShipmentDecision.PolarSink, nrOfForces, nrOfSpecialForces, -1, -1, Game.Map.PolarSink, false, false);
    }

    protected virtual void DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow()
    {
        LogInfo("DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow()");

        var bestLocation = Game.ResourcesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            Player.AnyForcesIn(kvp.Key) == 0 &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            NearbyBattalionsOutsideStrongholds(kvp.Key).Count == 0
        ).HighestOrDefault(kvp => kvp.Value).Key;

        if (bestLocation != null)
        {
            var forcesToRally = Math.Max(7, 2 + TotalMaxDialOfOpponents(bestLocation.Territory) 
                                              + (float)Game.ResourcesOnPlanet[bestLocation] / Game.ResourceCollectionRate(Player));

            if (DetermineShortageForShipment(forcesToRally, false, bestLocation, Faction.None, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 3)
            {
                DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, false, false);
                return;
            }
        }

        bestLocation = Game.DiscoveriesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            Player.AnyForcesIn(kvp.Key) == 0 &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
        ).Select(kvp => kvp.Key).FirstOrDefault();

        if (bestLocation != null)
        {
            var forcesToRally = Math.Max(5, 2 + TotalMaxDialOfOpponents(bestLocation.Territory));

            if (DetermineShortageForShipment(forcesToRally, false, bestLocation, Faction.None, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 3) 
                DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, false, false);
        }
    }

    protected virtual void DetermineShipment_ShipToStrongholdNearSpice()
    {
        LogInfo("DetermineShipment_ShipToStrongholdNearSpice()");

        if (BattalionThatShouldBeMovedDueToAllyPresence != null) return;

        var richestLocation = RichestLocationNearShippableStrongholdFarFromExistingForces;

        if (richestLocation != null)
        {
            var locationToShipTo = ValidShipmentLocations(false).FirstOrDefault(sh =>
                (!Player.HasAlly || Ally is Faction.Pink || AlliedPlayer.AnyForcesIn(sh) == 0 || Faction != Faction.White || !Game.HasLowThreshold(Faction.White)) &&
                !InStorm(sh) &&
                Player.AnyForcesIn(sh) <= 8 &&
                sh.IsStronghold &&
                WithinRange(sh, richestLocation, SampleBattalion(sh)));

            if (locationToShipTo != null && Player.AnyForcesIn(locationToShipTo) < 10)
            {
                var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min(6, DetermineForcesNeededForCollection(richestLocation)));
                var opponent = OccupyingOpponentsIn(richestLocation.Territory).FirstOrDefault();
                var opponentFaction = opponent?.Faction ?? Faction.None;
                DetermineShortageForShipment(forcesNeededForCollection + TotalMaxDialOfOpponents(richestLocation.Territory), 
                    false, locationToShipTo, opponentFaction, Player.ForcesInReserve, Player.SpecialForcesInReserve, 
                    out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 
                    0, 99, Faction == Faction.Grey);
                
                DoShipment(ShipmentDecision.StrongholdNearResources, locationToShipTo, nrOfForces, nrOfSpecialForces, noFieldValue, 
                    cunningNoFieldValue, richestLocation, true, true);
            }
        }
    }

    protected virtual void DetermineShipment_StrengthenWeakestStronghold(bool onlyIfThreatened, int extraForces, float shortageToAccept, bool takeReinforcementsIntoAccount)
    {
        LogInfo("DetermineShipment_StrengthenWeakStronghold()");

        var myWeakStrongholds = ValidShipmentLocations(false)
            .Where(s => s.IsStronghold 
                        && IDontHaveAdvisorsIn(s) 
                        && Player.OccupyingForcesIn(s) > 0 
                        && AllyDoesntBlock(s.Territory)
                        && (!onlyIfThreatened || OccupyingOpponentIn(s.Territory) != null) 
                        && !InStorm(s))
            .Select(s => new { Location = s, Difference = MaxPotentialForceShortage(takeReinforcementsIntoAccount, s) })
            .ToList();

        LogInfo("MyWeakStrongholds:" + string.Join(",", myWeakStrongholds));

        var weakestStronghold = myWeakStrongholds.Where(s => s.Difference > 0).HighestOrDefault(s => s.Difference);
        LogInfo("WeakestStronghold:" + weakestStronghold);

        if (weakestStronghold != null)
        {
            var opponentBattalion = Game.BattalionsIn(weakestStronghold.Location).FirstOrDefault(b => b.Faction != Faction && b.Faction != Ally);
            var opponentFaction = opponentBattalion?.Faction ?? Faction.None;
            var dialNeeded = MakeEvenIfEfficientForShipping(weakestStronghold.Difference + extraForces);

            if (DetermineShortageForShipment(dialNeeded, true, weakestStronghold.Location, opponentFaction, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, 0, 0, true) <= shortageToAccept)
            {
                LogInfo("Shipping to weakest stronghold: " + weakestStronghold);
                DoShipment(ShipmentDecision.StrengthenWeakStronghold, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, weakestStronghold.Location, true, true);
            }
        }
    }

    protected virtual void DetermineShipment_DummyAttack(int minResourcesToKeep)
    {
        LogInfo("DetermineShipment_DummyAttack()");

        var targetOfDummyAttack = ValidShipmentLocations(false)
            .FirstOrDefault(l => Player.AnyForcesIn(l.Territory) == 0 && AllyDoesntBlock(l.Territory) && !InStorm(l) && l.Territory.IsStronghold && OpponentIsSuitableForTraitorLure(OccupyingOpponentIn(l.Territory)));

        LogInfo("OpponentIsSuitableForTraitorLure: " + targetOfDummyAttack);

        if (targetOfDummyAttack == null && !MayUseUselessAsKarma && Player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Useless) && Player.TechTokens.Count == 0)
        {
            targetOfDummyAttack = ValidShipmentLocations(false)
                .FirstOrDefault(l => Player.AnyForcesIn(l.Territory) == 0 && AllyDoesntBlock(l.Territory) && !InStorm(l) && l.Territory.IsStronghold && OpponentIsSuitableForUselessCardDumpAttack(OccupyingOpponentIn(l.Territory)));
            LogInfo("OpponentIsSuitableForDummyAttack: " + targetOfDummyAttack);
        }

        if (targetOfDummyAttack != null)
        {
            if (Faction == Faction.White && Shipment.ValidNoFieldValues(Game, Player).Contains(0))
                DoZeroNoFieldShipment(ShipmentDecision.DummyShipment, targetOfDummyAttack);
            else if (DetermineShortageForShipment(0.5f, true, targetOfDummyAttack, 
                         OccupyingOpponentIn(targetOfDummyAttack.Territory)?.Faction, Player.ForcesInReserve, Player.SpecialForcesInReserve, 
                         out var nrOfForces, out var nrOfSpecialForces, 
                         out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, 1, false) <= 0) 
                DoShipment(ShipmentDecision.DummyShipment, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, targetOfDummyAttack, false, true);
        }
    }

    protected virtual void DetermineShipment_UnlockMoveBonus(int minResourcesToKeep)
    {
        if (!Game.HasOrnithopters(Player) && Player.AnyForcesIn(Game.Map.PolarSink) > 0)
        {
            LogInfo("DetermineShipment_UnlockMoveBonus()");

            var target = ValidShipmentLocations(false).Where(IDontHaveAdvisorsIn)
                .Where(l =>
                    (Equals(l, Game.Map.Arrakeen) && AllyDoesntBlock(Game.Map.Arrakeen)) ||
                    (Equals(l, Game.Map.Carthag) && AllyDoesntBlock(Game.Map.Carthag)))
                .LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));

            if (target != null)
            {
                var opponent = OccupyingOpponentIn(target.Territory);

                var needed = 0.5f;
                if (opponent != null) 
                    needed = Math.Max(0.5f, MaxPotentialForceShortage(OpponentsToShipAndMove.Contains(opponent), target) - Player.AnyForcesIn(Game.Map.PolarSink));

                if (DetermineShortageForShipment(needed, true, target, opponent?.Faction ?? Faction.None, Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, 1, false) <= 0) DoShipment(ShipmentDecision.DummyShipment, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, target, false, true);
            }
        }
    }

    protected virtual void DetermineShipment_TakeVacantStronghold(int forceStrength, int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_TakeVacantStronghold()");

        Location? target = null;

        if (Faction == Faction.Grey && VacantAndValid(Game.Map.HiddenMobileStronghold))
        {
            target = Game.Map.HiddenMobileStronghold;
        }
        else if (Player.AnyForcesIn(Game.Map.Carthag) < 8 && VacantAndValid(Game.Map.Arrakeen))
        {
            target = Game.Map.Arrakeen;
        }
        else if (Player.AnyForcesIn(Game.Map.Arrakeen) < 8 && VacantAndValid(Game.Map.Carthag))
        {
            target = Game.Map.Carthag;
        }
        else if (VacantAndValid(Game.Map.TueksSietch))
        {
            target = Game.Map.TueksSietch;
        }
        else if (VacantAndValid(Game.Map.HabbanyaSietch))
        {
            target = Game.Map.HabbanyaSietch;
        }
        else if (VacantAndValid(Game.Map.Carthag))
        {
            target = Game.Map.Carthag;
        }
        else if (VacantAndValid(Game.Map.Arrakeen))
        {
            target = Game.Map.Arrakeen;
        }
        else if (VacantAndValid(Game.Map.SietchTabr) && (LastTurn || !Game.IsInStorm(Game.Map.SietchTabr)))
        {
            target = Game.Map.SietchTabr;
        }
        else if (Game.IsSpecialStronghold(Game.Map.ShieldWall))
        {
            if (VacantAndValid(Game.Map.ShieldWall.Locations.First())) target = Game.Map.ShieldWall.Locations.First();
            else if (VacantAndValid(Game.Map.ShieldWall.Locations.Last())) target = Game.Map.ShieldWall.Locations.Last();
        }

        target ??= ValidShipmentLocations(false).Where(l => l.IsStronghold).FirstOrDefault(VacantAndValid);

        if (target != null)
        {
            var dialNeeded = MakeEvenIfEfficientForShipping(forceStrength);
            if (DetermineShortageForShipment(dialNeeded, false, target, Faction.None, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, 
                    out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 
                    minResourcesToKeep, maxUnsupportedForces, true) <= 2) DoShipment(ShipmentDecision.VacantStronghold, 
                nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, target, true, true);
        }
    }

    protected virtual void DetermineShipment_AttackEmptyHomeworld(int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_AttackHomeworld()");

        var validHomeworlds = ValidShipmentLocations(false)
            .Where(l => l is Homeworld)
            .Cast<Homeworld>();

        var target = Opponents.SelectMany(p => p.HomeWorlds)
            .Where(w => validHomeworlds.Contains(w) && Player.AnyForcesIn(w) == 0)
            .RandomOrDefault();
        
        if (target != null)
        {
            if (DetermineShortageForShipment(1, false, target, Faction.None, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, maxUnsupportedForces, true) <= 0)
            {
                LogInfo($"Shipping to: {target.Id} {target.Territory.Id} {target} {target.Territory}");
                DoShipment(ShipmentDecision.Homeworld, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, target, true, true);
            }
        }
    }

    private bool VacantAndValid(Location location)
    {
        return VacantAndSafeFromStorm(location) &&
               ValidShipmentLocations(false).Where(l => l.IsStronghold).Contains(location);
    }

    protected virtual void DetermineShipment_PreventYellowWin()
    {
        LogInfo("DetermineShipment_PreventYellowWin()");

        if (Game.IsPlaying(Faction.Yellow) && Game.YellowVictoryConditionMet)
        {
            if (ValidShipmentLocations(false).Where(l => AllyDoesntBlock(l.Territory) && IDontHaveAdvisorsIn(l)).Contains(Game.Map.HabbanyaSietch))
            {
                DetermineShortageForShipment(99, true, Game.Map.HabbanyaSietch, Faction.None, 
                    Player.ForcesInReserve, Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, 0, 99, true);
                DoShipment(ShipmentDecision.PreventYellowWin, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, Game.Map.HabbanyaSietch, true, true);
            }
            else if (ValidShipmentLocations(false).Where(l => AllyDoesntBlock(l.Territory) && IDontHaveAdvisorsIn(l)).Contains(Game.Map.SietchTabr))
            {
                DetermineShortageForShipment(99, true, Game.Map.SietchTabr, Faction.None, 
                    Player.ForcesInReserve,Player.SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, 
                    out var noFieldValue, out var cunningNoFieldValue, 0, 99, true);
                DoShipment(ShipmentDecision.PreventYellowWin, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, Game.Map.SietchTabr, true, true);
            }
        }
    }

    protected virtual void DetermineShipment_PreventNormalWin(int maximumChallengedStrongholds, int extraForces, float riskAppetite, int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_PreventNormalWin()");

        var potentialWinningOpponents = WinningOpponentsIWishToAttack(maximumChallengedStrongholds, false);
        //var p = Game.GetPlayer(Faction.Black);

        if (!potentialWinningOpponents.Any()) potentialWinningOpponents = WinningOpponentsIWishToAttack(maximumChallengedStrongholds, true);

        LogInfo("potentialWinningOpponents with too many unchallenged strongholds:" + string.Join(",", potentialWinningOpponents));

        var shippableStrongholdsOfWinningOpponents = ValidShipmentLocations(false).Where(l =>
                (l.Territory.IsStronghold || Game.IsSpecialStronghold(l.Territory)) &&
                !InStorm(l) &&
                AllyDoesntBlock(l.Territory) &&
                potentialWinningOpponents.Any(p => p.Occupies(l)) &&
                IDontHaveAdvisorsIn(l))
            .Select(s => ConstructAttack(s, extraForces, minResourcesToKeep, maxUnsupportedForces))
            .ToList();

        LogInfo("shippableStrongholdsOfWinningOpponents:" + string.Join(",", shippableStrongholdsOfWinningOpponents));

        var attack = shippableStrongholdsOfWinningOpponents.Where(a =>
                a.HasForces &&
                a.ShortageForShipment <= riskAppetite)
            .LowestOrDefault(a => a.DialNeeded);

        LogInfo("weakestShippableLocationOfWinningOpponent:" + attack + " with extra forces: " + extraForces);

        if (attack != null) DoShipment(ShipmentDecision.PreventNormalWin, attack, true, true);
    }

    protected virtual void DetermineShipment_AttackWeakStronghold(int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
    {
        var dangerousOpponents = Opponents.Where(IsAlmostWinningOpponent).ToArray();

        LogInfo("DetermineShipment_AttackWeakStronghold()");

        var possibleAttacks = ValidShipmentLocations(false)
            .Where(l => !dangerousOpponents.Any() || dangerousOpponents.Any(p => p.Occupies(l)))
            .Where(l => l.Territory.IsStronghold 
                        && Player.AnyForcesIn(l) == 0 
                        && AllyDoesntBlock(l.Territory) 
                        && !StormWillProbablyHit(l) 
                        && !InStorm(l) 
                        && IDontHaveAdvisorsIn(l))
            .Select(l => ConstructAttack(l, extraForces, minResourcesToKeep, maxUnsupportedForces))
            .Where(s => s.Opponent != null && !WinWasPredictedByMeThisTurn(s.Opponent.Faction));

        var attack = possibleAttacks
            .Where(s =>
                s.HasForces &&
                s.ShortageForShipment - UnlockedForcesInPolarSink(s.Location) <= extraForces)
            .LowestOrDefault(s => s.DialNeeded + DeterminePenalty(s.Opponent));

        LogInfo("WeakestEnemyStronghold:" + attack);

        if (attack != null) DoShipment(ShipmentDecision.AttackWeakStronghold, attack, true, true);
    }

    protected virtual int MakeEvenIfEfficientForShipping(float dialNeeded)
    {
        var roundedDialNeeded = (int)Math.Ceiling(dialNeeded);
        if (roundedDialNeeded <= 0) roundedDialNeeded = 1;

        if (Shipment.MayShipWithDiscount(Game, Player) && roundedDialNeeded % 2 != 0) return roundedDialNeeded + 1;

        return roundedDialNeeded;
    }

    public enum ShipmentDecision
    {
        None,
        PreventNormalWin,
        PreventYellowWin,
        VacantStronghold,
        StrengthenWeakStronghold,
        AttackWeakStronghold,
        StrongholdNearResources,
        AtResources,
        BackToReserves,
        PolarSink,
        DummyShipment,
        Homeworld
    }

    protected virtual float DetermineShortageForShipment(
        float dialNeeded,
        bool diallingForBattle,
        Location location,
        Faction? opponent,
        int forcesAvailable,
        int specialForcesAvailable,
        out int forces,
        out int specialForces,
        out int noFieldValue,
        out int cunningNoFieldValue,
        int minResourcesToKeep,
        int maxUnsupportedForces,
        bool preferSpecialForces)
    {
        LogInfo("DetermineValidForcesInShipment(dialNeeded: {0}, to-location: {1}, opponent: {2}, forcesAvailable: {3}, specialForcesAvailable: {4})",
            dialNeeded, location, opponent, forcesAvailable, specialForcesAvailable);

        noFieldValue = -1;
        cunningNoFieldValue = -1;
        specialForces = 0;
        forces = 0;

        var normalStrength = Battle.DetermineNormalForceStrength(Game, Faction);
        var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent ?? Faction.Black);
        var spiceAvailable = ResourcesIncludingAllyContribution - minResourcesToKeep;
        var noSpiceForForceModifier = Battle.MustPayForAnyForcesInBattle(Game, Player) ? 0.5f : 1;
        var costPerForceInBattle = diallingForBattle && Battle.MustPayForAnyForcesInBattle(Game, Player) ? 1 : 0;
        var maxForcesWithNoField = Shipment.ValidNoFieldValues(Game, Player).Any() ? Shipment.ValidNoFieldValues(Game, Player).Max() : -1;

        var shortage = DetermineForcesInShipment(dialNeeded, location, forcesAvailable, specialForcesAvailable, 
            out forces, out specialForces, maxUnsupportedForces, preferSpecialForces, normalStrength, specialStrength, 
            spiceAvailable, noSpiceForForceModifier, costPerForceInBattle);

        if (maxForcesWithNoField >= 0 && Shipment.DetermineCost(Game, Player, 2, 0, location, false, false, false, false, false) != 0 && spiceAvailable > 0)
        {
            int availableSpecialForcesNoField;
            int availableForcesNoField;
            if (preferSpecialForces)
            {
                availableSpecialForcesNoField = Math.Min(specialForcesAvailable, maxForcesWithNoField);
                availableForcesNoField = Math.Min(forcesAvailable, maxForcesWithNoField - availableSpecialForcesNoField);
            }
            else
            {
                availableForcesNoField = Math.Min(forcesAvailable, maxForcesWithNoField);
                availableSpecialForcesNoField = Math.Min(specialForcesAvailable, maxForcesWithNoField - availableForcesNoField);
            }

            var shortageWhenUsingNoField = DetermineForcesInShipment(dialNeeded, location, availableForcesNoField, availableSpecialForcesNoField, 
                out var forcesWithNoField, out var specialForcesWithNoField, maxUnsupportedForces, preferSpecialForces, normalStrength, specialStrength, 
                spiceAvailable, noSpiceForForceModifier, costPerForceInBattle);

            if (shortageWhenUsingNoField <= shortage)
            {
                var bigEnoughNoFields = Shipment.ValidNoFieldValues(Game, Player).Where(v => v >= forcesWithNoField + specialForcesWithNoField).ToArray();
                if (Shipment.MayUseCunningNoField(Player) && Shipment.ValidNoFieldValues(Game, Player).Any() && !bigEnoughNoFields.Any())
                {
                    var biggestAvailableNoField = Shipment.ValidNoFieldValues(Game, Player).Max();
                    var biggestCunningNoField = Shipment.ValidCunningNoFieldValues(Game, Player, biggestAvailableNoField).Max();
                    bigEnoughNoFields = Shipment.ValidNoFieldValues(Game, Player).Where(v => v + biggestCunningNoField >= forcesWithNoField + specialForcesWithNoField).ToArray();
                    cunningNoFieldValue = bigEnoughNoFields.Any() ? biggestCunningNoField : -1;
                }

                noFieldValue = bigEnoughNoFields.Any() ? bigEnoughNoFields.Min() : bigEnoughNoFields.Max();
                var specialForcesShipped = Math.Min(availableSpecialForcesNoField, noFieldValue);
                var forcesShipped = Math.Min(availableForcesNoField, noFieldValue - specialForcesShipped);
                specialForces = Faction == Faction.White ? 1 : specialForcesShipped;
                forces = Faction == Faction.White ? 0 : forcesShipped;
                return shortageWhenUsingNoField;
            }
        }

        return shortage;
    }

    private float DetermineForcesInShipment(float dialNeeded, Location location,
        int forcesAvailable, int specialForcesAvailable, out int forces, out int specialForces,
        int maxUnsupportedForces, bool preferSpecialForces, float normalStrength, float specialStrength, int spiceAvailable, float noSpiceForForceModifier, int costPerForceInBattle)
    {
        specialForces = 0;
        forces = 0;

        var costOfBattle = 0;
        var shipCost = 0;

        if (preferSpecialForces && specialStrength > normalStrength)
            while (
                dialNeeded > 0 &&
                (dialNeeded > normalStrength || forcesAvailable == 0) &&
                specialForcesAvailable >= 1 &&
                (shipCost = Shipment.DetermineCost(Game, Player, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
                shipCost + costOfBattle - spiceAvailable < maxUnsupportedForces)
            {
                specialForces++;
                specialForcesAvailable--;
                costOfBattle += costPerForceInBattle;
                dialNeeded -= specialStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
            }

        LogInfo("dialNeeded: {0}, specialForces: {1}, forces: {2}, shipCost: {3}, costOfBattle: {4}", dialNeeded, specialForces, forces, shipCost, costOfBattle);

        while (
            dialNeeded > 0 &&
            forcesAvailable >= 1 &&
            (shipCost = Shipment.DetermineCost(Game, Player, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
            shipCost + costOfBattle - spiceAvailable < maxUnsupportedForces)
        {
            forces++;
            forcesAvailable--;
            costOfBattle += costPerForceInBattle;
            dialNeeded -= normalStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
        }
        
        while (dialNeeded > 0 &&
               specialForcesAvailable >= 1 &&
               (shipCost = Shipment.DetermineCost(Game, Player, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
               shipCost + costOfBattle - spiceAvailable < maxUnsupportedForces)
        {
            specialForces++;
            specialForcesAvailable--;
            costOfBattle += costPerForceInBattle;
            dialNeeded -= specialStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
        }

        LogInfo("DetermineForcesInShipment() --> forces: {0}, specialForces: {1}, remaining dial needed: {2}, cost of shipment: {3}, cost of battle: {4})",
            forces,
            specialForces,
            dialNeeded,
            shipCost,
            costOfBattle);

        return dialNeeded;
    }

    private Battalion? FindOneTroopThatCanSafelyMove(Location from, Location to)
    {
        if (Player.ForcesInLocations.ContainsKey(from) && from.Sector != Game.SectorInStorm && NotOccupiedByOthers(from.Territory))
        {
            var bat = Player.ForcesInLocations[from];
            if (NotOccupiedByOthers(from.Territory) && bat.TotalAmountOfForces > 1)
            {
                var oneOfThem = bat.Take(1, true);
                if (PlacementEvent.ValidTargets(Game, Player, from, oneOfThem).Contains(to)) return oneOfThem;
            }
        }

        return null;
    }

    private void DoShipment(ShipmentDecision decision, int nrOfForces, int nrOfSpecialForces, int noFieldValue, int cunningNoFieldValue, Location destination, bool useKarma, bool useAllyResources)
    {
        DoShipment(decision, destination, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, null, useKarma, useAllyResources);
    }

    private void DoShipment(ShipmentDecision decision, Attack attack, bool useKarma, bool useAllyResources)
    {
        DoShipment(decision, attack.Location, attack.ForcesToShip, attack.SpecialForcesToShip, attack.NoFieldValue, attack.CunningNoFieldValue, null, useKarma, useAllyResources);
    }

    private void DoShipment(ShipmentDecision decision, Location destination, int nrOfForces, int nrOfSpecialForces, int noFieldValue, int cunningNoFieldValue, 
        Location? destinationForMove, bool useKarma, bool useAllyResources)
    {
        if (!WillLeaveMyHomeDefenseless(nrOfForces, nrOfSpecialForces))
        {
            var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, destination, useKarma, useAllyResources);

            var error = shipment.Validate();
            if (error == null)
            {
                DecidedShipmentAction = decision;
                FinalDestination = destinationForMove;
                DecidedShipment = shipment;
            }
            else
            {
                LogInfo(error);
            }
        }
    }

    private Shipment ConstructShipment(int nrOfForces, int nrOfSpecialForces, int noFieldValue, int cunningNoFieldValue, Location location, bool useKarma, bool useAllyResources)
    {
        var nrOfSmuggledForces = 0;
        var nrOfSmuggledSpecialForces = 0;
        if (Game.SkilledAs(Player, LeaderSkill.Smuggler))
        {
            if (nrOfSpecialForces > 0 && Faction != Faction.White)
                nrOfSmuggledSpecialForces = 1;
            else if (nrOfForces > 0) nrOfSmuggledForces = 1;
        }

        var result = new Shipment(Game, Faction)
        {
            ForceAmount = nrOfForces - nrOfSmuggledForces,
            SpecialForceAmount = nrOfSpecialForces - nrOfSmuggledSpecialForces,
            SmuggledAmount = nrOfSmuggledForces,
            SmuggledSpecialAmount = nrOfSmuggledSpecialForces,
            NoFieldValue = noFieldValue,
            CunningNoFieldValue = cunningNoFieldValue,
            Passed = false,
            ShipmentType = ShipmentType.ShipmentNormal,
            From = null,
            KarmaCard = null,
            To = location
        };

        if (useKarma && noFieldValue < 0) UseKarmaIfApplicable(result);
        if (useAllyResources) UseAllyResources(result);
        return result;
    }


    private Battalion SampleBattalion(Location l)
    {
        if (Faction == Faction.Blue && Player.SpecialForcesIn(l.Territory) > 0)
            //Forces shipped by BG into a territory with advisors must become advisors
            return new Battalion(Faction, 0, 1, l);
        return new Battalion(Faction, 1, 1, l);
    }

    private IEnumerable<Location> ValidShipmentLocations(bool fromPlanet, bool secretAlly = false)
    {
        var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
        return Shipment.ValidShipmentLocations(Game, Player, fromPlanet, secretAlly).Where(l => !forbidden.Contains(l.Territory));
    }

    private IEnumerable<KeyValuePair<Location, Battalion>> ForceLocationsOutsideStrongholds => ForcesOnPlanet.Where(f => !f.Key.IsStronghold);

    private Location? RichestLocationNearShippableStrongholdFarFromExistingForces => Game.ResourcesOnPlanet.Where(kvp =>
            !ForceLocationsOutsideStrongholds.Any(looseForce => WithinRange(looseForce.Key, kvp.Key, looseForce.Value)) &&
            ValidShipmentLocations(false).Any(sh => sh.IsStronghold && WithinRange(sh, kvp.Key, SampleBattalion(sh))) &&
            Player.AnyForcesIn(kvp.Key) == 0 &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            !StormWillProbablyHit(kvp.Key) &&
            ProbablySafeFromMonster(kvp.Key.Territory))
        .HighestOrDefault(kvp => kvp.Value).Key;

    private int DetermineForcesNeededForCollection(Location location)
    {
        if (Game.ResourcesOnPlanet.TryGetValue(location, out var value))
            return (int)Math.Ceiling((float)value / Game.ResourceCollectionRate(Player));
        
        return 0;
    }

    protected virtual void UseKarmaIfApplicable(Shipment shipment)
    {
        if (
            Player.HasKarma(Game) && !Game.KarmaPrevented(Faction) &&
            (!Param.Karma_SaveCardToUseSpecialKarmaAbility || Player.SpecialKarmaPowerUsed) &&
            !Shipment.MayShipWithDiscount(Game, Player) &&
            Shipment.DetermineCost(Game, Player, shipment) > 7)
            shipment.KarmaCard = Karma.ValidKarmaCards(Game, Player).FirstOrDefault();
    }

    protected virtual void UseAllyResources(Shipment shipment)
    {
        shipment.AllyContributionAmount = Math.Min(shipment.DetermineCostToInitiator(Game), Game.ResourcesYourAllyCanPay(Player));
    }

    private class Attack
    {
        internal required Location Location { get; init; }
        internal Player? Opponent { get; init; }
        internal float DialNeeded { get; init; }
        internal int ForcesToShip { get; init; }
        internal int SpecialForcesToShip { get; init; }
        internal int NoFieldValue { get; init; }
        internal int CunningNoFieldValue { get; init; }
        internal float ShortageForShipment { get; init; }
        internal bool HasForces => ForcesToShip > 0 || SpecialForcesToShip > 0;
    }

    private Attack ConstructAttack(Location location, int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
    {
        var opponent = OccupyingOpponentIn(location.Territory);

        if (opponent != null)
        {

            var dialNeeded = GetDialNeeded(location.Territory, opponent, true);
            var shortageForShipment = DetermineShortageForShipment(Math.Max(dialNeeded, 0.5f) + extraForces, true, location, opponent.Faction, 
                Player.ForcesInReserve, Player.SpecialForcesInReserve, out var forcesToShip, out var specialForcesToShip, 
                out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, maxUnsupportedForces, !RedVersusYellow(opponent));

            return new Attack
            {
                Location = location,
                Opponent = opponent,
                DialNeeded = dialNeeded,
                ForcesToShip = forcesToShip,
                SpecialForcesToShip = specialForcesToShip,
                NoFieldValue = noFieldValue,
                CunningNoFieldValue = cunningNoFieldValue,
                ShortageForShipment = shortageForShipment
            };
        }

        return new Attack
        {
            Location = location,
            Opponent = opponent,
            DialNeeded = 0,
            ForcesToShip = 0,
            SpecialForcesToShip = 0,
            NoFieldValue = -1,
            CunningNoFieldValue = -1,
            ShortageForShipment = 0
        };
    }

    private float MaxPotentialForceShortage(bool takeReinforcementsIntoAccount, Location s)
    {
        var opponents = OccupyingOpponentsIn(s.Territory);

        float maxDialOfOpponents = 0;
        if (opponents.Any()) maxDialOfOpponents = TotalMaxDialOfOpponents(s.Territory);

        float maxReinforcedDial = 0;
        if (takeReinforcementsIntoAccount)
        {
            if (!opponents.Any()) opponents = OpponentsToShipAndMove;

            var mostDangerousOpponent = opponents.HighestOrDefault(p => MaxReinforcedDialTo(p, s.Territory));

            if (mostDangerousOpponent != null) maxReinforcedDial = MaxReinforcedDialTo(mostDangerousOpponent, s.Territory);
        }

        return maxDialOfOpponents + maxReinforcedDial - MaxDial(Player, s.Territory, opponents.FirstOrDefault());
    }

    private bool RedVersusYellow(Player opponent)
    {
        return Faction == Faction.Red && opponent.Faction == Faction.Yellow;
    }

    private int UnlockedForcesInPolarSink(Location location) 
        => Equals(location, Game.Map.Arrakeen) || Equals(location, Game.Map.Carthag) 
            ? Player.AnyForcesIn(Game.Map.PolarSink) 
            : 0;

    private static float DeterminePenalty(Player? opponent)
        => opponent is { IsBot: true } && (!opponent.HasAlly || opponent.AlliedPlayer.IsBot)
            ? BotParameters.PenaltyForAttackingBots
            : 0;

    private void DoZeroNoFieldShipment(ShipmentDecision action, Location location)
    {
        var shipment = new Shipment(Game, Faction)
        {
            ForceAmount = 0,
            SpecialForceAmount = 1,
            Passed = false,
            From = null,
            KarmaCard = null,
            ShipmentType = ShipmentType.ShipmentNormal,
            NoFieldValue = 0,
            To = location
        };

        var error = shipment.Validate();
        if (error == null)
        {
            DecidedShipmentAction = action;
            DecidedShipment = shipment;
        }
        else
        {
            LogInfo(error);
        }
    }

    private bool OpponentIsSuitableForTraitorLure(Player? opponent)
    {
        if (opponent == null) return false;

        return opponent.Leaders.Any(l => Game.IsAlive(l) && (Player.Traitors.Contains(l) || Player.FaceDancers.Contains(l)))
               && (!KnownOpponentWeapons(opponent).Any() || Player.Has(TreacheryCardType.Mercenary));
    }

    private bool OpponentIsSuitableForUselessCardDumpAttack(Player? opponent)
    {
        if (opponent == null) return false;

        return !(Game.Applicable(Rule.BlackCapturesOrKillsLeaders) && opponent.Faction == Faction.Black) 
               && (KnownOpponentWeapons(opponent).Count == 0 || Player.Has(TreacheryCardType.Mercenary));
    }
}