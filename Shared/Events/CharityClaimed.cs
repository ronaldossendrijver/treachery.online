/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            var p = Player;
            if (p.Resources > 1) return "You are not eligable for charity";

            return "";
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
