/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PlayerReplaced : GameEvent
    {
        public PlayerReplaced(Game game) : base(game)
        {
        }

        public PlayerReplaced()
        {
        }

        public Faction ToReplace { get; set; }


        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(ToReplace, " player has been replaced");
        }

        public static IEnumerable<Faction> ValidTargets(Game g)
        {
            return g.Players.Select(p => p.Faction);
        }

    }
}
