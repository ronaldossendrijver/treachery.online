/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class SwitchedSkilledLeader : GameEvent
    {
        public SwitchedSkilledLeader(Game game) : base(game)
        {
        }

        public SwitchedSkilledLeader()
        {
        }

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
            return new Message(Initiator, "{0} switch their skilled leader.", Initiator);
        }

        public static bool CanBePlayed(Game game, Player player)
        {
            return player.Leaders.Any(l => game.Skilled(l) && !game.CapturedLeaders.ContainsKey(l));
        }
    }
}
