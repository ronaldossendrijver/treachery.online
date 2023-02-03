/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class CharityClaimed : GameEvent
    {
        public CharityClaimed(Game game) : base(game)
        {
        }

        public CharityClaimed()
        {
        }

        public override Message Validate()
        {
            var p = Player;
            if (p.Resources > 1) return Message.Express("You are not eligable for charity");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " claim charity");
        }
    }
}
