/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class KarmaPrescience : GameEvent
    {
        public KarmaPrescience(Game game) : base(game)
        {
        }

        public KarmaPrescience()
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
            return Message.Express("Using ", TreacheryCardType.Karma, Initiator, " see the entire enemy battle plan");
        }
    }
}
