/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class NexusVoted : PassableGameEvent
    {
        public NexusVoted(Game game) : base(game)
        {
        }

        public NexusVoted()
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
            return Message.Express(Initiator, " vote ", Passed ? "No" : "Yes");
        }
    }
}
