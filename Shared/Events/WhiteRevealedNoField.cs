/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteRevealedNoField : GameEvent
    {
        #region Construction

        public WhiteRevealedNoField(Game game, Faction initiator) : base(game, initiator) { }

        public WhiteRevealedNoField()
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
            Game.RevealCurrentNoField(Player);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " reveal a ", FactionSpecialForce.White);
        }

        #endregion Execution
    }
}
