/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BattleRevision : GameEvent
    {
        #region Construction

        public BattleRevision(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BattleRevision()
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
            if (By(Game.CurrentBattle.Aggressor))
            {
                Game.AggressorBattleAction = null;
            }
            else if (By(Game.CurrentBattle.Defender))
            {
                Game.DefenderBattleAction = null;
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " revise their battle plan");
        }

        #endregion Execution
    }
}
