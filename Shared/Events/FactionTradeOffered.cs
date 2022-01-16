/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            if (Game.Version >= 132)
            {
                if (!ValidTargets(Game, Player).Contains(Target)) return "Invalid target";
            }
            else
            {
                if (!Game.IsPlaying(Target)) return "Invalid target";
            }
            

            return "";
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
            return g.FactionsInPlay.Where(f => f != p.Faction);
        }
    }

}
