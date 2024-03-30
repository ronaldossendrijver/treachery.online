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
    #region BeginningOfRevival

    internal bool RevivalTechTokenIncome { get; set; }
    public List<Faction> FactionsThatTookFreeRevival { get; } = new();
    internal bool PurpleStartedRevivalWithLowThreshold { get; set; }
    internal RecruitsPlayed CurrentRecruitsPlayed { get; set; }
    public List<Faction> FactionsThatRevivedSpecialForcesThisTurn { get; } = new();
    public Faction[] FactionsWithIncreasedRevivalLimits { get; internal set; } = Array.Empty<Faction>();
    public List<RequestPurpleRevival> CurrentRevivalRequests { get; set; } = new();
    public Dictionary<IHero, int> EarlyRevivalsOffers { get; } = new();
    public BrownFreeRevivalPrevention CurrentFreeRevivalPrevention { get; internal set; }
    internal KarmaRevivalPrevention CurrentKarmaRevivalPrevention { get; set; }
    public int AmbassadorsPlacedThisTurn { get; internal set; } = 0;

    #endregion

    #region Revival

    internal void PrepareSkillAssignmentToRevivedLeader(Player player, Leader leader)
    {
        if (CurrentPhase != Phase.AssigningSkill) PhaseBeforeSkillAssignment = CurrentPhase;

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

        if (CurrentRecruitsPlayed != null) nrOfFreeRevivals *= 2;

        if (player.Ally == Faction.Yellow && player.Ally == Faction.Yellow && YellowAllowsThreeFreeRevivals) nrOfFreeRevivals = 3;

        if (CurrentYellowSecretAlly != null && CurrentYellowSecretAlly.Player == player) nrOfFreeRevivals = 3;

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

    public int GetRevivalLimit(Game g, Player p)
    {
        if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            return 100;
        if (CurrentRecruitsPlayed != null)
            return 7;
        if (FactionsWithIncreasedRevivalLimits.Contains(p.Faction))
            return 5;
        return 3;
    }

    private bool FreeRevivalPrevented(Faction f)
    {
        return CurrentFreeRevivalPrevention?.Target == f;
    }

    public bool PreventedFromReviving(Faction f)
    {
        return CurrentKarmaRevivalPrevention != null && CurrentKarmaRevivalPrevention.Target == f;
    }

    public Ambassador AmbassadorIn(Territory t)
    {
        return AmbassadorsOnPlanet.TryGetValue(t, out var value) ? value : Ambassador.None;
    }

    #endregion Information
}