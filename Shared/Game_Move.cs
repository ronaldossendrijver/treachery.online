/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public Location LastShippedOrMovedTo { get; private set; }

        public bool ShipsTechTokenIncome;
        public List<PlacementEvent> RecentMoves = new List<PlacementEvent>();
        private List<Faction> FactionsWithOrnithoptersAtStartOfMovement;
        private bool BeginningOfShipmentAndMovePhase;
        public int CurrentNoFieldValue { get; private set; } = -1;
        public int LatestRevealedNoFieldValue { get; private set; } = -1;

        private void EnterShipmentAndMovePhase()
        {
            MainPhaseStart(MainPhase.ShipmentAndMove);
            FactionsWithOrnithoptersAtStartOfMovement = Players.Where(p => OccupiesArrakeenOrCarthag(p)).Select(p => p.Faction).ToList();
            RecentMoves.Clear();
            ReceiveGraveyardTechIncome();
            BeginningOfShipmentAndMovePhase = true;

            if (Version >= 46)
            {
                FactionsWithIncreasedRevivalLimits = new Faction[] { };
                AllowedEarlyRevivals.Clear();
            }

            ShipsTechTokenIncome = false;
            CurrentFreeRevivalPrevention = null;
            Allow(FactionAdvantage.BlueAnnouncesBattle);
            Allow(FactionAdvantage.RedLetAllyReviveExtraForces);
            Allow(FactionAdvantage.PurpleReceiveRevive);
            Allow(FactionAdvantage.BrownRevival);
            
            HasActedOrPassed.Clear();
            LastShippedOrMovedTo = null;

            ShipmentAndMoveSequence.Start(false, 1);
            if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && Applicable(Rule.OrangeDetermineShipment))
            {
                ShipmentAndMoveSequence.NextPlayer(false);
            }

            Enter(Version >= 107, Phase.BeginningOfShipAndMove, StartShipAndMoveSequence);
        }

        private void StartShipAndMoveSequence()
        {
            Enter(JuiceForcesFirstPlayer && CurrentJuice.Initiator != Faction.Orange, Phase.NonOrangeShip, IsPlaying(Faction.Orange) && Applicable(Rule.OrangeDetermineShipment), Phase.OrangeShip, Phase.NonOrangeShip);
        }

        public bool OccupiesArrakeenOrCarthag(Player p)
        {
            return p.Occupies(Map.Arrakeen) || p.Occupies(Map.Carthag);
        }

        private void ReceiveGraveyardTechIncome()
        {
            if (RevivalTechTokenIncome)
            {
                var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Graveyard));
                if (techTokenOwner != null)
                {
                    var amount = techTokenOwner.TechTokens.Count;
                    techTokenOwner.Resources += amount;
                    CurrentReport.Add(techTokenOwner.Faction, "{0} receive {1} from {2}.", techTokenOwner.Faction, amount, TechToken.Graveyard);
                }
            }
        }

        public void HandleEvent(OrangeDelay e)
        {
            BeginningOfShipmentAndMovePhase = false;
            CurrentReport.Add(e);
            Enter(Phase.NonOrangeShip);
        }

        private bool BGMayAccompany;
        private List<Territory> ChosenDestinationsWithAllies = new List<Territory>();

        public void HandleEvent(Shipment s)
        {
            BeginningOfShipmentAndMovePhase = false;
            CurrentBlockedTerritories.Clear();
            StormLossesToTake.Clear();
            ChosenDestinationsWithAllies.Clear();

            BGMayAccompany = false;
            var initiator = GetPlayer(s.Initiator);

            MessagePart orangeIncome = new MessagePart("");

            int totalCost = 0;

            if (!s.Passed)
            {
                totalCost = PayForShipment(s, initiator);
                int orangeProfit = HandleOrangeProfit(s, initiator, ref orangeIncome);

                if (totalCost - orangeProfit >= 4)
                {
                    ActivateBanker();
                }

                if (s.IsNoField)
                {
                    RevealCurrentNoField(GetPlayer(Faction.White));
                    CurrentNoFieldValue = s.NoFieldValue;

                    if (s.Initiator != Faction.White)
                    {
                        //Immediately reveal No-Field
                        LatestRevealedNoFieldValue = s.NoFieldValue;
                        CurrentNoFieldValue = -1;
                    }
                }

                if (ContainsConflictingAlly(initiator, s.To))
                {
                    ChosenDestinationsWithAllies.Add(s.To.Territory);
                }

                LastShippedOrMovedTo = s.To;
                bool mustBeAdvisors = (initiator.Is(Faction.Blue) && initiator.SpecialForcesIn(s.To) > 0);

                if (s.IsSiteToSite)
                {
                    PerformSiteToSiteShipment(s, initiator);
                }
                else if (s.IsBackToReserves)
                {
                    PerformRetreatShipment(s, initiator);
                }
                else if (s.IsNoField && s.Initiator == Faction.White)
                {
                    PerformNormalShipment(initiator, s.To, 0, 1, false);
                }
                else
                {
                    PerformNormalShipment(initiator, s.To, s.ForceAmount, s.SpecialForceAmount, s.IsSiteToSite);
                }

                if (s.Initiator != Faction.Yellow)
                {
                    RecentMilestones.Add(Milestone.Shipment);
                }

                if (Version >= 89 || mustBeAdvisors) initiator.FlipForces(s.To, mustBeAdvisors);

                DetermineNextShipmentAndMoveSubPhase(DetermineIntrusionCaused(s), BGMayAccompany);
                               

                FlipBeneGesseritWhenAlone();
            }
            else
            {
                DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
            }

            CurrentReport.Add(s.GetVerboseMessage(totalCost, orangeIncome));
        }

        public bool ContainsConflictingAlly(Player initiator, Location to)
        {
            if (initiator.Ally == Faction.None || to == Map.PolarSink || to == null) return false;

            var ally = initiator.AlliedPlayer;

            if (initiator.Ally == Faction.Blue && Applicable(Rule.AdvisorsDontConflictWithAlly)) {

                return ally.ForcesIn(to.Territory) > 0;
            }
            else
            {
                return ally.AnyForcesIn(to.Territory) > 0;
            }
        }

        private int PayForShipment(Shipment s, Player initiator)
        {
            var costToInitiator = s.DetermineCostToInitiator(this);
            initiator.Resources -= costToInitiator;

            if (s.UsingKarma(this))
            {
                var karmaCard = s.GetKarmaCard(this, initiator);
                Discard(karmaCard);
                RecentMilestones.Add(Milestone.Karma);
            }

            if (s.AllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= s.AllyContributionAmount;
                if (Version >= 76) DecreasePermittedUseOfAllySpice(initiator.Faction, s.AllyContributionAmount);
            }

            return costToInitiator + s.AllyContributionAmount;
        }

        private int HandleOrangeProfit(Shipment s, Player initiator, ref MessagePart profitMessage)
        {
            int receiverProfit = 0;
            var orange = GetPlayer(Faction.Orange);
            
            if (orange != null && initiator != orange && !s.UsingKarma(this))
            {
                receiverProfit = s.DetermineOrangeProfits(this);

                if (receiverProfit > 0)
                {
                    if (!Prevented(FactionAdvantage.OrangeReceiveShipment))
                    {
                        orange.Resources += receiverProfit;

                        if (receiverProfit > 0)
                        {
                            profitMessage = new MessagePart(" {0} receive {1}.", Faction.Orange, receiverProfit);
                        }

                        if (receiverProfit >= 5)
                        {
                            ApplyBureaucracy(initiator.Faction, Faction.Orange);
                        }
                    }
                    else
                    {
                        profitMessage = new MessagePart(" {0} prevents {1} from receiving {2}.", TreacheryCardType.Karma, Faction.Orange, Concept.Resource);
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.OrangeReceiveShipment);
                    }
                }
            }

            return receiverProfit;
        }

        private void PerformRetreatShipment(Shipment s, Player initiator)
        {
            initiator.ShipForces(s.To, s.ForceAmount);
        }

        private void PerformSiteToSiteShipment(Shipment s, Player initiator)
        {
            initiator.MoveForces(s.From, s.To, s.ForceAmount);
            initiator.MoveSpecialForces(s.From, s.To, s.SpecialForceAmount);
        }

        private void PerformNormalShipment(Player initiator, Location to, int forceAmount, int specialForceAmount, bool isSiteToSite)
        {
            BGMayAccompany = (forceAmount > 0 || specialForceAmount > 0) && initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Blue;
            initiator.ShipForces(to, forceAmount);
            initiator.ShipSpecialForces(to, specialForceAmount);

            if (initiator.Is(Faction.Yellow) && IsInStorm(to))
            {
                int killCount;
                if (!Prevented(FactionAdvantage.YellowProtectedFromStorm) && Applicable(Rule.YellowStormLosses))
                {
                    killCount = 0;
                    StormLossesToTake.Add(new LossToTake() { Location = to, Amount = LossesToTake(forceAmount, specialForceAmount), Faction = Faction.Yellow });
                }
                else
                {
                    killCount = initiator.KillForces(to, forceAmount, specialForceAmount, false);
                }

                if (killCount > 0)
                {
                    CurrentReport.Add(initiator.Faction, "{0} {1} {2} are killed by the storm.", killCount, initiator.Faction, initiator.Force);
                }
            }

            if (initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Orange && !isSiteToSite)
            {
                ShipsTechTokenIncome = true;
            }
        }

        public bool MayShipAsGuild(Player p)
        {
            return ((!Prevented(FactionAdvantage.OrangeSpecialShipments) && p.Is(Faction.Orange)) || p.Ally == Faction.Orange && OrangeAllyMayShipAsGuild);
        }

        public bool MayShipWithDiscount(Player p)
        {
            return (p.Is(Faction.Orange) && !Prevented(FactionAdvantage.OrangeShipmentsDiscount)) ||
                   (p.Ally == Faction.Orange && OrangeAllyMayShipAsGuild && !Prevented(FactionAdvantage.OrangeShipmentsDiscountAlly));
        }

        private bool BlueMustBeFighterIn(Location l)
        {
            if (l == null) return true;

            var benegesserit = GetPlayer(Faction.Blue);
            return benegesserit.Occupies(l.Territory) || !IsOccupied(l.Territory) || !Applicable(Rule.BlueAdvisors);
        }

        public void HandleEvent(BlueAccompanies c)
        {
            var benegesserit = GetPlayer(c.Initiator);
            if (c.Accompanies && benegesserit.ForcesInReserve > 0)
            {
                if (BlueMustBeFighterIn(c.Location))
                {
                    benegesserit.ShipForces(c.Location, 1);
                    CurrentReport.Add(c.Initiator, "{0} ship a fighter to {1}.", c.Initiator, c.Location);
                }
                else
                {
                    benegesserit.ShipAdvisors(c.Location, 1);
                    CurrentReport.Add(c.Initiator, "{0} ship an advisor to {1}.", c.Initiator, c.Location);
                }
            }
            else
            {

                CurrentReport.Add(c.Initiator, "{0} don't accompany this shipment.", c.Initiator);
            }

            DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
        }

        private bool MayPerformExtraMove { get; set; }
        public void HandleEvent(Move m)
        {
            RecentMoves.Add(m);

            StormLossesToTake.Clear();
            var initiator = GetPlayer(m.Initiator);
            HasActedOrPassed.Add(m.Initiator);

            if (CurrentPhase == Phase.NonOrangeMove)
            {
                ShipmentAndMoveSequence.NextPlayer(false);

                if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && Applicable(Rule.OrangeDetermineShipment))
                {
                    ShipmentAndMoveSequence.NextPlayer(false);
                }
            }

            bool intrusionCaused = false;
            if (!m.Passed)
            {
                if (ContainsConflictingAlly(initiator, m.To))
                {
                    ChosenDestinationsWithAllies.Add(m.To.Territory);
                }

                PerformMoveFromLocations(initiator, m.ForceLocations, m.To, m.Initiator != Faction.Blue || m.AsAdvisors, false);
                intrusionCaused = DetermineIntrusionCaused(m);
            }
            else
            {
                CurrentReport.Add(m.Initiator, "{0} pass movement.", m.Initiator);
            }

            DetermineNextShipmentAndMoveSubPhase(intrusionCaused, false);
            CheckIfForcesShouldBeDestroyedByAllyPresence(initiator);

            if (Version >= 87)
            {
                FlipBeneGesseritWhenAlone();
            }

            MayPerformExtraMove = (CurrentFlightUsed != null && CurrentFlightUsed.ExtraMove);
            CurrentFlightUsed = null;

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowExtraMove);
            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyCyborgExtraMove);
            CurrentPlanetology = null;
        }

        private bool DetermineIntrusionCaused(PlacementEvent m)
        {
            return DetermineIntrusionCaused(m.Initiator, m.ForceLocations.Values.Sum(b => b.AmountOfForces) + m.ForceLocations.Values.Sum(b => b.AmountOfSpecialForces), m.To.Territory);
        }

        private bool DetermineIntrusionCaused(Shipment s)
        {
            return DetermineIntrusionCaused(s.Initiator, s.ForceAmount + s.SpecialForceAmount, s.To.Territory);
        }

        private bool DetermineIntrusionCaused(Faction initiator, int nrOfForces, Territory territory)
        {
            var bgPlayer = GetPlayer(Faction.Blue);

            return
                (Version <= 72 || territory != Map.PolarSink.Territory) &&
                Applicable(Rule.BlueAdvisors) &&
                bgPlayer != null &&
                initiator != bgPlayer.Ally &&
                initiator != Faction.Blue &&
                nrOfForces > 0 &&
                bgPlayer.ForcesIn(territory) > 0;
        }

        private Phase PausedPhase { get; set; }

        public void HandleEvent(Caravan e)
        {
            RecentMoves.Add(e);

            StormLossesToTake.Clear();
            var initiator = GetPlayer(e.Initiator);
            var card = initiator.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Caravan);

            Discard(initiator, TreacheryCardType.Caravan);
            PerformMoveFromLocations(initiator, e.ForceLocations, e.To, e.Initiator != Faction.Blue || e.AsAdvisors, true);

            if (ContainsConflictingAlly(initiator, e.To))
            {
                ChosenDestinationsWithAllies.Add(e.To.Territory);
            }

            if (DetermineIntrusionCaused(e))
            {
                PausedPhase = CurrentPhase;
                Enter(Phase.BlueIntrudedByCaravan);
            }

            CurrentFlightUsed = null;

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowExtraMove);
            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyCyborgExtraMove);
        }

        private void PerformMoveFromLocations(Player initiator, Dictionary<Location, Battalion> forceLocations, Location to, bool asAdvisors, bool byCaravan)
        {
            LastShippedOrMovedTo = to;
            bool wasOccupiedBeforeMove = IsOccupied(to.Territory);

            foreach (var fromTerritory in forceLocations.Keys.Select(l => l.Territory).Distinct())
            {
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;

                foreach (var fl in forceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                {
                    PerformMoveFromLocation(initiator, fl.Key, fl.Value, to, ref totalNumberOfForces, ref totalNumberOfSpecialForces);
                }

                if (initiator.Is(Faction.Blue))
                {
                    initiator.FlipForces(to, wasOccupiedBeforeMove && asAdvisors);
                }
                                
                LogMove(initiator, fromTerritory, to, totalNumberOfForces, totalNumberOfSpecialForces, wasOccupiedBeforeMove && asAdvisors, byCaravan);
            }

            RecentMilestones.Add(Milestone.Move);
            FlipBeneGesseritWhenAlone();
        }

        private void PerformMoveFromLocation(Player initiator, Location from, Battalion battalion, Location to, ref int totalNumberOfForces, ref int totalNumberOfSpecialForces)
        {
            bool mustMoveThroughStorm = MustMoveThroughStorm(initiator, from, to, battalion);
            if (IsInStorm(to) || mustMoveThroughStorm)
            {
                int killCount;
                if (initiator.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowProtectedFromStorm) && Applicable(Rule.YellowStormLosses))
                {
                    killCount = 0;
                    initiator.MoveForces(from, to, battalion.AmountOfForces);
                    initiator.MoveSpecialForces(from, to, battalion.AmountOfSpecialForces);
                    StormLossesToTake.Add(new LossToTake() { Location = to, Amount = LossesToTake(battalion), Faction = battalion.Faction });
                }
                else
                {
                    killCount = battalion.AmountOfForces + battalion.AmountOfSpecialForces;
                    initiator.KillForces(from, battalion.AmountOfForces, battalion.AmountOfSpecialForces, false);
                }

                if (killCount > 0)
                {
                    CurrentReport.Add(initiator.Faction, "{0} {1} {2} are killed by the storm while travelling.", killCount, initiator.Faction, initiator.Force);
                }
            }
            else
            {
                initiator.MoveForces(from, to, battalion.AmountOfForces);
                initiator.MoveSpecialForces(from, to, battalion.AmountOfSpecialForces);
            }

            totalNumberOfForces += battalion.AmountOfForces;
            totalNumberOfSpecialForces += battalion.AmountOfSpecialForces;
        }

        private FlightUsed CurrentFlightUsed = null;
        public void HandleEvent(FlightUsed e)
        {
            CurrentReport.Add(e);
            Discard(e.Player, TreacheryCardType.Flight);
            CurrentFlightUsed = e;
        }

        private bool MustMoveThroughStorm(Player initiator, Location from, Location to, Battalion moved)
        {
            if (from == null || to == null) return false;

            var max = DetermineMaximumMoveDistance(initiator, new Battalion[] { moved } );
            var targetsAvoidingStorm = Map.FindNeighbours(from, max, false, initiator.Faction, SectorInStorm, ForcesOnPlanet, CurrentBlockedTerritories);
            var targetsIgnoringStorm = Map.FindNeighbours(from, max, true, initiator.Faction, SectorInStorm, ForcesOnPlanet, CurrentBlockedTerritories);
            return !targetsAvoidingStorm.Contains(to) && targetsIgnoringStorm.Contains(to);
        }

        private void LogMove(Player initiator, Territory from, Location to, int forceAmount, int specialForceAmount, bool asAdvisors, bool byCaravan)
        {
            if (initiator.Is(Faction.Blue))
            {
                CurrentReport.Add(initiator.Faction, "{5}{0} move {1} forces from {2} to {3} as {4}.",
                    initiator.Faction, forceAmount + specialForceAmount, from, to, asAdvisors ? (object)initiator.SpecialForce : initiator.Force, CaravanMessage(byCaravan));
            }
            else if (specialForceAmount > 0)
            {
                CurrentReport.Add(initiator.Faction, "{7}{0} move {1} {6} and {2} {5} from {3} to {4}.",
                    initiator.Faction, forceAmount, specialForceAmount, from, to, initiator.SpecialForce, initiator.Force, CaravanMessage(byCaravan));
            }
            else
            {
                CurrentReport.Add(initiator.Faction, "{5}{0} move {1} {2} from {3} to {4}.",
                    initiator.Faction, forceAmount, initiator.Force, from, to, CaravanMessage(byCaravan));
            }
        }

        private MessagePart CaravanMessage(bool byCaravan)
        {
            if (byCaravan)
            {
                return new MessagePart("By {0}, ", TreacheryCardType.Caravan);
            }
            else
            {
                return new MessagePart("");
            }
        }

        public void HandleEvent(BlueFlip e)
        {
            var initiator = GetPlayer(e.Initiator);

            if (Version < 77)
            {
                initiator.FlipForces(LastShippedOrMovedTo, e.AsAdvisors);
            }
            else
            {
                initiator.FlipForces(LastShippedOrMovedTo.Territory, e.AsAdvisors);
            }

            CurrentReport.Add(e);
            if (Version >= 102) FlipBeneGesseritWhenAlone();
            DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
        }

        private void DetermineNextShipmentAndMoveSubPhase(bool intrusionCaused, bool bgMayAccompany)
        {
            if (StormLossesToTake.Count > 0)
            {
                PhaseBeforeStormLoss = CurrentPhase;
                intrusionCausedBeforeStormLoss = intrusionCaused;
                bgMayAccompanyBeforeStormLoss = bgMayAccompany;
                Enter(Phase.StormLosses);
            }
            else
            {
                switch (CurrentPhase)
                {
                    case Phase.YellowRidingMonsterA:
                        Enter(intrusionCaused, Phase.BlueIntrudedByYellowRidingMonsterA, EndWormRide);
                        break;

                    case Phase.YellowRidingMonsterB:
                        Enter(intrusionCaused, Phase.BlueIntrudedByYellowRidingMonsterB, EndWormRide);
                        break;

                    case Phase.OrangeShip:
                        Enter(intrusionCaused, Phase.BlueIntrudedByOrangeShip, IsPlaying(Faction.Blue) && bgMayAccompany, Phase.BlueAccompaniesOrange, Phase.OrangeMove);
                        break;

                    case Phase.NonOrangeShip:
                        Enter(intrusionCaused, Phase.BlueIntrudedByNonOrangeShip, IsPlaying(Faction.Blue) && bgMayAccompany, Phase.BlueAccompaniesNonOrange, Phase.NonOrangeMove);
                        break;

                    case Phase.BlueIntrudedByOrangeShip:
                        Enter(IsPlaying(Faction.Blue) && bgMayAccompany, Phase.BlueAccompaniesOrange, Phase.OrangeMove);
                        break;

                    case Phase.BlueIntrudedByNonOrangeShip:
                        Enter(IsPlaying(Faction.Blue) && bgMayAccompany, Phase.BlueAccompaniesNonOrange, Phase.NonOrangeMove);
                        break;

                    case Phase.BlueAccompaniesOrange: Enter(Phase.OrangeMove); break;
                    case Phase.BlueAccompaniesNonOrange: Enter(Phase.NonOrangeMove); break;

                    case Phase.OrangeMove:
                        Enter(intrusionCaused, Phase.BlueIntrudedByOrangeMove, DetermineNextSubPhaseAfterOrangeMove);
                        break;

                    case Phase.NonOrangeMove:
                        Enter(intrusionCaused, Phase.BlueIntrudedByNonOrangeMove, DetermineNextSubPhaseAfterNonOrangeMove);
                        break;

                    case Phase.BlueIntrudedByOrangeMove: DetermineNextSubPhaseAfterOrangeMove(); break;
                    case Phase.BlueIntrudedByCaravan: Enter(PausedPhase); break;
                    case Phase.BlueIntrudedByNonOrangeMove: DetermineNextSubPhaseAfterNonOrangeMove(); break;
                    case Phase.BlueIntrudedByYellowRidingMonsterA:
                        {
                            Enter(Phase.YellowRidingMonsterA);
                            EndWormRide();
                            break;
                        }
                    case Phase.BlueIntrudedByYellowRidingMonsterB:
                        {
                            Enter(Phase.YellowRidingMonsterB);
                            EndWormRide();
                            break;
                        }
                }
            }
        }

        private void CheckIfForcesShouldBeDestroyedByAllyPresence(Player p)
        {
            if (p.Ally != Faction.None)
            {
                if (Version >= 86)
                {
                    //Forces that must be destroyed because moves ended where allies are
                    foreach (var t in ChosenDestinationsWithAllies)
                    {
                        if (p.AnyForcesIn(t) > 0)
                        {
                            CurrentReport.Add(p.Faction, "All {0} forces in {1} were killed due to ally presence.", p.Faction, t);
                            p.KillAllForces(t, false);
                        }
                    }
                }

                if (HasActedOrPassed.Contains(p.Ally))
                {
                    //Forces that must me destroyed if both the player and his ally have moved

                    var playerTerritories = Applicable(Rule.AdvisorsDontConflictWithAlly) ? p.OccupiedTerritories : p.TerritoriesWithForces;
                    var allyTerritories = Applicable(Rule.AdvisorsDontConflictWithAlly) ? GetPlayer(p.Ally).OccupiedTerritories : GetPlayer(p.Ally).TerritoriesWithForces;

                    foreach (var t in playerTerritories.Intersect(allyTerritories).ToList())
                    {
                        if (t != Map.PolarSink.Territory)
                        {
                            CurrentReport.Add(p.Faction, "All {0} forces in {1} were killed due to ally presence.", p.Faction, t);
                            p.KillAllForces(t, false);
                        }
                    }
                }
            }
        }

        public bool ThreatenedByAllyPresence(Player p, Territory t)
        {
            if (t == Map.PolarSink.Territory) return false;

            var ally = GetPlayer(p.Ally);
            if (ally == null) return false;

            if (Applicable(Rule.AdvisorsDontConflictWithAlly))
            {

                if (p.Is(Faction.Blue) && !p.Occupies(t))
                {
                    return false;
                }
                else if (p.Ally == Faction.Blue && !ally.Occupies(t))
                {
                    return false;
                }
                else
                {
                    return ally.AnyForcesIn(t) > 0;
                }
            }
            else
            {
                return ally.AnyForcesIn(t) > 0;
            }
        }

        private void DetermineNextSubPhaseAfterNonOrangeMove()
        {
            if (MayPerformExtraMove)
            {
                MayPerformExtraMove = false;
                Enter(Phase.NonOrangeMove);
            }
            else
            {
                Enter(
                    EveryoneActedOrPassed, ConcludeShipmentAndMove,
                    IsPlaying(Faction.Orange) && !HasActedOrPassed.Any(p => p == Faction.Orange) && Applicable(Rule.OrangeDetermineShipment), Phase.OrangeShip,
                    Phase.NonOrangeShip);
            }
        }

        public bool OrangeMayDelay => Applicable(Rule.OrangeDetermineShipment) && Players.Count - HasActedOrPassed.Count > (JuiceForcesLastPlayer && !HasActedOrPassed.Contains(CurrentJuice.Initiator) ? 2 : 1);

        private void DetermineNextSubPhaseAfterOrangeMove()
        {
            if (MayPerformExtraMove)
            {
                MayPerformExtraMove = false;
                Enter(Phase.OrangeMove);
            }
            else
            {
                Enter(!EveryoneActedOrPassed, Phase.NonOrangeShip, ConcludeShipmentAndMove);
            }
        }

        private void ConcludeShipmentAndMove()
        {
            MainPhaseEnd();
            Enter(Phase.ShipmentAndMoveConcluded);
            ReceiveShipsTechIncome();
            BrownHasExtraMove = false;
        }

        public List<Territory> CurrentBlockedTerritories = new List<Territory>();
        public void HandleEvent(BrownMovePrevention e)
        {
            CurrentReport.Add(e);
            Discard(e.CardUsed());
            CurrentBlockedTerritories.Add(e.Territory);
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }

        private bool BrownHasExtraMove { get; set; } = false;
        public void HandleEvent(BrownExtraMove e)
        {
            CurrentReport.Add(e);
            Discard(e.CardUsed());
            BrownHasExtraMove = true;
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }

        private void ReceiveShipsTechIncome()
        {
            if (ShipsTechTokenIncome)
            {
                var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Ships));
                if (techTokenOwner != null)
                {
                    var amount = techTokenOwner.TechTokens.Count;
                    techTokenOwner.Resources += amount;
                    CurrentReport.Add(techTokenOwner.Faction, "{0} receive {1} from {2}.", techTokenOwner.Faction, amount, TechToken.Ships);
                }
            }
        }
    }
}
