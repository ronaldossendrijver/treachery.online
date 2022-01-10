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
            return Message.Express(Initiator, " don't use their ", TreacheryCardType.PoisonTooth);
        }
    }
}
