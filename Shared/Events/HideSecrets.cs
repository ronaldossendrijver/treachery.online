/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class HideSecrets : GameEvent
    {
        #region Construction

        public HideSecrets(Game game) : base(game)
        {
        }

        public HideSecrets()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.SecretsRemainHidden.Add(Initiator);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " hide their secrets at end of game");
        }

        #endregion Execution
    }
}
