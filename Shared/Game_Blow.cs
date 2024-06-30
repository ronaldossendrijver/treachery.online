/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public partial class Game
{
    #region State

    public ResourceCard LatestSpiceCardA { get; private set; }
    public ResourceCard LatestSpiceCardB { get; private set; }

    private List<ResourceCard> IgnoredMonsters { get; } = new();
    public List<MonsterAppearence> Monsters { get; } = new();
    public int NumberOfMonsters { get; internal set; }

    internal List<NexusVoted> NexusVotes { get; } = new();
    internal bool ThumperUsed { get; set; }

    private ResourceCard IgnoredSandtrout { get; set; }
    public ResourceCard SandTrout { get; private set; }
    private bool SandTroutDoublesResources { get; set; }

    public List<Faction> FactionsThatMayDrawNexusCard { get; private set; } = new();
    public List<Faction> FactionsThatDrewNexusCard { get; } = new();

    #endregion State

    #region BeginningOfResourceBlow

    internal void EnterSpiceBlowPhase()
    {
        MainPhaseStart(MainPhase.Blow);
        MonsterAppearedInTerritoryWithoutForces = false;
        IgnoredMonsters.Clear();
        IgnoredSandtrout = null;
        HasActedOrPassed.Clear();

        var sequenceToDetermineFirstPlayer = new PlayerSequence(this);

        if (Version < 135)
            Enter(Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal) && (Version <= 102 || CurrentTurn > 1), Phase.Thumper, EnterBlowA);
        else
            Enter(Phase.Thumper);
    }

    #endregion BeginningOfResourceBlow

    #region ResourceBlow

    internal void DrawResourceCard()
    {
        ResourceCard drawn = null;
        while (ThumperUsed || !(drawn = DrawAndDiscardResourceCard(CurrentDiscardPile)).IsSpiceBlow)
        {
            if (ThumperUsed && Version <= 150)
            {
                ThumperUsed = false;
                NumberOfMonsters++;
                LetMonsterAppear(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud || PreviousBlowCard.IsGreatMaker ? null : PreviousBlowCard.Territory, false);
                if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB) break;
            }
            else if (drawn != null && (drawn.IsShaiHulud || drawn.IsGreatMaker) && CurrentTurn == 1)
            {
                Log(drawn.IsShaiHulud ? Concept.Monster : Concept.GreatMonster, " on turn 1 was ignored");
                IgnoredMonsters.Add(CurrentDiscardPile.Draw());
            }
            else if ((ThumperUsed && Version > 150) || drawn.IsShaiHulud || drawn.IsGreatMaker)
            {
                if (!ThumperUsed)
                {
                    if (drawn.IsShaiHulud)
                        Stone(Milestone.Monster);
                    else
                        Stone(Milestone.GreatMonster);
                }

                if (!SandTroutOccured)
                {
                    SandTroutDoublesResources = false;
                    NumberOfMonsters++;
                    LetMonsterAppear(PreviousBlowCard == null || PreviousBlowCard.IsShaiHulud || PreviousBlowCard.IsGreatMaker ? null : PreviousBlowCard.Territory, !ThumperUsed && drawn.IsGreatMaker);
                    if (CurrentPhase == Phase.YellowSendingMonsterA || CurrentPhase == Phase.YellowSendingMonsterB) break;
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
                    IgnoredSandtrout = CurrentDiscardPile.Draw();
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
            if (p.Ally != Faction.None) BreakAlliance(p.Faction);
    }

    private ResourceCard DrawAndDiscardResourceCard(Deck<ResourceCard> discardPile)
    {
        if (ResourceCardDeck.IsEmpty) ReshuffleResourceDeck();

        var drawn = ResourceCardDeck.Draw();
        discardPile.PutOnTop(drawn);
        return drawn;
    }

    private void ReshuffleResourceDeck()
    {
        if (Applicable(Rule.IncreasedResourceFlow))
            Log(ResourceCardDiscardPileA.Items.Count + ResourceCardDiscardPileB.Items.Count, " cards were shuffled from ", Concept.Resource, " discard piles A en B into a new deck.");
        else
            Log(ResourceCardDiscardPileA.Items.Count, " cards were shuffled from the ", Concept.Resource, " discard pile into a new deck");

        var excludeDiscoveryCardsWhenReshuffling = Version >= 158;

        if (excludeDiscoveryCardsWhenReshuffling && (ResourceCardDiscardPileA.Items.Any(c => c.IsDiscovery) || ResourceCardDiscardPileB.Items.Any(c => c.IsDiscovery))) Log("Discovery were removed from the game");

        foreach (var i in ResourceCardDiscardPileA.Items.Where(c => !excludeDiscoveryCardsWhenReshuffling || !c.IsDiscovery)) ResourceCardDeck.Items.Add(i);
        ResourceCardDiscardPileA.Clear();

        foreach (var i in ResourceCardDiscardPileB.Items.Where(c => !excludeDiscoveryCardsWhenReshuffling || !c.IsDiscovery)) ResourceCardDeck.Items.Add(i);
        ResourceCardDiscardPileB.Clear();

        ResourceCardDeck.Shuffle();
        Stone(Milestone.Shuffled);
    }


    private void ProcessBlowCard(ResourceCard blowCard)
    {
        Stone(Milestone.Resource);

        if (blowCard.IsDiscovery)
        {
            if (AnyForcesIn(blowCard.Territory)) KillAllForcesIn(blowCard.Territory, false);

            var devouredResources = RemoveResources(blowCard.Territory);
            LogIf(devouredResources > 0, Payment.Of(devouredResources), " in ", blowCard.Territory, " is destroyed");

            var drawnToken = DiscoveryToken.None;
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

        var spiceFactor = SandTroutDoublesResources ? 2 : 1;
        var spiceAmount = spiceFactor * blowCard.Location.SpiceBlowAmount;

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

    private static MessagePart SandtroutMessage(bool SandTroutDoublesResources)
    {
        return MessagePart.ExpressIf(SandTroutDoublesResources, ", doubled by ", Concept.BabyMonster);
    }

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

    public bool MonsterAppearedInTerritoryWithoutForces { get; private set; }

    internal void LetMonsterAppear(Territory t, bool isGreatMonster)
    {
        var m = new MonsterAppearence(t, isGreatMonster);

        if (CurrentTurn != 1)
        {
            if (m.IsGreatMonster)
                Log(Concept.GreatMonster, " appears in ", m.Territory);
            else if (Monsters.Count > 0)
                Log(Concept.Monster, " appears a ", Natural(Monsters.Count + 1), " time during this ", MainPhase.Blow);
            else
                Log(Concept.Monster, " appears in ", m.Territory);

            if (!AnyForcesIn(m.Territory)) MonsterAppearedInTerritoryWithoutForces = true;

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

    internal void PerformMonster(MonsterAppearence m)
    {
        foreach (var l in m.Territory.Locations)
        foreach (var p in Players)
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

        var devouredResources = RemoveResources(m.Territory);
        LogIf(devouredResources > 0, m.DescribingConcept, " devours ", Payment.Of(devouredResources), " in ", m.Territory);

        FlipBeneGesseritWhenAlone();
    }

    internal void EnterBlowA()
    {
        Monsters.Clear();
        Enter(Phase.BlowA);
        LogIf(Applicable(Rule.IncreasedResourceFlow), "*** Spice Blow A ***");
        DrawResourceCard();
        LetFactionsDiscardSurplusCards();
    }

    internal void EnterBlowB()
    {
        Monsters.Clear();
        Enter(Phase.BlowB);
        Log("*** Spice Blow B ***");
        DrawResourceCard();
        LetFactionsDiscardSurplusCards();
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
                EnterBlowB();
            else
                StartNexusCardPhase();
        }
    }

    #endregion ResourceBlow

    #region EndOfResourceBlow

    internal void StartNexusCardPhase()
    {
        if (NexusHasOccured && Applicable(Rule.NexusCards) && Players.Any(p => p.HasAlly) && Players.Any(p => !p.HasAlly))
            EnterNexusCardPhase();
        else
            EndBlowPhase();

        if (Version >= 164)
        {
            NexusHasOccured = false;
        }
    }


    private void EnterNexusCardPhase()
    {
        if (Version < 164)
        {
            NexusHasOccured = false;
        }
        
        FactionsThatMayDrawNexusCard = Players.Where(p => !p.HasAlly).Select(p => p.Faction).ToList();
        FactionsThatDrewNexusCard.Clear();
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
        FactionsThatMayDrawNexusCard.Clear();
        CurrentYellowSecretAlly = null;
        HasActedOrPassed.Clear();
        ReshuffleIgnoredMonsters();
        MainPhaseEnd();
        Enter(Phase.BlowReport);
    }

    private void ReshuffleIgnoredMonsters()
    {
        if (IgnoredMonsters.Count > 0 || IgnoredSandtrout != null)
        {
            if (IgnoredMonsters.Count > 0)
            {
                Log(IgnoredMonsters.Count, " ignored ", Concept.Monster, " cards were shuffled back into the ", Concept.Resource, " deck");
                foreach (var c in IgnoredMonsters) ResourceCardDeck.Items.Add(c);
            }

            if (IgnoredSandtrout != null)
            {
                Log(Concept.BabyMonster, " card was shuffled back into the ", Concept.Resource, " deck");
                ResourceCardDeck.Items.Add(IgnoredSandtrout);
            }

            ResourceCardDeck.Shuffle();
            Stone(Milestone.Shuffled);
        }
    }

    #endregion EndOfResourceBlow

    #region Information

    public bool SandTroutOccured => SandTrout != null;

    public static string Natural(int count)
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
            _ => count + "th"
        };
    }

    private Deck<ResourceCard> CurrentDiscardPile => CurrentPhase == Phase.BlowA ? ResourceCardDiscardPileA : ResourceCardDiscardPileB;

    private ResourceCard PreviousBlowCard
    {
        get => CurrentPhase == Phase.BlowA ? LatestSpiceCardA : LatestSpiceCardB;
        set
        {
            if (CurrentPhase == Phase.BlowA)
                LatestSpiceCardA = value;
            else
                LatestSpiceCardB = value;
        }
    }

    public bool ProtectedFromMonster(Player p)
    {
        if (CurrentYellowSecretAlly != null && CurrentYellowSecretAlly.Player == p)
            return true;
        if (p.Is(Faction.Yellow))
            return !Prevented(FactionAdvantage.YellowProtectedFromMonster);
        return p.Ally == Faction.Yellow && YellowWillProtectFromMonster && !Prevented(FactionAdvantage.YellowProtectedFromMonsterAlly);
    }

    #endregion Information
}