/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class LoyaltyDecided : GameEvent
    {
        public int _leaderId;

        public LoyaltyDecided(Game game) : base(game)
        {
        }

        public LoyaltyDecided()
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
            if (!ValidLeaders(Player).Contains(Leader)) return Message.Express("Invalid leader");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Leader, " will always be loyal to ", Initiator);
        }

        public static IEnumerable<Leader> ValidLeaders(Player p) => p.Leaders.Where(l => l.HeroType != HeroType.PinkAndCyan);
    }
}
