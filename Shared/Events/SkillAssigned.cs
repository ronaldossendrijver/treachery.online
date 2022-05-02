/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SkillAssigned : PassableGameEvent
    {
        public LeaderSkill Skill;

        public int _leaderId;

        public SkillAssigned(Game game) : base(game)
        {
        }

        public SkillAssigned()
        {
        }

        [JsonIgnore]
        public Leader Leader
        {
            get
            {
                return LeaderManager.LeaderLookup.Find(_leaderId);
            }
            set
            {
                _leaderId = LeaderManager.LeaderLookup.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (Passed && Game.CurrentPhase == Phase.AssigningInitialSkills) return "You must assign a leader skill";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Leader, " becomes ", Skill);
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

    }
}
