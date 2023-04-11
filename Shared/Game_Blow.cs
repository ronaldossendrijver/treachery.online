/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        public List<MonsterAppearence> Monsters { get; private set; } = new();
        private readonly List<ResourceCard> ignoredMonsters = new();
        private ResourceCard ignoredSandtrout = null;
        internal List<NexusVoted> NexusVotes { get; set; } = new();

        #endregion State

        #region BeginningOfSpiceBlow

        internal void EnterSpiceBlowPhase()
        {
            MainPhaseStart(MainPhase.Blow);
            MonsterAppearedInTerritoryWithoutForces = false;
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
            Stone(Milestone.Thumper);
            ThumperUsed = true;
            EnterBlowA();
        }

        #endregion BeginningOfSpiceBlow

        #region ExecuteResourceBlow

        public int NumberOfMonsters { get; internal set; } = 0;

        public ResourceCard SandTrout { get; private set; } = null;

        public bool SandTroutOccured => SandTrout != null;

        private bool SandTroutDoublesResources { get; set; } = false;

        private bool NexusVoteMustHappen { get; set; } = false;

        private void DrawResourceCard()
        {
            ResourceCard drawn = null;
            while (ThumperUsed || !(drawn = DrawAndDiscardResourceCard(CurrentDiscardPile)).IsSpiceBlow)
            {
                if (ThumperUsed && Version <= 150)
                {
                    ThumperUsed = false;
                    NumberOfMonsters++;
                    LetMonsterAppear(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud || PreviousBlowCard.IsGreatMaker ? null : PreviousBlowCard.Territory, false);
                    if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB)
                    {
                        break;
                    }
                }
                else if (drawn != null && (drawn.IsShaiHulud || drawn.IsGreatMaker) && CurrentTurn == 1)
                {
                    Log(drawn.IsShaiHulud ? Concept.Monster : Concept.GreatMonster, " on turn 1 was ignored");
                    ignoredMonsters.Add(CurrentDiscardPile.Draw());
                }
                else if (ThumperUsed && Version > 150 || drawn.IsShaiHulud || drawn.IsGreatMaker)
                {
                    if (!ThumperUsed && drawn.IsGreatMaker && Monsters.Count == 0)
                    {
                        NexusVoteMustHappen = true;
                    }
                    else
                    {
                        NexusVoteMustHappen = false;
                    }

                    if (!ThumperUsed)
                    {
                        if (drawn.IsShaiHulud)
                        {
                            Stone(Milestone.Monster);
                        }
                        else
                        {
                            Stone(Milestone.GreatMonster);
                        }
                    }

                    if (!SandTroutOccured)
                    {
                        SandTroutDoublesResources = false;
                        NumberOfMonsters++;
                        LetMonsterAppear(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud || PreviousBlowCard.IsGreatMaker ? null : PreviousBlowCard.Territory, !ThumperUsed && drawn.IsGreatMaker);
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
                        Stone(Milestone.BabyMonster);
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

                ThumperUsed = false;
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
            Stone(Milestone.Shuffled);
        }


        private void ProcessBlowCard(ResourceCard blowCard)
        {
            Stone(Milestone.Resource);

            if (blowCard.IsDiscovery)
            {
                if (AnyForcesIn(blowCard.Territory))
                {
                    KillAllForcesIn(blowCard.Territory, false);
                }

                var devouredResources = RemoveResources(blowCard.Territory);
                LogIf(devouredResources > 0, Payment.Of(devouredResources), " in ", blowCard.Territory, " is destroyed");

                DiscoveryToken drawnToken = DiscoveryToken.None;
                if (blowCard.DiscoveryLocation.DiscoveryTokenType == DiscoveryTokenType.Orange)
                {
                    if (!OrangeDiscoveryTokens.IsEmpty)
                    {
                        drawnToken = OrangeDiscoveryTokens.Draw();
                        DiscoveriesOnPlanet.Add(blowCard.DiscoveryLocation, new Discovery(drawnToken, DiscoveryTokenType.Orange, blowCard.DiscoveryLocation));
                    }
                    else
                    {
                        Log("There are no more ", DiscoveryTokenType.Orange, " discoveries left");
                    }
                }
                else if (blowCard.DiscoveryLocation.DiscoveryTokenType == DiscoveryTokenType.Yellow)
                {
                    if (!YellowDiscoveryTokens.IsEmpty)
                    {
                        drawnToken = YellowDiscoveryTokens.Draw();
                        DiscoveriesOnPlanet.Add(blowCard.DiscoveryLocation, new Discovery(drawnToken, DiscoveryTokenType.Yellow, blowCard.DiscoveryLocation));
                    }
                    else
                    {
                        Log("There are no more ", DiscoveryTokenType.Yellow, " discoveries left");
                    }
                }

                if (drawnToken != DiscoveryToken.None)
                {
                    Stone(Milestone.DiscoveryAppeared);
                    Log("A ", blowCard.DiscoveryLocation.DiscoveryTokenType, " discovery awaits in ", blowCard.DiscoveryLocation.Territory, "...");
                }
            }

            int spiceFactor = SandTroutDoublesResources ? 2 : 1;
            int spiceAmount = spiceFactor * blowCard.Location.SpiceBlowAmount;

            if (blowCard.Location.Sector != SectorInStorm)
            {
                Log(Payment.Of(spiceAmount), " detected in ", blowCard, SandtroutMessage(SandTroutDoublesResources));
                SandTroutDoublesResources = false;
                ChangeResourcesOnPlanet(blowCard.Location, spiceAmount);
            }
            else
            {
                Log(Payment.Of(spiceAmount), " in ", blowCard, " is lost in the storm");
            }

            Enter(Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal), CurrentPhase == Phase.BlowA ? Phase.HarvesterA : Phase.HarvesterB, MoveToNextPhaseAfterResourceBlow);
        }

        private MessagePart SandtroutMessage(bool SandTroutDoublesResources) => MessagePart.ExpressIf(SandTroutDoublesResources, ", doubled by ", Concept.BabyMonster);

        internal void MoveToNextPhaseAfterResourceBlow()
        {
            if (Monsters.Count == 0)
            {
                Enter((CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA) && Applicable(Rule.IncreasedResourceFlow), EnterBlowB, StartNexusCardPhase);
            }
            else
            {
                CurrentAllianceOffers.Clear();

                if (Monsters.Count == 1 && Monsters[0].IsGreatMonster)
                {
                    NexusVotes.Clear();
                    Enter(CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA, Phase.VoteAllianceA, Phase.VoteAllianceB);
                }
                else
                {
                    Enter(CurrentPhase == Phase.BlowA || CurrentPhase == Phase.HarvesterA, Phase.AllianceA, Phase.AllianceB);
                }
            }
        }

        public bool MonsterAppearedInTerritoryWithoutForces { get; private set; } = false;

        internal void LetMonsterAppear(Territory t, bool isGreatMonster)
        {
            var m = new MonsterAppearence(t, isGreatMonster);

            if (CurrentTurn != 1)
            {
                if (m.IsGreatMonster)
                {
                    Log(Concept.GreatMonster, " appears in ", m.Territory);
                }
                else if (Monsters.Count > 0)
                {
                    Log(Concept.Monster, " appears a ", Natural(Monsters.Count + 1), " time during this ", MainPhase.Blow);
                }
                else
                {
                    Log(Concept.Monster, " appears in ", m.Territory);
                }

                if (!AnyForcesIn(m.Territory))
                {
                    MonsterAppearedInTerritoryWithoutForces = true;
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
                            Monsters.Add(m);
                            LogPreventionByKarma(FactionAdvantage.YellowControlsMonster);
                            if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowControlsMonster);
                        }
                    }
                    else
                    {
                        Monsters.Add(m);
                    }
                }
                else if (Monsters.Count == 0)
                {
                    Monsters.Add(m);
                    PerformMonster(m);
                }
            }
            else
            {
                Log(m.DescribingConcept, " on turn 1 was ignored");
            }
        }

        private void PerformMonster(MonsterAppearence m)
        {
            foreach (var l in m.Territory.Locations)
            {
                foreach (var p in Players)
                {
                    if (p.AnyForcesIn(l) > 0)
                    {
                        if (!ProtectedFromMonster(p))
                        {
                            RevealCurrentNoField(p);

                            Log(m.DescribingConcept, " devours ", p.AnyForcesIn(l), p.Faction, " forces in ", l);
                            p.KillAllForces(l, false);

                            if (p.Is(Faction.Yellow))
                            {
                                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromMonster);
                            }
                            else if (p.Ally == Faction.Yellow && YellowWillProtectFromMonster)
                            {
                                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromMonsterAlly);
                            }
                        }
                        else
                        {
                            Log(p.Faction, " survive ", m.DescribingConcept, " in ", l);
                        }
                    }
                }
            }

            var devouredResources = RemoveResources(m.Territory);
            LogIf(devouredResources > 0, m.DescribingConcept, " devours ", Payment.Of(devouredResources), " in ", m.Territory);

            FlipBeneGesseritWhenAloneOrWithPinkAlly();
        }

        internal void EnterBlowA()
        {
            Monsters.Clear();
            NexusVoteMustHappen = false;
            Enter(Phase.BlowA);
            LogIf(Applicable(Rule.IncreasedResourceFlow), "*** Spice Blow A ***");
            DrawResourceCard();
            LetFactionsDiscardSurplusCards();
        }

        internal void EnterBlowB()
        {
            Monsters.Clear();
            NexusVoteMustHappen = false;
            Enter(Phase.BlowB);
            Log("*** Spice Blow B ***");
            DrawResourceCard();
            LetFactionsDiscardSurplusCards();
        }

        #endregion




        #region WormSendingAndRiding

        public void HandleEvent(YellowSentMonster e)
        {
            Log(e);
            var m = new MonsterAppearence(e.Territory, false);
            Monsters.Add(m);
            PerformMonster(m);
            Enter(CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
            DrawResourceCard();
            LetFactionsDiscardSurplusCards();
        }

        public void HandleEvent(YellowRidesMonster e)
        {
            var toRide = YellowRidesMonster.ToRide(this);

            if (Version <= 150)
            {
                Monsters.RemoveAt(0);
            }
            else
            {
                Monsters.Remove(toRide);
            }

            if (!e.Passed)
            {
                if (e.ForceLocations.Keys.Any(l => l.Territory != toRide.Territory))
                {
                    PlayNexusCard(e.Player, "cunning", "to ride from any territory on the planet");
                }

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
                        " ride from ",
                        from,
                        " to ",
                        e.To);
                }

                if (e.ForcesFromReserves > 0 || e.SpecialForcesFromReserves > 0)
                {
                    if (e.ForcesFromReserves > 0) initiator.ShipForces(e.To, e.ForcesFromReserves);
                    if (e.SpecialForcesFromReserves > 0) initiator.ShipSpecialForces(e.To, e.SpecialForcesFromReserves);
                    Log(
                        MessagePart.ExpressIf(e.ForcesFromReserves > 0, e.ForcesFromReserves, initiator.Force),
                        MessagePart.ExpressIf(e.SpecialForcesFromReserves > 0, e.SpecialForcesFromReserves, initiator.SpecialForce),
                        " ride from their reserves to ",
                        e.To);
                }

                FlipBeneGesseritWhenAloneOrWithPinkAlly();
                CheckIntrusion(e);
            }
            else
            {
                Log(e.Initiator, " pass a ride on ", Concept.Monster);
            }

            DetermineNextShipmentAndMoveSubPhase();
        }

        private void EndWormRideDuringPhase(Phase phase)
        {
            CurrentPhase = phase;

            if (YellowRidesMonster.IsApplicable(this))
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

        internal void StartNexusCardPhase()
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

        public List<Faction> FactionsThatMayDrawNexusCard { get; private set; }
        public List<Faction> FactionsThatDrewNexusCard { get; private set; }

        private void EnterNexusCardPhase()
        {
            NexusHasOccured = false;
            FactionsThatMayDrawNexusCard = Players.Where(p => !p.HasAlly).Select(p => p.Faction).ToList();
            FactionsThatDrewNexusCard = new();
            Enter(Phase.NexusCards);
        }

        

        internal void DiscardNexusCard(Player p)
        {
            if (p.Nexus != Faction.None)
            {
                Log(p.Faction, " discard the ", p.Nexus, " Nexus Card");
                Stone(Milestone.NexusPlayed);
                NexusDiscardPile.Add(p.Nexus);
                p.Nexus = Faction.None;
            }
        }

        internal void EndBlowPhase()
        {
            CurrentYellowNexus = null;
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
                    Log(ignoredMonsters.Count, " ignored ", Concept.Monster, " cards were shuffled back into the ", Concept.Resource, " deck");
                    foreach (var c in ignoredMonsters)
                    {
                        ResourceCardDeck.Items.Add(c);
                    }
                }

                if (ignoredSandtrout != null)
                {
                    Log(Concept.BabyMonster, " card was shuffled back into the ", Concept.Resource, " deck");
                    ResourceCardDeck.Items.Add(ignoredSandtrout);
                }

                ResourceCardDeck.Shuffle();
                Stone(Milestone.Shuffled);
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
            if (CurrentYellowNexus != null && CurrentYellowNexus.Player == p)
            {
                return true;
            }
            else if (p.Is(Faction.Yellow))
            {
                return !Prevented(FactionAdvantage.YellowProtectedFromMonster);
            }
            else
            {
                return p.Ally == Faction.Yellow && YellowWillProtectFromMonster && !Prevented(FactionAdvantage.YellowProtectedFromMonsterAlly);
            }
        }

        #endregion
    }
}
