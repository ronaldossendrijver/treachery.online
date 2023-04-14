/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SkillAssigned : PassableGameEvent
    {
        #region Construction

        public SkillAssigned(Game game) : base(game)
        {
        }

        public SkillAssigned()
        {
        }

        #endregion Construction

        #region Properties

        public LeaderSkill Skill;

        public int _leaderId;

        [JsonIgnore]
        public Leader Leader
        {
            get => LeaderManager.LeaderLookup.Find(_leaderId);
            set => _leaderId = LeaderManager.LeaderLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Passed && Game.CurrentPhase == Phase.AssigningInitialSkills) return Message.Express("You must assign a leader skill");

            return null;
        }

        public static IEnumerable<LeaderSkill> ValidSkills(Player p)
        {
            return p.SkillsToChooseFrom;
        }

        public static IEnumerable<Leader> ValidLeaders(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.AssigningInitialSkills)
            {
                return p.Leaders.Where(l => l.HeroType != HeroType.Auditor);
            }
            else
            {
                return new Leader[] { p.MostRecentlyRevivedLeader };
            }
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.SetSkill(Leader, Skill);
            Player.SkillsToChooseFrom.Remove(Skill);
            Game.SetInFrontOfShield(Leader, true);
            Game.SkillDeck.PutOnTop(Player.SkillsToChooseFrom);
            Player.SkillsToChooseFrom.Clear();

            if (!Game.Players.Any(p => p.SkillsToChooseFrom.Any()))
            {
                Game.SkillDeck.Shuffle();
                Game.Enter(Game.CurrentPhase != Phase.AssigningInitialSkills, Game.PhaseBeforeSkillAssignment, Game.TreacheryCardsBeforeTraitors, Game.DealTraitors, Game.SetupSpiceAndForces);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Leader, " becomes ", Skill);
        }


        #endregion Execution
    }
}
