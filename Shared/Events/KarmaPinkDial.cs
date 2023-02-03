/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class KarmaPinkDial : GameEvent
    {
        public KarmaPinkDial(Game game) : base(game)
        {
        }

        public KarmaPinkDial()
        {
        }

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
            return Message.Express("Using ", TreacheryCardType.Karma, Initiator, " add the difference between leader discs to their dial");
        }
    }
}
