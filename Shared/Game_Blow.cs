/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

            if (Version < 135)
            {
                Enter(Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal) && (Version <= 102 || CurrentTurn > 1), Phase.Thumper, EnterBlowA);
            }
            else
            {
                Enter(Phase.Thumper);
            }

        }

        private bool ThumperUsed = false;
        public void HandleEvent(ThumperPlayed e)
        {
            Discard(GetPlayer(e.Initiator), TreacheryCardType.Thumper);
            CurrentReport.Express(e);
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
                    CurrentReport.Express(Concept.Monster, " on turn 1 was ignored");
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
                        CurrentReport.Express(Concept.Monster, " is ignored due to ", Concept.BabyMonster);
                    }
                }
                else if (drawn.IsSandTrout)
                {
                    RecentMilestones.Add(Milestone.BabyMonster);
                    CurrentReport.Express(Concept.BabyMonster, " detected! All alliances are cancelled.");
                    CancelAllAlliances();
                    CurrentDiscardPile.Items.Remove(drawn);
                    SandTroutOccured = true;
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

        private Deck<ResourceCard> CurrentDiscardPile => (CurrentPhase == Phase.BlowA) ? ResourceCardDiscardPileA : ResourceCardDiscardPileB;

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
                CurrentReport.Express(ResourceCardDiscardPileA.Items.Count + ResourceCardDiscardPileB.Items.Count, " cards were shuffled from ", Concept.Resource, " discard piles A en B into a new deck.");
            }
            else
            {
                CurrentReport.Express(ResourceCardDiscardPileA.Items.Count, " cards were shuffled from the ", Concept.Resource, " discard pile into a new deck");
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
                CurrentReport.Express(Payment(spiceAmount), " detected in ", blowCard.Location, SandtroutMessage(SandTroutDoublesResources));
                SandTroutDoublesResources = false;
                ChangeResourcesOnPlanet(blowCard.Location, spiceAmount);
            }
            else
            {
                CurrentReport.Express(Payment(spiceAmount), " in ", blowCard.Location, " is lost in the storm");
            }

            Enter(Applicable(Rule.GreyAndPurpleExpansionTreacheryCardsExceptPBandSSandAmal), CurrentPhase == Phase.BlowA ? Phase.HarvesterA : Phase.HarvesterB, MoveToNextPhaseAfterResourceBlow);
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

            CurrentReport.Express(e);
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
                    CurrentReport.Express(Concept.Monster, " appears a ", Natural(Monsters.Count + 1), " time during this ", MainPhase.Blow);
                }
                else
                {
                    CurrentReport.Express(Concept.Monster, " appears in ", t);
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
                CurrentReport.Express(Concept.Monster, " on turn 1 was ignored");
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
            CurrentReport.Express(e);
            Monsters.Add(e.Territory);
            PerformMonster(e.Territory);
            Enter(CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
            DrawSpiceCard();
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

                            CurrentReport.Express(Concept.Monster, " devours ", p.AnyForcesIn(l), p.Faction, " forces in ", l);
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
                            CurrentReport.Express(p.Faction, " survive ", Concept.Monster, " in ", l);
                        }
                    }
                }
            }

            var devouredResources = RemoveResources(territory);
            CurrentReport.ExpressIf(devouredResources > 0, Concept.Monster, " devours ", Payment(devouredResources), " in ", territory);

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
                CurrentReport.Express(e.Initiator, " and ", matchingOffer.Initiator, " are now allies");

                AllianceOffered invalidOffer;
                while ((invalidOffer = CurrentAllianceOffers.FirstOrDefault(x => x.By(e.Initiator) || x.Initiator == e.Target)) != null)
                {
                    CurrentAllianceOffers.Remove(invalidOffer);
                }
            }
            else
            {
                CurrentReport.Express(e);
                CurrentAllianceOffers.Add(e);
            }
        }

        public void HandleEvent(AllianceBroken e)
        {
            CurrentReport.Express(e);
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
                    CurrentReport.Express(
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
                CurrentReport.Express(e.Initiator, " pass a ride on ", Concept.Monster);
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
                CurrentReport.Express("*** Spice Blow A ***");
            }
            DrawSpiceCard();
        }

        private void EnterBlowB()
        {
            Monsters.Clear();
            Enter(Phase.BlowB);
            CurrentReport.Express("*** Spice Blow B ***");
            DrawSpiceCard();
        }

        private void EnterStormReportPhase()
        {
            if (CurrentTurn == 1 && ignoredMonsters.Count > 0)
            {
                CurrentReport.Express(ignoredMonsters.Count, " ignored ", Concept.Monster, "cards were shuffled back into the ", Concept.Resource, " deck");
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
