/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
            if (Passed)
            {
                return Message.Express(Initiator, " won't capture or kill a leader");
            }
            else
            {
                return Message.Express(Initiator, " will capture or kill a random leader...");
            }
        }
    }
}
