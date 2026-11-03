/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
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

    internal bool ExtortionToBeReturned { get; set; }
    public Dictionary<Location, Faction> StrongholdOwnership { get; private set; } = new();
    public bool CyanHasPerformedTerror { get; internal set; }
    public List<Player> Winners { get; } = [];
    public WinMethod WinMethod { get; private set; }

    #endregion State

    #region Contemplate

    internal void EnterContemplatePhase()
    {
        foreach (var player in Players) player.TransferableResources = 0;

        MainPhaseStart(MainPhase.Contemplate, Version >= 103);
        AllowAllPreventedFactionAdvantages([]);
        HandleEconomics();
        if (Version >= 108) AddBribesToPlayerResources();
        CyanHasPerformedTerror = false;

        foreach (var p in Players)
            if (p.BankedResources > 0)
            {
                p.Resources += p.BankedResources;
                Log(p.Faction, " add ", Payment.Of(p.BankedResources), " received as ", LeaderSkill.Banker, " to their reserves");
                p.BankedResources = 0;
            }

        Enter(Version >= 103, EnterContemplatePause, ContinueContemplatePhase);
    }

    private void HandleEconomics()
    {
        switch (EconomicsStatus)
        {
            case BrownEconomicsStatus.Double:
                EconomicsStatus = BrownEconomicsStatus.CancelFlipped;
                Log(Faction.Brown, " The Economics Token flips to Cancel.");
                Stone(Milestone.Economics);
                break;

            case BrownEconomicsStatus.Cancel:
                EconomicsStatus = BrownEconomicsStatus.DoubleFlipped;
                Log(Faction.Brown, " The Economics Token flips to Double.");
                Stone(Milestone.Economics);
                break;

            case BrownEconomicsStatus.DoubleFlipped:
            case BrownEconomicsStatus.CancelFlipped:
                EconomicsStatus = BrownEconomicsStatus.RemovedFromGame;
                Log(Faction.Brown, " The Economics Token has been removed from the game.");
                Stone(Milestone.Economics);
                break;
        }
    }

    private void EnterContemplatePause()
    {
        DetermineIfCyanDrawsNewTraitor();
        ExtortionToBeReturned = Players.Any(p => p.Extortion > 0);
        GainExtortions();

        if (BlackTraitorWasCancelled)
        {
            BlackTraitorWasCancelled = false;
            GetPlayer(Faction.Black)?.Traitors.Add(TraitorDeck!.Draw());
            Log(Faction.Black, " drew a new traitor after having been betrayed");
        }

        Enter(ExtortionToBeReturned, Phase.Extortion, EndContemplatePause);
    }

    private void DetermineIfCyanDrawsNewTraitor()
    {
        var cyan = GetPlayer(Faction.Cyan);
        if (cyan == null) return;
        
        foreach (var l in Assassinated.Where(l => cyan.RevealedTraitors.Contains(l)).ToList())
        {
            Log(Faction.Cyan, " draw a new traitor to replace ", l);
            cyan.Traitors.Remove(l);
            cyan.RevealedTraitors.Remove(l);
            cyan.Traitors.Add(TraitorDeck!.Draw());
        }
    }

    internal void EndContemplatePause()
    {
        if (ExtortionToBeReturned)
        {
            Log(Faction.Cyan, " regain their ", TerrorType.Extortion, " token");
            UnplacedTerrorTokens.Add(TerrorType.Extortion);
        }

        Enter(Phase.Contemplate);
        DetermineOccupationAtStartOrEndOfTurn();
    }

    private void GainExtortions()
    {
        foreach (var p in Players.Where(p => p.Extortion > 0).ToList())
        {
            p.Resources += p.Extortion;
            Log(p.Faction, " add ", Payment.Of(p.Extortion), " gained from ", TerrorType.Extortion, " to their reserves");
            p.Extortion = 0;
        }
    }

    internal void ContinueContemplatePhase()
    {
        CheckNormalWin();
        CheckBluePrediction();
        CheckFinalTurnWin();

        if (Winners.Count > 0 || CurrentTurn == MaximumTurns)
        {
            CurrentMainPhase = MainPhase.Ended;
            Enter(Phase.GameEnded);
            Stone(Milestone.GameWon);
            Log("The game has ended.");

            foreach (var w in Winners) Log(w.Faction, " win!");
        }
        else
        {
            Enter(IsPlaying(Faction.Purple) && (Version < 113 || !Prevented(FactionAdvantage.PurpleReplacingFaceDancer)), Phase.ReplacingFaceDancer, Phase.TurnConcluded);
        }

        DetermineStrongholdOwnership();

        if (WhenToSetAsideVidal == VidalMoment.EndOfTurn) SetAsideVidal();

        MainPhaseEnd();
    }

    internal void AddBribesToPlayerResources()
    {
        foreach (var p in Players)
            if (p.Bribes > 0)
            {
                p.Resources += p.Bribes;
                Log(p.Faction, " add ", Payment.Of(p.Bribes), " from bribes to their reserves");
                p.Bribes = 0;
            }
    }

    private void DetermineStrongholdOwnership()
    {
        if (Applicable(Rule.StrongholdBonus))
        {
            DetermineStrongholdOwnership(Map.Arrakeen);
            DetermineStrongholdOwnership(Map.Carthag);
            DetermineStrongholdOwnership(Map.SietchTabr);
            DetermineStrongholdOwnership(Map.HabbanyaSietch);
            DetermineStrongholdOwnership(Map.TueksSietch);
            DetermineStrongholdOwnership(Map.HiddenMobileStronghold);
        }
    }

    private void DetermineStrongholdOwnership(Location location)
    {
        var currentOwner = StrongholdOwnership.GetValueOrDefault(location, Faction.None);
        var newOwningPlayer = Players.FirstOrDefault(p => p.Controls(this, location, Applicable(Rule.ContestedStongholdsCountAsOccupied)));
        var newOwner = newOwningPlayer?.Faction ?? Faction.None;

        if (currentOwner != newOwner)
        {
            if (currentOwner != Faction.None && newOwner != Faction.None)
            {
                StrongholdOwnership.Remove(location);
                StrongholdOwnership.Add(location, newOwner);
                Log(newOwner, " take control over ", location, " from ", currentOwner);
            }
            else if (currentOwner != Faction.None)
            {
                StrongholdOwnership.Remove(location);
                Log(currentOwner, " lose control over ", location);
            }
            else if (newOwner != Faction.None)
            {
                StrongholdOwnership.Add(location, newOwner);
                Log(newOwner, " take control over ", location);
            }
        }
    }

    #endregion Contemplate

    #region Victory

    private void CheckFinalTurnWin()
    {
        if (CurrentTurn != MaximumTurns) 
            return;
        
        if (Winners.Count == 0) CheckSpecialWinConditions();

        if (Winners.Count != 0) 
            return;
        
        CheckOtherWinConditions();
        CheckBluePrediction();
    }

    private void CheckBluePrediction()
    {
        var blue = GetPlayer(Faction.Blue);
        if (blue != null && blue.PredictedTurn == CurrentTurn && Winners.Any(w => w.Faction == blue.PredictedFaction))
        {
            Log(Faction.Blue, " predicted ", blue.PredictedFaction, " victory in turn ", blue.PredictedTurn, "! They had everything planned...");
            WinMethod = WinMethod.Prediction;
            Winners.Clear();
            Winners.Add(blue);
        }
    }

    private void CheckNormalWin()
    {
        if (Applicable(Rule.SE_Victory))
        {
            CheckSecondEditionNormalWin();
            return;
        }

        var checkWinSequence = new PlayerSequence(this);

        for (var i = 0; i < Players.Count; i++)
        {
            var p = checkWinSequence.CurrentPlayer;

            if (MeetsNormalVictoryCondition(p, Applicable(Rule.ContestedStongholdsCountAsOccupied)))
            {
                WinMethod = WinMethod.Strongholds;

                Winners.Add(p);

                var ally = GetPlayer(p.Ally);
                if (ally != null)
                {
                    Winners.Add(ally);
                    LogNormalWin(p, ally);
                }
            }

            if (Winners.Count > 0) break;

            checkWinSequence.NextPlayer();
        }
    }

    private void CheckSecondEditionNormalWin()
    {
        var sequence = new PlayerSequence(this);
        var sequenceOrder = new Dictionary<Player, int>();
        for (var i = 0; i < Players.Count; i++)
        {
            sequenceOrder[sequence.CurrentPlayer] = i;
            sequence.NextPlayer();
        }

        var candidates = new List<(Player[] Members, int Priority, int Strongholds, int Forces, int Spice, int Sequence)>();
        foreach (var player in Players)
        {
            var ally = player.AlliedPlayer;
            if (ally != null && player.Faction.CompareTo(ally.Faction) > 0) continue;

            var members = ally == null ? new[] { player } : new[] { player, ally };
            var strongholds = CountSecondEditionStrongholds(members);
            var hasAllTechTokens = members.Any(member => member.TechTokens.Count == 3);
            var priority = ally == null
                ? strongholds >= 3 ? 0 : strongholds >= 2 && hasAllTechTokens ? 1 : -1
                : strongholds >= 5 ? 2
                : strongholds >= 4 && hasAllTechTokens ? 3
                : strongholds >= 4 ? 4
                : strongholds >= 3 && hasAllTechTokens ? 5
                : -1;
            if (priority < 0) continue;

            candidates.Add((
                members,
                priority,
                strongholds,
                CountForcesInSecondEditionStrongholds(members),
                members.Sum(member => member.Resources),
                members.Min(member => sequenceOrder[member])));
        }

        var winner = candidates
            .OrderBy(candidate => candidate.Priority)
            .ThenByDescending(candidate => candidate.Strongholds)
            .ThenByDescending(candidate => candidate.Forces)
            .ThenByDescending(candidate => candidate.Spice)
            .ThenBy(candidate => candidate.Sequence)
            .FirstOrDefault();
        if (winner.Members == null) return;

        WinMethod = WinMethod.Strongholds;
        Winners.AddRange(winner.Members);
        Log(string.Join(" and ", winner.Members.Select(member => member.Faction)), " have the highest-priority Second Edition victory");
    }

    private int CountSecondEditionStrongholds(IReadOnlyCollection<Player> players)
        => Strongholds.Count(location => players.Any(player => player.Controls(this, location, Applicable(Rule.ContestedStongholdsCountAsOccupied))));

    private int CountForcesInSecondEditionStrongholds(IReadOnlyCollection<Player> players)
        => Strongholds
            .Where(location => players.Any(player => player.Controls(this, location, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
            .Sum(location => players.Sum(player => player.ForcesIn(location) + player.SpecialForcesIn(location)));

    private void LogNormalWin(Player player, Player? ally)
    {
        if (Players.Any(p => !Winners.Contains(p) && MeetsNormalVictoryCondition(p, Applicable(Rule.ContestedStongholdsCountAsOccupied))))
        {
            if (ally == null)
                Log(player.Faction, " are the first in front of the storm with enough victory points to win the game");
            else
                Log(player.Faction, " and ", player.Ally, " are the first in front of the storm with enough victory points to win the game");
        }
        else
        {
            if (ally == null)
                Log(player.Faction, " have enough victory points to win the game");
            else
                Log(player.Faction, " and ", player.Ally, " have enough victory points to win the game");
        }
    }

    public bool MeetsNormalVictoryCondition(Player p, bool contestedStrongholdsCountAsOccupied)
    {
        return
            MeetsPinkVictoryCondition(p, contestedStrongholdsCountAsOccupied) ||
            MeetsHighThresholdPinkVictoryCondition(p, contestedStrongholdsCountAsOccupied) ||
            NumberOfVictoryPoints(p, contestedStrongholdsCountAsOccupied) >= ThresholdForWin(p);
    }

    public int CountChallengedVictoryPoints(Player p)
    {
        return NumberOfVictoryPoints(p, true) - NumberOfVictoryPoints(p, false);
    }

    public int ThresholdForWin(Player p)
    {
        if (p.Ally != Faction.None)
            return 4;
        return Players.Count <= 2 ? 4 : 3;
    }

    private IEnumerable<Territory> Strongholds => Map.Territories(false).Where(t => t.IsStronghold || IsSpecialStronghold(t));

    private bool MeetsPinkVictoryCondition(Player p, bool contestedStrongholdsCountAsOccupied)
    {
        return ((p.Is(Faction.Pink) && p.HasAlly) || p.Ally == Faction.Pink) &&
               Strongholds.Count(t =>
                   p.Controls(this, t, contestedStrongholdsCountAsOccupied) &&
                   p.AlliedPlayer!.Controls(this, t, contestedStrongholdsCountAsOccupied)) >= 3;
    }

    private bool MeetsHighThresholdPinkVictoryCondition(Player p, bool contestedStrongholdsCountAsOccupied)
    {
        return HasHighThreshold(Faction.Pink) &&
               ((p.Is(Faction.Pink) && p.HasAlly) || p.Ally == Faction.Pink) &&
               Strongholds.Any(t =>
                   p.Controls(this, t, contestedStrongholdsCountAsOccupied) &&
                   p.AlliedPlayer!.Controls(this, t, contestedStrongholdsCountAsOccupied)) &&
               HomeworldOccupation.Values.Count(f => f == p.Faction || f == p.Ally) >= 2;
    }

    public int NumberOfVictoryPoints(Player p, bool contestedStrongholdsCountAsOccupied)
    {
        if (p.AlliedPlayer != null)
        {
            var techTokenPoint = p.TechTokens.Count == 3 || p.AlliedPlayer.TechTokens.Count == 3 ? 1 : 0;
            return techTokenPoint + Map.Territories(false)
                .Where(t => t.IsStronghold 
                            || IsSpecialStronghold(t)).Count(l => p.Controls(this, l, contestedStrongholdsCountAsOccupied) 
                                                                  || p.AlliedPlayer.Controls(this, l, contestedStrongholdsCountAsOccupied));
        }
        else
        {
            var techTokenPoint = p.TechTokens.Count == 3 ? 1 : 0;
            return techTokenPoint + NumberOfOccupiedStrongholds(p, contestedStrongholdsCountAsOccupied);
        }
    }

    private int NumberOfOccupiedStrongholds(Player p, bool contestedStrongholdsCountAsOccupied)
    {
        return Map.Territories(false).Where(t => t.IsStronghold || IsSpecialStronghold(t))
            .Count(l => p.Controls(this, l, contestedStrongholdsCountAsOccupied));
    }

    private void CheckSpecialWinConditions()
    {
        var yellow = GetPlayer(Faction.Yellow);
        var orange = GetPlayer(Faction.Orange);

        if (Applicable(Rule.SE_Victory))
        {
            if (yellow != null && SecondEditionYellowVictoryConditionMet)
            {
                Log(Faction.Yellow, " special victory conditions are met!");
                WinMethod = WinMethod.YellowSpecial;
                Winners.Add(yellow);
                if (yellow.HasAlly) Winners.Add(yellow.AlliedPlayer);
            }
            else if (orange != null && SecondEditionGuildVictoryConditionMet(orange))
            {
                Log(Faction.Orange, " special victory conditions are met!");
                WinMethod = WinMethod.GuildSpecial;
                Winners.Add(orange);
                if (orange.HasAlly) Winners.Add(orange.AlliedPlayer);
            }
            return;
        }

        if (yellow != null && YellowVictoryConditionMet)
        {
            Log(Faction.Yellow, " special victory conditions are met!");
            WinMethod = WinMethod.YellowSpecial;
            Winners.Add(yellow);
            if (yellow.HasAlly) Winners.Add(yellow.AlliedPlayer);
        }
        else if (orange != null && !Applicable(Rule.DisableOrangeSpecialVictory))
        {
            Log(Faction.Orange, " special victory conditions are met!");
            WinMethod = WinMethod.OrangeSpecial;
            Winners.Add(orange);
            if (orange.HasAlly) Winners.Add(orange.AlliedPlayer);
        }
        else if (yellow != null && !Applicable(Rule.DisableOrangeSpecialVictory))
        {
            Log(Faction.Yellow, " win because ", Faction.Orange, " are not playing and no one else won");
            WinMethod = WinMethod.OrangeSpecial;
            Winners.Add(yellow);
            if (yellow.HasAlly) Winners.Add(yellow.AlliedPlayer);
        }
    }

    private bool SecondEditionYellowVictoryConditionMet
    {
        get
        {
            var yellow = GetPlayer(Faction.Yellow);
            if (yellow == null) return false;

            var protectedLocations = new[] { Map.SietchTabr, Map.HabbanyaSietch };
            if (Players.Any(player => player.Faction != Faction.Yellow && player.Faction != yellow.Ally &&
                                      protectedLocations.Any(player.Occupies))) return false;

            var restrictedFactions = new[] { Faction.Green, Faction.Black, Faction.Grey, Faction.Red };
            return !Players.Any(player => restrictedFactions.Contains(player.Faction) &&
                                          player.Faction != yellow.Ally && player.Occupies(Map.TueksSietch));
        }
    }

    private bool SecondEditionGuildVictoryConditionMet(Player guild)
    {
        var guildSide = guild.AlliedPlayer == null ? new[] { guild } : new[] { guild, guild.AlliedPlayer };
        var guildStrongholds = CountSecondEditionStrongholds(guildSide);

        var opponents = Players.Where(player => !guildSide.Contains(player)).ToList();
        var opponentSides = new List<Player[]>();
        if (guild.AlliedPlayer == null)
        {
            opponentSides.AddRange(opponents.Select(opponent => new[] { opponent }));
        }
        else
        {
            foreach (var opponent in opponents)
            {
                var ally = opponent.AlliedPlayer;
                if (ally != null && !guildSide.Contains(ally) && opponent.Faction.CompareTo(ally.Faction) > 0) continue;
                opponentSides.Add(ally != null && !guildSide.Contains(ally) ? [opponent, ally] : [opponent]);
            }
        }

        var highestOpponentStrongholds = opponentSides
            .Select(CountSecondEditionStrongholds)
            .DefaultIfEmpty(0)
            .Max();
        return guildStrongholds >= highestOpponentStrongholds;
    }

    public bool YellowVictoryConditionMet
    {
        get
        {
            var sietchTabrOccupiedByOtherThanYellow = Players.Any(p => p.Faction != Faction.Yellow && (p.Faction != Faction.Pink || p.Ally != Faction.Yellow) && p.Occupies(Map.SietchTabr));
            var habbanyaSietchOccupiedByOtherThanYellow = Players.Any(p => p.Faction != Faction.Yellow && (p.Faction != Faction.Pink || p.Ally != Faction.Yellow) && p.Occupies(Map.HabbanyaSietch));
            var tueksSietchOccupiedByGreenOrBlackOrRedOrWhite = Players.Any(p => (p.Is(Faction.Green) || p.Is(Faction.Black) || p.Is(Faction.Red) || p.Is(Faction.White)) && p.Occupies(Map.TueksSietch));

            return CurrentTurn == MaximumTurns && !sietchTabrOccupiedByOtherThanYellow && !habbanyaSietchOccupiedByOtherThanYellow && !tueksSietchOccupiedByGreenOrBlackOrRedOrWhite;
        }
    }

    private void CheckOtherWinConditions()
    {
        if (Applicable(Rule.SE_Victory))
        {
            CheckSecondEditionFinalTurnWin();
            return;
        }

        Player? withMostPoints = null;
        var highestNumberOfPoints = -1;
        var checkWinSequence = new PlayerSequence(this);

        for (var i = 0; i < Players.Count; i++)
        {
            var p = checkWinSequence.CurrentPlayer;

            var pointsOfPlayer = NumberOfVictoryPoints(p, Applicable(Rule.ContestedStongholdsCountAsOccupied));
            if (pointsOfPlayer > highestNumberOfPoints)
            {
                withMostPoints = p;
                highestNumberOfPoints = pointsOfPlayer;
            }

            checkWinSequence.NextPlayer();
        }

        if (withMostPoints != null)
        {
            Winners.Add(withMostPoints);
            WinMethod = WinMethod.Strongholds;
            Log(withMostPoints.Faction, " are the first after the storm with most victory points");

            if (withMostPoints.HasAlly) Winners.Add(withMostPoints.AlliedPlayer);
        }
    }

    private void CheckSecondEditionFinalTurnWin()
    {
        var sequence = new PlayerSequence(this);
        var sequenceOrder = new Dictionary<Player, int>();
        for (var i = 0; i < Players.Count; i++)
        {
            sequenceOrder[sequence.CurrentPlayer] = i;
            sequence.NextPlayer();
        }

        var candidates = new List<(Player[] Members, int Strongholds, bool Unallied, int Forces, int Spice, int Sequence)>();
        foreach (var player in Players)
        {
            var ally = player.AlliedPlayer;
            if (ally != null && player.Faction.CompareTo(ally.Faction) > 0) continue;
            var members = ally == null ? new[] { player } : new[] { player, ally };
            candidates.Add((
                members,
                CountSecondEditionStrongholds(members),
                ally == null,
                CountForcesInSecondEditionStrongholds(members),
                members.Sum(member => member.Resources),
                members.Min(member => sequenceOrder[member])));
        }

        var winner = candidates
            .OrderByDescending(candidate => candidate.Strongholds)
            .ThenByDescending(candidate => candidate.Unallied)
            .ThenByDescending(candidate => candidate.Forces)
            .ThenByDescending(candidate => candidate.Spice)
            .ThenBy(candidate => candidate.Sequence)
            .First();
        Winners.AddRange(winner.Members);
        WinMethod = WinMethod.Strongholds;
        Log(string.Join(" and ", winner.Members.Select(member => member.Faction)), " win by Second Edition end-of-round priorities");
    }

    #endregion Victory

    #region Information

    public bool HasStrongholdAdvantage(Faction f, StrongholdAdvantage advantage, Territory battleTerritory)
    {
        if (battleTerritory == Map.HiddenMobileStronghold.Territory && OwnsStronghold(f, Map.HiddenMobileStronghold) && ChosenHmsAdvantage == advantage) return true;

        return battleTerritory.Advantage == advantage && battleTerritory.Locations.Any(l => OwnsStronghold(f, l));
    }

    public bool OwnsStronghold(Faction f, Location stronghold)
    {
        return StrongholdOwnership.TryGetValue(stronghold, out var owner) && owner == f;
    }

    public bool IsSpecialStronghold(Territory t)
    {
        return t == Map.ShieldWall && Applicable(Rule.Ssw) && NumberOfMonsters >= 4;
    }

    public IEnumerable<TerrorType> TerrorIn(Territory? t)
    {
        return t == null 
            ? [] 
            : TerrorOnPlanet.Where(kvp => kvp.Value == t).Select(kvp => kvp.Key);
    }

    #endregion Information
}