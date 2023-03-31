/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public ILocationEvent LastShipmentOrMovement { get; internal set; }

        private Queue<Intrusion> Intrusions { get; } = new();

        public Intrusion LastBlueIntrusion => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.BlueIntrusion ? Intrusions.Peek() : null;
        public Intrusion LastTerrorTrigger => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.Terror ? Intrusions.Peek() : null;
        public Intrusion LastAmbassadorTrigger => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.Ambassador ? Intrusions.Peek() : null;

        public PlayerSequence ShipmentAndMoveSequence { get; internal set; }
        public bool ShipsTechTokenIncome { get; internal set; }
        public List<IPlacement> RecentMoves { get; internal set; } = new();
        public int CurrentNoFieldValue { get; internal set; } = -1;
        public int LatestRevealedNoFieldValue { get; internal set; } = -1;

        public Dictionary<Faction, ShipmentPermission> ShipmentPermissions { get; private set; } = new();

        internal List<Faction> FactionsWithOrnithoptersAtStartOfMovement { get; set; }

        internal bool BeginningOfShipmentAndMovePhase { get; set; }
                

        internal void StartShipAndMoveSequence()
        {
            if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && OrangeMayShipOutOfTurnOrder)
            {
                ShipmentAndMoveSequence.NextPlayer();
            }

            Enter(JuiceForcesFirstPlayer && CurrentJuice.Initiator != Faction.Orange, Phase.NonOrangeShip, IsPlaying(Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip, Phase.NonOrangeShip);
        }

        internal bool OccupiesArrakeenOrCarthag(Player p)
        {
            return p.Occupies(Map.Arrakeen) || p.Occupies(Map.Carthag);
        }

        

        public void HandleEvent(OrangeDelay e)
        {
            BeginningOfShipmentAndMovePhase = false;
            Log(e);
            Enter(Phase.NonOrangeShip);
        }


        internal readonly List<Territory> ChosenDestinationsWithAllies = new();

        public void HandleEvent(Shipment s)
        {
            BeginningOfShipmentAndMovePhase = false;
            CurrentBlockedTerritories.Clear();
            StormLossesToTake.Clear();
            ChosenDestinationsWithAllies.Clear();

            BlueMayAccompany = false;
            var initiator = GetPlayer(s.Initiator);

            MessagePart receivedPaymentMessage = MessagePart.Express();
            int totalCost = 0;

            if (!s.Passed)
            {
                var ownerOfKarma = s.KarmaCard != null ? OwnerOf(s.KarmaCard) : null;
                totalCost = PayForShipment(s, initiator);

                if (s.IsNoField)
                {
                    if (s.CunningNoFieldValue >= 0)
                    {
                        RevealCurrentNoField(GetPlayer(Faction.White));
                        initiator.ShipSpecialForces(s.To, 1);
                        CurrentNoFieldValue = s.CunningNoFieldValue;
                        PlayNexusCard(s.Player, "Cunning", "ship and reveal a second ", FactionSpecialForce.White);
                    }

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

                LastShipmentOrMovement = s;
                bool mustBeAdvisors = (initiator.Is(Faction.Blue) && (initiator.SpecialForcesIn(s.To) > 0) || Version >= 148 && initiator.SpecialForcesIn(s.To.Territory) > 0);

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
                    PerformNormalShipment(initiator, s.To, s.ForceAmount + s.SmuggledAmount, 1, false);
                }
                else
                {
                    PerformNormalShipment(initiator, s.To, s.ForceAmount + s.SmuggledAmount, s.SpecialForceAmount + s.SmuggledSpecialAmount, s.IsSiteToSite);
                }

                if (s.Initiator != Faction.Yellow)
                {
                    Stone(Milestone.Shipment);
                }

                initiator.FlipForces(s.To, mustBeAdvisors);

                if (!s.IsBackToReserves) CheckIntrusion(s);

                DetermineNextShipmentAndMoveSubPhase();

                int receivedPayment = HandleReceivedShipmentPayment(s, initiator, ref receivedPaymentMessage);
                Log(s.GetVerboseMessage(totalCost, receivedPaymentMessage, ownerOfKarma));

                if (totalCost - receivedPayment >= 4)
                {
                    ActivateBanker(initiator);
                }

                FlipBeneGesseritWhenAloneOrWithPinkAlly();
                DetermineOccupationAfterLocationEvent(s);
            }
            else
            {
                Log(s.GetVerboseMessage(totalCost, receivedPaymentMessage, null));
                DetermineNextShipmentAndMoveSubPhase();
            }
        }

        public bool ContainsConflictingAlly(Player initiator, Location to)
        {
            if (initiator.Ally == Faction.None || to == Map.PolarSink || initiator.Faction == Faction.Pink || initiator.Ally == Faction.Pink || to == null) return false;

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

            if (s.IsUsingKarma)
            {
                var karmaCard = s.KarmaCard;
                Discard(karmaCard);
                Stone(Milestone.Karma);
            }

            if (s.AllyContributionAmount > 0)
            {
                GetPlayer(initiator.Ally).Resources -= s.AllyContributionAmount;
                DecreasePermittedUseOfAllySpice(initiator.Faction, s.AllyContributionAmount);
            }

            return costToInitiator + s.AllyContributionAmount;
        }

        private int HandleReceivedShipmentPayment(Shipment s, Player initiator, ref MessagePart profitMessage)
        {
            int totalCost = Shipment.DetermineCost(this, initiator, s);

            int receiverProfit = 0;
            var orange = GetPlayer(Faction.Orange);

            if (orange != null && initiator != orange && !s.IsUsingKarma)
            {
                receiverProfit = s.DetermineOrangeProfits(this);

                if (receiverProfit > 0)
                {
                    if (!Prevented(FactionAdvantage.OrangeReceiveShipment))
                    {
                        ModifyIncomeBasedOnThresholdOrOccupation(orange, ref receiverProfit);
                        orange.Resources += receiverProfit;

                        if (receiverProfit > 0)
                        {
                            profitMessage = MessagePart.Express(" → ", Faction.Orange, " get ", Payment.Of(receiverProfit));

                            if (receiverProfit >= 5)
                            {
                                ApplyBureaucracy(initiator.Faction, Faction.Orange);
                            }
                        }
                    }
                    else
                    {
                        profitMessage = MessagePart.Express(" → ", TreacheryCardType.Karma, " prevents ", Faction.Orange, " from receiving", Payment.Of(receiverProfit));
                        if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.OrangeReceiveShipment);
                    }
                }
            }

            SetRecentPayment(receiverProfit, initiator.Faction, Faction.Orange, s);
            SetRecentPayment(totalCost - receiverProfit, initiator.Faction, s);

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
            BlueMayAccompany = (forceAmount > 0 || specialForceAmount > 0) && initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Blue;

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
                    Log(killCount, initiator.Faction, " forces are killed by the storm");
                }
            }

            if (initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Orange && !isSiteToSite)
            {
                ShipsTechTokenIncome = true;
            }
        }

        public bool MayShipCrossPlanet(Player p)
        {
            return p.Is(Faction.Orange) && !Prevented(FactionAdvantage.OrangeSpecialShipments) ||
                   p.Ally == Faction.Orange && AllyMayShipAsOrange ||
                   p.Initiated(CurrentOrangeNexus) ||
                   HasShipmentPermission(p, ShipmentPermission.Cross);
        }

        public bool MayShipToReserves(Player p)
        {
            return p.Is(Faction.Orange) && !Prevented(FactionAdvantage.OrangeSpecialShipments) ||
                   p.Initiated(CurrentOrangeNexus) ||
                   HasShipmentPermission(p, ShipmentPermission.ToHomeworld);
        }

        public bool MayShipWithDiscount(Player p)
        {
            return p.Is(Faction.Orange) && !Prevented(FactionAdvantage.OrangeShipmentsDiscount) ||
                   p.Ally == Faction.Orange && AllyMayShipAsOrange && !Prevented(FactionAdvantage.OrangeShipmentsDiscountAlly) ||
                   p.Initiated(CurrentOrangeNexus);
        }

        private bool MayPerformExtraMove { get; set; }
        public void HandleEvent(Move m)
        {
            StormLossesToTake.Clear();
            var initiator = GetPlayer(m.Initiator);

            MayPerformExtraMove = (CurrentFlightUsed != null && CurrentFlightUsed.Initiator == m.Initiator && CurrentFlightUsed.ExtraMove);

            if (!MayPerformExtraMove && !InOrangeCunningShipment)
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

            if (InOrangeCunningShipment)
            {
                CurrentOrangeNexus = null;
                InOrangeCunningShipment = false;
            }

            if (!m.Passed)
            {
                RecentMoves.Add(m);

                if (ContainsConflictingAlly(initiator, m.To))
                {
                    ChosenDestinationsWithAllies.Add(m.To.Territory);
                }

                PerformMoveFromLocations(initiator, m.ForceLocations, m, m.Initiator != Faction.Blue || m.AsAdvisors, false);
                CheckIntrusion(m);
            }
            else
            {
                Log(m.Initiator, " pass movement");
            }

            DetermineNextShipmentAndMoveSubPhase();
            CheckIfForcesShouldBeDestroyedByAllyPresence(initiator);
            FlipBeneGesseritWhenAloneOrWithPinkAlly();

            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowExtraMove);
            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyCyborgExtraMove);

            CurrentFlightUsed = null;
            CurrentFlightDiscoveryUsed = null;
            CurrentPlanetology = null;
        }

        internal bool BlueMayAccompany { get; set; } = false;

        internal void CheckIntrusion(ILocationEvent e)
        {
            CheckBlueIntrusion(e, e.Initiator, e.To.Territory);
            CheckAmbassadorTriggered(e);
            CheckTerrorTriggered(e);
        }

        public bool IsOccupiedByFactionOrTheirAlly(World world, Player p)
        {
            var occupier = OccupierOf(world);
            return occupier != null && (occupier == p || occupier.Ally == p.Faction);
        }

        public bool IsOccupiedByFactionOrTheirAlly(World world, Faction f)
        {
            var occupier = OccupierOf(world);
            return occupier != null && (occupier.Is(f) || occupier.Ally == f);
        }

        private void CheckBlueIntrusion(ILocationEvent e, Faction initiator, Territory territory)
        {
            var bgPlayer = GetPlayer(Faction.Blue);
            if (bgPlayer != null &&
                territory != Map.PolarSink.Territory &&
                !territory.IsHomeworld &&
                Applicable(Rule.BlueAdvisors) &&
                initiator != bgPlayer.Ally &&
                initiator != Faction.Blue &&
                bgPlayer.ForcesIn(territory) > 0)
            {
                if (Prevented(FactionAdvantage.BlueIntrusion))
                {
                    LogPreventionByKarma(FactionAdvantage.BlueIntrusion);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueIntrusion);
                }
                else
                {
                    QueueIntrusion(e, IntrusionType.BlueIntrusion);
                }
            }
        }

        private void CheckAmbassadorTriggered(ILocationEvent e)
        {
            var pinkPlayer = GetPlayer(Faction.Pink);
            if (pinkPlayer != null &&
                e.Initiator != Faction.Pink &&
                e.Initiator != pinkPlayer.Ally &&
                AmbassadorIn(e.To.Territory) != Ambassador.None &&
                AmbassadorIn(e.To.Territory) != AmbassadorOf(e.Initiator))
            {
                QueueIntrusion(e, IntrusionType.Ambassador);
            }
        }

        internal Ambassador AmbassadorOf(Faction faction) => (Ambassador)((int)faction);

        private void CheckTerrorTriggered(ILocationEvent e)
        {
            var cyanPlayer = GetPlayer(Faction.Cyan);
            if (cyanPlayer != null &&
                (e.TotalAmountOfForces >= 3 || !cyanPlayer.HasLowThreshold()) &&
                e.Initiator != Faction.Cyan &&
                e.Initiator != cyanPlayer.Ally &&
                !IsOccupiedByFactionOrTheirAlly(World.Cyan, e.Initiator) &&
                TerrorIn(e.To.Territory).Any())
            {
                QueueIntrusion(e, IntrusionType.Terror);
            }
        }

        private void QueueIntrusion(ILocationEvent e, IntrusionType type)
        {
            Intrusions.Enqueue(new Intrusion(e, type));
        }

        public Intrusion LatestIntrusion { get; private set; }

        internal void DequeueIntrusion(IntrusionType type)
        {
            if (Intrusions.Count == 0)
            {
                throw new ArgumentException($"No intrusions to dequeue");
            }
            else if (Intrusions.Peek().Type != type)
            {
                throw new ArgumentException($"Wrong intrusion type: actual:{type} vs expected:{Intrusions.Peek().Type}");
            }

            LatestIntrusion = Intrusions.Dequeue();
        }

        internal Phase PausedPhase { get; set; }

        internal void PerformMoveFromLocations(Player initiator, Dictionary<Location, Battalion> forceLocations, ILocationEvent evt, bool asAdvisors, bool byCaravan)
        {
            LastShipmentOrMovement = evt;
            bool wasOccupiedBeforeMove = IsOccupied(evt.To.Territory);
            var dist = DetermineMaximumMoveDistance(initiator, forceLocations.Values);

            foreach (var fromTerritory in forceLocations.Keys.Select(l => l.Territory).Distinct())
            {
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;

                foreach (var fl in forceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                {
                    if (Version <= 150 || fl.Value.TotalAmountOfForces > 0)
                    {
                        PerformMoveFromLocation(initiator, fl.Key, fl.Value, evt.To, ref totalNumberOfForces, ref totalNumberOfSpecialForces);
                        CheckSandmaster(initiator, evt.To, dist, fl);
                    }
                }

                if (initiator.Is(Faction.Blue))
                {
                    initiator.FlipForces(evt.To, wasOccupiedBeforeMove && asAdvisors);
                }

                if (totalNumberOfForces > 0 || totalNumberOfSpecialForces > 0)
                {
                    LogMove(initiator, fromTerritory, evt.To, totalNumberOfForces, totalNumberOfSpecialForces, wasOccupiedBeforeMove && asAdvisors, byCaravan);
                }
            }

            Stone(Milestone.Move);
            FlipBeneGesseritWhenAloneOrWithPinkAlly();
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
                    Log(initiator.Faction, " ", LeaderSkill.Sandmaster, " collects ", Payment.Of(mostSpice), " along the way");
                }
            }
        }

        internal void PerformMoveFromLocation(Player initiator, Location from, Battalion battalion, Location to, ref int totalNumberOfForces, ref int totalNumberOfSpecialForces)
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
                    Log(killCount, initiator.Faction, "forces are killed by the storm while travelling");
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

        internal FlightUsed CurrentFlightUsed { get; set; }

        public void HandleEvent(FlightUsed e)
        {
            Log(e);
            Discard(e.Player, TreacheryCardType.Flight);
            CurrentFlightUsed = e;
        }

        internal FlightDiscoveryUsed CurrentFlightDiscoveryUsed { get; set; }

        public void HandleEvent(FlightDiscoveryUsed e)
        {
            Log(e);
            OwnerOfFlightDiscovery = Faction.None;
            CurrentFlightDiscoveryUsed = e;
        }

        private bool MustMoveThroughStorm(Player initiator, Location from, Location to, Battalion moved)
        {
            if (from == null || to == null) return false;

            var max = DetermineMaximumMoveDistance(initiator, new Battalion[] { moved });
            var targetsAvoidingStorm = Map.FindNeighbours(from, max, false, initiator.Faction, this);
            var targetsIgnoringStorm = Map.FindNeighbours(from, max, true, initiator.Faction, this);
            return !targetsAvoidingStorm.Contains(to) && targetsIgnoringStorm.Contains(to);
        }

        internal void LogMove(Player initiator, Territory from, Location to, int forceAmount, int specialForceAmount, bool asAdvisors, bool byCaravan)
        {
            Log(
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

        internal AmbassadorActivated CurrentAmbassadorActivated { get; set; }
        public Faction AllianceByAmbassadorOfferedTo { get; internal set; }
        internal Phase PausedAmbassadorPhase { get; set; }

        

        private Player PlayerToSetAsideVidal { get; set; }
        private VidalMoment WhenToSetAsideVidal { get; set; }
        internal void TakeVidal(Player p, VidalMoment whenToSetAside)
        {
            var vidal = Vidal;

            var currentOwner = OwnerOf(vidal);
            if (currentOwner != null)
            {
                currentOwner.Leaders.Remove(vidal);

                if (IsAlive(vidal))
                {
                    Log(currentOwner.Faction, " lose ", vidal);
                }

                CapturedLeaders.Remove(vidal);
            }

            p.Leaders.Add(vidal);
            PlayerToSetAsideVidal = p;
            WhenToSetAsideVidal = whenToSetAside;

            Log(p.Faction, " take ", vidal);

            SetInFrontOfShield(vidal, false);
        }

        internal Phase PausedTerrorPhase { get; set; }
        public bool AllianceByTerrorWasOffered { get; internal set; } = false;
        public Faction AllianceByTerrorOfferedTo { get; internal set; }
        public void HandleEvent(TerrorRevealed e)
        {
            var initiator = GetPlayer(e.Initiator);
            var territory = TerrorRevealed.GetTerritory(this);
            var victim = TerrorRevealed.GetVictim(this);
            var victimPlayer = GetPlayer(victim);

            if (e.Passed)
            {
                Log(e.Initiator, " don't terrorize ", territory);
                AllianceByTerrorWasOffered = false;
                DequeueIntrusion(IntrusionType.Terror);
                DetermineNextShipmentAndMoveSubPhase();
            }
            else if (e.AllianceOffered)
            {
                Log(e.Initiator, " offer an alliance to ", victim, " as an alternative to terror");
                AllianceByTerrorOfferedTo = victim;
                AllianceByTerrorWasOffered = true;
                PausedTerrorPhase = CurrentPhase;
                Enter(Phase.AllianceByTerror);
            }
            else
            {
                Stone(Milestone.TerrorRevealed);

                TerrorOnPlanet.Remove(e.Type);

                if (e.Passed || !TerrorIn(territory).Any())
                {
                    DequeueIntrusion(IntrusionType.Terror);
                }

                AllianceByTerrorWasOffered = false;

                switch (e.Type)
                {
                    case TerrorType.Assassination:

                        var randomLeader = victimPlayer.Leaders.RandomOrDefault(Random);
                        if (randomLeader != null)
                        {
                            Log(e.Initiator, " gain ", Payment.Of(randomLeader.CostToRevive), " from assassinating ", randomLeader, " in ", territory);
                            initiator.Resources += randomLeader.CostToRevive;
                            KillHero(randomLeader);
                        }
                        else
                        {
                            Log(e.Initiator, " fail to assassinate a leader in ", territory);
                        }
                        break;

                    case TerrorType.Atomics:

                        KillAllForcesIn(territory, false);

                        KillAmbassadorIn(territory);

                        AtomicsAftermath = territory;

                        if (initiator.TreacheryCards.Count > initiator.MaximumNumberOfCards)
                        {
                            Discard(initiator, initiator.TreacheryCards.RandomOrDefault(Random));
                        }

                        Stone(Milestone.MetheorUsed);
                        Log(e.Initiator, " DETONATE ATOMICS in ", territory);
                        break;

                    case TerrorType.Extortion:

                        Log(e.Initiator, " will get ", Payment.Of(5), " from ", e.Type, " during ", MainPhase.Contemplate);
                        initiator.Extortion += 5;
                        break;

                    case TerrorType.Robbery:

                        if (e.RobberyTakesCard)
                        {
                            initiator.TreacheryCards.Add(DrawTreacheryCard());
                            Log(e.Initiator, " draw a Treachery Card");
                        }
                        else
                        {
                            var amountStolen = (int)Math.Ceiling(0.5f * victimPlayer.Resources);
                            initiator.Resources += amountStolen;
                            victimPlayer.Resources -= amountStolen;
                            Log(e.Initiator, " steal ", Payment.Of(amountStolen), " by ", e.Type, " in ", territory);
                        }
                        break;

                    case TerrorType.Sabotage:

                        Log(e.Initiator, " sabotage ", victimPlayer.Faction);

                        if (victimPlayer.TreacheryCards.Any())
                        {
                            Discard(victimPlayer.TreacheryCards.RandomOrDefault(Random));
                        }
                        else
                        {
                            Log(victimPlayer.Faction, " have no treachery cards to discard");
                        }

                        if (e.CardToGiveInSabotage != null)
                        {
                            initiator.TreacheryCards.Remove(e.CardToGiveInSabotage);
                            victimPlayer.TreacheryCards.Add(e.CardToGiveInSabotage);
                            Log(e.Initiator, " give a treachery card to ", victimPlayer.Faction);
                        }

                        break;

                    case TerrorType.SneakAttack:

                        if (e.SneakAttackTo != null)
                        {
                            Log(e.Initiator, " sneak attack ", territory, " with ", e.ForcesInSneakAttack, initiator.Force);
                            initiator.ShipForces(e.SneakAttackTo, e.ForcesInSneakAttack);
                            CheckIntrusion(e);
                        }
                        break;

                }

                DetermineNextShipmentAndMoveSubPhase();
            }

            if (!e.Passed && e.Type == TerrorType.Robbery && e.RobberyTakesCard && initiator.TreacheryCards.Count > initiator.MaximumNumberOfCards)
            {
                LetPlayerDiscardTreacheryCardOfChoice(e.Initiator);
            }
        }

        internal void LetPlayerDiscardTreacheryCardOfChoice(Faction f)
        {
            PhaseBeforeDiscarding = CurrentPhase;
            FactionsThatMustDiscard.Add(f);
            Enter(Phase.Discarding);
        }

        private void KillAmbassadorIn(Territory territory)
        {
            var ambassador = AmbassadorIn(territory);
            if (ambassador != Ambassador.None)
            {
                var pink = GetPlayer(Faction.Pink);
                AmbassadorsOnPlanet.Remove(territory);
                pink.Ambassadors.Add(ambassador);
                Log("The ambassador in ", territory, " returns to ", Faction.Pink);
            }
        }

        internal void DetermineNextShipmentAndMoveSubPhase()
        {
            CleanupObsoleteIntrusions();

            if (BlueMayAccompany)
            {
                if (Prevented(FactionAdvantage.BlueAccompanies))
                {
                    LogPreventionByKarma(FactionAdvantage.BlueAccompanies);
                    BlueMayAccompany = false;
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueAccompanies);
                }
                else if (HasLowThreshold(Faction.Blue))
                {
                    BlueMayAccompany = false;
                }
            }

            if (StormLossesToTake.Count > 0)
            {
                PhaseBeforeStormLoss = CurrentPhase;
                Enter(Phase.StormLosses);
            }
            else if (Intrusions.Any())
            {
                switch (Intrusions.Peek().Type)
                {
                    case IntrusionType.BlueIntrusion:

                        DetermineNextShipmentAndMoveSubPhaseOnBlueIntruded();
                        break;

                    case IntrusionType.Terror:

                        DetermineNextShipmentAndMoveSubPhaseOnTerrorTriggered();
                        break;

                    case IntrusionType.Ambassador:

                        DetermineNextShipmentAndMoveSubPhaseOnAmbassadorTriggered();
                        break;
                }
            }
            else
            {
                DetermineNextShipmentAndMoveSubPhaseOnNoIntrusion();
            }
        }

        private void CleanupObsoleteIntrusions()
        {
            if (Intrusions.Any())
            {

                int i = 0;
                bool search = true;
                while (search)
                {
                    i++;
                    if (i > 100) throw new Exception("Stuck");

                    if (LastBlueIntrusion != null && GetPlayer(Faction.Blue).ForcesIn(LastBlueIntrusion.Territory) == 0)
                    {
                        DequeueIntrusion(IntrusionType.BlueIntrusion);
                    }
                    else if (LastTerrorTrigger != null && !TerrorIn(LastTerrorTrigger.Territory).Any())
                    {
                        DequeueIntrusion(IntrusionType.Terror);
                    }
                    else if (LastAmbassadorTrigger != null && AmbassadorIn(LastAmbassadorTrigger.Territory) == Ambassador.None)
                    {
                        DequeueIntrusion(IntrusionType.Ambassador);
                    }
                    else
                    {
                        search = false;
                    }
                }
            }
        }

        private void DetermineNextShipmentAndMoveSubPhaseOnNoIntrusion()
        {
            switch (CurrentPhase)
            {
                case Phase.YellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.TerrorTriggeredByYellowRidingMonsterA:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                    EndWormRideDuringPhase(Phase.YellowRidingMonsterA);
                    break;

                case Phase.YellowRidingMonsterB:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.TerrorTriggeredByYellowRidingMonsterB:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                    EndWormRideDuringPhase(Phase.YellowRidingMonsterB);
                    break;

                case Phase.OrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.TerrorTriggeredByOrangeShip:
                case Phase.AmbassadorTriggeredByOrangeShip:
                    Enter(IsPlaying(Faction.Blue) && BlueMayAccompany, Phase.BlueAccompaniesOrangeShip, !InOrangeCunningShipment, Phase.OrangeMove, Phase.NonOrangeMove);
                    break;

                case Phase.BlueAccompaniesOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                    Enter(!InOrangeCunningShipment, Phase.OrangeMove, Phase.NonOrangeMove);
                    break;

                case Phase.NonOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.TerrorTriggeredByNonOrangeShip:
                case Phase.AmbassadorTriggeredByNonOrangeShip:
                    Enter(IsPlaying(Faction.Blue) && BlueMayAccompany, Phase.BlueAccompaniesNonOrangeShip, Phase.NonOrangeMove);
                    break;

                case Phase.BlueAccompaniesNonOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                    Enter(Phase.NonOrangeMove);
                    break;

                case Phase.OrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.TerrorTriggeredByOrangeMove:
                case Phase.AmbassadorTriggeredByOrangeMove:
                    DetermineNextSubPhaseAfterOrangeShipAndMove();
                    break;

                case Phase.NonOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.TerrorTriggeredByNonOrangeMove:
                case Phase.AmbassadorTriggeredByNonOrangeMove:
                    DetermineNextSubPhaseAfterNonOrangeMove();
                    break;

                case Phase.BlueIntrudedByCaravan:
                case Phase.TerrorTriggeredByCaravan:
                case Phase.AmbassadorTriggeredByCaravan:
                    Enter(PausedPhase);
                    break;
            }
        }

        private void DetermineNextShipmentAndMoveSubPhaseOnBlueIntruded()
        {
            switch (CurrentPhase)
            {
                case Phase.YellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.TerrorTriggeredByYellowRidingMonsterA:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                    Enter(Phase.BlueIntrudedByYellowRidingMonsterA);
                    break;

                case Phase.YellowRidingMonsterB:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.TerrorTriggeredByYellowRidingMonsterB:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                    Enter(Phase.BlueIntrudedByYellowRidingMonsterB);
                    break;

                case Phase.OrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.TerrorTriggeredByOrangeShip:
                case Phase.AmbassadorTriggeredByOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                    Enter(Phase.BlueIntrudedByOrangeShip);
                    break;

                case Phase.NonOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.TerrorTriggeredByNonOrangeShip:
                case Phase.AmbassadorTriggeredByNonOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                    Enter(Phase.BlueIntrudedByNonOrangeShip);
                    break;

                case Phase.OrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.TerrorTriggeredByOrangeMove:
                case Phase.AmbassadorTriggeredByOrangeMove:
                    Enter(Phase.BlueIntrudedByOrangeMove);
                    break;

                case Phase.NonOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.TerrorTriggeredByNonOrangeMove:
                case Phase.AmbassadorTriggeredByNonOrangeMove:
                    Enter(Phase.BlueIntrudedByNonOrangeMove);
                    break;

                case Phase.BlueIntrudedByCaravan:
                case Phase.TerrorTriggeredByCaravan:
                case Phase.AmbassadorTriggeredByCaravan:
                    Enter(Phase.BlueIntrudedByCaravan);
                    break;

                default:
                    throw new Exception($"Blue intrusion triggered during undefined phase: {CurrentPhase}");
            }
        }

        private void DetermineNextShipmentAndMoveSubPhaseOnTerrorTriggered()
        {
            switch (CurrentPhase)
            {
                case Phase.YellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.TerrorTriggeredByYellowRidingMonsterA:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                    Enter(Phase.TerrorTriggeredByYellowRidingMonsterA);
                    break;

                case Phase.YellowRidingMonsterB:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.TerrorTriggeredByYellowRidingMonsterB:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                    Enter(Phase.TerrorTriggeredByYellowRidingMonsterB);
                    break;

                case Phase.OrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.TerrorTriggeredByOrangeShip:
                case Phase.AmbassadorTriggeredByOrangeShip:
                    Enter(Phase.TerrorTriggeredByOrangeShip);
                    break;

                case Phase.BlueAccompaniesOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                    Enter(Phase.TerrorTriggeredByOrangeShip);
                    break;

                case Phase.NonOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.TerrorTriggeredByNonOrangeShip:
                case Phase.AmbassadorTriggeredByNonOrangeShip:
                    Enter(Phase.TerrorTriggeredByNonOrangeShip);
                    break;

                case Phase.BlueAccompaniesNonOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                    Enter(Phase.TerrorTriggeredByNonOrangeShip);
                    break;

                case Phase.OrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.TerrorTriggeredByOrangeMove:
                case Phase.AmbassadorTriggeredByOrangeMove:
                    Enter(Phase.TerrorTriggeredByOrangeMove);
                    break;

                case Phase.NonOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.TerrorTriggeredByNonOrangeMove:
                case Phase.AmbassadorTriggeredByNonOrangeMove:
                    Enter(Phase.TerrorTriggeredByNonOrangeMove);
                    break;

                case Phase.BlueIntrudedByCaravan:
                case Phase.TerrorTriggeredByCaravan:
                case Phase.AmbassadorTriggeredByCaravan:
                    Enter(Phase.TerrorTriggeredByCaravan);
                    break;

                default:
                    throw new Exception($"Terror triggered during undefined phase: {CurrentPhase}");
            }
        }

        private void DetermineNextShipmentAndMoveSubPhaseOnAmbassadorTriggered()
        {
            switch (CurrentPhase)
            {
                case Phase.YellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.TerrorTriggeredByYellowRidingMonsterA:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                    Enter(Phase.AmbassadorTriggeredByYellowRidingMonsterA);
                    break;

                case Phase.YellowRidingMonsterB:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.TerrorTriggeredByYellowRidingMonsterB:
                case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                    Enter(Phase.AmbassadorTriggeredByYellowRidingMonsterB);
                    break;

                case Phase.OrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.TerrorTriggeredByOrangeShip:
                case Phase.AmbassadorTriggeredByOrangeShip:
                    Enter(Phase.AmbassadorTriggeredByOrangeShip);
                    break;

                case Phase.BlueAccompaniesOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                    Enter(Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip);
                    break;

                case Phase.NonOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.TerrorTriggeredByNonOrangeShip:
                case Phase.AmbassadorTriggeredByNonOrangeShip:
                    Enter(Phase.AmbassadorTriggeredByNonOrangeShip);
                    break;

                case Phase.BlueAccompaniesNonOrangeShip:
                case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
                case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                    Enter(Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip);
                    break;

                case Phase.OrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.TerrorTriggeredByOrangeMove:
                case Phase.AmbassadorTriggeredByOrangeMove:
                    Enter(Phase.AmbassadorTriggeredByOrangeMove);
                    break;

                case Phase.NonOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.TerrorTriggeredByNonOrangeMove:
                case Phase.AmbassadorTriggeredByNonOrangeMove:
                    Enter(Phase.AmbassadorTriggeredByNonOrangeMove);
                    break;

                case Phase.BlueIntrudedByCaravan:
                case Phase.TerrorTriggeredByCaravan:
                case Phase.AmbassadorTriggeredByCaravan:
                    Enter(Phase.AmbassadorTriggeredByCaravan);
                    break;

                default:
                    throw new Exception($"Ambassador triggered during undefined phase: {CurrentPhase}");
            }
        }

        //private bool IsFirst(Faction a, Faction b) => !PlayerSequence.IsAfter(this, GetPlayer(a), GetPlayer(b));

        internal void CheckIfForcesShouldBeDestroyedByAllyPresence(Player p)
        {
            if (p.Ally != Faction.None && p.Faction != Faction.Pink && p.Ally != Faction.Pink)
            {
                //Forces that must be destroyed because moves ended where allies are
                foreach (var t in ChosenDestinationsWithAllies)
                {
                    if (p.AnyForcesIn(t) > 0)
                    {
                        Log("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
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
                            Log("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
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

            if (p.Faction == Faction.Pink || p.Ally == Faction.Pink)
            {
                return false;
            }
            else if (Applicable(Rule.AdvisorsDontConflictWithAlly))
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
                    IsPlaying(Faction.Orange) && !HasActedOrPassed.Any(p => p == Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip,
                    Phase.NonOrangeShip);
            }
        }

        public bool OrangeMayDelay => OrangeMayShipOutOfTurnOrder && Players.Count - HasActedOrPassed.Count > (JuiceForcesLastPlayer && !HasActedOrPassed.Contains(CurrentJuice.Initiator) ? 2 : 1);

        private bool OrangeMayShipOutOfTurnOrder => Applicable(Rule.OrangeDetermineShipment) && (Version < 113 || !Prevented(FactionAdvantage.OrangeDetermineMoveMoment));

        public bool InOrangeCunningShipment { get; private set; }
        private void DetermineNextSubPhaseAfterOrangeShipAndMove()
        {
            if (MayPerformExtraMove)
            {
                MayPerformExtraMove = false;
                Enter(Phase.OrangeMove);
            }
            else if (!InOrangeCunningShipment && CurrentOrangeNexus != null && CurrentOrangeNexus.Initiator == Faction.Orange)
            {
                InOrangeCunningShipment = true;
                Enter(Phase.OrangeShip);
            }
            else
            {
                Enter(!EveryoneActedOrPassed, Phase.NonOrangeShip, ConcludeShipmentAndMove);
            }
        }

        private void ConcludeShipmentAndMove()
        {
            CheckIfCyanGainsVidal();
            CurrentKarmaShipmentPrevention = null;
            MainPhaseEnd();
            Enter(Phase.ShipmentAndMoveConcluded);
            ReceiveShipsTechIncome();
            BrownHasExtraMove = false;
            CurrentBlueNexus = null;
        }

        public Leader Vidal => LeaderState.Keys.FirstOrDefault(h => h.HeroType == HeroType.Vidal) as Leader;

        private void CheckIfCyanGainsVidal()
        {
            var cyan = GetPlayer(Faction.Cyan);
            if (cyan != null && !Prevented(FactionAdvantage.CyanGainingVidal))
            {
                var vidal = Vidal;

                if (VidalIsAlive)
                {
                    var pink = GetPlayer(Faction.Pink);
                    var nrOfBattlesInStrongholds = Battle.BattlesToBeFought(this, cyan).Select(batt => batt.Territory)
                        .Where(t => (pink == null || pink.AnyForcesIn(t) == 0) && (t.IsStronghold || IsSpecialStronghold(t))).Distinct().Count();

                    if (nrOfBattlesInStrongholds >= 2 && !VidalIsCapturedOrGhola && OccupierOf(World.Pink) == null)
                    {
                        TakeVidal(cyan, VidalMoment.EndOfTurn);
                    }
                }
            }
        }

        public bool VidalIsAlive => Vidal != null && IsAlive(Vidal);

        public bool VidalIsCapturedOrGhola
        {
            get
            {
                var playerWithVidal = Players.FirstOrDefault(p => p.Leaders.Any(l => l.HeroType == HeroType.Vidal));
                return playerWithVidal != null && (CapturedLeaders.Keys.Any(h => h.HeroType == HeroType.Vidal) || playerWithVidal.Is(Faction.Purple));
            }
        }

        public List<Territory> CurrentBlockedTerritories { get; private set; } = new List<Territory>();

        internal bool BrownHasExtraMove { get; set; } = false;

        private void ReceiveShipsTechIncome()
        {
            if (ShipsTechTokenIncome)
            {
                var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Ships));
                if (techTokenOwner != null)
                {
                    var amount = techTokenOwner.TechTokens.Count;
                    techTokenOwner.Resources += amount;
                    Log(techTokenOwner.Faction, " receive ", Payment.Of(amount), " from ", TechToken.Ships);
                }
            }
        }

        public bool PreventedFromShipping(Faction f) => CurrentKarmaShipmentPrevention != null && CurrentKarmaShipmentPrevention.Target == f;

        private KarmaShipmentPrevention CurrentKarmaShipmentPrevention = null;
        public void HandleEvent(KarmaShipmentPrevention e)
        {
            CurrentKarmaShipmentPrevention = e;
            Discard(e.Player, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            e.Player.SpecialKarmaPowerUsed = true;
            Log(e);
            Stone(Milestone.Karma);
        }

        public void HandleEvent(SetShipmentPermission e)
        {
            Log(e);

            foreach (var f in e.Factions)
            {
                ShipmentPermissions.Remove(f);
            }

            if (e.Permission != ShipmentPermission.None)
            {
                foreach (var f in e.Factions)
                {
                    ShipmentPermissions.Add(f, e.Permission);
                }
            }
        }

        public bool HasShipmentPermission(Player p, ShipmentPermission permission)
        {
            return ShipmentPermissions.TryGetValue(p.Faction, out var permissions) && (permissions & permission) == permission;
        }
    }
}
