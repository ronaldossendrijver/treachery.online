/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
                return Message.Express(Initiator, " won't capture or kill a leader", Initiator);
            }
            else
            {
                return Message.Express(Initiator, " will capture or kill a random leader...", Initiator);
            }
        }
    }
}
