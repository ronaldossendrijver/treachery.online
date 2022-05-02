/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class OrangeDelay : GameEvent
    {
        public OrangeDelay(Game game) : base(game)
        {
        }

        public OrangeDelay()
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
            return Message.Express(Initiator, " delay their turn");
        }
    }
}
