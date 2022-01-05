/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Initiator, "{0} hide their secrets at end of game.", Initiator);
        }
    }
}
