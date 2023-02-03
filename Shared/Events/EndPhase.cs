/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
            Game.HandleEndPhaseEvent();
        }

        public override Message GetMessage()
        {
            return Message.Express("Phase ended by ", Initiator);
        }
    }
}
