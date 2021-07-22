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

            bool winning = IAmWinning;
            bool willDoEverythingToPreventNormalWin = Faction == Faction.Orange || Ally == Faction.Orange || Faction == Faction.Yellow && !Game.IsPlaying(Faction.Orange);
            int extraForces = LastTurn || (Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors)) ? 1 : D(1, Param.Shipment_DialForExtraForcesToShip);
            int minResourcesToKeep = Game.Applicable(Rule.AdvancedCombat) ? Param.Shipment_MinimumResourcesToKeepForBattle : 0;
            bool stillNeedsResources = ResourcesIncludingAllyContribution < 15 + D(1, 20);

            try
            {
                DetermineShipment_PreventNormalWin(LastTurn && willDoEverythingToPreventNormalWin ? 99 : Param.Shipment_MinimumOtherPlayersITrustToPreventAWin - NrOfNonWinningPlayersToShipAndMoveIncludingMe, Param.Shipment_DialShortageToAccept, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && Faction != Faction.Yellow && Ally != Faction.Yellow) DetermineShipment_PreventFremenWin();
                if (decidedShipment == null && Faction == Faction.Grey && decidedShipment == null && AnyForcesIn(Game.Map.HiddenMobileStronghold) == 0) DetermineShipment_AttackWeakHMS(1, Param.Shipment_DialShortageToAccept, 0, LastTurn ? 99 : Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null) DetermineShipment_StrengthenWeakestThreatenedStronghold(extraForces, Param.Shipment_DialShortageToAccept, !MayFlipToAdvisors);
                if (decidedShipment == null && !winning) DetermineShipment_TakeVacantStronghold(extraForces, minResourcesToKeep, Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && !winning && Param.Shipment_WillAttackWeakStrongholds) DetermineShipment_AttackWeakStronghold(extraForces, minResourcesToKeep, LastTurn ? 99 : Param.Battle_MaximumUnsupportedForces);
                if (decidedShipment == null && Faction != Faction.Yellow && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipToStrongholdNearSpice();
                if (decidedShipment == null && Faction == Faction.Yellow && !winning && !LastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceAsYellow();
                if (decidedShipment == null && Game.MayShipAsGuild(this) && !winning && !AlmostLastTurn && stillNeedsResources) DetermineShipment_ShipDirectlyToSpiceAsOrangeOrOrangeAlly();
                if (decidedShipment == null && Faction == Faction.Orange && !LastTurn) DetermineShipment_BackToReserves();
                if (decidedShipment == null) DetermineShipment_DummyAttack(minResourcesToKeep);
                if (decidedShipment == null && Faction == Faction.Yellow && AnyForcesIn(Game.Map.PolarSink) <= 2 && (LastTurn || ForcesInReserve + SpecialForcesInReserve * 2 > 8)) DetermineShipment_PolarSink();
            }
            catch (Exception e)
            {
                LogInfo(e.Message);
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
            LogInfo("DetermineShipment_ShipToSpice()");

            var safeLocationWithMostUnclaimedResources = Game.ResourcesOnPlanet.Where(kvp =>
                ValidShipmentLocations.Contains(kvp.Key) &&
                TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                AllyNotIn(kvp.Key.Territory) &&
                !StormWillProbablyHit(kvp.Key) &&
                ProbablySafeFromShaiHulud(kvp.Key.Territory)
                ).OrderByDescending(kvp => kvp.Value).FirstOrDefault();

            if (safeLocationWithMostUnclaimedResources.Key != null)
            {
                var forcesNeededForCollection = MakeEvenIfEfficientForShipping(Math.Min((float)Game.ResourcesOnPlanet[safeLocationWithMostUnclaimedResources.Key] / Game.ResourceCollectionRate(this), 4) + TotalMaxDialOfOpponents(safeLocationWithMostUnclaimedResources.Key.Territory));

                int nrOfForces = 0;
                int nrOfSpecialForces = 0;
                DetermineValidForcesInShipment(forcesNeededForCollection, false, safeLocationWithMostUnclaimedResources.Key, Faction.Black, ForcesInReserve, 0, ref nrOfForces, ref nrOfSpecialForces, 0, 99, false);

                var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, safeLocationWithMostUnclaimedResources.Key, true, true);

                if (shipment.IsValid)
                {
                    decidedShipmentAction = ShipmentDecision.AtResources;
                    decidedShipment = shipment;
                }
                else
                {
                    LogInfo(shipment.Validate());
                }
            }
        }

        private Shipment ConstructShipment(int nrOfForces, int nrOfSpecialForces, Location location, bool useKarma, bool useAllyResources)
        {
            int usableNoField = Shipment.ValidNoFieldValues(Game, this).OrderByDescending(v => v).FirstOrDefault(v => v >= nrOfForces + nrOfSpecialForces + 1 - D(1, 3));
            Shipment result;
            if (Shipment.ValidNoFieldValues(Game, this).Contains(usableNoField))
            {
                result = new Shipment(Game) { 
                    Initiator = Faction, 
                    ForceAmount = Faction == Faction.White ? 0 : nrOfForces,  
                    SpecialForceAmount = Faction == Faction.White ? 1 : nrOfSpecialForces, 
                    Passed = false, 
                    From = null, 
                    KarmaCard = null, 
                    KarmaShipment = false, 
                    NoFieldValue = usableNoField,
                    To = location };
            }
            else
            {
                result = new Shipment(Game) { Initiator = Faction, ForceAmount = nrOfForces, SpecialForceAmount = nrOfSpecialForces, Passed = false, From = null, KarmaCard = null, KarmaShipment = false, To = location };
                if (useKarma) UseKarmaIfApplicable(result);
            }

            if (useAllyResources) UseAllyResources(result);
            return result;
        }

        protected virtual void DetermineShipment_BackToReserves()
        {
            LogInfo("DetermineShipment_BackToReserves()");

            var battaltionToEvacuate = BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice;
            if (battaltionToEvacuate.Key != null)
            {
                var shipment = ConstructShipment(-battaltionToEvacuate.Value.AmountOfForces, -battaltionToEvacuate.Value.AmountOfSpecialForces, battaltionToEvacuate.Key, false, true);
                if (shipment.IsValid)
                {
                    decidedShipmentAction = ShipmentDecision.BackToReserves;
                    decidedShipment = shipment;
                }
                else
                {
                    LogInfo(shipment.Validate());
                }
            }
        }

        protected virtual void DetermineShipment_AttackWeakHMS(int extraForces, float riskAppitite, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_AttackWeakHMS()");

            var opponent = GetOpponentThatOccupies(Game.Map.HiddenMobileStronghold.Territory);

            if (opponent != null && ValidShipmentLocations.Contains(Game.Map.HiddenMobileStronghold))
            {
                var dialNeeded = GetDialNeeded(Game.Map.HiddenMobileStronghold.Territory, opponent, true);

                int forces = 0;
                int specialForces = 0;
                if (DetermineValidForcesInShipment(dialNeeded + extraForces, true, Game.Map.HiddenMobileStronghold, opponent.Faction, ForcesInReserve, SpecialForcesInReserve, ref forces, ref specialForces, minResourcesToKeep, maxUnsupportedForces, !(Faction == Faction.Red && opponent.Faction == Faction.Yellow)) <= riskAppitite)
                {
                    var shipment = ConstructShipment(forces, specialForces, Game.Map.HiddenMobileStronghold, true, true);

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.AttackWeakStronghold;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
            }
        }

        protected virtual void DetermineShipment_PolarSink()
        {
            LogInfo("DetermineShipment_PolarSink()");

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

            var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, Game.Map.PolarSink, false, false );

            if (shipment.IsValid)
            {
                decidedShipmentAction = ShipmentDecision.PolarSink;
                decidedShipment = shipment;
            }
            else
            {
                LogInfo(shipment.Validate());
            }

        }

        protected virtual void DetermineShipment_ShipDirectlyToSpiceAsYellow()
        {
            LogInfo("DetermineShipment_ShipToSpice()");

            var safeLocationWithMostResources = Game.ResourcesOnPlanet.Where(kvp =>
                ValidShipmentLocations.Contains(kvp.Key) &&
                AnyForcesIn(kvp.Key) == 0 &&
                TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                AllyNotIn(kvp.Key.Territory) &&
                !StormWillProbablyHit(kvp.Key)
                ).OrderByDescending(kvp => kvp.Value).FirstOrDefault();

            if (safeLocationWithMostResources.Key != null)
            {
                var forcesToRally = Math.Max(7, 2 + TotalMaxDialOfOpponents(safeLocationWithMostResources.Key.Territory) + Game.ResourcesOnPlanet[safeLocationWithMostResources.Key] / Game.ResourceCollectionRate(this));

                int nrOfForces = 0;
                int nrOfSpecialForces = 0;
                if (DetermineValidForcesInShipment(forcesToRally, false, safeLocationWithMostResources.Key, Faction.Black, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, 0, 99, false) <= 3)
                {
                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, safeLocationWithMostResources.Key, false, false);

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.AtResources;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
            }
        }

        private Battalion SampleBattalion(Location l)
        {
            //Does not take into account Cyborg movement

            if (Faction != Faction.Blue || SpecialForcesIn(l.Territory) == 0)
            {
                return new Battalion() { Faction = Faction, AmountOfForces = 1, AmountOfSpecialForces = 0 };
            }
            else
            {
                //Forces shipped by BG into a territory with advisors must become advisors
                return new Battalion() { Faction = Faction, AmountOfForces = 0, AmountOfSpecialForces = 1 };
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

        protected KeyValuePair<Location, int> RichestSafeLocationWithUnclaimedResourcesNearShippableStrongholdFarFromExistingForces => Game.ResourcesOnPlanet.Where(kvp =>
                 !ForceLocationsOutsideStrongholds.Any(looseForce => WithinRange(looseForce.Key, kvp.Key, looseForce.Value)) &&
                 ValidShipmentLocations.Any(sh => sh.IsStronghold && WithinRange(sh, kvp.Key, SampleBattalion(sh))) &&
                 AnyForcesIn(kvp.Key) == 0 &&
                 AllyNotIn(kvp.Key.Territory) &&
                 TotalMaxDialOfOpponents(kvp.Key.Territory) <= Param.Shipment_MaxEnemyForceStrengthFightingForSpice &&
                 !StormWillProbablyHit(kvp.Key) &&
                 ProbablySafeFromShaiHulud(kvp.Key.Territory))
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault();

        protected virtual void DetermineShipment_ShipToStrongholdNearSpice()
        {
            LogInfo("DetermineShipment_ShipToStrongholdNearSpice()");

            if (BattalionThatShouldBeMovedDueToAllyPresence.Value != null)
            {
                return;
            }

            var richestSafeLocationWithUnclaimedResourcesNearShippableStronghold = RichestSafeLocationWithUnclaimedResourcesNearShippableStrongholdFarFromExistingForces;

            if (richestSafeLocationWithUnclaimedResourcesNearShippableStronghold.Key != null)
            {
                var shippableStrongholdNearRichestLocation = ValidShipmentLocations
                    .FirstOrDefault(sh => AnyForcesIn(sh) <= 8 && sh.IsStronghold && WithinRange(sh, richestSafeLocationWithUnclaimedResourcesNearShippableStronghold.Key, SampleBattalion(sh)));

                if (shippableStrongholdNearRichestLocation != null && AnyForcesIn(shippableStrongholdNearRichestLocation) < 10)
                {
                    var forcesNeededForCollection = MakeEvenIfEfficientForShipping(DetermineForcesNeededForCollection(richestSafeLocationWithUnclaimedResourcesNearShippableStronghold.Key));

                    int nrOfForces = 0;
                    int nrOfSpecialForces = 0;
                    DetermineValidForcesInShipment(forcesNeededForCollection + TotalMaxDialOfOpponents(richestSafeLocationWithUnclaimedResourcesNearShippableStronghold.Key.Territory), false, shippableStrongholdNearRichestLocation, Faction.Black, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, 0, 99, Faction == Faction.Grey);

                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, shippableStrongholdNearRichestLocation, true, true);

                    LogInfo("decidedShipment: {0}", shipment.GetMessage());

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.StrongholdNearResources;
                        finalDestination = richestSafeLocationWithUnclaimedResourcesNearShippableStronghold.Key;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
            }
        }

        protected int DetermineForcesNeededForCollection(Location location)
        {
            return (int)Math.Ceiling((float)Game.ResourcesOnPlanet[location] / Game.ResourceCollectionRate(this));
        }

        protected virtual void UseKarmaIfApplicable(Shipment shipment)
        {
            if (
                HasKarma && !Game.KarmaPrevented(Faction) &&
                ((Faction != Faction.Black || Faction != Faction.Red) || SpecialKarmaPowerUsed) && 
                !Game.MayShipAsGuild(this) && 
                Shipment.DetermineCost(Game, this, shipment) > 7)
            {
                shipment.KarmaShipment = true;
                shipment.KarmaCard = Karma.ValidKarmaCards(Game, this).FirstOrDefault();
            }
        }

        protected virtual void UseAllyResources(Shipment shipment)
        {
            shipment.AllyContributionAmount = Math.Min(shipment.DetermineCostToInitiator(), Game.GetPermittedUseOfAllySpice(Faction));
        }

        protected virtual void DetermineShipment_StrengthenWeakestThreatenedStronghold(int extraForces, float shortageToAccept, bool takeReinforcementsIntoAccount)
        {
            LogInfo("DetermineShipment_StrengthenWeakStronghold()");

            var myThreatenedStrongholds = ValidShipmentLocations
                .Where(s => s.IsStronghold && OccupyingForces(s) > 0 && AllyNotIn(s.Territory) && OccupyingOpponentIn(s.Territory) != null)
                .Select(s => new { Location = s, Difference = TotalMaxDialOfOpponents(s.Territory) + (takeReinforcementsIntoAccount ? (int)Math.Ceiling(MaxReinforcedDialTo(OccupyingOpponentIn(s.Territory), s.Territory)) : 0) - MaxDial(this, s.Territory, OccupyingOpponentIn(s.Territory).Faction) });

            LogInfo("MyThreatenedStrongholds:" + string.Join(",", myThreatenedStrongholds));

            var mostThreatenedStrongholdWithFiveOrLessOpponentForces = myThreatenedStrongholds.Where(s => s.Difference >= -1).OrderByDescending(s => s.Difference).FirstOrDefault();
            LogInfo("MostThreatenedStrongholdWithFiveOrLessOpponentForces:" + mostThreatenedStrongholdWithFiveOrLessOpponentForces);

            if (mostThreatenedStrongholdWithFiveOrLessOpponentForces != null)
            {
                var opponentBattaltion = Game.ForcesOnPlanet[mostThreatenedStrongholdWithFiveOrLessOpponentForces.Location].Where(b => b.Faction != Faction && b.Faction != Ally).FirstOrDefault();
                var opponentFaction = opponentBattaltion != null ? opponentBattaltion.Faction : Faction.None;
                var dialNeeded = MakeEvenIfEfficientForShipping(mostThreatenedStrongholdWithFiveOrLessOpponentForces.Difference + extraForces);

                int nrOfForces = 0;
                int nrOfSpecialForces = 0;
                if (DetermineValidForcesInShipment(dialNeeded, true, mostThreatenedStrongholdWithFiveOrLessOpponentForces.Location, opponentFaction, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, 0, 0, true) <= shortageToAccept)
                {
                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, mostThreatenedStrongholdWithFiveOrLessOpponentForces.Location, true, true);

                    if (shipment.IsValid)
                    {
                        LogInfo("Shipping to threatened stronghold:" + shipment.GetMessage());
                        decidedShipmentAction = ShipmentDecision.StrengthenWeakStronghold;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
            }
        }

        protected virtual void DetermineShipment_AttackWeakStronghold(int extraForces, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_AttackWeakStronghold()");

            var enemyWeakStrongholds = ValidShipmentLocations
                .Where(l => AnyForcesIn(l) == 0 && AllyNotIn(l.Territory) && l.Territory.IsStronghold && !StormWillProbablyHit(l))
                .Select(l => new { Stronghold = l, Opponent = OccupyingOpponentIn(l.Territory) })
                .Where(s => s.Opponent != null && (Game.CurrentTurn > PredictedTurn || s.Opponent.Faction != PredictedFaction)).Select(s => new
                {
                    s.Stronghold,
                    Opponent = s.Opponent.Faction,
                    DialNeeded = GetDialNeeded(s.Stronghold.Territory, s.Opponent, true)
                });

            int forces = 0;
            int specialForces = 0;
            var weakestEnemyStronghold = enemyWeakStrongholds
                .Where(s =>
                (s.Stronghold == Game.Map.Arrakeen || s.Stronghold == Game.Map.Carthag) && AnyForcesIn(Game.Map.PolarSink) > 5 ||
                DetermineValidForcesInShipment(s.DialNeeded + extraForces, true, s.Stronghold, s.Opponent, ForcesInReserve, SpecialForcesInReserve, ref forces, ref specialForces, minResourcesToKeep, maxUnsupportedForces, !(Faction == Faction.Red && s.Opponent == Faction.Yellow)) <= 0
                )
                .OrderBy(s => s.DialNeeded).FirstOrDefault();

            LogInfo("WeakestEnemyStronghold:" + weakestEnemyStronghold);

            if (weakestEnemyStronghold != null)
            {
                int nrOfForces = 0;
                int nrOfSpecialForces = 0;

                DetermineValidForcesInShipment(MakeEvenIfEfficientForShipping(weakestEnemyStronghold.DialNeeded + extraForces), true, weakestEnemyStronghold.Stronghold, weakestEnemyStronghold.Opponent, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, minResourcesToKeep, maxUnsupportedForces, !(Faction == Faction.Red && weakestEnemyStronghold.Opponent == Faction.Yellow));

                var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, weakestEnemyStronghold.Stronghold, true, true);

                if (shipment.IsValid)
                {
                    decidedShipmentAction = ShipmentDecision.AttackWeakStronghold;
                    decidedShipment = shipment;
                }
                else
                {
                    LogInfo(shipment.Validate());
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
                int nrOfForces = 0;
                int nrOfSpecialForces = 0;

                DetermineValidForcesInShipment(0.5f, true, targetOfDummyAttack, OccupyingOpponentIn(targetOfDummyAttack.Territory).Faction, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, minResourcesToKeep, 1, false);

                var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, targetOfDummyAttack, false, true);

                if (shipment.IsValid)
                {
                    decidedShipmentAction = ShipmentDecision.DummyShipment;
                    decidedShipment = shipment;
                }
                else
                {
                    LogInfo(shipment.Validate());
                }
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

        

        protected virtual void DetermineShipment_TakeVacantStronghold(int forcestrength, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_TakeVacantStronghold()");

            Location unoccupiedStronghold = null;
            var validStrongholdsToShipTo = ValidShipmentLocations.Where(l => l.IsStronghold).ToList();

            if (Faction == Faction.Grey && VacantAndSafeFromStorm(Game.Map.HiddenMobileStronghold) && validStrongholdsToShipTo.Contains(Game.Map.HiddenMobileStronghold)) unoccupiedStronghold = Game.Map.HiddenMobileStronghold;
            else if (AnyForcesIn(Game.Map.Arrakeen) == 0 && VacantAndSafeFromStorm(Game.Map.Carthag) && validStrongholdsToShipTo.Contains(Game.Map.Carthag)) unoccupiedStronghold = Game.Map.Carthag;
            else if (AnyForcesIn(Game.Map.Carthag) == 0 && VacantAndSafeFromStorm(Game.Map.Arrakeen) && validStrongholdsToShipTo.Contains(Game.Map.Arrakeen)) unoccupiedStronghold = Game.Map.Arrakeen;
            else if (VacantAndSafeFromStorm(Game.Map.HabbanyaSietch) && validStrongholdsToShipTo.Contains(Game.Map.HabbanyaSietch)) unoccupiedStronghold = Game.Map.HabbanyaSietch;
            else if (VacantAndSafeFromStorm(Game.Map.TueksSietch) && validStrongholdsToShipTo.Contains(Game.Map.TueksSietch)) unoccupiedStronghold = Game.Map.TueksSietch;
            else if (VacantAndSafeFromStorm(Game.Map.Carthag) && validStrongholdsToShipTo.Contains(Game.Map.Carthag)) unoccupiedStronghold = Game.Map.Carthag;
            else if (VacantAndSafeFromStorm(Game.Map.Arrakeen) && validStrongholdsToShipTo.Contains(Game.Map.Arrakeen)) unoccupiedStronghold = Game.Map.Arrakeen;
            else if (VacantAndSafeFromStorm(Game.Map.SietchTabr) && validStrongholdsToShipTo.Contains(Game.Map.SietchTabr)) unoccupiedStronghold = Game.Map.SietchTabr;
            else if (Game.IsSpecialStronghold(Game.Map.ShieldWall) && VacantAndSafeFromStorm(Game.Map.ShieldWall.Locations.First()) && ValidShipmentLocations.Contains(Game.Map.ShieldWall.Locations.First())) unoccupiedStronghold = Game.Map.ShieldWall.Locations.First();

            if (unoccupiedStronghold != null)
            {
                var dialNeeded = MakeEvenIfEfficientForShipping(forcestrength);

                int nrOfForces = 0;
                int nrOfSpecialForces = 0;
                DetermineValidForcesInShipment(dialNeeded, false, unoccupiedStronghold, Faction.Yellow, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, minResourcesToKeep, maxUnsupportedForces, true);

                var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, unoccupiedStronghold, true, true);

                if (shipment.IsValid)
                {
                    decidedShipmentAction = ShipmentDecision.VacantStronghold;
                    decidedShipment = shipment;
                }
                else
                {
                    LogInfo(shipment.Validate());
                }
            }
        }

        protected virtual void DetermineShipment_PreventFremenWin()
        {
            LogInfo("DetermineShipment_PreventFremenWin()");

            if (Game.IsPlaying(Faction.Yellow) && Game.YellowVictoryConditionMet)
            {
                int nrOfForces = 0;
                int nrOfSpecialForces = 0;

                if (ValidShipmentLocations.Where(l => AllyNotIn(l.Territory)).Contains(Game.Map.HabbanyaSietch))
                {
                    DetermineValidForcesInShipment(99, true, Game.Map.HabbanyaSietch, Faction.Black, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, 0, 99, true);
                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, Game.Map.HabbanyaSietch, true, true);

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.PreventFremenWin;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }

                if (ValidShipmentLocations.Where(l => AllyNotIn(l.Territory)).Contains(Game.Map.SietchTabr))
                {
                    DetermineValidForcesInShipment(99, true, Game.Map.SietchTabr, Faction.Black, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, 0, 99, true);
                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, Game.Map.SietchTabr, true, true);
                    UseKarmaIfApplicable(shipment);
                    UseAllyResources(shipment);

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.PreventFremenWin;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
            }
        }

        protected virtual void DetermineShipment_PreventNormalWin(int maximumChallengedStrongholds, float riskAppetite, int minResourcesToKeep, int maxUnsupportedForces)
        {
            LogInfo("DetermineShipment_PreventNormalWin()");

            var potentialWinningOpponents = Game.Players.Where(p => p != this && p != AlliedPlayer && Game.MeetsNormalVictoryCondition(p, true) && Game.CountChallengedStongholds(p) <= maximumChallengedStrongholds && !WinWasPredictedByMeThisTurn(p.Faction));
            LogInfo("potentialWinningOpponents with too many unchallenged strongholds:" + string.Join(",", potentialWinningOpponents));

            if (!potentialWinningOpponents.Any())
            {
                potentialWinningOpponents = Game.Players.Where(p => p != this && p != AlliedPlayer && Game.NumberOfVictoryPoints(p, true) + 1 >= Game.TresholdForWin(p) && CanShip(p) && Game.CountChallengedStongholds(p) <= maximumChallengedStrongholds && !WinWasPredictedByMeThisTurn(p.Faction));
                LogInfo("potentialWinningOpponents with too many victory points:" + string.Join(",", potentialWinningOpponents));
            }

            var shippableStrongholdsOfWinningOpponents = ValidShipmentLocations.Where(l => (l.Territory.IsStronghold || Game.IsSpecialStronghold(Game.Map.ShieldWall)) && AllyNotIn(l.Territory) && potentialWinningOpponents.Any(p => p.Occupies(l)) && IDontHaveAdvisorsIn(l));
            LogInfo("shippableStrongholdsOfWinningOpponents:" + string.Join(",", shippableStrongholdsOfWinningOpponents));

            var weakestShippableLocationOfWinningOpponent = shippableStrongholdsOfWinningOpponents.Select(s => new { Stronghold = s, Strength = potentialWinningOpponents.Sum(p => MaxDial(p, s.Territory, Faction) - MaxDial(this, s.Territory, p.Faction)) }).Where(l => l.Strength > 0).OrderBy(l => l.Strength).FirstOrDefault();
            LogInfo("weakestShippableLocationOfWinningOpponent:" + weakestShippableLocationOfWinningOpponent);

            if (weakestShippableLocationOfWinningOpponent != null)
            {
                var opponent = Game.GetPlayer(Game.ForcesOnPlanet[weakestShippableLocationOfWinningOpponent.Stronghold].First().Faction);

                var dialNeeded = GetDialNeeded(weakestShippableLocationOfWinningOpponent.Stronghold.Territory, opponent, true);
                int dialNeededMadeEfficient = MakeEvenIfEfficientForShipping(dialNeeded);

                int nrOfForces = 0;
                int nrOfSpecialForces = 0;
                if (DetermineValidForcesInShipment(dialNeededMadeEfficient, true, weakestShippableLocationOfWinningOpponent.Stronghold, opponent.Faction, ForcesInReserve, SpecialForcesInReserve, ref nrOfForces, ref nrOfSpecialForces, minResourcesToKeep, maxUnsupportedForces, !(Faction == Faction.Red && opponent.Faction == Faction.Yellow)) <= riskAppetite)
                {
                    var shipment = ConstructShipment(nrOfForces, nrOfSpecialForces, weakestShippableLocationOfWinningOpponent.Stronghold, true, true);

                    if (shipment.IsValid)
                    {
                        decidedShipmentAction = ShipmentDecision.PreventNormalWin;
                        decidedShipment = shipment;
                    }
                    else
                    {
                        LogInfo(shipment.Validate());
                    }
                }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialNeeded"></param>
        /// <param name="amountOfResourcesToSave"></param>
        /// <param name="toStronghold"></param>
        /// <param name="opponent"></param>
        /// <param name="forcesAvailable"></param>
        /// <param name="specialForcesAvailable"></param>
        /// <param name="forces"></param>
        /// <param name="specialForces"></param>
        /// <returns>The amount that cannot be dialled. If this value <= 0, you have enough forces to reach the dial needed.</returns>
        protected virtual float DetermineValidForcesInShipment(float dialNeeded, bool diallingForBattle, Location location, Faction opponent, int forcesAvailable, int specialForcesAvailable, ref int forces, ref int specialForces, int minResourcesToKeep, int maxUnsupportedForces, bool preferSpecialForces)
        {
            LogInfo("DetermineValidForcesInShipment(dialNeeded: {0}, tolocation: {1}, opponent: {2}, forcesAvailable: {3}, specialForcesAvailable: {4}, forces: {5}, specialForces: {6})",
                dialNeeded, location, opponent, forcesAvailable, specialForcesAvailable, forces, specialForces);

            var normalStrength = Battle.DetermineNormalForceStrength(Faction);
            var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
            float spiceLeft = ResourcesIncludingAllyContribution - minResourcesToKeep;
            float costModifier = Game.MayShipAsGuild(this) ? 0.5f : 1f;
            float costPerForceToShip = Shipment.ShipsForFree(Game, this, location) ? 0 : costModifier * (location.IsStronghold ? 1 : 2);
            float noSpiceForForceModifier = Battle.MustPayForForcesInBattle(Game, this) ? 0.5f : 1;
            int costPerForceInBattle = diallingForBattle && Battle.MustPayForForcesInBattle(Game, this) ? 1 : 0;
            int costOfBattle = 0;

            if (preferSpecialForces && specialStrength > normalStrength)
            {
                specialForces = 0;
                while (dialNeeded > 0 && (dialNeeded > normalStrength || forcesAvailable == 0) && specialForcesAvailable >= 1 && spiceLeft >= costPerForceToShip && costOfBattle - spiceLeft < maxUnsupportedForces)
                {
                    specialForces++;
                    specialForcesAvailable--;
                    spiceLeft -= costPerForceToShip;
                    costOfBattle += costPerForceInBattle;
                    dialNeeded -= specialStrength * (costOfBattle <= spiceLeft ? 1 : noSpiceForForceModifier);
                }
            }

            forces = 0;
            while (dialNeeded > 0 && forcesAvailable >= 1 && spiceLeft >= costPerForceToShip && costOfBattle - spiceLeft < maxUnsupportedForces)
            {
                forces++;
                forcesAvailable--;
                spiceLeft -= costPerForceToShip;
                costOfBattle += costPerForceInBattle;
                dialNeeded -= normalStrength * (costOfBattle <= spiceLeft ? 1 : noSpiceForForceModifier);
            }

            while (dialNeeded > 0 && specialForcesAvailable >= 1 && spiceLeft >= costPerForceToShip && costOfBattle - spiceLeft < maxUnsupportedForces)
            {
                specialForces++;
                specialForcesAvailable--;
                spiceLeft -= costPerForceToShip;
                costOfBattle += costPerForceInBattle;
                dialNeeded -= specialStrength * (costOfBattle <= spiceLeft ? 1 : noSpiceForForceModifier);
            }

            LogInfo("DetermineValidForcesInShipment() --> forces: {0}, specialForces: {1}, remaining dial needed: {2}, cost of battle: {3})", forces, specialForces, dialNeeded, costOfBattle);

            return dialNeeded;
        }

        

        protected virtual bool IsSafeAndNearby(Location source, Location destination, Battalion b, bool mayFight)
        {
            /*LogInfo("IsSafeAndNearby({0},{1}): ally:{2}, worm:{3}, strengthcheck:{4}, storm:{5}",
                source,
                destination,
                AllyNotIn(destination.Territory),
                ProbablySafeFromShaiHulud(destination.Territory),
                StrengthOfOpponents(destination.Territory) <= maxEnemyForceStrength,
                (Game.IsProtectedFromStorm(destination) || !StormWillProbablyHit(destination))
                );*/

            var opponent = GetOpponentThatOccupies(destination.Territory);

            return WithinRange(source, destination, b) &&
                AllyNotIn(destination.Territory) &&
                ProbablySafeFromShaiHulud(destination.Territory) &&
                (opponent == null || mayFight && GetDialNeeded(destination.Territory, opponent, false) < MaxDial(Resources, b, opponent.Faction)) &&
                !StormWillProbablyHit(destination);
        }

        private Location BestSafeAndNearbyResources(Location location, Battalion b, bool mayFight = false)
        {
            return Game.ResourcesOnPlanet.OrderByDescending(r => r.Value).FirstOrDefault(l => IsSafeAndNearby(location, l.Key, b, mayFight)).Key;
        }

        private Location DetermineMostSuitableNearbyLocation(KeyValuePair<Location, Battalion> battalionAtLocation, bool includeSecondBestLocations, bool mustMove)
        {
            return DetermineMostSuitableNearbyLocation(battalionAtLocation.Key, battalionAtLocation.Value, includeSecondBestLocations, mustMove);
        }

        private Location DetermineMostSuitableNearbyLocation(Location location, Battalion battalion, bool includeSecondBestLocations, bool mustMove)
        {
            var result = VacantAndSafeNearbyStronghold(location, battalion);
            LogInfo("Suitable EmptyAndSafeNearbyStronghold: {0}", result);

            if (result == null) result = WeakAndSafeNearbyStronghold(location, battalion);
            LogInfo("Suitable WeakAndSafeNearbyStronghold: {0}", result);

            if (result == null && !LastTurn) result = BestSafeAndNearbyResources(location, battalion, false);
            LogInfo("Suitable BestSafeAndNearbyResources without fighting: {0}", result);

            if (result == null) result = UnthreatenedAndSafeNearbyStronghold(location, battalion);
            LogInfo("Suitable UnthreatenedAndSafeNearbyStronghold: {0}", result);

            if (result == null) result = WinnableNearbyStronghold(location, battalion);
            LogInfo("Suitable WinnableNearbyStronghold: {0}", result);

            if (result == null && !LastTurn) result = BestSafeAndNearbyResources(location, battalion, true);
            LogInfo("Suitable BestSafeAndNearbyResources with fighting: {0}", result);

            if (includeSecondBestLocations)
            {
                if (result == null && !LastTurn && WithinRange(location, Game.Map.PolarSink, battalion)) result = Game.Map.PolarSink;
                LogInfo("Suitable - Polar Sink nearby? {0}", result);

                if (result == null && location != Game.Map.PolarSink) result = Game.Map.Locations.Where(l => Game.IsProtectedFromStorm(l) && WithinRange(location, l, battalion) && NotOccupiedByOthers(l.Territory) && l.Territory != location.Territory).FirstOrDefault();
                LogInfo("Suitable nearby Rock: {0}", result);
            }

            if (result == null && mustMove) result = PlacementEvent.ValidTargets(Game, this, location, battalion).FirstOrDefault(l => AllyNotIn(l.Territory) && l != location);
            LogInfo("Suitable - any location without my ally: {0}", result);

            return result;
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

        public IEnumerable<Battalion> ForcesInTerritory(Territory t)
        {
            return ForcesOnPlanet.Where(f => f.Key.Territory == t).Select(f => f.Value);
        }

    }

}
