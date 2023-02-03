/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BattleRevision : GameEvent
    {
        public BattleRevision(Game game) : base(game)
        {
        }

        public BattleRevision()
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
            return Message.Express(Initiator, " revise their battle plan");
        }
    }
}
