﻿/*
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
    #region BeginningOfRevival

    internal bool RevivalTechTokenIncome { get; set; }
    public List<Faction> FactionsThatTookFreeRevival { get; } = [];
    internal bool PurpleStartedRevivalWithLowThreshold { get; set; }
    public RecruitsPlayed CurrentRecruitsPlayed { get; set; }
    public List<Faction> FactionsThatRevivedSpecialForcesThisTurn { get; } = [];
    public Faction[] FactionsWithIncreasedRevivalLimits { get; internal set; } = [];
    public List<RequestPurpleRevival> CurrentRevivalRequests { get; } = [];
    public Dictionary<IHero, int> EarlyRevivalsOffers { get; } = new();
    public BrownFreeRevivalPrevention CurrentFreeRevivalPrevention { get; internal set; }
    internal KarmaRevivalPrevention CurrentKarmaRevivalPrevention { get; set; }
    public int AmbassadorsPlacedThisTurn { get; internal set; }

    #endregion

    #region Revival

    internal void PrepareSkillAssignmentToRevivedLeader(Player player, Leader leader)
    {
        if (CurrentPhase != Phase.AssigningSkill) 
            PhaseBeforeSkillAssignment = CurrentPhase;

        player.MostRecentlyRevivedLeader = leader;
        SkillDeck.Shuffle();
        Stone(Milestone.Shuffled);
        player.SkillsToChooseFrom.Add(SkillDeck.Draw());
        player.SkillsToChooseFrom.Add(SkillDeck.Draw());
        Enter(Phase.AssigningSkill);
    }

    #endregion

    #region Information

    public int FreeRevivals(Player player, bool usesRedSecretAlly)
    {
        if (FactionsThatTookFreeRevival.Contains(player.Faction) || FreeRevivalPrevented(player.Faction)) return 0;

        var nrOfFreeRevivals = 0;

        switch (player.Faction)
        {
            case Faction.Yellow: nrOfFreeRevivals = 3; break;
            case Faction.Green: nrOfFreeRevivals = 2; break;
            case Faction.Black: nrOfFreeRevivals = 2; break;
            case Faction.Red: nrOfFreeRevivals = 1; break;
            case Faction.Orange: nrOfFreeRevivals = 1; break;
            case Faction.Blue: nrOfFreeRevivals = 1; break;
            case Faction.Grey: nrOfFreeRevivals = 1; break;
            case Faction.Purple: nrOfFreeRevivals = 2; break;

            case Faction.Brown: nrOfFreeRevivals = 0; break;
            case Faction.White: nrOfFreeRevivals = 2; break;
            case Faction.Pink: nrOfFreeRevivals = 2; break;
            case Faction.Cyan: nrOfFreeRevivals = 2; break;
        }
        
        if (Version < 176 && CurrentRecruitsPlayed != null) nrOfFreeRevivals *= 2;

        if (player.Ally == Faction.Yellow && YellowAllowsThreeFreeRevivals ||
            CurrentYellowSecretAlly != null && CurrentYellowSecretAlly.Player == player) nrOfFreeRevivals = 3;
        
        if (Version >= 176 && CurrentRecruitsPlayed != null) nrOfFreeRevivals *= 2;

        if (usesRedSecretAlly) nrOfFreeRevivals += 3;

        if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(player)) nrOfFreeRevivals += 1;

        return nrOfFreeRevivals;
    }

    private bool GetsExtraCharityAndFreeRevivalDueToLowThreshold(Player player)
    {
        return (!(player.Is(Faction.Red) || player.Is(Faction.Brown)) && player.HasLowThreshold()) ||
               (player.Is(Faction.Red) && player.HasLowThreshold(World.Red)) ||
               (player.Is(Faction.Brown) && player.HasLowThreshold() && OccupierOf(World.Brown) == null);
    }

    public int GetRevivalLimit(Game g, Player p, bool redSecretAllyUsed)
    {
        int nrOfFreeRevivals = g.FreeRevivals(p, redSecretAllyUsed);
        
        if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            return Math.Max(100, nrOfFreeRevivals);
        
        if (CurrentRecruitsPlayed != null)
            return Math.Max(7, nrOfFreeRevivals);
        
        if (FactionsWithIncreasedRevivalLimits.Contains(p.Faction))
            return Math.Max(5, nrOfFreeRevivals);
        
        return Math.Max(3, nrOfFreeRevivals);
    }

    private bool FreeRevivalPrevented(Faction f)
    {
        return CurrentFreeRevivalPrevention?.Target == f;
    }

    private bool PreventedFromReviving(Faction f)
    {
        return CurrentKarmaRevivalPrevention != null && CurrentKarmaRevivalPrevention.Target == f;
    }

    public Ambassador AmbassadorIn(Territory t)
    {
        return AmbassadorsOnPlanet.GetValueOrDefault(t, Ambassador.None);
    }

    #endregion Information
}