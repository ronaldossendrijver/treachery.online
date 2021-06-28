/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Initiator, "Using {0}, {1} see the entire enemy battle plan.", TreacheryCardType.Karma, Initiator);
        }
    }
}
