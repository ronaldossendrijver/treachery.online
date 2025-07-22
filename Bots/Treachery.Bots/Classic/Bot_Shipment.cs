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
    public ShipmentDecision decidedShipmentAction;
    public Shipment decidedShipment;
    public Location finalDestination;

    protected virtual Shipment DetermineShipment()
    {
        LogInfo("DetermineShipment()");

        decidedShipmentAction = ShipmentDecision.None;
        decidedShipment = null;
        finalDestination = null;

        if (!Game.PreventedFromShipping(Faction) && ForcesInReserve + SpecialForcesInReserve > 0 && ResourcesIncludingAllyContribution > 0)
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
            var hasWeapons = TreacheryCards.Any(c => c.IsWeapon);
            var hasCards = TreacheryCards.Any();
            var feelingConfident = Resources >= 20 && ForcesKilled + SpecialForcesKilled < 10;

            DetermineShipment_PreventNormalWin(LastTurn && willDoEverythingToPreventNormalWin ? 20 : Param.Shipment_MinimumOtherPlayersITrustToPreventAWin - NrOfNonWinningPlayersToShipAndMoveIncludingMe, extraForces, Param.Shipment_DialShortageToAccept, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (decidedShipment == null && Faction == Faction.Yellow && Game.CurrentTurn < 3 && !winning) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow();
            if (decidedShipment == null && Faction != Faction.Yellow && Ally != Faction.Yellow) DetermineShipment_PreventFremenWin();
            if (decidedShipment == null && winning && hasCards) DetermineShipment_StrengthenWeakestStronghold(false, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
            if (decidedShipment == null && !winning && !AlmostLastTurn && inGreatNeedOfSpice && !Is(Faction.Red)) DetermineShipment_ShipToStrongholdNearSpice();
            if (decidedShipment == null && !winning && hasCards) DetermineShipment_TakeVacantStronghold(OpponentsToShipAndMove.Count() + extraForces, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (decidedShipment == null && !winning && hasCards) DetermineShipment_AttackEmptyHomeworld(minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
            if (decidedShipment == null && Faction == Faction.Grey && decidedShipment == null && AnyForcesIn(Game.Map.HiddenMobileStronghold) == 0) DetermineShipment_AttackWeakHMS(1, Param.Shipment_DialShortageToAccept, 0, LastTurn ? 99 : Param.Battle_MaximumUnsupportedForces);
            if (decidedShipment == null && !winning && ((feelingConfident && hasCards) || hasWeapons)) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, feelingConfident || LastTurn ? 20 : 0);
            if (decidedShipment == null && Faction != Faction.Yellow && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipToStrongholdNearSpice();
            if (decidedShipment == null && Faction == Faction.Yellow && !winning && !LastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow();
            if (decidedShipment == null && Shipment.MayShipWithDiscount(Game, this) && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsOrangeOrOrangeAlly();
            if (decidedShipment == null) DetermineShipment_UnlockMoveBonus(minResourcesToKeep);
            if (decidedShipment == null && Faction == Faction.Orange && !LastTurn && Shipment.MayShipToReserves(Game, this)) DetermineShipment_BackToReserves();
            if (decidedShipment == null) DetermineShipment_DummyAttack(minResourcesToKeep);
            if (decidedShipment == null) DetermineShipment_StrengthenWeakestStronghold(true, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
            if (decidedShipment == null && Faction == Faction.Yellow && AnyForcesIn(Game.Map.PolarSink) <= 2 && (AlmostLastTurn || LastTurn || ForcesInReserve + SpecialForcesInReserve * 2 >= 8)) DetermineShipment_PolarSinkAsYellow();
            if (decidedShipment == null && !winning && hasCards) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, feelingConfident || LastTurn ? 20 : 0);
        }

        if (decidedShipment == null)
        {
            decidedShipmentAction = ShipmentDecision.None;
            decidedShipment = new Shipment(Game, Faction) { Passed = true };
        }

        return decidedShipment;
    }

    private bool WillLeaveMyHomeDefenseless(int shippedAmountOfForces, int shippedAmountOfSpecialForces)
    {
        if (Game.Applicable(Rule.Homeworlds))
            if ((Faction != Faction.Red && ForcesInReserve + SpecialForcesInReserve - shippedAmountOfForces - shippedAmountOfSpecialForces <= 3) ||
                (Faction == Faction.Red && (ForcesInReserve - shippedAmountOfForces <= 3 || SpecialForcesInReserve - shippedAmountOfSpecialForces <= 2)))
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
            var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min((float)Game.ResourcesOnPlanet[bestLocation] / Game.ResourceCollectionRate(this), 4) + TotalMaxDialOfOpponents(bestLocation.Territory));

            if (DetermineShortageForShipment(forcesNeededForCollection, false, bestLocation, Faction.None, ForcesInReserve, 0, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 0)
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

            if (DetermineShortageForShipment(forcesNeeded, false, bestLocation, Faction.None, ForcesInReserve, 0, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 0) DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, true, true);
        }
    }

    protected virtual void DetermineShipment_BackToReserves()
    {
        LogInfo("DetermineShipment_BackToReserves()");

        var battaltionToEvacuate = BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice;
        if (battaltionToEvacuate.Key != null) DoShipment(ShipmentDecision.BackToReserves, -battaltionToEvacuate.Value.AmountOfForces, -battaltionToEvacuate.Value.AmountOfSpecialForces, -1, -1, battaltionToEvacuate.Key, false, true);
    }

    protected virtual void DetermineShipment_AttackWeakHMS(int extraForces, float riskAppitite, int minResourcesToKeep, int maxUnsupportedForces)
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
            nrOfSpecialForces = SpecialForcesInReserve;
            nrOfForces = ForcesInReserve;
        }
        else
        {
            nrOfSpecialForces = Math.Min(1, SpecialForcesInReserve);
            nrOfForces = Math.Min(ForcesInReserve, 8 - Math.Min(8, 2 * nrOfSpecialForces));
        }

        DoShipment(ShipmentDecision.PolarSink, nrOfForces, nrOfSpecialForces, -1, -1, Game.Map.PolarSink, false, false);
    }

    protected virtual void DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow()
    {
        LogInfo("DetermineShipment_ShipDirectlyToSpiceOrDiscoveryAsYellow()");

        var bestLocation = Game.ResourcesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            AnyForcesIn(kvp.Key) == 0 &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
        ).HighestOrDefault(kvp => kvp.Value).Key;

        if (bestLocation != null)
        {
            var forcesToRally = Math.Max(7, 2 + TotalMaxDialOfOpponents(bestLocation.Territory) + Game.ResourcesOnPlanet[bestLocation] / Game.ResourceCollectionRate(this));

            if (DetermineShortageForShipment(forcesToRally, false, bestLocation, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 3)
            {
                DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, false, false);
                return;
            }
        }

        bestLocation = Game.DiscoveriesOnPlanet.Where(kvp =>
            ValidShipmentLocations(false).Contains(kvp.Key) &&
            AnyForcesIn(kvp.Key) == 0 &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
        ).Select(kvp => kvp.Key).FirstOrDefault();

        if (bestLocation != null)
        {
            var forcesToRally = Math.Max(5, 2 + TotalMaxDialOfOpponents(bestLocation.Territory));

            if (DetermineShortageForShipment(forcesToRally, false, bestLocation, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, false) <= 3) DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, bestLocation, false, false);
        }
    }

    protected virtual void DetermineShipment_ShipToStrongholdNearSpice()
    {
        LogInfo("DetermineShipment_ShipToStrongholdNearSpice()");

        if (BattalionThatShouldBeMovedDueToAllyPresence.Value != null) return;

        var richestLocation = RichestLocationNearShippableStrongholdFarFromExistingForces;

        if (richestLocation != null)
        {
            var locationToShipTo = ValidShipmentLocations(false).FirstOrDefault(sh =>
                (!HasAlly || Ally is Faction.Pink || AlliedPlayer.AnyForcesIn(sh) == 0 || Faction != Faction.White || !Game.HasLowThreshold(Faction.White)) &&
                !InStorm(sh) &&
                AnyForcesIn(sh) <= 8 &&
                sh.IsStronghold &&
                WithinRange(sh, richestLocation, SampleBattalion(sh)));

            if (locationToShipTo != null && AnyForcesIn(locationToShipTo) < 10)
            {
                var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min(6, DetermineForcesNeededForCollection(richestLocation)));
                var opponent = OccupyingOpponentsIn(richestLocation.Territory).FirstOrDefault();
                var opponentFaction = opponent?.Faction ?? Faction.None;
                DetermineShortageForShipment(forcesNeededForCollection + TotalMaxDialOfOpponents(richestLocation.Territory), false, locationToShipTo, opponentFaction, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, Faction == Faction.Grey);
                DoShipment(ShipmentDecision.StrongholdNearResources, locationToShipTo, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, richestLocation, true, true);
            }
        }
    }

    protected virtual void DetermineShipment_StrengthenWeakestStronghold(bool onlyIfThreatened, int extraForces, float shortageToAccept, bool takeReinforcementsIntoAccount)
    {
        LogInfo("DetermineShipment_StrengthenWeakStronghold()");

        var myWeakStrongholds = ValidShipmentLocations(false)
            .Where(s => s.IsStronghold && IDontHaveAdvisorsIn(s) && OccupyingForcesIn(s) > 0 && AllyDoesntBlock(s.Territory) && (!onlyIfThreatened || OccupyingOpponentIn(s.Territory) != null) && !InStorm(s))
            .Select(s => new { Location = s, Difference = MaxPotentialForceShortage(takeReinforcementsIntoAccount, s) });

        LogInfo("MyWeakStrongholds:" + string.Join(",", myWeakStrongholds));

        var weakestStronghold = myWeakStrongholds.Where(s => s.Difference > 0).HighestOrDefault(s => s.Difference);
        LogInfo("WeakestStronghold:" + weakestStronghold);

        if (weakestStronghold != null)
        {
            var opponentBattaltion = Game.BattalionsIn(weakestStronghold.Location).Where(b => b.Faction != Faction && b.Faction != Ally).FirstOrDefault();
            var opponentFaction = opponentBattaltion != null ? opponentBattaltion.Faction : Faction.None;
            var dialNeeded = MakeEvenIfEfficientForShipping(weakestStronghold.Difference + extraForces);

            if (DetermineShortageForShipment(dialNeeded, true, weakestStronghold.Location, opponentFaction, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 0, true) <= shortageToAccept)
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
            .FirstOrDefault(l => AnyForcesIn(l.Territory) == 0 && AllyDoesntBlock(l.Territory) && !InStorm(l) && l.Territory.IsStronghold && OpponentIsSuitableForTraitorLure(OccupyingOpponentIn(l.Territory)));

        LogInfo("OpponentIsSuitableForTraitorLure: " + targetOfDummyAttack);

        if (targetOfDummyAttack == null && !MayUseUselessAsKarma && TreacheryCards.Any(c => c.Type == TreacheryCardType.Useless) && !TechTokens.Any())
        {
            targetOfDummyAttack = ValidShipmentLocations(false)
                .FirstOrDefault(l => AnyForcesIn(l.Territory) == 0 && AllyDoesntBlock(l.Territory) && !InStorm(l) && l.Territory.IsStronghold && OpponentIsSuitableForUselessCardDumpAttack(OccupyingOpponentIn(l.Territory)));
            LogInfo("OpponentIsSuitableForDummyAttack: " + targetOfDummyAttack);
        }

        if (targetOfDummyAttack != null)
        {
            if (Faction == Faction.White && Shipment.ValidNoFieldValues(Game, this).Contains(0))
                DoZeroNoFieldShipment(ShipmentDecision.DummyShipment, targetOfDummyAttack);
            else if (DetermineShortageForShipment(0.5f, true, targetOfDummyAttack, OccupyingOpponentIn(targetOfDummyAttack.Territory).Faction, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, 1, false) <= 0) DoShipment(ShipmentDecision.DummyShipment, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, targetOfDummyAttack, false, true);
        }
    }

    protected virtual void DetermineShipment_UnlockMoveBonus(int minResourcesToKeep)
    {
        if (!Game.HasOrnithopters(this) && AnyForcesIn(Game.Map.PolarSink) > 0)
        {
            LogInfo("DetermineShipment_UnlockMoveBonus()");

            var target = ValidShipmentLocations(false).Where(l => IDontHaveAdvisorsIn(l))
                .Where(l =>
                    (l == Game.Map.Arrakeen && AllyDoesntBlock(Game.Map.Arrakeen)) ||
                    (l == Game.Map.Carthag && AllyDoesntBlock(Game.Map.Carthag)))
                .LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));

            if (target != null)
            {
                var opponent = OccupyingOpponentIn(target.Territory);

                var needed = 0.5f;
                if (opponent != null) needed = Math.Max(0.5f, MaxPotentialForceShortage(opponent != null && OpponentsToShipAndMove.Contains(opponent), target) - AnyForcesIn(Game.Map.PolarSink));

                if (DetermineShortageForShipment(needed, true, target, opponent != null ? opponent.Faction : Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, 1, false) <= 0) DoShipment(ShipmentDecision.DummyShipment, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, target, false, true);
            }
        }
    }

    protected virtual void DetermineShipment_TakeVacantStronghold(int forcestrength, int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_TakeVacantStronghold()");

        Location target = null;

        if (Faction == Faction.Grey && VacantAndValid(Game.Map.HiddenMobileStronghold))
        {
            target = Game.Map.HiddenMobileStronghold;
        }
        else if (AnyForcesIn(Game.Map.Carthag) < 8 && VacantAndValid(Game.Map.Arrakeen))
        {
            target = Game.Map.Arrakeen;
        }
        else if (AnyForcesIn(Game.Map.Arrakeen) < 8 && VacantAndValid(Game.Map.Carthag))
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

        if (target == null) target = ValidShipmentLocations(false).Where(l => l.IsStronghold).FirstOrDefault(s => VacantAndValid(s));

        if (target != null)
        {
            var dialNeeded = MakeEvenIfEfficientForShipping(forcestrength);
            if (DetermineShortageForShipment(dialNeeded, false, target, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, maxUnsupportedForces, true) <= 2) DoShipment(ShipmentDecision.VacantStronghold, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, target, true, true);
        }
    }

    protected virtual void DetermineShipment_AttackEmptyHomeworld(int minResourcesToKeep, int maxUnsupportedForces)
    {
        LogInfo("DetermineShipment_AttackHomeworld()");

        var validHomeworlds = ValidShipmentLocations(false).Where(l => l is Homeworld hw).Cast<Homeworld>();

        var target = Opponents.SelectMany(p => p.HomeWorlds)
            .Where(w => validHomeworlds.Contains(w) && AnyForcesIn(w) == 0)
            .RandomOrDefault();
        
        if (target != null)
        {
            if (DetermineShortageForShipment(1, false, target, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, maxUnsupportedForces, true) <= 0)
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

    protected virtual void DetermineShipment_PreventFremenWin()
    {
        LogInfo("DetermineShipment_PreventFremenWin()");

        if (Game.IsPlaying(Faction.Yellow) && Game.YellowVictoryConditionMet)
        {
            if (ValidShipmentLocations(false).Where(l => AllyDoesntBlock(l.Territory) && IDontHaveAdvisorsIn(l)).Contains(Game.Map.HabbanyaSietch))
            {
                DetermineShortageForShipment(99, true, Game.Map.HabbanyaSietch, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, true);
                DoShipment(ShipmentDecision.PreventFremenWin, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, Game.Map.HabbanyaSietch, true, true);
            }
            else if (ValidShipmentLocations(false).Where(l => AllyDoesntBlock(l.Territory) && IDontHaveAdvisorsIn(l)).Contains(Game.Map.SietchTabr))
            {
                DetermineShortageForShipment(99, true, Game.Map.SietchTabr, Faction.None, ForcesInReserve, SpecialForcesInReserve, out var nrOfForces, out var nrOfSpecialForces, out var noFieldValue, out var cunningNoFieldValue, 0, 99, true);
                DoShipment(ShipmentDecision.PreventFremenWin, nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, Game.Map.SietchTabr, true, true);
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
            .Select(s => ConstructAttack(s, extraForces, minResourcesToKeep, maxUnsupportedForces));

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
        var dangerousOpponents = Opponents.Where(p => IsAlmostWinningOpponent(p));

        LogInfo("DetermineShipment_AttackWeakStronghold()");

        var possibleAttacks = ValidShipmentLocations(false)
            .Where(l => !dangerousOpponents.Any() || dangerousOpponents.Any(p => p.Occupies(l)))
            .Where(l => l.Territory.IsStronghold && AnyForcesIn(l) == 0 && AllyDoesntBlock(l.Territory) && !StormWillProbablyHit(l) && !InStorm(l) && IDontHaveAdvisorsIn(l))
            .Select(l => ConstructAttack(l, extraForces, minResourcesToKeep, maxUnsupportedForces))
            .Where(s => s.HasOpponent && !WinWasPredictedByMeThisTurn(s.Opponent.Faction));

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

        if (Shipment.MayShipWithDiscount(Game, this) && roundedDialNeeded % 2 != 0) return roundedDialNeeded + 1;

        return roundedDialNeeded;
    }

    public enum ShipmentDecision
    {
        None,
        PreventNormalWin,
        PreventFremenWin,
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
        Faction opponent,
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
        LogInfo("DetermineValidForcesInShipment(dialNeeded: {0}, tolocation: {1}, opponent: {2}, forcesAvailable: {3}, specialForcesAvailable: {4})",
            dialNeeded, location, opponent, forcesAvailable, specialForcesAvailable);

        noFieldValue = -1;
        cunningNoFieldValue = -1;
        specialForces = 0;
        forces = 0;

        var normalStrength = Battle.DetermineNormalForceStrength(Game, Faction);
        var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
        var spiceAvailable = ResourcesIncludingAllyContribution - minResourcesToKeep;
        var noSpiceForForceModifier = Battle.MustPayForAnyForcesInBattle(Game, this) ? 0.5f : 1;
        var costPerForceInBattle = diallingForBattle && Battle.MustPayForAnyForcesInBattle(Game, this) ? 1 : 0;
        var maxForcesWithNoField = Shipment.ValidNoFieldValues(Game, this).Any() ? Shipment.ValidNoFieldValues(Game, this).Max() : -1;

        var shortage = DetermineForcesInShipment(dialNeeded, location, forcesAvailable, specialForcesAvailable, ref forces, ref specialForces, maxUnsupportedForces, preferSpecialForces, normalStrength, specialStrength, spiceAvailable, noSpiceForForceModifier, costPerForceInBattle);

        if (maxForcesWithNoField >= 0 && Shipment.DetermineCost(Game, this, 2, 0, location, false, false, false, false, false) != 0 && spiceAvailable > 0)
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

            var specialForcesWithNoField = 0;
            var forcesWithNoField = 0;

            var shortageWhenUsingNoField = DetermineForcesInShipment(dialNeeded, location, availableForcesNoField, availableSpecialForcesNoField, ref forcesWithNoField, ref specialForcesWithNoField, maxUnsupportedForces, preferSpecialForces, normalStrength, specialStrength, spiceAvailable, noSpiceForForceModifier, costPerForceInBattle);

            if (shortageWhenUsingNoField <= shortage)
            {
                var bigEnoughNoFields = Shipment.ValidNoFieldValues(Game, this).Where(v => v >= forcesWithNoField + specialForcesWithNoField);
                if (Shipment.MayUseCunningNoField(this) && Shipment.ValidNoFieldValues(Game, this).Any() && !bigEnoughNoFields.Any())
                {
                    var biggestAvailableNoField = Shipment.ValidNoFieldValues(Game, this).Max();
                    var biggestCunningNoField = Shipment.ValidCunningNoFieldValues(Game, this, biggestAvailableNoField).Max();
                    bigEnoughNoFields = Shipment.ValidNoFieldValues(Game, this).Where(v => v + biggestCunningNoField >= forcesWithNoField + specialForcesWithNoField);
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
        int forcesAvailable, int specialForcesAvailable, ref int forces, ref int specialForces,
        int maxUnsupportedForces, bool preferSpecialForces, float normalStrength, float specialStrength, int spiceAvailable, float noSpiceForForceModifier, int costPerForceInBattle)
    {
        specialForces = 0;
        forces = 0;

        var costOfBattle = 0;
        var shipcost = 0;

        if (preferSpecialForces && specialStrength > normalStrength)
            while (
                dialNeeded > 0 &&
                (dialNeeded > normalStrength || forcesAvailable == 0) &&
                specialForcesAvailable >= 1 &&
                (shipcost = Shipment.DetermineCost(Game, this, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
                shipcost + costOfBattle - spiceAvailable < maxUnsupportedForces)
            {
                specialForces++;
                specialForcesAvailable--;
                costOfBattle += costPerForceInBattle;
                dialNeeded -= specialStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
            }

        LogInfo("dialNeeded: {0}, specialForces: {1}, forces: {2}, shipcost: {3}, costofbattle: {4}", dialNeeded, specialForces, forces, shipcost, costOfBattle);

        while (
            dialNeeded > 0 &&
            forcesAvailable >= 1 &&
            (shipcost = Shipment.DetermineCost(Game, this, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
            shipcost + costOfBattle - spiceAvailable < maxUnsupportedForces)
        {
            forces++;
            forcesAvailable--;
            costOfBattle += costPerForceInBattle;
            dialNeeded -= normalStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
        }

        //LogInfo("3. dialNeeded: {0}, specialForces: {1}, forces: {2}, shipcost: {3}, costofbattle: {4}", dialNeeded, specialForces, forces, shipcost, costOfBattle);

        while (dialNeeded > 0 &&
               specialForcesAvailable >= 1 &&
               (shipcost = Shipment.DetermineCost(Game, this, forces, specialForces + 1, location, false, false, false, false, false)) <= spiceAvailable &&
               shipcost + costOfBattle - spiceAvailable < maxUnsupportedForces)
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
            shipcost,
            costOfBattle);

        return dialNeeded;
    }


    protected Battalion FindOneTroopThatCanSafelyMove(Location from, Location to)
    {
        if (ForcesInLocations.ContainsKey(from) && from.Sector != Game.SectorInStorm && NotOccupiedByOthers(from.Territory))
        {
            var bat = ForcesInLocations[from];
            if (NotOccupiedByOthers(from.Territory) && bat.TotalAmountOfForces > 1)
            {
                var oneOfThem = bat.Take(1, true);
                if (PlacementEvent.ValidTargets(Game, this, from, oneOfThem).Contains(to)) return oneOfThem;
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

    private void DoShipment(ShipmentDecision decision, Location destination, int nrOfForces, int nrOfSpecialForces, int noFieldValue, int cunningNoFieldValue, Location destinationforMove, bool useKarma, bool useAllyResources)
    {
        if (!WillLeaveMyHomeDefenseless(nrOfForces, nrOfSpecialForces))
        {
            var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, noFieldValue, cunningNoFieldValue, destination, useKarma, useAllyResources);

            var error = shipment.Validate();
            if (error == null)
            {
                decidedShipmentAction = decision;
                finalDestination = destinationforMove;
                decidedShipment = shipment;
            }
            else
            {
                LogInfo(error);
            }
        }
    }

    private Shipment ConstructShipment(int nrOfForces, int nrOfSpecialForces, int noFieldValue, int cunningNoFieldValue, Location location, bool useKarma, bool useAllyResources)
    {
        Shipment result;

        var nrOfSmuggledForces = 0;
        var nrOfSmuggledSpecialForces = 0;
        if (Game.SkilledAs(this, LeaderSkill.Smuggler))
        {
            if (nrOfSpecialForces > 0 && Faction != Faction.White)
                nrOfSmuggledSpecialForces = 1;
            else if (nrOfForces > 0) nrOfSmuggledForces = 1;
        }

        result = new Shipment(Game, Faction)
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
            //KarmaShipment = false,
            To = location
        };

        if (useKarma && noFieldValue < 0) UseKarmaIfApplicable(result);
        if (useAllyResources) UseAllyResources(result);
        return result;
    }


    private Battalion SampleBattalion(Location l)
    {
        if (Faction == Faction.Blue && SpecialForcesIn(l.Territory) > 0)
            //Forces shipped by BG into a territory with advisors must become advisors
            return new Battalion(Faction, 0, 1, l);
        return new Battalion(Faction, 1, 1, l);
    }

    protected IEnumerable<Location> ValidShipmentLocations(bool fromPlanet, bool secretAlly = false)
    {
        var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
        return Shipment.ValidShipmentLocations(Game, this, fromPlanet, secretAlly).Where(l => !forbidden.Contains(l.Territory));
    }

    protected IEnumerable<KeyValuePair<Location, Battalion>> ForceLocationsOutsideStrongholds => ForcesOnPlanet.Where(f => !f.Key.IsStronghold);

    protected Location RichestLocationNearShippableStrongholdFarFromExistingForces => Game.ResourcesOnPlanet.Where(kvp =>
            !ForceLocationsOutsideStrongholds.Any(looseForce => WithinRange(looseForce.Key, kvp.Key, looseForce.Value)) &&
            ValidShipmentLocations(false).Any(sh => sh.IsStronghold && WithinRange(sh, kvp.Key, SampleBattalion(sh))) &&
            AnyForcesIn(kvp.Key) == 0 &&
            AllyDoesntBlock(kvp.Key.Territory) &&
            TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
            !StormWillProbablyHit(kvp.Key) &&
            ProbablySafeFromMonster(kvp.Key.Territory))
        .HighestOrDefault(kvp => kvp.Value).Key;

    protected int DetermineForcesNeededForCollection(Location location)
    {
        if (Game.ResourcesOnPlanet.ContainsKey(location))
            return (int)Math.Ceiling((float)Game.ResourcesOnPlanet[location] / Game.ResourceCollectionRate(this));
        return 0;
    }

    protected virtual void UseKarmaIfApplicable(Shipment shipment)
    {
        if (
            HasKarma(Game) && !Game.KarmaPrevented(Faction) &&
            (!Param.Karma_SaveCardToUseSpecialKarmaAbility || SpecialKarmaPowerUsed) &&
            !Shipment.MayShipWithDiscount(Game, this) &&
            Shipment.DetermineCost(Game, this, shipment) > 7)
            //shipment.KarmaShipment = true;
            shipment.KarmaCard = Karma.ValidKarmaCards(Game, this).FirstOrDefault();
    }

    protected virtual void UseAllyResources(Shipment shipment)
    {
        shipment.AllyContributionAmount = Math.Min(shipment.DetermineCostToInitiator(Game), Game.ResourcesYourAllyCanPay(this));
    }

    private class Attack
    {
        internal Location Location { get; set; }
        internal Player Opponent { get; set; }
        internal float DialNeeded { get; set; }
        internal int ForcesToShip { get; set; }
        internal int SpecialForcesToShip { get; set; }
        internal int NoFieldValue { get; set; }
        internal int CunningNoFieldValue { get; set; }
        internal float ShortageForShipment { get; set; }
        internal bool HasOpponent => Opponent != null;
        internal bool HasForces => ForcesToShip > 0 || SpecialForcesToShip > 0;

        internal Message GetMessage()
        {
            return Message.Express(Location, " -> ", Opponent, " (ToShip: ", ForcesToShip, "/", SpecialForcesToShip, "*, Shortage: ", ShortageForShipment, ", DialNeeded: ", DialNeeded, ")");
        }
    }

    private Attack ConstructAttack(Location location, int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
    {
        var opponent = OccupyingOpponentIn(location.Territory);

        if (opponent != null)
        {

            var dialNeeded = GetDialNeeded(location.Territory, opponent, true);
            var shortageForShipment = DetermineShortageForShipment(Math.Max(dialNeeded, 0.5f) + extraForces, true, location, opponent.Faction, ForcesInReserve, SpecialForcesInReserve, out var forcesToShip, out var specialForcesToShip, out var noFieldValue, out var cunningNoFieldValue, minResourcesToKeep, maxUnsupportedForces, !RedVersusYellow(opponent));

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

        return maxDialOfOpponents + maxReinforcedDial - MaxDial(this, s.Territory, opponents.FirstOrDefault());
    }

    private bool RedVersusYellow(Player opponent)
    {
        return Faction == Faction.Red && opponent.Faction == Faction.Yellow;
    }

    private int UnlockedForcesInPolarSink(Location location)
    {
        return location == Game.Map.Arrakeen || location == Game.Map.Carthag ? AnyForcesIn(Game.Map.PolarSink) : 0;
    }

    private static float DeterminePenalty(Player opponent)
    {
        return opponent != null && opponent.IsBot && (!opponent.HasAlly || opponent.AlliedPlayer.IsBot)
            ? BotParameters.PenaltyForAttackingBots
            : 0;
    }

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
            //KarmaShipment = false,
            NoFieldValue = 0,
            To = location
        };

        var error = shipment.Validate();
        if (error == null)
        {
            decidedShipmentAction = action;
            decidedShipment = shipment;
        }
        else
        {
            LogInfo(error);
        }
    }

    private bool OpponentIsSuitableForTraitorLure(Player opponent)
    {
        if (opponent == null) return false;

        return opponent.Leaders.Any(l => Game.IsAlive(l) && (Traitors.Contains(l) || FaceDancers.Contains(l)))
               && (!KnownOpponentWeapons(opponent).Any() || Has(TreacheryCardType.Mercenary));
    }

    private bool OpponentIsSuitableForUselessCardDumpAttack(Player opponent)
    {
        if (opponent == null) return false;

        return !(Game.Applicable(Rule.BlackCapturesOrKillsLeaders) && opponent.Faction == Faction.Black) && (!KnownOpponentWeapons(opponent).Any() || Has(TreacheryCardType.Mercenary));
    }
}