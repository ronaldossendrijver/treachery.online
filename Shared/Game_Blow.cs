/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public List<Territory> Monsters { get; set; } = new List<Territory>();
        private readonly List<ResourceCard> ignoredMonsters = new List<ResourceCard>();

        private void EnterSpiceBlowPhase()
        {
            MainPhaseStart(MainPhase.Blow);
            ignoredMonsters.Clear();
            var sequenceToDetermineFirstPlayer = new PlayerSequence(this);
            Enter(Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal) && (Version <= 102 || CurrentTurn > 1), Phase.Thumper, EnterBlowA);
        }

        private bool ThumperUsed = false;
        public void HandleEvent(ThumperPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Thumper);
            CurrentReport.Add(e);
            RecentMilestones.Add(Milestone.Thumper);
            ThumperUsed = true;
            EnterBlowA();
        }

        public int NumberOfMonsters { get; set; } = 0;
        public bool SandTroutOccured { get; set; } = false;
        private bool SandTroutDoublesResources = false;

        private void DrawSpiceCard()
        {
            ResourceCard drawn = null;
            while (ThumperUsed || !(drawn = DrawAndDiscardResourceCard(CurrentDiscardPile)).IsSpiceBlow)
            {
                if (ThumperUsed)
                {
                    ThumperUsed = false;
                    NumberOfMonsters++;
                    ProcessMonsterCard(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud ? null : PreviousBlowCard.Location.Territory);
                    if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB)
                    {
                        break;
                    }
                }
                else if (drawn.IsShaiHulud && CurrentTurn == 1)
                {
                    CurrentReport.Add("{0} on turn 1 was ignored.", Concept.Monster);
                    ignoredMonsters.Add(CurrentDiscardPile.Draw());
                }
                else if (drawn.IsShaiHulud)
                {
                    SandTroutDoublesResources = false;
                    RecentMilestones.Add(Milestone.Monster);

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
                        SandTroutOccured = false;
                        SandTroutDoublesResources = true;
                        CurrentReport.Add("{0} is ignored due to {1}.", Concept.Monster, Concept.BabyMonster);
                    }
                }
                else if (drawn.IsSandTrout)
                {
                    RecentMilestones.Add(Milestone.BabyMonster);
                    CurrentReport.Add(Faction.None, "{0} detected! All alliances are cancelled.", Concept.BabyMonster);
                    CancelAllAlliances();
                    CurrentDiscardPile.Items.Remove(drawn);
                    SandTroutOccured = true;
                }
                else
                {
                    CurrentReport.Add(Faction.None, "Unexpected card...");
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

        private Deck<ResourceCard> CurrentDiscardPile
        {
            get
            {
                return (CurrentPhase == Phase.BlowA) ? ResourceCardDiscardPileA : ResourceCardDiscardPileB;
            }
        }

        public ResourceCard LatestSpiceCardA = null;
        public ResourceCard LatestSpiceCardB = null;

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
                CurrentReport.Add("{0} cards were shuffled from {1} discard piles A en B into a new deck.", ResourceCardDiscardPileA.Items.Count + ResourceCardDiscardPileB.Items.Count, Concept.Resource);
            }
            else
            {
                CurrentReport.Add("{0} cards were shuffled from the {1} discard pile into a new deck.", ResourceCardDiscardPileA.Items.Count, Concept.Resource);
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

            if (blowCard.Location.Sector != SectorInStorm)
            {
                int spiceFactor = SandTroutDoublesResources ? 2 : 1;
                CurrentReport.Add("{1} detected in {0}{2}.", blowCard.Location, Concept.Resource, SandtroutMessage(SandTroutDoublesResources));
                SandTroutDoublesResources = false;
                ChangeResourcesOnPlanet(blowCard.Location, spiceFactor * blowCard.Location.SpiceBlowAmount);
            }
            else
            {
                CurrentReport.Add("{1} in {0} is destroyed by the storm.", blowCard.Location, Concept.Resource);
            }

            Enter(Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal), CurrentPhase == Phase.BlowA ? Phase.HarvesterA : Phase.HarvesterB, MoveToNextPhaseAfterResourceBlow);
        }

        private MessagePart SandtroutMessage(bool SandTroutDoublesResources)
        {
            if (SandTroutDoublesResources)
            {
                return new MessagePart(", doubled by {0}", Concept.BabyMonster);
            }
            else
            {
                return new MessagePart("");
            }
        }

        public void HandleEvent(HarvesterPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Harvester);
            var lastResourceCard = CurrentPhase == Phase.HarvesterA ? LatestSpiceCardA : LatestSpiceCardB;
            int currentAmountOfSpice = ResourcesOnPlanet.ContainsKey(lastResourceCard.Location) ? ResourcesOnPlanet[lastResourceCard.Location] : 0;

            if (currentAmountOfSpice > 0)
            {
                ChangeResourcesOnPlanet(lastResourceCard.Location, currentAmountOfSpice);
            }

            CurrentReport.Add(e);
            MoveToNextPhaseAfterResourceBlow();
            RecentMilestones.Add(Milestone.Harvester);
        }

        private void MoveToNextPhaseAfterResourceBlow()
        {
            if (Monsters.Count == 0)
            {
                Enter((CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA) && Applicable(Rule.IncreasedResourceFlow), EnterBlowB, EnterStormReportPhase);
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
                    CurrentReport.Add("{0} appears a {1} time during this {2}...", Concept.Monster, Natural(Monsters.Count + 1), MainPhase.Blow);
                }
                else
                {
                    if (t != null)
                    {
                        CurrentReport.Add("{0} appears in {1}.", Concept.Monster, t);
                    }
                    else
                    {
                        CurrentReport.Add("{0} appears somewhere.", Concept.Monster);
                    }
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
                            CurrentReport.Add(Faction.Yellow, "{0} prevents {1} from sending {2} where they want.", TreacheryCardType.Karma, Faction.Yellow, Concept.Monster);
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
                CurrentReport.Add("{0} on turn 1 was ignored.", Concept.Monster);
            }
        }

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

        public void HandleEvent(YellowSentMonster e)
        {
            CurrentReport.Add(e);
            Monsters.Add(e.Territory);
            PerformMonster(e.Territory);
            Enter(CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
            DrawSpiceCard();
        }

        private void PerformMonster(Territory t)
        {
            foreach (var l in t.Locations)
            {
                foreach (var p in Players)
                {
                    if (p.AnyForcesIn(l) > 0)
                    {
                        if (!ProtectedFromMonster(p))
                        {
                            RevealCurrentNoField(p);

                            CurrentReport.Add("{3} devours {0} {1} forces in {2}.", p.AnyForcesIn(l), p.Faction, l, Concept.Monster);
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
                            CurrentReport.Add("{0} survive {1} in {2}.", p.Faction, Concept.Monster, l);
                        }
                    }
                }
            }

            var devouredResources = RemoveResources(t);
            if (devouredResources > 0)
            {
                CurrentReport.Add("{2} devours {0} {3} in {1}.", devouredResources, t, Concept.Monster, Concept.Resource);
            }

            FlipBeneGesseritWhenAlone();
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

        public readonly IList<AllianceOffered> CurrentAllianceOffers = new List<AllianceOffered>();

        public void HandleEvent(AllianceOffered e)
        {
            var matchingOffer = CurrentAllianceOffers.FirstOrDefault(x => x.Initiator == e.Target && x.Target == e.Initiator);
            if (matchingOffer != null)
            {
                var initiator = GetPlayer(e.Initiator);
                var target = GetPlayer(e.Target);
                initiator.Ally = e.Target;
                target.Ally = e.Initiator;
                CurrentReport.Add("{0} and {1} are now allies.", e.Initiator, matchingOffer.Initiator);

                AllianceOffered invalidOffer;
                while ((invalidOffer = CurrentAllianceOffers.FirstOrDefault(x => x.By(e.Initiator) || x.Initiator == e.Target)) != null)
                {
                    CurrentAllianceOffers.Remove(invalidOffer);
                }
            }
            else
            {
                CurrentReport.Add(e);
                CurrentAllianceOffers.Add(e);
            }
        }

        public void HandleEvent(AllianceBroken e)
        {
            CurrentReport.Add(e);
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

        private void EndNexus()
        {
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
                    Enter(Applicable(Rule.IncreasedResourceFlow), EnterBlowB, EnterStormReportPhase);
                }
                else if (CurrentPhase == Phase.AllianceB)
                {
                    EnterStormReportPhase();
                }
            }
        }

        public void HandleEvent(YellowRidesMonster e)
        {
            Monsters.RemoveAt(0);

            if (!e.Passed)
            {
                var initiator = GetPlayer(e.Initiator);

                LastShippedOrMovedTo = e.To;
                int totalNumberOfForces = 0;
                int totalNumberOfSpecialForces = 0;
                foreach (var fl in e.ForceLocations)
                {
                    var from = fl.Key;
                    initiator.MoveForces(from, e.To, fl.Value.AmountOfForces);
                    initiator.MoveSpecialForces(from, e.To, fl.Value.AmountOfSpecialForces);
                    totalNumberOfForces += fl.Value.AmountOfForces;
                    totalNumberOfSpecialForces += fl.Value.AmountOfSpecialForces;
                    CurrentReport.Add(e.Initiator, "{0} {4} {7} and {1} {5} ride {6} from {2} to {3}.",
                        fl.Value.AmountOfForces, fl.Value.AmountOfSpecialForces, from, e.To, e.Initiator, initiator.SpecialForce, Concept.Monster, initiator.Force);
                }

                FlipBeneGesseritWhenAlone();
            }
            else
            {
                CurrentReport.Add(e.Initiator, "{0} pass a ride on {1}.", e.Initiator, Concept.Monster);
            }

            bool bgIntruded = DetermineIntrusionCaused(e);
            DetermineNextShipmentAndMoveSubPhase(bgIntruded, false);
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
                    EnterStormReportPhase();
                }
            }
        }

        private void EnterBlowA()
        {
            Monsters.Clear();
            Enter(Phase.BlowA);
            if (Applicable(Rule.IncreasedResourceFlow))
            {
                CurrentReport.Add("*** Spice Blow A ***");
            }
            DrawSpiceCard();
        }

        private void EnterBlowB()
        {
            Monsters.Clear();
            Enter(Phase.BlowB);
            CurrentReport.Add("*** Spice Blow B ***");
            DrawSpiceCard();
        }

        private void EnterStormReportPhase()
        {
            if (CurrentTurn == 1 && ignoredMonsters.Count > 0)
            {
                CurrentReport.Add("{0} ignored {1}-cards were shuffled back into the {2} deck.", ignoredMonsters.Count, Concept.Monster, Concept.Resource);
                foreach (var c in ignoredMonsters)
                {
                    ResourceCardDeck.Items.Add(c);
                }
                ResourceCardDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
            }

            MainPhaseEnd();
            Enter(Phase.BlowReport);
        }
    }
}
