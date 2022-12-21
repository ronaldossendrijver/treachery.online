/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        public List<Territory> Monsters { get; private set; } = new List<Territory>();
        private readonly List<ResourceCard> ignoredMonsters = new List<ResourceCard>();
        private ResourceCard ignoredSandtrout = null;

        #endregion State

        #region BeginningOfSpiceBlow

        private void EnterSpiceBlowPhase()
        {
            MainPhaseStart(MainPhase.Blow);
            ignoredMonsters.Clear();
            ignoredSandtrout = null;
            HasActedOrPassed.Clear();

            var sequenceToDetermineFirstPlayer = new PlayerSequence(this);

            if (Version < 135)
            {
                Enter(Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal) && (Version <= 102 || CurrentTurn > 1), Phase.Thumper, EnterBlowA);
            }
            else
            {
                Enter(Phase.Thumper);
            }

        }

        private bool ThumperUsed { get; set; } = false;
        public void HandleEvent(ThumperPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Thumper);
            Log(e);
            RecentMilestones.Add(Milestone.Thumper);
            ThumperUsed = true;
            EnterBlowA();
        }

        #endregion BeginningOfSpiceBlow

        #region ExecuteResourceBlow

        public int NumberOfMonsters { get; private set; } = 0;

        public ResourceCard SandTrout { get; private set; } = null;

        public bool SandTroutOccured => SandTrout != null;

        private bool SandTroutDoublesResources { get; set; } = false;

        private void DrawSpiceCard()
        {
            ResourceCard drawn = null;
            while (ThumperUsed || !(drawn = DrawAndDiscardResourceCard(CurrentDiscardPile)).IsSpiceBlow)
            {
                if (ThumperUsed && Version <= 150)
                {
                    ThumperUsed = false;
                    NumberOfMonsters++;
                    ProcessMonsterCard(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud ? null : PreviousBlowCard.Location.Territory);
                    if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB)
                    {
                        break;
                    }
                }
                else if (drawn != null && drawn.IsShaiHulud && CurrentTurn == 1)
                {
                    Log(Concept.Monster, " on turn 1 was ignored");
                    ignoredMonsters.Add(CurrentDiscardPile.Draw());
                }
                else if (ThumperUsed && Version > 150 || drawn.IsShaiHulud)
                {
                    ThumperUsed = false;
                    SandTroutDoublesResources = false;

                    if (!ThumperUsed)
                    {
                        RecentMilestones.Add(Milestone.Monster);
                    }

                    if (!SandTroutOccured)
                    {
                        SandTroutDoublesResources = false;
                        NumberOfMonsters++;
                        ProcessMonsterCard(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud ? null : PreviousBlowCard.Location.Territory);
                        if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB)
                        {
                            break;
                        }
                    }
                    else
                    {
                        //Sandtrout triggers
                        if (Version >= 150)
                        {
                            if (drawn != null) CurrentDiscardPile.Items.Remove(drawn);
                            CurrentDiscardPile.PutOnTop(SandTrout);
                            if (drawn != null) CurrentDiscardPile.PutOnTop(drawn);
                        }

                        SandTrout = null;
                        SandTroutDoublesResources = true;
                        Log(Concept.Monster, " is ignored due to ", Concept.BabyMonster);
                    }
                }
                else if (drawn.IsSandTrout)
                {
                    if (Version < 150 || CurrentTurn > 1)
                    {
                        RecentMilestones.Add(Milestone.BabyMonster);
                        Log(Concept.BabyMonster, " detected! All alliances are cancelled.");
                        CancelAllAlliances();
                        CurrentDiscardPile.Items.Remove(drawn);
                        SandTrout = drawn;
                    }
                    else
                    {
                        Log(Concept.BabyMonster, " on turn 1 was ignored");
                        ignoredSandtrout = CurrentDiscardPile.Draw();
                    }
                }
            }

            if (CurrentPhase != Phase.YellowSendingMonsterA && CurrentPhase != Phase.YellowSendingMonsterB)
            {
                PreviousBlowCard = drawn;
                ProcessBlowCard(drawn);
            }
        }

        private void CancelAllAlliances()
        {
            foreach (var p in Players)
            {
                if (p.Ally != Faction.None)
                {
                    BreakAlliance(p.Faction);
                }
            }
        }

        private ResourceCard DrawAndDiscardResourceCard(Deck<ResourceCard> discardPile)
        {
            if (ResourceCardDeck.IsEmpty)
            {
                ReshuffleResourceDeck();
            }

            var drawn = ResourceCardDeck.Draw();
            discardPile.PutOnTop(drawn);
            return drawn;
        }

        private void ReshuffleResourceDeck()
        {
            if (Applicable(Rule.IncreasedResourceFlow))
            {
                Log(ResourceCardDiscardPileA.Items.Count + ResourceCardDiscardPileB.Items.Count, " cards were shuffled from ", Concept.Resource, " discard piles A en B into a new deck.");
            }
            else
            {
                Log(ResourceCardDiscardPileA.Items.Count, " cards were shuffled from the ", Concept.Resource, " discard pile into a new deck");
            }

            foreach (var i in ResourceCardDiscardPileA.Items)
            {
                ResourceCardDeck.Items.Add(i);
            }
            ResourceCardDiscardPileA.Clear();

            foreach (var i in ResourceCardDiscardPileB.Items)
            {
                ResourceCardDeck.Items.Add(i);
            }
            ResourceCardDiscardPileB.Clear();

            ResourceCardDeck.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
        }


        private void ProcessBlowCard(ResourceCard blowCard)
        {
            RecentMilestones.Add(Milestone.Resource);

            int spiceFactor = SandTroutDoublesResources ? 2 : 1;
            int spiceAmount = spiceFactor * blowCard.Location.SpiceBlowAmount;

            if (blowCard.Location.Sector != SectorInStorm)
            {
                Log(Payment(spiceAmount), " detected in ", blowCard.Location.Territory, SandtroutMessage(SandTroutDoublesResources));
                SandTroutDoublesResources = false;
                ChangeResourcesOnPlanet(blowCard.Location, spiceAmount);
            }
            else
            {
                Log(Payment(spiceAmount), " in ", blowCard.Location.Territory, " is lost in the storm");
            }

            Enter(Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal), CurrentPhase == Phase.BlowA ? Phase.HarvesterA : Phase.HarvesterB, MoveToNextPhaseAfterResourceBlow);
        }

        private MessagePart SandtroutMessage(bool SandTroutDoublesResources) => MessagePart.ExpressIf(SandTroutDoublesResources, ", doubled by ", Concept.BabyMonster);

        public void HandleEvent(HarvesterPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Harvester);
            var lastResourceCard = CurrentPhase == Phase.HarvesterA ? LatestSpiceCardA : LatestSpiceCardB;
            int currentAmountOfSpice = ResourcesOnPlanet.ContainsKey(lastResourceCard.Location) ? ResourcesOnPlanet[lastResourceCard.Location] : 0;

            if (currentAmountOfSpice > 0)
            {
                ChangeResourcesOnPlanet(lastResourceCard.Location, currentAmountOfSpice);
            }

            Log(e);
            MoveToNextPhaseAfterResourceBlow();
            RecentMilestones.Add(Milestone.Harvester);
        }

        private void MoveToNextPhaseAfterResourceBlow()
        {
            if (Monsters.Count == 0)
            {
                Enter((CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA) && Applicable(Rule.IncreasedResourceFlow), EnterBlowB, StartNexusCardPhase);
            }
            else
            {
                CurrentAllianceOffers.Clear();
                Enter((CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA), Phase.AllianceA, Phase.AllianceB);
            }
        }

        private void ProcessMonsterCard(Territory t)
        {
            if (CurrentTurn != 1)
            {
                if (Monsters.Count > 0)
                {
                    Log(Concept.Monster, " appears a ", Natural(Monsters.Count + 1), " time during this ", MainPhase.Blow);
                }
                else
                {
                    Log(Concept.Monster, " appears in ", t);
                }

                if (Monsters.Count > 0)
                {
                    if (IsPlaying(Faction.Yellow) && Applicable(Rule.YellowSendingMonster))
                    {
                        if (!Prevented(FactionAdvantage.YellowControlsMonster))
                        {
                            Enter(CurrentPhase == Phase.BlowA, Phase.YellowSendingMonsterA, Phase.YellowSendingMonsterB);
                        }
                        else
                        {
                            Monsters.Add(t);
                            LogPrevention(FactionAdvantage.YellowControlsMonster);
                            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowControlsMonster);
                        }
                    }
                    else
                    {
                        Monsters.Add(t);
                    }
                }
                else if (Monsters.Count == 0)
                {
                    Monsters.Add(t);
                    PerformMonster(t);
                }
            }
            else
            {
                Log(Concept.Monster, " on turn 1 was ignored");
            }
        }

        private void PerformMonster(Territory territory)
        {
            foreach (var l in territory.Locations)
            {
                foreach (var p in Players)
                {
                    if (p.AnyForcesIn(l) > 0)
                    {
                        if (!ProtectedFromMonster(p))
                        {
                            RevealCurrentNoField(p);

                            Log(Concept.Monster, " devours ", p.AnyForcesIn(l), p.Faction, " forces in ", l);
                            p.KillAllForces(l, false);

                            if (p.Is(Faction.Yellow))
                            {
                                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromMonster);
                            }
                            else if (p.Ally == Faction.Yellow && YellowWillProtectFromShaiHulud)
                            {
                                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromMonsterAlly);
                            }
                        }
                        else
                        {
                            Log(p.Faction, " survive ", Concept.Monster, " in ", l);
                        }
                    }
                }
            }

            var devouredResources = RemoveResources(territory);
            LogIf(devouredResources > 0, Concept.Monster, " devours ", Payment(devouredResources), " in ", territory);

            FlipBeneGesseritWhenAlone();
        }

        private void EnterBlowA()
        {
            Monsters.Clear();
            Enter(Phase.BlowA);
            LogIf(Applicable(Rule.IncreasedResourceFlow), "*** Spice Blow A ***");
            DrawSpiceCard();
        }

        private void EnterBlowB()
        {
            Monsters.Clear();
            Enter(Phase.BlowB);
            Log("*** Spice Blow B ***");
            DrawSpiceCard();
        }

        #endregion

        #region Nexus

        public readonly IList<AllianceOffered> CurrentAllianceOffers = new List<AllianceOffered>();

        public void HandleEvent(AllianceOffered e)
        {
            var matchingOffer = CurrentAllianceOffers.FirstOrDefault(x => x.Initiator == e.Target && x.Target == e.Initiator);
            if (matchingOffer != null)
            {
                MakeAlliance(e.Initiator, e.Target);

                AllianceOffered invalidOffer;
                while ((invalidOffer = CurrentAllianceOffers.FirstOrDefault(x => x.By(e.Initiator) || x.Initiator == e.Target)) != null)
                {
                    CurrentAllianceOffers.Remove(invalidOffer);
                }

                if (Version > 150)
                {
                    HasActedOrPassed.Add(e.Initiator);
                    HasActedOrPassed.Add(e.Target);
                }
            }
            else
            {
                Log(e);
                CurrentAllianceOffers.Add(e);
            }
        }

        private void MakeAlliance(Faction a, Faction b)
        {
            var playerA = GetPlayer(a);
            var playerB = GetPlayer(b);
            playerA.Ally = b;
            playerB.Ally = a;
            DiscardNexusCard(playerA);
            DiscardNexusCard(playerB);
            Log(a, " and ", b, " are now allies");
        }

        public void HandleEvent(AllianceBroken e)
        {
            Log(e);
            BreakAlliance(e.Initiator);
        }

        private void BreakAlliance(Faction f)
        {
            var initiator = GetPlayer(f);
            var currentAlly = GetPlayer(initiator.Ally);

            if (f == Faction.Orange || initiator.Ally == Faction.Orange)
            {
                OrangeAllyMayShipAsGuild = false;
            }

            if (f == Faction.Red || initiator.Ally == Faction.Red)
            {
                RedWillPayForExtraRevival = 0;
            }

            if (f == Faction.Yellow || initiator.Ally == Faction.Yellow)
            {
                YellowWillProtectFromShaiHulud = false;
                YellowAllowsThreeFreeRevivals = false;
            }

            if (PermittedUseOfAllySpice.ContainsKey(f))
            {
                PermittedUseOfAllySpice.Remove(f);
            }

            if (PermittedUseOfAllySpice.ContainsKey(initiator.Ally))
            {
                PermittedUseOfAllySpice.Remove(initiator.Ally);
            }

            if (PermittedUseOfAllyKarma.ContainsKey(f))
            {
                PermittedUseOfAllyKarma.Remove(f);
            }

            if (PermittedUseOfAllyKarma.ContainsKey(initiator.Ally))
            {
                PermittedUseOfAllyKarma.Remove(initiator.Ally);
            }

            initiator.Ally = Faction.None;
            currentAlly.Ally = Faction.None;
        }

        private bool NexusHasOccured { get; set; } = false;

        private void EndNexus()
        {
            NexusHasOccured = true;
            CurrentAllianceOffers.Clear();

            bool fremenCanRide = YellowRidesMonster.ValidSources(this).Any();
            if (fremenCanRide)
            {
                Enter(CurrentPhase == Phase.AllianceA, Phase.YellowRidingMonsterA, Phase.YellowRidingMonsterB);
            }
            else
            {
                if (CurrentPhase == Phase.AllianceA)
                {
                    Enter(Applicable(Rule.IncreasedResourceFlow), EnterBlowB, StartNexusCardPhase);
                }
                else if (CurrentPhase == Phase.AllianceB)
                {
                    StartNexusCardPhase();
                }
            }
        }

        #endregion


        #region WormSendingAndRiding

        public void HandleEvent(YellowSentMonster e)
        {
            Log(e);
            Monsters.Add(e.Territory);
            PerformMonster(e.Territory);
            Enter(CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
            DrawSpiceCard();
        }

        public void HandleEvent(YellowRidesMonster e)
        {
            Monsters.RemoveAt(0);

            if (!e.Passed)
            {
                var initiator = GetPlayer(e.Initiator);
                LastShipmentOrMovement = e;
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;
                foreach (var fl in e.ForceLocations)
                {
                    var from = fl.Key;
                    initiator.MoveForces(from, e.To, fl.Value.AmountOfForces);
                    initiator.MoveSpecialForces(from, e.To, fl.Value.AmountOfSpecialForces);
                    totalNumberOfForces += fl.Value.AmountOfForces;
                    totalNumberOfSpecialForces += fl.Value.AmountOfSpecialForces;
                    Log(
                        MessagePart.ExpressIf(fl.Value.AmountOfForces > 0, fl.Value.AmountOfForces, initiator.Force),
                        MessagePart.ExpressIf(fl.Value.AmountOfSpecialForces > 0, fl.Value.AmountOfSpecialForces, initiator.SpecialForce),
                        " ride ",
                        Concept.Monster,
                        " from ",
                        from,
                        " to ",
                        e.To);
                }

                FlipBeneGesseritWhenAlone();
            }
            else
            {
                Log(e.Initiator, " pass a ride on ", Concept.Monster);
            }

            CheckIntrusion(e);
            DetermineNextShipmentAndMoveSubPhase();
        }

        private void EndWormRide(Phase beforeIntrusion)
        {
            CurrentPhase = beforeIntrusion;
            EndWormRide();
        }

        private void EndWormRide()
        {
            bool fremenCanRide = YellowRidesMonster.ValidSources(this).Any();
            if (fremenCanRide)
            {
                Enter(CurrentPhase == Phase.AllianceA || CurrentPhase == Phase.YellowRidingMonsterA, Phase.YellowRidingMonsterA, Phase.YellowRidingMonsterB);
            }
            else
            {
                if (CurrentPhase == Phase.YellowRidingMonsterA && Applicable(Rule.IncreasedResourceFlow))
                {
                    EnterBlowB();
                }
                else
                {
                    StartNexusCardPhase();
                }
            }
        }

        #endregion


        #region EndOfSpiceBlow

        private void StartNexusCardPhase()
        {
            if (NexusHasOccured && Applicable(Rule.NexusCards) && Players.Any(p => p.HasAlly) && Players.Any(p => !p.HasAlly))
            {
                EnterNexusCardPhase();
            }
            else
            {
                EndBlowPhase();
            }
        }

        public List<Faction> FactionsThatDrewNexusCards { get; private set; }

        private void EnterNexusCardPhase()
        {
            NexusHasOccured = false;
            FactionsThatDrewNexusCards = new();
            Enter(Phase.NexusCards);
        }

        public void HandleEvent(NexusCardDrawn e)
        {
            DealNexusCard(e.Player);
            FactionsThatDrewNexusCards.Add(e.Initiator);

            if (!Players.Any(p => NexusCardDrawn.Applicable(this, p)))
            {
                EndBlowPhase();
            }
        }

        private void DealNexusCard(Player p)
        {
            DiscardNexusCard(p);

            if (NexusCardDeck.IsEmpty)
            {
                NexusCardDeck.Items.AddRange(NexusDiscardPile);
                NexusDiscardPile.Clear();
                NexusCardDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
                Log("The Nexus Card discard pile was shuffled into a new Nexus Card deck");
            }

            Log(p.Faction, " draw a Nexus Card");
            p.Nexus = NexusCardDeck.Draw();
        }

        private void DiscardNexusCard(Player p)
        {
            if (p.Nexus != Faction.None)
            {
                Log(p.Faction, " discard the ", p.Nexus, " Nexus Card");
                NexusDiscardPile.Add(p.Nexus);
                p.Nexus = Faction.None;
            }
        }

        private void EndBlowPhase()
        {
            HasActedOrPassed.Clear();
            ReshuffleIgnoredMonsters();
            MainPhaseEnd();
            Enter(Phase.BlowReport);
        }

        private void ReshuffleIgnoredMonsters()
        {
            if (ignoredMonsters.Count > 0 || ignoredSandtrout != null)
            {
                if (ignoredMonsters.Count > 0)
                {
                    Log(ignoredMonsters.Count, " ignored ", Concept.Monster, "cards were shuffled back into the ", Concept.Resource, " deck");
                    foreach (var c in ignoredMonsters)
                    {
                        ResourceCardDeck.Items.Add(c);
                    }
                }

                if (ignoredSandtrout != null)
                {
                    Log(Concept.BabyMonster, "card was shuffled back into the ", Concept.Resource, " deck");
                    ResourceCardDeck.Items.Add(ignoredSandtrout);
                }

                ResourceCardDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
            }
        }

        #endregion

        #region Support

        private string Natural(int count)
        {
            return count switch
            {
                1 => "first",
                2 => "second",
                3 => "third",
                4 => "fourth",
                5 => "fifth",
                6 => "sixth",
                7 => "seventh",
                8 => "eighth",
                9 => "ninth",
                10 => "tenth",
                _ => count + "th",
            };
        }

        private Deck<ResourceCard> CurrentDiscardPile => (CurrentPhase == Phase.BlowA) ? ResourceCardDiscardPileA : ResourceCardDiscardPileB;

        public ResourceCard LatestSpiceCardA { get; private set; } = null;
        public ResourceCard LatestSpiceCardB { get; private set; } = null;

        private ResourceCard PreviousBlowCard
        {
            get
            {
                return (CurrentPhase == Phase.BlowA) ? LatestSpiceCardA : LatestSpiceCardB;
            }
            set
            {
                if (CurrentPhase == Phase.BlowA)
                {
                    LatestSpiceCardA = value;
                }
                else
                {
                    LatestSpiceCardB = value;
                }
            }
        }

        public bool ProtectedFromMonster(Player p)
        {
            if (p.Is(Faction.Yellow))
            {
                return !Prevented(FactionAdvantage.YellowProtectedFromMonster);
            }
            else
            {
                return p.Ally == Faction.Yellow && YellowWillProtectFromShaiHulud && !Prevented(FactionAdvantage.YellowProtectedFromMonsterAlly);
            }
        }

        #endregion
    }
}
