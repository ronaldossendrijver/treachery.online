/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BrownEconomics : GameEvent
    {
        #region Construction

        public BrownEconomics(Game game) : base(game)
        {
        }

        public BrownEconomics()
        {
        }

        #endregion Construction

        #region Properties

        public BrownEconomicsStatus Status;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.EconomicsStatus = Status;
            Game.Stone(Milestone.Economics);
            Log();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play Inflation: ", Status);
        }

        #endregion Execution
    }
}
