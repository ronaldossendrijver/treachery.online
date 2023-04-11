/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class OrangeDelay : GameEvent
    {
        #region Construction

        public OrangeDelay(Game game) : base(game)
        {
        }

        public OrangeDelay()
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
            Game.BeginningOfShipmentAndMovePhase = false;
            Log();
            Game.Enter(Phase.NonOrangeShip);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " delay their turn");
        }

        #endregion Execution
    }
}
