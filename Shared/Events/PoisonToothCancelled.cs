/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class PoisonToothCancelled : GameEvent
    {
        #region Construction

        public PoisonToothCancelled(Game game) : base(game)
        {
        }

        public PoisonToothCancelled()
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
            Game.PoisonToothCancelled = true;
            Log();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " don't use their ", TreacheryCardType.PoisonTooth);
        }

        #endregion Execution
    }
}
