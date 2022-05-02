/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class EndPhase : GameEvent
    {
        public EndPhase(Game game) : base(game)
        {
        }

        public EndPhase()
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
            return Message.Express("Phase ended by ", Initiator);
        }
    }
}
