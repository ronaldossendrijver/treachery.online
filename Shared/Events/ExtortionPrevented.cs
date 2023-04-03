/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ExtortionPrevented : GameEvent
    {
        #region Construction

        public ExtortionPrevented(Game game) : base(game)
        {
        }

        public ExtortionPrevented()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return p.Faction != Faction.Cyan && p.Resources >= 3;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.ExtortionToBeReturned = false;
            Player.Resources -= 3;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pay ", Payment.Of(3), " to prevent ", Faction.Cyan, " from regaining ", TerrorType.Extortion);
        }

        #endregion Execution
    }
}
