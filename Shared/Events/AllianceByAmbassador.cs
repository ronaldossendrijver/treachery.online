/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceByAmbassador : PassableGameEvent
    {
        public AllianceByAmbassador(Game game) : base(game)
        {
        }

        public AllianceByAmbassador()
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
            return Message.Express(Initiator, !Passed ? "" : " don't", " agree to ally");
        }
    }
}
