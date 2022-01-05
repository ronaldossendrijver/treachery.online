/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class PoisonToothCancelled : GameEvent
    {
        public PoisonToothCancelled(Game game) : base(game)
        {
        }

        public PoisonToothCancelled()
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
            return new Message(Initiator, "{0} don't use their {1}.", Initiator, TreacheryCardType.PoisonTooth);
        }
    }
}
