/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
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
        public Dictionary<IHero, int> EarlyRevivalsOffers { get; private set; } = new();
        public BrownFreeRevivalPrevention CurrentFreeRevivalPrevention { get; internal set; }
        internal KarmaRevivalPrevention CurrentKarmaRevivalPrevention { get; set; }
        public int AmbassadorsPlacedThisTurn { get; internal set; } = 0;

        #endregion

        #region Revival

        internal void PrepareSkillAssignmentToRevivedLeader(Player player, Leader leader)
        {
            if (CurrentPhase != Phase.AssigningSkill)
            {
                PhaseBeforeSkillAssignment = CurrentPhase;
            }

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
            if (FactionsThatTookFreeRevival.Contains(player.Faction) || FreeRevivalPrevented(player.Faction))
            {
                return 0;
            }
            else
            {
                int nrOfFreeRevivals = 0;

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

                if (CurrentRecruitsPlayed != null)
                {
                    nrOfFreeRevivals *= 2;
                }

                if (player.Ally == Faction.Yellow && player.Ally == Faction.Yellow && YellowAllowsThreeFreeRevivals)
                {
                    nrOfFreeRevivals = 3;
                }

                if (CurrentYellowNexus != null && CurrentYellowNexus.Player == player)
                {
                    nrOfFreeRevivals = 3;
                }

                if (usesRedSecretAlly)
                {
                    nrOfFreeRevivals += 3;
                }

                if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(player))
                {
                    nrOfFreeRevivals += 1;
                }

                return nrOfFreeRevivals;
            }
        }

        private bool GetsExtraCharityAndFreeRevivalDueToLowThreshold(Player player) =>
            !(player.Is(Faction.Red) || player.Is(Faction.Brown)) && player.HasLowThreshold() ||
            player.Is(Faction.Red) && player.HasLowThreshold(World.Red) ||
            player.Is(Faction.Brown) && player.HasLowThreshold() && OccupierOf(World.Brown) == null;

        public int GetRevivalLimit(Game g, Player p)
        {
            if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            {
                return 100;
            }
            else if (CurrentRecruitsPlayed != null)
            {
                return 7;
            }
            else if (FactionsWithIncreasedRevivalLimits.Contains(p.Faction))
            {
                return 5;
            }
            else
            {
                return 3;
            }
        }

        private bool FreeRevivalPrevented(Faction f) => CurrentFreeRevivalPrevention?.Target == f;

        public bool PreventedFromReviving(Faction f) => CurrentKarmaRevivalPrevention != null && CurrentKarmaRevivalPrevention.Target == f;

        public Ambassador AmbassadorIn(Territory t) => AmbassadorsOnPlanet.TryGetValue(t, out Ambassador value) ? value : Ambassador.None;

        #endregion Information
    }
}
