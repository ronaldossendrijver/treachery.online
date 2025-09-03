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
    #region BeginningOfRevival

    internal bool RevivalTechTokenIncome { get; set; }
    public Dictionary<Faction, int> TotalRevivalsThisTurn { get; set; } = [];
    public Dictionary<Faction, int> FreeRevivalsThisTurn { get; } = [];
    public List<Faction> FactionsThatRevivedSpecialForcesThisTurn { get; } = [];
    public List<Faction> FactionsThatRevivedLeadersThisTurn { get; } = [];
    internal bool PurpleStartedRevivalWithLowThreshold { get; set; }
    public RecruitsPlayed CurrentRecruitsPlayed { get; set; }
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
        if (FreeRevivalPrevented(player.Faction)) return 0;

        if (Version < 181 && FreeRevivalsThisTurn.ContainsKey(player.Faction)) return 0;

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
        
        if (Version is >= 176 and <= 179 && CurrentRecruitsPlayed != null) nrOfFreeRevivals *= 2;

        if (usesRedSecretAlly) nrOfFreeRevivals += 3;

        if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(player)) nrOfFreeRevivals += 1;
        
        if (Version >= 179 && CurrentRecruitsPlayed != null) nrOfFreeRevivals *= 2;

        return Math.Max(nrOfFreeRevivals - FreeRevivalsThisTurn.GetValueOrDefault(player.Faction, 0), 0);
    }

    private bool GetsExtraCharityAndFreeRevivalDueToLowThreshold(Player player)
    {
        return (!(player.Is(Faction.Red) || player.Is(Faction.Brown)) && player.HasLowThreshold()) ||
               (player.Is(Faction.Red) && player.HasLowThreshold(World.Red)) ||
               (player.Is(Faction.Brown) && player.HasLowThreshold() && OccupierOf(World.Brown) == null);
    }

    public int GetMaxRevivals(Game g, Player p, bool redSecretAllyUsed)
    {
        //int nrOfFreeRevivals = g.FreeRevivals(p, redSecretAllyUsed);

        if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            return int.MaxValue;

        var redSecretAlly = redSecretAllyUsed ? 3 : 0;
        var lowThreshold = g.HasLowThreshold(p.Faction) ? 1 : 0;
        var alreadyRevived = g.TotalRevivalsThisTurn.GetValueOrDefault(p.Faction, 0);
        
        if (CurrentRecruitsPlayed != null)
            return Math.Max(7 + redSecretAlly + lowThreshold - alreadyRevived, 0);
        
        if (FactionsWithIncreasedRevivalLimits.Contains(p.Faction))
            return Math.Max(5 + lowThreshold + redSecretAlly - alreadyRevived, 0);
        
        return Math.Max(3 + lowThreshold + redSecretAlly - alreadyRevived, 0);
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

    public void ClaimFreeRevival(Player player)
    {
        var maxFreeRevivals = FreeRevivals(player, false);
        
        if (maxFreeRevivals <= 0) return;
        
        var freeRevivedSpecialForces = 0;
        var freeRevivedNormalForces = 0;
        
        var maxRevivableSpecialForces = Revival.ValidMaxRevivals(this, player, true, false);
        var maxRevivableNormalForces = Revival.ValidMaxRevivals(this, player, false, false);
        
        while (freeRevivedSpecialForces < maxFreeRevivals &&
               freeRevivedSpecialForces < maxRevivableSpecialForces)
        {
            freeRevivedSpecialForces++;
        }
        
        while (freeRevivedSpecialForces + freeRevivedNormalForces < maxFreeRevivals &&
               freeRevivedNormalForces < maxRevivableNormalForces)
        {
            freeRevivedNormalForces++;
        }
        
        if (freeRevivedSpecialForces > 0)
            player.ReviveSpecialForces(freeRevivedSpecialForces);
        
        if (freeRevivedNormalForces > 0)
            player.ReviveForces(freeRevivedNormalForces);
        
        if (player.Faction != Faction.Purple) RevivalTechTokenIncome = true;

        var purple = GetPlayer(Faction.Purple);
        
        var purpleReceivesIncome = purple != null && !PurpleStartedRevivalWithLowThreshold && !Prevented(FactionAdvantage.PurpleReceiveRevive);

        if (purpleReceivesIncome)
        {
            purple.Resources += 1;
        }
        
        FreeRevivalsThisTurn[player.Faction] = FreeRevivalsThisTurn.GetValueOrDefault(player.Faction, 0) + freeRevivedNormalForces + freeRevivedSpecialForces;
        TotalRevivalsThisTurn[player.Faction] = TotalRevivalsThisTurn.GetValueOrDefault(player.Faction, 0) + freeRevivedNormalForces + freeRevivedSpecialForces;
        
        Log(player.Faction, " revive ",
            MessagePart.ExpressIf(freeRevivedSpecialForces > 0, freeRevivedSpecialForces, player.SpecialForce),
            MessagePart.ExpressIf(freeRevivedNormalForces > 0, freeRevivedNormalForces, player.Force),
            " for free",
            MessagePart.ExpressIf(purpleReceivesIncome, Faction.Purple, " receive ", Payment.Of(1)));
        
        Stone(Milestone.Revival);
    }
}