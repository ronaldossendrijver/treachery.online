/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public partial class Game
{
    #region State

    public int HmsMovesLeft { get; internal set; }
    public List<Faction> HasBattleWheel { get; } = new();
    internal List<int> Dials { get; set; } = new();
    public bool CurrentTestingStationUsed { get; internal set; }
    internal Phase PhaseBeforeStormLoss { get; set; }
    public List<LossToTake> StormLossesToTake { get; } = new();

    #endregion State

    #region Storm

    internal void EnterStormPhase()
    {
        CurrentTurn++;
        MainPhaseStart(MainPhase.Storm, CurrentTurn > 1);
        FactionsThatRevivedSpecialForcesThisTurn.Clear();
        StormLossesToTake.Clear();
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
                MoveHMSBeforeDiallingStorm();
        }

        DetermineOccupationAtStartOrEndOfTurn();
    }

    internal void MoveHMSBeforeDiallingStorm()
    {
        if (IsPlaying(Faction.Grey) && GetPlayer(Faction.Grey).AnyForcesIn(Map.HiddenMobileStronghold) > 0)
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
                if (Map.HiddenMobileStronghold.AttachedToLocation.Sector == SectorInStorm)
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
        return Random.Next(6) + 1;
    }

    internal void DetermineStorm()
    {
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
        if (ResourcesOnPlanet.TryGetValue(l, out var value))
        {
            var collected = Math.Min(value, maximumAmount);
            ChangeResourcesOnPlanet(l, -collected);
            var collector = GetPlayer(faction);
            collector.Resources += collected;
            Log(faction, " collect ", Payment.Of(collected), " from ", l);
        }
    }

    internal void MoveStormAndDetermineNext(int amount)
    {
        if (amount == 0) PerformStorm();

        for (var i = 0; i < amount; i++)
        {
            SectorInStorm = (SectorInStorm + 1) % Map.NUMBER_OF_SECTORS;
            PerformStorm();
        }

        if (UseStormDeck) NextStormMoves = DetermineLaterStormWithStormDeck();

        Stone(Milestone.Storm);
    }

    internal void PerformStorm()
    {
        foreach (var l in Forces().Where(f => f.Key.Sector == SectorInStorm && !IsProtectedFromStorm(f.Key)))
        foreach (var battalion in l.Value)
        {
            var player = GetPlayer(battalion.Faction);

            var killCount = 0;
            if (battalion.Is(Faction.Yellow) && Applicable(Rule.YellowStormLosses))
            {
                if (!Prevented(FactionAdvantage.YellowProtectedFromStorm))
                {
                    StormLossesToTake.Add(new LossToTake { Location = l.Key, Amount = TakeLosses.HalfOf(battalion.AmountOfForces, battalion.AmountOfSpecialForces), Faction = battalion.Faction });
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
                StormLossesToTake.Add(new LossToTake { Location = l.Key, Amount = battalion.TotalAmountOfForces, Faction = battalion.Faction });
            }
            else
            {
                RevealCurrentNoField(player, l.Key);
                killCount += player.AnyForcesIn(l.Key);
                player.KillAllForces(l.Key, false);
            }

            if (killCount > 0) Log("The storm kills ", killCount, battalion.Faction, " forces in ", l.Key);
        }

        if (Version >= 163) FlipBeneGesseritWhenAlone();

        foreach (var t in Strongholds.Where(s => s.Locations.Any(l => l.Sector == SectorInStorm && !IsProtectedFromStorm(l))))
        {
            var ambassador = AmbassadorIn(t);
            if (ambassador != Ambassador.None)
            {
                var pink = GetPlayer(Faction.Pink);
                var succ = AmbassadorsOnPlanet.Remove(t);
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
        JustRevealedDiscoveryStrongholds.Clear();

        MainPhaseEnd();

        if (StormLossesToTake.Count > 0)
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

    internal bool UseStormDeck => Applicable(Rule.StormDeckWithoutYellow) || (IsPlaying(Faction.Yellow) && Applicable(Rule.YellowSeesStorm));

    public bool IsProtectedFromStorm(Location l)
    {
        if (!ShieldWallDestroyed)
            return l.Territory.IsProtectedFromStorm;
        return l.Territory.IsProtectedFromStorm && !(l == Map.Arrakeen || l.Territory == Map.ImperialBasin || l == Map.Carthag);
    }

    #endregion Information
}