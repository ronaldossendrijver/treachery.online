/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceBroken : GameEvent
    {
        #region Construction

        public AllianceBroken(Game game, Faction initiator) : base(game, initiator) 
        { 
        }

        public AllianceBroken()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (!Player.HasAlly) return Message.Express("You currently have no ally");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.BreakAlliance(Initiator);
            Game.LetFactionsDiscardSurplusCards();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " end their alliance");
        }

        #endregion Execution
    }
}
