/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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


        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(ToReplace, "{0} player has been replaced.", ToReplace);
        }

        public static IEnumerable<Faction> ValidTargets(Game g)
        {
            return g.Players.Select(p => p.Faction);
        }

    }
}
