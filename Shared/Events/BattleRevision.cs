/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Initiator, "{0} revise their battle plan", Initiator);
        }
    }
}
