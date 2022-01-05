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

        public override string Validate()
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
                return new Message(Initiator, "{0} grant a revival limit of 5 to {1}.", Initiator, string.Join(", ", Factions.Select(f => Skin.Current.Describe(f))));
            }
            else
            {
                return new Message(Initiator, "{0} don't grant increased revival limits.", Initiator);
            }
        }
    }
}
