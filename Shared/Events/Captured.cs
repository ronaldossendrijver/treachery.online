/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Captured : PassableGameEvent
    {
        public Captured(Game game) : base(game)
        {
        }

        public Captured()
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
            if (Passed)
            {
                return new Message(Initiator, "{0} don't capture a leader.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} capture a leader.", Initiator);
            }
        }
    }
}
