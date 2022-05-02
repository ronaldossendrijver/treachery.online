/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SetIncreasedRevivalLimits : GameEvent
    {
        public SetIncreasedRevivalLimits(Game game) : base(game)
        {
        }

        public SetIncreasedRevivalLimits()
        {
        }

        public Faction[] Factions { get; set; }

        public override Message Validate()
        {
            return "";
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Factions.Any())
            {
                return Message.Express(Initiator, " grant a revival limit of ", 5, " to ", Factions);
            }
            else
            {
                return Message.Express(Initiator, " don't grant increased revival limits");
            }
        }
    }
}
