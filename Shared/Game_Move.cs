/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public Location LastShippedOrMovedTo { get; private set; }
        public PlayerSequence ShipmentAndMoveSequence { get; private set; }
        public bool ShipsTechTokenIncome { get; private set; }
        public List<PlacementEvent> RecentMoves { get; private set; } = new List<PlacementEvent>();
        public int CurrentNoFieldValue { get; private set; } = -1;
        public int LatestRevealedNoFieldValue { get; private set; } = -1;

        private List<Faction> FactionsWithOrnithoptersAtStartOfMovement;
        private bool BeginningOfShipmentAndMovePhase;

        private void EnterShipmentAndMovePhase()
        {
            MainPhaseStart(MainPhase.ShipmentAndMove);
            FactionsWithOrnithoptersAtStartOfMovement = Players.Where(p => OccupiesArrakeenOrCarthag(p)).Select(p => p.Faction).ToList();
            RecentMoves.Clear();
            BeginningOfShipmentAndMovePhase = true;
            FactionsWithIncreasedRevivalLimits = new Faction[] { };
            AllowedEarlyRevivals.Clear();

            ShipsTechTokenIncome = false;
            CurrentFreeRevivalPrevention = null;
            Allow(FactionAdvantage.BlueAnnouncesBattle);
            Allow(FactionAdvantage.RedLetAllyReviveExtraForces);
            Allow(FactionAdvantage.PurpleReceiveRevive);
            Allow(FactionAdvantage.BrownRevival);

            HasActedOrPassed.Clear();
            LastShippedOrMovedTo = null;

            ShipmentAndMoveSequence = new PlayerSequence(this);
            if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && OrangeMayShipOutOfTurnOrder)
            {
                ShipmentAndMoveSequence.NextPlayer();
            }

            Enter(Version >= 107, Phase.BeginningOfShipAndMove, StartShipAndMoveSequence);
        }

        private void StartShipAndMoveSequence()
        {
            Enter(JuiceForcesFirstPlayer && CurrentJuice.Initiator != Faction.Orange, Phase.NonOrangeShip, IsPlaying(Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip, Phase.NonOrangeShip);
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
                    CurrentReport.Express(techTokenOwner.Faction, " receive ", Payment(amount), " from ", TechToken.Graveyard);
                }
            }
        }

        public void HandleEvent(OrangeDelay e)
        {
            BeginningOfShipmentAndMovePhase = false;
            CurrentReport.Express(e);
            Enter(Phase.NonOrangeShip);
        }

        private bool BGMayAccompany;
        private readonly List<Territory> ChosenDestinationsWithAllies = new List<Territory>();

        public void HandleEvent(Shipment s)
        {
            BeginningOfShipmentAndMovePhase = false;
            CurrentBlockedTerritories.Clear();
            StormLossesToTake.Clear();
            ChosenDestinationsWithAllies.Clear();

            BGMayAccompany = false;
            var initiator = GetPlayer(s.Initiator);

            MessagePart orangeIncome = MessagePart.Express();
            int totalCost = 0;

            if (!s.Passed)
            {
                var ownerOfKarma = s.KarmaCard != null ? OwnerOf(s.KarmaCard) : null;
                totalCost = PayForShipment(s, initiator);

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
                    PerformNormalShipment(initiator, s.To, 0 + s.SmuggledAmount, 1, false);
                }
                else
                {
                    PerformNormalShipment(initiator, s.To, s.ForceAmount + s.SmuggledAmount, s.SpecialForceAmount + s.SmuggledSpecialAmount, s.IsSiteToSite);
                }

                if (s.Initiator != Faction.Yellow)
                {
                    RecentMilestones.Add(Milestone.Shipment);
                }

                initiator.FlipForces(s.To, mustBeAdvisors);

                DetermineNextShipmentAndMoveSubPhase(DetermineIntrusionCaused(s), BGMayAccompany);

                int orangeProfit = HandleOrangeProfit(s, initiator, ref orangeIncome);
                CurrentReport.Express(s.GetVerboseMessage(totalCost, orangeIncome, ownerOfKarma));

                if (totalCost - orangeProfit >= 4)
                {
                    ActivateBanker(initiator);
                }

                FlipBeneGesseritWhenAlone();
            }
            else
            {
                CurrentReport.Express(s.GetVerboseMessage(totalCost, orangeIncome, null));
                DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
            }
        }

        public bool ContainsConflictingAlly(Player initiator, Location to)
        {
            if (initiator.Ally == Faction.None || to == Map.PolarSink || to == null) return false;

            var ally = initiator.AlliedPlayer;

            if (initiator.Ally == Faction.Blue && Applicable(Rule.AdvisorsDontConflictWithAlly))
            {

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
                var karmaCard = s.KarmaCard;
                Discard(karmaCard);
                RecentMilestones.Add(Milestone.Karma);
            }

            if (s.AllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= s.AllyContributionAmount;
                DecreasePermittedUseOfAllySpice(initiator.Faction, s.AllyContributionAmount);
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
                            profitMessage = MessagePart.Express(" → ", Faction.Orange, " get ", Payment(receiverProfit));
                        }

                        if (receiverProfit >= 5)
                        {
                            ApplyBureaucracy(initiator.Faction, Faction.Orange);
                        }
                    }
                    else
                    {
                        profitMessage = MessagePart.Express(" → ", TreacheryCardType.Karma, " prevents ", Faction.Orange, " from receiving", new Payment(receiverProfit));
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
                    CurrentReport.Express(killCount, initiator.Faction, " forces are killed by the storm");
                }
            }

            if (initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Orange && !isSiteToSite)
            {
                ShipsTechTokenIncome = true;
            }
        }

        public bool MayShipAsGuild(Player p)
        {
            return !Prevented(FactionAdvantage.OrangeSpecialShipments) && (p.Is(Faction.Orange) || p.Ally == Faction.Orange && OrangeAllyMayShipAsGuild);
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
                }
                else
                {
                    benegesserit.ShipAdvisors(c.Location, 1);
                }

                CurrentReport.Express(c.Initiator, " accompany to ", c.Location);
            }
            else
            {

                CurrentReport.Express(c.Initiator, " don't accompany");
            }

            DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
        }

        private bool MayPerformExtraMove { get; set; }
        public void HandleEvent(Move m)
        {
            RecentMoves.Add(m);

            StormLossesToTake.Clear();
            var initiator = GetPlayer(m.Initiator);

            MayPerformExtraMove = (CurrentFlightUsed != null && CurrentFlightUsed.Initiator == m.Initiator && CurrentFlightUsed.ExtraMove);

            if (!MayPerformExtraMove)
            {
                HasActedOrPassed.Add(m.Initiator);

                if (CurrentPhase == Phase.NonOrangeMove)
                {
                    ShipmentAndMoveSequence.NextPlayer();

                    if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && OrangeMayShipOutOfTurnOrder)
                    {
                        ShipmentAndMoveSequence.NextPlayer();
                    }
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
                CurrentReport.Express(m.Initiator, " pass movement");
            }

            DetermineNextShipmentAndMoveSubPhase(intrusionCaused, false);
            CheckIfForcesShouldBeDestroyedByAllyPresence(initiator);
            FlipBeneGesseritWhenAlone();

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowExtraMove);
            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyCyborgExtraMove);

            CurrentFlightUsed = null;
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
                territory != Map.PolarSink.Territory &&
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
            var dist = DetermineMaximumMoveDistance(initiator, forceLocations.Values);

            foreach (var fromTerritory in forceLocations.Keys.Select(l => l.Territory).Distinct())
            {
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;

                foreach (var fl in forceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                {
                    PerformMoveFromLocation(initiator, fl.Key, fl.Value, to, ref totalNumberOfForces, ref totalNumberOfSpecialForces);
                    CheckSandmaster(initiator, to, dist, fl);
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

        private void CheckSandmaster(Player initiator, Location to, int dist, KeyValuePair<Location, Battalion> fl)
        {
            if (SkilledAs(initiator, LeaderSkill.Sandmaster))
            {
                var paths = Map.FindPaths(fl.Key, to, dist, initiator.Faction == Faction.Yellow && Applicable(Rule.YellowMayMoveIntoStorm), initiator.Faction, this);
                int mostSpice = 0;
                List<Location> pathWithMostSpice = null;
                foreach (var p in paths)
                {
                    int amountOfSpiceLocations = p.Where(l => ResourcesOnPlanet.ContainsKey(l)).Distinct().Count();
                    if (amountOfSpiceLocations > mostSpice)
                    {
                        mostSpice = amountOfSpiceLocations;
                        pathWithMostSpice = p;
                    }
                }

                if (mostSpice > 0)
                {
                    foreach (var loc in pathWithMostSpice)
                    {
                        if (ResourcesOnPlanet.ContainsKey(loc))
                        {
                            ChangeResourcesOnPlanet(loc, -1);
                        }
                    }

                    initiator.Resources += mostSpice;
                    CurrentReport.Express(initiator.Faction, " ", LeaderSkill.Sandmaster, " collects ", Payment(mostSpice), " along the way");
                }
            }
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
                    CurrentReport.Express(killCount, initiator.Faction, "forces are killed by the storm while travelling");
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

        private FlightUsed CurrentFlightUsed { get; set; }

        public void HandleEvent(FlightUsed e)
        {
            CurrentReport.Express(e);
            Discard(e.Player, TreacheryCardType.Flight);
            CurrentFlightUsed = e;
        }

        private bool MustMoveThroughStorm(Player initiator, Location from, Location to, Battalion moved)
        {
            if (from == null || to == null) return false;

            var max = DetermineMaximumMoveDistance(initiator, new Battalion[] { moved });
            var targetsAvoidingStorm = Map.FindNeighbours(from, max, false, initiator.Faction, this);
            var targetsIgnoringStorm = Map.FindNeighbours(from, max, true, initiator.Faction, this);
            return !targetsAvoidingStorm.Contains(to) && targetsIgnoringStorm.Contains(to);
        }

        private void LogMove(Player initiator, Territory from, Location to, int forceAmount, int specialForceAmount, bool asAdvisors, bool byCaravan)
        {
            CurrentReport.Express(
                CaravanMessage(byCaravan),
                initiator.Faction,
                " move ",
                MessagePart.ExpressIf(forceAmount > 0, forceAmount, initiator.Force),
                MessagePart.ExpressIf(specialForceAmount > 0, specialForceAmount, initiator.SpecialForce),
                " from ",
                from,
                " to ",
                to,
                MessagePart.ExpressIf(initiator.Is(Faction.Blue) && Applicable(Rule.BlueAdvisors), " as ", asAdvisors ? (object)initiator.SpecialForce : initiator.Force));
        }

        private MessagePart CaravanMessage(bool byCaravan) => MessagePart.ExpressIf(byCaravan, "By ", TreacheryCardType.Caravan, ", ");

        public void HandleEvent(BlueFlip e)
        {
            var initiator = GetPlayer(e.Initiator);

            CurrentReport.Express(e.GetDynamicMessage(this));

            initiator.FlipForces(LastShippedOrMovedTo.Territory, e.AsAdvisors);

            if (Version >= 102) FlipBeneGesseritWhenAlone();
            DetermineNextShipmentAndMoveSubPhase(false, BGMayAccompany);
        }

        private void DetermineNextShipmentAndMoveSubPhase(bool intrusionCaused, bool bgMayAccompany)
        {
            if (intrusionCaused && Prevented(FactionAdvantage.BlueIntrusion))
            {
                LogPrevention(FactionAdvantage.BlueIntrusion);
                intrusionCaused = false;
                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueIntrusion);
            }

            if (bgMayAccompany && Prevented(FactionAdvantage.BlueAccompanies))
            {
                LogPrevention(FactionAdvantage.BlueAccompanies);
                bgMayAccompany = false;
                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueAccompanies);
            }

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
                //Forces that must be destroyed because moves ended where allies are
                foreach (var t in ChosenDestinationsWithAllies)
                {
                    if (p.AnyForcesIn(t) > 0)
                    {
                        CurrentReport.Express("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
                        RevealCurrentNoField(p, t);
                        p.KillAllForces(t, false);
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
                            CurrentReport.Express("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
                            RevealCurrentNoField(p, t);
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
            //Console.WriteLine("DetermineNextSubPhaseAfterNonOrangeMove: " + MayPerformExtraMove);
            if (MayPerformExtraMove)
            {
                MayPerformExtraMove = false;
                Enter(Phase.NonOrangeMove);
            }
            else
            {
                Enter(
                    EveryoneActedOrPassed, ConcludeShipmentAndMove,
                    IsPlaying(Faction.Orange) && !HasActedOrPassed.Any(p => p == Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip,
                    Phase.NonOrangeShip);
            }
        }

        public bool OrangeMayDelay => OrangeMayShipOutOfTurnOrder && Players.Count - HasActedOrPassed.Count > (JuiceForcesLastPlayer && !HasActedOrPassed.Contains(CurrentJuice.Initiator) ? 2 : 1);

        private bool OrangeMayShipOutOfTurnOrder => Applicable(Rule.OrangeDetermineShipment) && (Version < 113 || !Prevented(FactionAdvantage.OrangeDetermineMoveMoment));

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
            CurrentKarmaShipmentPrevention = null;
            MainPhaseEnd();
            Enter(Phase.ShipmentAndMoveConcluded);
            ReceiveShipsTechIncome();
            BrownHasExtraMove = false;
        }

        public List<Territory> CurrentBlockedTerritories { get; private set; } = new List<Territory>();
        public void HandleEvent(BrownMovePrevention e)
        {
            CurrentReport.Express(e);
            Discard(e.CardUsed());
            CurrentBlockedTerritories.Add(e.Territory);
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }

        private bool BrownHasExtraMove { get; set; } = false;
        public void HandleEvent(BrownExtraMove e)
        {
            CurrentReport.Express(e);
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
                    CurrentReport.Express(techTokenOwner.Faction, " receive ", Payment(amount), " from ", TechToken.Ships);
                }
            }
        }

        public bool PreventedFromShipping(Faction f) => CurrentKarmaShipmentPrevention != null && CurrentKarmaShipmentPrevention.Target == f;

        private KarmaShipmentPrevention CurrentKarmaShipmentPrevention = null;
        public void HandleEvent(KarmaShipmentPrevention e)
        {
            CurrentKarmaShipmentPrevention = e;
            Discard(e.Player, TreacheryCardType.Karma);
            e.Player.SpecialKarmaPowerUsed = true;
            CurrentReport.Express(e);
            RecentMilestones.Add(Milestone.Karma);
        }
    }
}
