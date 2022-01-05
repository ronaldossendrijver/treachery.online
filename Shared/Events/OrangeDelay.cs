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
            return new Message(Initiator, "{0} delay their turn.", Initiator);
        }
    }
}
