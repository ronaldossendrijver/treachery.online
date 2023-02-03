/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FactionTradeOffered : GameEvent
    {
        public FactionTradeOffered(Game game) : base(game)
        {
        }

        public FactionTradeOffered()
        {
        }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            if (Game.Version >= 132)
            {
                if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");
            }
            else
            {
                if (!Game.IsPlaying(Target)) return Message.Express("Invalid target");
            }


            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " offer to trade factions with ", Target);
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.FactionsInPlay.Union(g.Players.Select(p => p.Faction)).Where(f => f != p.Faction);
        }
    }

}
