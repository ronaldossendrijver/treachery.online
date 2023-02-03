/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class HideSecrets : GameEvent
    {
        public HideSecrets(Game game) : base(game)
        {
        }

        public HideSecrets()
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
            return Message.Express(Initiator, " hide their secrets at end of game");
        }
    }
}
