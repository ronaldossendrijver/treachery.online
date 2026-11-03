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

    public int HmsMovesLeft { get; internal set; }
    public List<Faction> HasBattleWheel { get; } = [];
    internal List<int> Dials { get; } = [];
    public bool CurrentTestingStationUsed { get; internal set; }
    internal Phase PhaseBeforeStormLoss { get; private set; }
    public List<LossToTake> LossesToTake { get; } = [];
    internal Deck<StormCard>? StormDeck { get; set; }
    public List<StormCard> StormDiscardPile { get; } = [];
    public StormCard CurrentStormCard { get; internal set; }
    public int StormCardsRemaining => StormDeck?.Items.Count ?? 0;
    internal bool StormInsightReady { get; set; }
    internal bool WeatherControlledStorm { get; set; }
    internal Faction LastStormResponse { get; set; }

    #endregion State

    #region Storm

    internal void EnterStormPhase()
    {
        CurrentTurn++;
        MainPhaseStart(MainPhase.Storm, CurrentTurn > 1);
        FactionsThatRevivedSpecialForcesThisTurn.Clear();
        LossesToTake.Clear();
        CurrentTestingStationUsed = false;

        if (CurrentTurn == 1)
        {
            Dials.Clear();
            HasActedOrPassed.Clear();
            Enter(Phase.DiallingStorm);
        }
        else
        {
            if (JustRevealedDiscoveryStrongholds.Any())
                Enter(Phase.BeginningOfStorm);
            else
                MoveHmsBeforeDiallingStorm();
        }

        DetermineOccupationAtStartOrEndOfTurn();
    }

    internal void MoveHmsBeforeDiallingStorm()
    {
        var grey = GetPlayer(Faction.Grey);
        if (grey != null && grey.AnyForcesIn(Map.HiddenMobileStronghold) > 0)
        {
            if (Prevented(FactionAdvantage.GreyMovingHms))
            {
                LogPreventionByKarma(FactionAdvantage.GreyMovingHms);
                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.GreyMovingHms);
                DetermineStorm();
            }
            else if (HasLowThreshold(Faction.Grey))
            {
                LogPreventionByLowThreshold(FactionAdvantage.GreyMovingHms);
                DetermineStorm();
            }
            else
            {
                if (Map.HiddenMobileStronghold.AttachedToLocation?.Sector == SectorInStorm)
                {
                    Log("The storm prevents ", Map.HiddenMobileStronghold, " from moving");
                    DetermineStorm();
                }
                else
                {
                    HmsMovesLeft = 3;
                    Enter(Phase.HmsMovement);
                }
            }
        }
        else
        {
            DetermineStorm();
        }
    }

    internal int DetermineLaterStormWithStormDeck()
    {
        return Random!.Next(6) + 1;
    }

    internal void DetermineStorm()
    {
        if (Applicable(Rule.SE_StormDeck))
        {
            CurrentStormCard = StormCard.None;
            WeatherControlledStorm = false;
            LastStormResponse = Faction.None;
            NextStormMoves = -1;
            Enter(Phase.BeforeStormCard);
            return;
        }
        if (UseStormDeck)
        {
            RevealStorm();
        }
        else
        {
            Dials.Clear();
            HasActedOrPassed.Clear();
            Enter(Phase.DiallingStorm);
        }
    }

    internal void RevealStorm()
    {
        HasActedOrPassed.Clear();
        Log("The storm will move ", NextStormMoves, " sectors...");
        Enter(Phase.MetheorAndStormSpell);
    }

    internal void CollectSpiceFrom(Faction faction, Location l, int maximumAmount)
    {
        if (!ResourcesOnPlanet.TryGetValue(l, out var value)) return;
        
        var collected = Math.Min(value, maximumAmount);
        ChangeResourcesOnPlanet(l, -collected);
        var collector = GetPlayer(faction);
        if (collector == null) return;
        collector.Resources += collected;
        Log(faction, " collect ", Payment.Of(collected), " from ", l);
    }

    internal void MoveStormAndDetermineNext(int amount)
    {
        if (amount == 0 && !Applicable(Rule.SE_StormDeck)) PerformStorm();

        for (var i = 0; i < amount; i++)
        {
            SectorInStorm = (SectorInStorm + 1) % Map.NumberOfSectors;
            PerformStorm();
        }

        if (UseStormDeck && !Applicable(Rule.SE_StormDeck)) NextStormMoves = DetermineLaterStormWithStormDeck();

        Stone(Milestone.Storm);
    }

    internal void PerformStorm()
    {
        foreach (var l in Forces().Where(f => f.Key.Sector == SectorInStorm && !IsProtectedFromStorm(f.Key)))
        foreach (var battalion in l.Value)
        {
            var player = GetPlayer(battalion.Faction);
            if (player is null) continue;

            var killCount = 0;
            if (battalion.Is(Faction.Yellow) && Applicable(Rule.YellowStormLosses))
            {
                if (!Prevented(FactionAdvantage.YellowProtectedFromStorm))
                {
                    LossesToTake.Add(new LossToTake { Location = l.Key, Amount = TakeLosses.HalfOf(battalion.AmountOfForces, battalion.AmountOfSpecialForces), Faction = battalion.Faction });
                }
                else
                {
                    killCount += player.AnyForcesIn(l.Key);
                    player.KillAllForces(l.Key, false);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.YellowProtectedFromStorm);
                }
            }
            else if (battalion.Is(Faction.Brown) && (!Prevented(FactionAdvantage.BrownDiscarding) || Applicable(Rule.NexusCards)))
            {
                LossesToTake.Add(new LossToTake { Location = l.Key, Amount = battalion.TotalAmountOfForces, Faction = battalion.Faction });
            }
            else
            {
                RevealCurrentNoField(player, l.Key);
                killCount += player.AnyForcesIn(l.Key);
                player.KillAllForces(l.Key, false);
            }

            if (killCount > 0) Log("The storm kills ", killCount, battalion.Faction, " forces in ", l.Key);
        }

        if (Version >= 163) FlipBlueAdvisorsWhenAlone();

        foreach (var t in Strongholds.Where(s => s.Locations.Any(l => l.Sector == SectorInStorm && !IsProtectedFromStorm(l))))
        {
            var ambassador = AmbassadorIn(t);
            if (ambassador != Ambassador.None)
            {
                var pink = GetPlayer(Faction.Pink);
                if (pink is null) continue;
                AmbassadorsOnPlanet.Remove(t);
                pink.Ambassadors.Add(ambassador);
                Log("The ambassador in ", t, " returns to ", Faction.Pink);
            }
        }

        foreach (var l in Map.Locations(false).Where(l => l.Sector == SectorInStorm))
        {
            var removed = RemoveResources(l);
            if (removed > 0) Log("The storm destroys ", Payment.Of(removed), " in ", l);
        }
    }

    internal void EndStormPhase()
    {
        if (Applicable(Rule.SE_StormDeck))
        {
            StormInsightReady = true;
            NextStormMoves = -1;
        }
        JustRevealedDiscoveryStrongholds.Clear();

        MainPhaseEnd();

        if (LossesToTake.Count > 0)
        {
            PhaseBeforeStormLoss = Phase.BlowA;
            Enter(Phase.StormLosses);
        }
        else
        {
            if (Version >= 103)
            {
                if (Version >= 132) MainPhaseEnd();
                Enter(Phase.StormReport);
            }
            else
            {
                EnterSpiceBlowPhase();
            }
        }
    }

    #endregion Storm

    #region Information

    internal bool UseStormDeck => Applicable(Rule.SE_StormDeck) || Applicable(Rule.StormDeckWithoutYellow) ||
        (IsPlaying(Faction.Yellow) && Applicable(Rule.YellowDeterminesStorm));

    internal void CreateSecondEditionStormDeck()
    {
        StormDeck = new Deck<StormCard>(Enumerations.GetValuesExceptDefault(StormCard.None), Random!);
        StormDeck.Shuffle();
        Stone(Milestone.Shuffled);
    }

    public StormCard GetKnownNextStormCard(Player player)
        => Applicable(Rule.SE_StormDeck) && StormInsightReady && CurrentPhase != Phase.StormLosses &&
           HasStormPrescience(player) ? StormDeck!.Top : StormCard.None;

    public int GetKnownNextStormMoves(Player player)
    {
        if (!Applicable(Rule.SE_StormDeck)) return HasStormPrescience(player) ? NextStormMoves : -1;
        return GetKnownNextStormCard(player) switch
        {
            >= StormCard.Zero and <= StormCard.Six => (int)GetKnownNextStormCard(player) - (int)StormCard.Zero,
            StormCard.NextFactionMarker => (NextFactionMarkerSector() - SectorInStorm + Map.NumberOfSectors) % Map.NumberOfSectors,
            _ => -1
        };
    }

    public bool KnownNextStormWillPassOver(Player player, Location location)
    {
        if (GetKnownNextStormCard(player) == StormCard.NextFactionMarker)
            return location.Sector == NextFactionMarkerSector();
        return NextStormWillPassOver(location, GetKnownNextStormMoves(player));
    }

    private int NextFactionMarkerSector()
    {
        var next = Players.OrderBy(p =>
        {
            var distance = ((double)p.Seat * Map.NumberOfSectors / MaximumPlayers -
                            SectorInStorm + Map.NumberOfSectors) % Map.NumberOfSectors;
            return distance == 0 ? Map.NumberOfSectors : distance;
        }).First();
        return (int)Math.Ceiling((double)next.Seat * Map.NumberOfSectors / MaximumPlayers) % Map.NumberOfSectors;
    }

    public bool MayRespondToSecondEditionStorm(Player player)
        => !Applicable(Rule.SE_CardTiming) || LastStormResponse == Faction.None ||
           (GetPlayer(LastStormResponse) is { } previous && PlayerSequence.IsAfter(this, player, previous));

    internal void DrawSecondEditionStormCard()
    {
        if (WeatherControlledStorm)
        {
            ResolveSecondEditionStorm();
            return;
        }
        StormInsightReady = false;
        CurrentStormCard = StormDeck!.Draw();
        StormDiscardPile.Insert(0, CurrentStormCard);
        Log("The Storm card is ", CurrentStormCard);
        if (CurrentStormCard == StormCard.Dial)
        {
            if (NrOfBattlesFought == 0)
            {
                var candidates = Players.Where(p => (double)p.Seat * Map.NumberOfSectors / MaximumPlayers != SectorInStorm)
                    .OrderBy(p => ((double)p.Seat * Map.NumberOfSectors / MaximumPlayers -
                                  SectorInStorm + Map.NumberOfSectors) % Map.NumberOfSectors).ToList();
                if (candidates.Count < 2) candidates = Players.OrderBy(p => p.Seat).ToList();
                HasBattleWheel.Clear();
                HasBattleWheel.Add(candidates.First().Faction);
                HasBattleWheel.Add(candidates.Last().Faction);
            }
            Dials.Clear();
            HasActedOrPassed.Clear();
            Enter(Phase.DiallingStorm);
        }
        else
        {
            NextStormMoves = CurrentStormCard == StormCard.NextFactionMarker
                ? (NextFactionMarkerSector() - SectorInStorm + Map.NumberOfSectors) % Map.NumberOfSectors
                : (int)CurrentStormCard - (int)StormCard.Zero;
            RevealStorm();
        }
    }

    internal void ResolveSecondEditionStorm()
    {
        Log("The storm moves ", NextStormMoves, " sectors");
        if (!WeatherControlledStorm && CurrentStormCard == StormCard.NextFactionMarker)
        {
            SectorInStorm = NextFactionMarkerSector();
            PerformStorm();
            Stone(Milestone.Storm);
        }
        else MoveStormAndDetermineNext(NextStormMoves);
        if (!WeatherControlledStorm && CurrentStormCard is StormCard.Three or StormCard.Four or StormCard.Five)
        {
            StormDeck!.Items.AddRange(StormDiscardPile);
            StormDiscardPile.Clear();
            StormDeck.Shuffle();
            Stone(Milestone.Shuffled);
            Log("The Storm deck is reshuffled");
        }
        EndStormPhase();
    }

    public bool IsProtectedFromStorm(Location l)
    {
        if (!ShieldWallDestroyed)
            return l.Territory.IsProtectedFromStorm;
        return l.Territory.IsProtectedFromStorm && !(l.Id == Map.Arrakeen.Id || l.Territory == Map.ImperialBasin || l.Id == Map.Carthag.Id);
    }

    #endregion Information
}