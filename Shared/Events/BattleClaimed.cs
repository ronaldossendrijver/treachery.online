/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
                return Message.Express(Faction.Pink, " will fight this battle");
            }
            else
            {
                return Message.Express(Faction.Pink, "'s ally will fight this battle");
            }
        }
    }
}
