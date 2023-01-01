/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BattleClaimed : PassableGameEvent
    {
        public BattleClaimed(Game game) : base(game)
        {
        }

        public BattleClaimed()
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
                return Message.Express(Initiator, " let their ally fight this battle");
            }
            else
            {
                return Message.Express(Initiator, " fight this battle for their ally");
            }
        }
    }
}
