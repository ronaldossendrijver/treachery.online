/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
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

            if (ForcesInReserve + SpecialForcesInReserve > 0 && ResourcesIncludingAllyContribution > 0)
            {
                bool winning = IAmWinning;
                bool willDoEverythingToPreventNormalWin = Faction == Faction.Orange || Ally == Faction.Orange || Faction == Faction.Yellow && !Game.IsPlaying(Faction.Orange);
                int extraForces = LastTurn || (Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors)) ? 1 : D(1, Param.Shipment_DialForExtraForcesToShip);
                int minResourcesToKeep = Game.Applicable(Rule.AdvancedCombat) ? Param.Shipment_MinimumResourcesToKeepForBattle : 0;
                bool inGreatNeedOfSpice = (Faction == Faction.Black || Faction == Faction.Green || Faction == Faction.White) && ResourcesIncludingAllyContribution <= 2;
                bool stillNeedsResources = ResourcesIncludingAllyContribution < 15 + D(1, 20);

                DetermineShipment_PreventNormalWin(LastTurn && willDoEverythingToPreventNormalWin ? 20 : Param.Shipment_MinimumOtherPlayersITrustToPreventAWin - NrOfNonWinningPlayersToShipAndMoveIncludingMe, extraForces, Param.Shipment_DialShortageToAccept, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && Faction == Faction.Yellow && Game.CurrentTurn < 3 && !winning) DetermineShipment_ShipDirectlyToSpiceAsYellow();
                if (decidedShipment == null && Faction != Faction.Yellow && Ally != Faction.Yellow) DetermineShipment_PreventFremenWin();
                if (decidedShipment == null && !winning && !AlmostLastTurn && inGreatNeedOfSpice) DetermineShipment_ShipToStrongholdNearSpice();
                if (decidedShipment == null && Faction == Faction.Grey && decidedShipment == null && AnyForcesIn(Game.Map.HiddenMobileStronghold) == 0) DetermineShipment_AttackWeakHMS(1, Param.Shipment_DialShortageToAccept, 0, LastTurn ? 99 : Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && winning) DetermineShipment_StrengthenWeakestStronghold(false, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
                if (decidedShipment == null && !winning) DetermineShipment_TakeVacantStronghold(extraForces, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && !winning) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, LastTurn ? 20 : 0);
                if (decidedShipment == null && Faction != Faction.Yellow && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipToStrongholdNearSpice();
                if (decidedShipment == null && Faction == Faction.Yellow && !winning && !LastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceAsYellow();
                if (decidedShipment == null && Game.MayShipAsGuild(this) && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceAsOrangeOrOrangeAlly();
                if (decidedShipment == null && Faction == Faction.Orange && !LastTurn) DetermineShipment_BackToReserves();
                if (decidedShipment == null) DetermineShipment_DummyAttack(minResourcesToKeep);
                if (decidedShipment == null) DetermineShipment_StrengthenWeakestStronghold(true, extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
                if (decidedShipment == null && Faction == Faction.Yellow && AnyForcesIn(Game.Map.PolarSink) <= 2 && (LastTurn || ForcesInReserve + SpecialForcesInReserve * 2 > 8)) DetermineShipment_PolarSinkAsYellow();
            }

            if (decidedShipment == null)
            {
                decidedShipmentAction = ShipmentDecision.None;
                decidedShipment = new Shipment(Game) { Initiator = Faction, Passed = true };
            }

            return decidedShipment;
        }

        protected virtual void DetermineShipment_ShipDirectlyToSpiceAsOrangeOrOrangeAlly()
        {
            LogInfo("DetermineShipment_ShipDirectlyToSpiceAsOrangeOrOrangeAlly()");

            var bestLocationWithSpice = Game.ResourcesOnPlanet.Where(kvp =>
                ValidShipmentLocations.Contains(kvp.Key) &&
                TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                AllyNotIn(kvp.Key.Territory) &&
                !StormWillProbablyHit(kvp.Key) &&
                ProbablySafeFromShaiHulud(kvp.Key.Territory) &&
                !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
                ).HighestOrDefault(kvp => kvp.Value).Key;

            if (bestLocationWithSpice != null)
            {
                var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min((float)Game.ResourcesOnPlanet[bestLocationWithSpice] / Game.ResourceCollectionRate(this), 4) + TotalMaxDialOfOpponents(bestLocationWithSpice.Territory));
                //LogInfo("" + bestLocationWithSpice + " -> " + forcesNeededForCollection);
                
                if (DetermineShortageForShipment(forcesNeededForCollection, false, bestLocationWithSpice, Faction.Black, ForcesInReserve, 0, out int nrOfForces, out int nrOfSpecialForces, 0, 99, false) <= 0)
                {
                    DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, bestLocationWithSpice, true, true);
                }
            }
        }

        protected virtual void DetermineShipment_BackToReserves()
        {
            LogInfo("DetermineShipment_BackToReserves()");

            var battaltionToEvacuate = BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice;
            if (battaltionToEvacuate.Key != null)
            {
                DoShipment(ShipmentDecision.BackToReserves , -battaltionToEvacuate.Value.AmountOfForces, -battaltionToEvacuate.Value.AmountOfSpecialForces, battaltionToEvacuate.Key, false, true);
            }
        }

        protected virtual void DetermineShipment_AttackWeakHMS(int extraForces, float riskAppitite, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_AttackWeakHMS()");

            var opponent = GetOpponentThatOccupies(Game.Map.HiddenMobileStronghold.Territory);

            if (opponent != null && ValidShipmentLocations.Contains(Game.Map.HiddenMobileStronghold))
            {
                var dialNeeded = GetDialNeeded(Game.Map.HiddenMobileStronghold.Territory, opponent, true);

                if (DetermineShortageForShipment(dialNeeded + extraForces, true, Game.Map.HiddenMobileStronghold, opponent.Faction, ForcesInReserve, SpecialForcesInReserve, out int forces, out int specialForces, minResourcesToKeep, maxUnsupportedForces, !(Faction == Faction.Red && opponent.Faction == Faction.Yellow)) <= riskAppitite)
                {
                    DoShipment(ShipmentDecision.AttackWeakStronghold, forces, specialForces, Game.Map.HiddenMobileStronghold, true, true);
                }
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
                nrOfForces = Math.Min(ForcesInReserve, 6 - Math.Min(6, 2 * nrOfSpecialForces));
            }

            DoShipment(ShipmentDecision.PolarSink, nrOfForces, nrOfSpecialForces, Game.Map.PolarSink, false, false);
        }

        protected virtual void DetermineShipment_ShipDirectlyToSpiceAsYellow()
        {
            LogInfo("DetermineShipment_ShipDirectlyToSpiceAsYellow()");

            var bestLocation = Game.ResourcesOnPlanet.Where(kvp =>
                ValidShipmentLocations.Contains(kvp.Key) &&
                AnyForcesIn(kvp.Key) == 0 &&
                TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                AllyNotIn(kvp.Key.Territory) &&
                !StormWillProbablyHit(kvp.Key) &&
                !NearbyBattalionsOutsideStrongholds(kvp.Key).Any()
                ).HighestOrDefault(kvp => kvp.Value).Key;

            if (bestLocation != null)
            {
                var forcesToRally = Math.Max(7, 2 + TotalMaxDialOfOpponents(bestLocation.Territory) + Game.ResourcesOnPlanet[bestLocation] / Game.ResourceCollectionRate(this));

                if (DetermineShortageForShipment(forcesToRally, false, bestLocation, Faction.Black, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, 0, 99, false) <= 3)
                {
                    DoShipment(ShipmentDecision.AtResources, nrOfForces, nrOfSpecialForces, bestLocation, false, false);
                }
            }
        }

        protected virtual void DetermineShipment_ShipToStrongholdNearSpice()
        {
            LogInfo("DetermineShipment_ShipToStrongholdNearSpice()");

            if (BattalionThatShouldBeMovedDueToAllyPresence.Value != null)
            {
                return;
            }

            var richestLocation = RichestLocationNearShippableStrongholdFarFromExistingForces;

            if (richestLocation != null)
            {
                var locationToShipTo = ValidShipmentLocations.FirstOrDefault(sh => 
                    AnyForcesIn(sh) <= 8 && 
                    sh.IsStronghold && 
                    WithinRange(sh, richestLocation, SampleBattalion(sh)));

                if (locationToShipTo != null && AnyForcesIn(locationToShipTo) < 10)
                {
                    var forcesNeededForCollection = MakeEvenIfEfficientForShipping(DetermineForcesNeededForCollection(richestLocation));
                    var opponent = OccupyingOpponentsIn(richestLocation.Territory).FirstOrDefault();
                    var opponentFaction = opponent != null ? opponent.Faction : Faction.Black;
                    DetermineShortageForShipment(forcesNeededForCollection + TotalMaxDialOfOpponents(richestLocation.Territory), false, locationToShipTo, opponentFaction, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, 0, 99, Faction == Faction.Grey);
                    DoShipment(ShipmentDecision.StrongholdNearResources, locationToShipTo, nrOfForces, nrOfSpecialForces, richestLocation, true, true);
                }
            }
        }
                
        protected virtual void DetermineShipment_StrengthenWeakestStronghold(bool onlyIfThreatened, int extraForces, float shortageToAccept, bool takeReinforcementsIntoAccount)
        {
            LogInfo("DetermineShipment_StrengthenWeakStronghold()");

            var myWeakStrongholds = ValidShipmentLocations
                .Where(s => s.IsStronghold && OccupyingForces(s) > 0 && AllyNotIn(s.Territory) && (!onlyIfThreatened || OccupyingOpponentIn(s.Territory) != null))
                .Select(s => new { Location = s, Difference = MaxPotentialForceShortage(takeReinforcementsIntoAccount, s) });

            LogInfo("MyWeakStrongholds:" + string.Join(",", myWeakStrongholds));

            var weakestStronghold = myWeakStrongholds.Where(s => s.Difference > 0).HighestOrDefault(s => s.Difference);
            LogInfo("WeakestStronghold:" + weakestStronghold);

            if (weakestStronghold != null)
            {
                var opponentBattaltion = Game.ForcesOnPlanet[weakestStronghold.Location].Where(b => b.Faction != Faction && b.Faction != Ally).FirstOrDefault();
                var opponentFaction = opponentBattaltion != null ? opponentBattaltion.Faction : Faction.None;
                var dialNeeded = MakeEvenIfEfficientForShipping(weakestStronghold.Difference + extraForces);

                if (DetermineShortageForShipment(dialNeeded, true, weakestStronghold.Location, opponentFaction, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, 0, 0, true) <= shortageToAccept)
                {
                    LogInfo("Shipping to weakest stronghold: " + weakestStronghold);
                    DoShipment(ShipmentDecision.StrengthenWeakStronghold, nrOfForces, nrOfSpecialForces, weakestStronghold.Location, true, true);
                }
            }
        }

        protected virtual void DetermineShipment_DummyAttack(int minResourcesToKeep)
        {
            LogInfo("DetermineShipment_DummyAttack()");

            var targetOfDummyAttack = ValidShipmentLocations
                .FirstOrDefault(l => AnyForcesIn(l) == 0 && AllyNotIn(l.Territory) && l.Territory.IsStronghold && !StormWillProbablyHit(l) && OpponentIsSuitableForTraitorLure(OccupyingOpponentIn(l.Territory)));

            LogInfo("OpponentIsSuitableForTraitorLure: " + targetOfDummyAttack);

            if (targetOfDummyAttack == null && !MayUseUselessAsKarma && TreacheryCards.Any(c => c.Type == TreacheryCardType.Useless))
            {
                targetOfDummyAttack = ValidShipmentLocations
                .FirstOrDefault(l => AnyForcesIn(l) == 0 && AllyNotIn(l.Territory) && l.Territory.IsStronghold && !StormWillProbablyHit(l) && OpponentIsSuitableForUselessCardDumpAttack(OccupyingOpponentIn(l.Territory)));
                LogInfo("OpponentIsSuitableForDummyAttack: " + targetOfDummyAttack);
            }

            if (targetOfDummyAttack != null)
            {
                if (Faction == Faction.White && Shipment.ValidNoFieldValues(Game, this).Contains(0))
                {
                    DoZeroNoFieldShipment(ShipmentDecision.DummyShipment, targetOfDummyAttack);
                }
                else if (DetermineShortageForShipment(0.5f, true, targetOfDummyAttack, OccupyingOpponentIn(targetOfDummyAttack.Territory).Faction, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, minResourcesToKeep, 1, false) <= 0)
                {
                    DoShipment(ShipmentDecision.DummyShipment, nrOfForces, nrOfSpecialForces, targetOfDummyAttack, false, true);
                }
            }
        }

        protected virtual void DetermineShipment_TakeVacantStronghold(int forcestrength, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_TakeVacantStronghold()");

            Location target = null;
            var validStrongholdsToShipTo = ValidShipmentLocations.Where(l => l.IsStronghold).ToList();

            if (Faction == Faction.Grey && VacantAndValid(Game.Map.HiddenMobileStronghold)) target = Game.Map.HiddenMobileStronghold;
            else if (AnyForcesIn(Game.Map.Arrakeen) < 8 && VacantAndValid(Game.Map.Carthag)) target = Game.Map.Carthag;
            else if (AnyForcesIn(Game.Map.Carthag) < 8 && VacantAndValid(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
            else if (VacantAndValid(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;
            else if (VacantAndValid(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
            else if (VacantAndValid(Game.Map.Carthag)) target = Game.Map.Carthag;
            else if (VacantAndValid(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
            else if (VacantAndValid(Game.Map.SietchTabr)) target = Game.Map.SietchTabr;
            else if (Game.IsSpecialStronghold(Game.Map.ShieldWall))
            {
                if (VacantAndValid(Game.Map.ShieldWall.Locations.First())) target = Game.Map.ShieldWall.Locations.First();
                else if (VacantAndValid(Game.Map.ShieldWall.Locations.Last())) target = Game.Map.ShieldWall.Locations.Last();
            }

            if (target != null)
            {
                var dialNeeded = MakeEvenIfEfficientForShipping(forcestrength);
                DetermineShortageForShipment(dialNeeded, false, target, Faction.Yellow, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, minResourcesToKeep, maxUnsupportedForces, true);
                DoShipment(ShipmentDecision.VacantStronghold, nrOfForces, nrOfSpecialForces, target, true, true);
            }
        }

        private bool VacantAndValid(Location location) => VacantAndSafeFromStorm(location) && ValidShipmentLocations.Where(l => l.IsStronghold).Contains(location);

        protected virtual void DetermineShipment_PreventFremenWin()
        {
            LogInfo("DetermineShipment_PreventFremenWin()");

            if (Game.IsPlaying(Faction.Yellow) && Game.YellowVictoryConditionMet)
            {
                if (ValidShipmentLocations.Where(l => AllyNotIn(l.Territory)).Contains(Game.Map.HabbanyaSietch))
                {
                    DetermineShortageForShipment(99, true, Game.Map.HabbanyaSietch, Faction.Black, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, 0, 99, true);
                    DoShipment(ShipmentDecision.PreventFremenWin, nrOfForces, nrOfSpecialForces, Game.Map.HabbanyaSietch, true, true);
                }
                else if (ValidShipmentLocations.Where(l => AllyNotIn(l.Territory)).Contains(Game.Map.SietchTabr))
                {
                    DetermineShortageForShipment(99, true, Game.Map.SietchTabr, Faction.Black, ForcesInReserve, SpecialForcesInReserve, out int nrOfForces, out int nrOfSpecialForces, 0, 99, true);
                    DoShipment(ShipmentDecision.PreventFremenWin, nrOfForces, nrOfSpecialForces, Game.Map.SietchTabr, true, true);
                }
            }
        }
                
        protected virtual void DetermineShipment_PreventNormalWin(int maximumChallengedStrongholds, int extraForces, float riskAppetite, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_PreventNormalWin()");

            var potentialWinningOpponents = WinningOpponentsIWishToAttack(maximumChallengedStrongholds);
            LogInfo("potentialWinningOpponents with too many unchallenged strongholds:" + string.Join(",", potentialWinningOpponents));

            if (!potentialWinningOpponents.Any())
            {
                potentialWinningOpponents = AlmostWinningOpponentsIWishToAttack(maximumChallengedStrongholds);
                LogInfo("potentialWinningOpponents with too many victory points:" + string.Join(",", potentialWinningOpponents));
            }

            var shippableStrongholdsOfWinningOpponents = ValidShipmentLocations.Where(l => 
                (l.Territory.IsStronghold || Game.IsSpecialStronghold(l.Territory)) && 
                AllyNotIn(l.Territory) && 
                potentialWinningOpponents.Any(p => p.Occupies(l)) && 
                IDontHaveAdvisorsIn(l))
                .Select(s => ConstructAttack(s, extraForces, minResourcesToKeep, maxUnsupportedForces));

            LogInfo("shippableStrongholdsOfWinningOpponents:" + string.Join(",", shippableStrongholdsOfWinningOpponents));

            var attack = shippableStrongholdsOfWinningOpponents.Where(a => 
                a.HasForces &&
                a.DialNeeded + DeterminePenalty(a.Opponent) <= Param.Battle_MaxStrengthOfDialledForces &&
                a.ShortageForShipment <= riskAppetite)
                .LowestOrDefault(a => a.DialNeeded);

            LogInfo("weakestShippableLocationOfWinningOpponent:" + attack + " with extra forces: " + extraForces);

            if (attack != null)
            {
                DoShipment(ShipmentDecision.PreventNormalWin, attack.ForcesToShip, attack.SpecialForcesToShip, attack.Location, true, true);
            }
        }

        protected virtual void DetermineShipment_AttackWeakStronghold(int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_AttackWeakStronghold()");

            var possibleAttacks = ValidShipmentLocations
                .Where(l => l.Territory.IsStronghold && AnyForcesIn(l) == 0 && AllyNotIn(l.Territory) && !StormWillProbablyHit(l))
                .Select(l => ConstructAttack(l, extraForces, minResourcesToKeep, maxUnsupportedForces))
                .Where(s => s.HasOpponent && !WinWasPredictedByMeThisTurn(s.Opponent.Faction));

            var attack = possibleAttacks
                .Where(s =>
                s.HasForces &&
                s.DialNeeded + DeterminePenalty(s.Opponent) <= Param.Battle_MaxStrengthOfDialledForces &&
                s.ShortageForShipment - UnlockedForcesInPolarSink(s.Location) <= 0)
                .LowestOrDefault(s => s.DialNeeded);

            LogInfo("WeakestEnemyStronghold:" + attack);

            if (attack != null)
            {
                DoShipment(ShipmentDecision.AttackWeakStronghold, attack, true, true);
            }
        }

        protected virtual int MakeEvenIfEfficientForShipping(float dialNeeded)
        {
            int roundedDialNeeded = (int)Math.Ceiling(dialNeeded);
            if (roundedDialNeeded <= 0) roundedDialNeeded = 1;

            if (Game.MayShipAsGuild(this) && (roundedDialNeeded % 2) != 0)
            {
                return roundedDialNeeded + 1;
            }

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
            DummyShipment
        }

        protected virtual float DetermineShortageForShipment(float dialNeeded, bool diallingForBattle, Location location, Faction opponent, int forcesAvailable, int specialForcesAvailable, out int forces, out int specialForces, int minResourcesToKeep, int maxUnsupportedForces, bool preferSpecialForces)
        {
            specialForces = 0;
            forces = 0;

            /*LogInfo("DetermineValidForcesInShipment(dialNeeded: {0}, tolocation: {1}, opponent: {2}, forcesAvailable: {3}, specialForcesAvailable: {4}, forces: {5}, specialForces: {6})",
                dialNeeded, location, opponent, forcesAvailable, specialForcesAvailable);*/

            var normalStrength = Battle.DetermineNormalForceStrength(Faction);
            var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
            int spiceAvailable = ResourcesIncludingAllyContribution - minResourcesToKeep;
            float noSpiceForForceModifier = Battle.MustPayForForcesInBattle(Game, this) ? 0.5f : 1;
            int costPerForceInBattle = diallingForBattle && Battle.MustPayForForcesInBattle(Game, this) ? 1 : 0;
            int costOfBattle = 0;
            int shipcost = 0;

            //LogInfo("1. dialNeeded: {0}, specialForces: {1}, forces: {2}, shipcost: {3}, costofbattle: {4}", dialNeeded, specialForces, forces, shipcost, costOfBattle);

            if (preferSpecialForces && specialStrength > normalStrength)
            {
                while (
                    dialNeeded > 0 &&
                    (dialNeeded > normalStrength || forcesAvailable == 0) &&
                    specialForcesAvailable >= 1 &&
                    (shipcost = Shipment.DetermineCost(Game, this, forces + specialForces + 1, location, false, false, false)) <= spiceAvailable &&
                    shipcost + costOfBattle - spiceAvailable < maxUnsupportedForces)
                {
                    specialForces++;
                    specialForcesAvailable--;
                    costOfBattle += costPerForceInBattle;
                    dialNeeded -= specialStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
                }
            }

            //LogInfo("2. dialNeeded: {0}, specialForces: {1}, forces: {2}, shipcost: {3}, costofbattle: {4}", dialNeeded, specialForces, forces, shipcost, costOfBattle);

            while (
                dialNeeded > 0 &&
                forcesAvailable >= 1 &&
                (shipcost = Shipment.DetermineCost(Game, this, forces + specialForces + 1, location, false, false, false)) <= spiceAvailable &&
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
                (shipcost = Shipment.DetermineCost(Game, this, forces + specialForces + 1, location, false, false, false)) <= spiceAvailable &&
                shipcost + costOfBattle - spiceAvailable < maxUnsupportedForces)
            {
                specialForces++;
                specialForcesAvailable--;
                costOfBattle += costPerForceInBattle;
                dialNeeded -= specialStrength * (costOfBattle <= spiceAvailable ? 1 : noSpiceForForceModifier);
            }

            LogInfo("DetermineValidForcesInShipment() --> forces: {0}, specialForces: {1}, remaining dial needed: {2}, cost of shipment: {3}, cost of battle: {4})", 
                forces, 
                specialForces, 
                dialNeeded,
                shipcost, 
                costOfBattle);

            return dialNeeded;
        }

        private IEnumerable<Battalion> NearbyBattalions(Location l) => ForcesOnPlanet.Where(kvp => WithinRange(kvp.Key, l, kvp.Value)).Select(kvp => kvp.Value);

        private IEnumerable<Battalion> NearbyBattalionsOutsideStrongholds(Location l) => ForcesOnPlanet.Where(kvp => !kvp.Key.IsStronghold && WithinRange(kvp.Key, l, kvp.Value)).Select(kvp => kvp.Value);

        protected virtual bool IsSafeAndNearby(Location source, Location destination, Battalion b, bool mayFight)
        {
            var opponent = GetOpponentThatOccupies(destination.Territory);

            return WithinRange(source, destination, b) &&
                AllyNotIn(destination.Territory) &&
                ProbablySafeFromShaiHulud(destination.Territory) &&
                (opponent == null || mayFight && GetDialNeeded(destination.Territory, opponent, false) < MaxDial(Resources, b, opponent.Faction)) &&
                !StormWillProbablyHit(destination);
        }

        private Location BestSafeAndNearbyResources(Location location, Battalion b, bool mayFight = false)
        {
            return Game.ResourcesOnPlanet.Where(l => IsSafeAndNearby(location, l.Key, b, mayFight)).HighestOrDefault(r => r.Value).Key;
        }

        protected Battalion FindOneTroopThatCanSafelyMove(Location from, Location to)
        {
            if (ForcesOnPlanet.ContainsKey(from) && from.Sector != Game.SectorInStorm && NotOccupiedByOthers(from.Territory))
            {
                var bat = ForcesOnPlanet[from];
                if (NotOccupiedByOthers(from.Territory) && bat.TotalAmountOfForces > 1)
                {
                    var oneOfThem = bat.Take(1, true);
                    if (PlacementEvent.ValidTargets(Game, this, from, oneOfThem).Contains(to))
                    {
                        return oneOfThem;
                    }
                }
            }

            return null;
        }

        private void DoShipment(ShipmentDecision decision, int nrOfForces, int nrOfSpecialForces, Location destination, bool useKarma, bool useAllyResources)
        {
            DoShipment(decision, destination, nrOfForces, nrOfSpecialForces, null, useKarma, useAllyResources);
        }

        private void DoShipment(ShipmentDecision decision, Attack attack, bool useKarma, bool useAllyResources)
        {
            DoShipment(decision, attack.Location, attack.ForcesToShip, attack.SpecialForcesToShip, null, useKarma, useAllyResources);
        }

        private void DoShipment(ShipmentDecision decision, Location destination, int nrOfForces, int nrOfSpecialForces, Location destinationforMove, bool useKarma, bool useAllyResources)
        {
            var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, destination, true, true);

            if (shipment.IsValid)
            {
                decidedShipmentAction = decision;
                finalDestination = destinationforMove;
                decidedShipment = shipment;
            }
            else
            {
                var error = shipment.Validate();
                LogInfo(error);
                throw new Exception(error);
            }
        }

        private Shipment ConstructShipment(int nrOfForces, int nrOfSpecialForces, Location location, bool useKarma, bool useAllyResources)
        {
            int usableNoField = Shipment.ValidNoFieldValues(Game, this).Where(v => v >= nrOfForces + nrOfSpecialForces + 1 - D(1, 3)).OrderByDescending(v => v).DefaultIfEmpty(-1).First();
            Shipment result;
            if (Shipment.ValidNoFieldValues(Game, this).Contains(usableNoField))
            {
                int forces = nrOfForces;
                int specialForces = nrOfSpecialForces;

                if (Faction == Faction.White)
                {
                    forces = 0;
                    specialForces = 1;
                }
                else
                {
                    forces = Math.Min(usableNoField, ForcesInReserve);
                    specialForces = Math.Min(usableNoField - forces, SpecialForcesInReserve);
                }

                result = new Shipment(Game)
                {
                    Initiator = Faction,
                    ForceAmount = forces,
                    SpecialForceAmount = specialForces,
                    Passed = false,
                    From = null,
                    KarmaCard = null,
                    KarmaShipment = false,
                    NoFieldValue = usableNoField,
                    To = location
                };
            }
            else
            {
                result = new Shipment(Game) { Initiator = Faction, ForceAmount = nrOfForces, SpecialForceAmount = nrOfSpecialForces, Passed = false, From = null, KarmaCard = null, KarmaShipment = false, To = location };
                if (useKarma) UseKarmaIfApplicable(result);
            }

            if (useAllyResources) UseAllyResources(result);
            return result;
        }


        private Battalion SampleBattalion(Location l)
        {
            //Does not take into account Cyborg movement

            if (Faction == Faction.Blue && SpecialForcesIn(l.Territory) > 0)
            {
                //Forces shipped by BG into a territory with advisors must become advisors
                return new Battalion() { Faction = Faction, AmountOfForces = 0, AmountOfSpecialForces = 1 };
            }
            else
            {
                return new Battalion() { Faction = Faction, AmountOfForces = 1, AmountOfSpecialForces = 0 };
            }
        }

        protected IEnumerable<Location> ValidShipmentLocations
        {
            get
            {
                var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
                return Shipment.ValidShipmentLocations(Game, this).Where(l => !forbidden.Contains(l.Territory));
            }
        }

        protected IEnumerable<KeyValuePair<Location, Battalion>> ForceLocationsOutsideStrongholds => ForcesOnPlanet.Where(f => !f.Key.IsStronghold);

        protected Location RichestLocationNearShippableStrongholdFarFromExistingForces => Game.ResourcesOnPlanet.Where(kvp =>
                 !ForceLocationsOutsideStrongholds.Any(looseForce => WithinRange(looseForce.Key, kvp.Key, looseForce.Value)) &&
                 ValidShipmentLocations.Any(sh => sh.IsStronghold && WithinRange(sh, kvp.Key, SampleBattalion(sh))) &&
                 AnyForcesIn(kvp.Key) == 0 &&
                 AllyNotIn(kvp.Key.Territory) &&
                 TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                 !StormWillProbablyHit(kvp.Key) &&
                 ProbablySafeFromShaiHulud(kvp.Key.Territory))
                .HighestOrDefault(kvp => kvp.Value).Key;

        protected int DetermineForcesNeededForCollection(Location location)
        {
            if (Game.ResourcesOnPlanet.ContainsKey(location))
            {
                return (int)Math.Ceiling((float)Game.ResourcesOnPlanet[location] / Game.ResourceCollectionRate(this));
            }
            else
            {
                return 0;
            }
        }

        protected virtual void UseKarmaIfApplicable(Shipment shipment)
        {
            if (
                HasKarma && !Game.KarmaPrevented(Faction) &&
                (!Param.Karma_SaveCardToUseSpecialKarmaAbility || SpecialKarmaPowerUsed) &&
                !Game.MayShipAsGuild(this) &&
                Shipment.DetermineCost(Game, this, shipment) > 7)
            {
                shipment.KarmaShipment = true;
                shipment.KarmaCard = Karma.ValidKarmaCards(Game, this).FirstOrDefault();
            }
        }

        protected virtual void UseAllyResources(Shipment shipment)
        {
            shipment.AllyContributionAmount = Math.Min(shipment.DetermineCostToInitiator(Game), Game.SpiceYourAllyCanPay(this));
        }

        class Attack
        {
            internal Location Location { get; set; }
            internal Player Opponent { get; set; }
            internal float DialNeeded { get; set; }
            internal int ForcesToShip { get; set; }
            internal int SpecialForcesToShip { get; set; }
            internal float ShortageForShipment { get; set; }
            internal bool HasOpponent => Opponent != null;
            internal bool HasForces => ForcesToShip > 0 || SpecialForcesToShip > 0;

            override public string ToString()
            {
                return "" + Location + "->" + Opponent;
            }
        }

        private Attack ConstructAttack(Location location, int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
        {
            var opponent = OccupyingOpponentIn(location.Territory);

            if (opponent != null) {

                var dialNeeded = GetDialNeeded(location.Territory, opponent, true);
                var shortageForShipment = DetermineShortageForShipment(Math.Min(dialNeeded, 0.5f) + extraForces, true, location, opponent.Faction, ForcesInReserve, SpecialForcesInReserve, out int forcesToShip, out int specialForcesToShip, minResourcesToKeep, maxUnsupportedForces, !RedVersusYellow(opponent));

                return new Attack()
                {
                    Location = location,
                    Opponent = opponent,
                    DialNeeded = dialNeeded,
                    ForcesToShip = forcesToShip,
                    SpecialForcesToShip = specialForcesToShip,
                    ShortageForShipment = shortageForShipment
                };
            }
            else
            {
                return new Attack()
                {
                    Location = location,
                    Opponent = opponent,
                    DialNeeded = 0,
                    ForcesToShip = 0,
                    SpecialForcesToShip = 0,
                    ShortageForShipment = 0
                };
            }
        }

        private float MaxPotentialForceShortage(bool takeReinforcementsIntoAccount, Location s)
        {
            var opponents = OccupyingOpponentsIn(s.Territory);

            float maxDialOfOpponents = 0;
            if (opponents.Any())
            {
                maxDialOfOpponents = TotalMaxDialOfOpponents(s.Territory);
            }

            float maxReinforcedDial = 0;
            if (takeReinforcementsIntoAccount)
            {
                if (!opponents.Any())
                {
                    opponents = OpponentsToShipAndMove;
                }

                var mostDangerousOpponent = opponents.HighestOrDefault(p => MaxReinforcedDialTo(p, s.Territory));

                if (mostDangerousOpponent != null)
                {
                    maxReinforcedDial = MaxReinforcedDialTo(mostDangerousOpponent, s.Territory);
                }
            }

            return maxDialOfOpponents + maxReinforcedDial - MaxDial(this, s.Territory, opponents.FirstOrDefault());
        }

        private bool RedVersusYellow(Player opponent) => Faction == Faction.Red && opponent.Faction == Faction.Yellow;

        private int UnlockedForcesInPolarSink(Location location) => location == Game.Map.Arrakeen || location == Game.Map.Carthag ? AnyForcesIn(Game.Map.PolarSink) : 0;

        private float DeterminePenalty(Player opponent) => opponent != null && opponent.IsBot ? BotParameters.PenaltyForAttackingBots : 0;

        private void DoZeroNoFieldShipment(ShipmentDecision action, Location location)
        {
            var shipment = new Shipment(Game)
            {
                Initiator = Faction,
                ForceAmount = 0,
                SpecialForceAmount = 1,
                Passed = false,
                From = null,
                KarmaCard = null,
                KarmaShipment = false,
                NoFieldValue = 0,
                To = location
            };

            if (shipment.IsValid)
            {
                decidedShipmentAction = action;
                decidedShipment = shipment;
            }
            else
            {
                LogInfo(shipment.Validate());
            }
        }

        private bool OpponentIsSuitableForTraitorLure(Player opponent)
        {
            if (opponent == null) return false;

            return opponent.Leaders.Any(l => Game.IsAlive(l) && (Traitors.Contains(l) || FaceDancers.Contains(l)))
                && !KnownOpponentWeapons(opponent).Any();
        }

        private bool OpponentIsSuitableForUselessCardDumpAttack(Player opponent)
        {
            if (opponent == null) return false;

            return !(Game.Applicable(Rule.BlackCapturesOrKillsLeaders) && opponent.Faction == Faction.Black) && !KnownOpponentWeapons(opponent).Any();
        }
    }
}
