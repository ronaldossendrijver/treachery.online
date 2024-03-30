/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public partial class Game
{
    #region State

    public List<FactionTradeOffered> CurrentTradeOffers { get; } = new();
    internal Phase PhaseBeforeSkillAssignment { get; set; }
    public Faction NextFactionToPerformCustomSetup => Players.Select(p => p.Faction).Where(f => !HasActedOrPassed.Contains(f)).FirstOrDefault();
    public Deck<TreacheryCard> StartingTreacheryCards { get; private set; }
    private TreacheryCard ExtraStartingCardForBlack { get; set; }

    #endregion State

    #region Setup

    private void EnterPhaseAwaitingPlayers()
    {
        Players = new List<Player>();
        CurrentTurn = 0;
        Enter(Phase.AwaitingPlayers);
    }

    internal void AssignFactionsAndEnterFactionTrade()
    {
        var inPlay = new Deck<Faction>(FactionsInPlay, Random);
        Stone(Milestone.Shuffled);
        inPlay.Shuffle();

        foreach (var p in Players.Where(p => p.Faction == Faction.None)) p.Faction = inPlay.Draw();

        DeterminePositionsAtTable();

        Enter(Applicable(Rule.CustomDecks) && Version < 134, Phase.CustomizingDecks, EnterPhaseTradingFactions);
    }

    private void DeterminePositionsAtTable()
    {
        if (Players.Count <= MaximumNumberOfPlayers)
        {
            var positions = new Deck<int>(Random);
            for (var i = 0; i < MaximumNumberOfPlayers; i++) positions.PutOnTop(i);
            positions.Shuffle();

            foreach (var p in Players) p.PositionAtTable = positions.Draw();
        }
        else
        {
            throw new ArgumentException("Number of players cannot exceed number of positions at the table.");
        }
    }

    internal void EnterPhaseTradingFactions()
    {
        CurrentTradeOffers.Clear();
        Enter(Phase.TradingFactions);
    }

    internal void EnterSetupPhase()
    {
        Enter(Applicable(Rule.CustomDecks) && Version >= 134, Phase.CustomizingDecks, EnterPhaseTradingFactions);

        CurrentTradeOffers.Clear();

        HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).First().Faction);
        HasBattleWheel.Add(Players.OrderBy(p => p.PositionAtTable).Last().Faction);

        foreach (var p in Players) p.AssignLeaders(this);

        var pink = GetPlayer(Faction.Pink);
        if (pink != null)
        {
            AssignInitialAmbassadors(pink);
            Log(Faction.Pink, " get ", Ambassador.Pink, " and draw 4 random ambassadors");

            if (Applicable(Rule.PinkLoyalty))
            {
                PinkLoyalLeader = pink.Leaders.RandomOrDefault(Random);
                Log(PinkLoyalLeader, " is forever loyal to ", Faction.Pink);
            }
        }

        Enter(IsPlaying(Faction.Blue), Phase.BluePredicting, TreacheryCardsBeforeTraitors, DealStartingTreacheryCards, DealTraitors);
    }

    internal void DealTraitors()
    {
        Stone(Milestone.Shuffled);
        TraitorDeck = CreateAndShuffleTraitorDeck(Random);

        if (Applicable(Rule.BlackMulligan) && IsPlaying(Faction.Black))
        {
            DealBlackTraitorCards();
            Enter(Phase.BlackMulligan);
        }
        else
        {
            for (var i = 1; i <= 4; i++)
                foreach (var p in Players.Where(p => p.Faction != Faction.Purple)) p.Traitors.Add(TraitorDeck.Draw());

            EnterSelectTraitors();
        }
    }

    private Deck<IHero> CreateAndShuffleTraitorDeck(Random random)
    {
        var result = new Deck<IHero>(TraitorsInPlay, random);

        if (PinkLoyalLeader != null)
        {
            result.Items.Remove(PinkLoyalLeader);
            foreach (var p in Players) p.KnownNonTraitors.Add(PinkLoyalLeader);
        }

        if (Vidal != null)
            foreach (var p in Players) p.KnownNonTraitors.Add(Vidal);

        result.Shuffle();
        return result;
    }

    internal void DealBlackTraitorCards()
    {
        var black = GetPlayer(Faction.Black);
        for (var i = 1; i <= 4; i++) black.Traitors.Add(TraitorDeck.Draw());
    }

    internal void EnterSelectTraitors()
    {
        HasActedOrPassed.Clear();

        if (IsPlaying(Faction.Black)) HasActedOrPassed.Add(Faction.Black);

        if (IsPlaying(Faction.Purple)) HasActedOrPassed.Add(Faction.Purple);

        Enter(Players.Any(p => !(p.Is(Faction.Black) || p.Is(Faction.Purple))), Phase.SelectingTraitors, AssignFaceDancers);
    }

    internal void AssignFaceDancers()
    {
        var purple = GetPlayer(Faction.Purple);
        if (purple != null)
        {
            TraitorDeck.Shuffle();
            for (var i = 0; i < 3; i++)
            {
                var leader = TraitorDeck.Draw();
                purple.FaceDancers.Add(leader);
                purple.KnownNonTraitors.Add(leader);
            }
        }

        Enter(TreacheryCardsBeforeTraitors, SetupSpiceAndForces, AssignLeaderSkills);
    }

    private void AssignLeaderSkills()
    {
        if (Applicable(Rule.LeaderSkills))
        {
            SkillDeck = new Deck<LeaderSkill>(Enumerations.GetValuesExceptDefault(typeof(LeaderSkill), LeaderSkill.None), Random);
            SkillDeck.Shuffle();
            Stone(Milestone.Shuffled);

            var nrOfSkillsToAssign = Players.Count <= 7 ? 2 : 1;
            for (var i = 0; i < nrOfSkillsToAssign; i++)
                foreach (var p in Players) p.SkillsToChooseFrom.Add(SkillDeck.Draw());

            Enter(Phase.AssigningInitialSkills);
        }
        else
        {
            Enter(TreacheryCardsBeforeTraitors, DealTraitors, SetupSpiceAndForces);
        }
    }

    internal void SetupSpiceAndForces()
    {
        foreach (var p in Players) SetupPlayerHomeworld(p);

        if (Applicable(Rule.CustomInitialForcesAndResources))
        {
            HasActedOrPassed.Clear();
            Enter(Phase.PerformCustomSetup);
        }
        else
        {
            foreach (var p in Players) SetupPlayerSpiceAndForcesOnPlanet(p);

            Action methodAfterSettingUp;
            if (TreacheryCardsBeforeTraitors)
                methodAfterSettingUp = EnterStormPhase;
            else
                methodAfterSettingUp = DealStartingTreacheryCards;

            Enter(
                IsPlaying(Faction.Yellow), Phase.YellowSettingUp,
                IsPlaying(Faction.Blue) && PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(this), Phase.BlueSettingUp,
                IsPlaying(Faction.Cyan), Phase.CyanSettingUp,
                methodAfterSettingUp);
        }
    }

    private void SetupPlayerHomeworld(Player p)
    {
        var normalForceWorld = Map.Homeworlds.First(w => w.Faction == p.Faction && w.IsHomeOfNormalForces);

        switch (p.Faction)
        {
            case Faction.Yellow:
                p.InitializeHomeworld(normalForceWorld, Applicable(Rule.YellowSpecialForces) ? 17 : 20, Applicable(Rule.YellowSpecialForces) ? 3 : 0);
                break;

            case Faction.Red:
                p.InitializeHomeworld(normalForceWorld, Applicable(Rule.RedSpecialForces) ? 15 : 20, 0);
                if (Applicable(Rule.RedSpecialForces)) p.InitializeHomeworld(Map.Homeworlds.First(w => w.Faction == p.Faction && w.IsHomeOfSpecialForces), 0, 5);

                break;

            case Faction.Grey:
                p.InitializeHomeworld(normalForceWorld, 13, 7);
                break;

            case Faction.Green:
            case Faction.Black:
            case Faction.Orange:
            case Faction.Blue:
            case Faction.Purple:
            case Faction.Brown:
            case Faction.White:
            case Faction.Pink:
            case Faction.Cyan:
                p.InitializeHomeworld(normalForceWorld, 20, 0);
                break;
        }
    }

    private void SetupPlayerSpiceAndForcesOnPlanet(Player p)
    {
        switch (p.Faction)
        {
            case Faction.Yellow:
                p.Resources = 3;
                break;

            case Faction.Green:
                p.Resources = 10;
                p.AddForces(Map.Arrakeen, 10, true);
                break;

            case Faction.Black:
                p.Resources = 10;
                p.AddForces(Map.Carthag, 10, true);
                break;

            case Faction.Red:
                p.Resources = 10;
                break;

            case Faction.Orange:
                p.Resources = 5;
                p.AddForces(Map.TueksSietch, 5, true);
                break;

            case Faction.Blue:
                p.Resources = 5;
                if (!PerformBluePlacement.BlueMayPlaceFirstForceInAnyTerritory(this)) p.AddForces(Map.PolarSink, 1, true);
                break;

            case Faction.Grey:
                p.Resources = 10;
                p.AddForces(Map.HiddenMobileStronghold, 3, true);
                p.AddSpecialForces(Map.HiddenMobileStronghold, 3, true);
                break;

            case Faction.Purple:
                p.Resources = 5;
                break;

            case Faction.Brown:
                p.Resources = 2;
                break;

            case Faction.White:
                p.Resources = 5;
                break;

            case Faction.Pink:
                p.Resources = 12;
                p.AddForces(Map.ImperialBasin.MiddleLocation, 6, true);
                break;

            case Faction.Cyan:
                p.Resources = 12;
                break;
        }
    }

    private void AssignInitialAmbassadors(Player p)
    {
        p.Ambassadors.Add(Ambassador.Pink);
        UnassignedAmbassadors.Items.Remove(Ambassador.Pink);
        Log(p.Faction, " receive the ", Faction.Pink, " ambassador");
        AssignRandomAmbassadors(p);
    }

    internal void AssignRandomAmbassadors(Player p)
    {
        foreach (var item in AmbassadorsSetAside) UnassignedAmbassadors.Items.Add(item);
        AmbassadorsSetAside.Clear();

        UnassignedAmbassadors.Shuffle();
        Stone(Milestone.Shuffled);
        Log(p.Faction, " draw 5 random Ambassadors");
        for (var i = 0; i < 5; i++) p.Ambassadors.Add(UnassignedAmbassadors.Draw());
    }

    internal void DealStartingTreacheryCards()
    {
        StartingTreacheryCards = new Deck<TreacheryCard>(Random);
        foreach (var p in Players)
        {
            StartingTreacheryCards.Items.Add(TreacheryDeck.Draw());

            if (p.Is(Faction.Black)) ExtraStartingCardForBlack = TreacheryDeck.Draw();
        }

        Enter(IsPlaying(Faction.Grey), Phase.GreySelectingCard, DealRemainingStartingTreacheryCardsToNonGrey);
    }

    internal void DealRemainingStartingTreacheryCardsToNonGrey()
    {
        foreach (var p in Players.Where(p => p.Faction != Faction.Grey))
        {
            var card = StartingTreacheryCards.Draw();
            p.TreacheryCards.Add(card);
            LogTo(p.Faction, "Your starting treachery card is: ", card);

            if (p.Is(Faction.Black))
            {
                p.TreacheryCards.Add(ExtraStartingCardForBlack);
                LogTo(Faction.Black, "Your extra card is: ", ExtraStartingCardForBlack);
            }
        }

        Enter(TreacheryCardsBeforeTraitors, AssignLeaderSkills, EnterStormPhase);
    }

    internal bool TreacheryCardsBeforeTraitors => Version >= 121 && Applicable(Rule.LeaderSkills);

    #endregion Setup
}