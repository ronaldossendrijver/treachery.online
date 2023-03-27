/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class BattleRevision : GameEvent
    {
        public BattleRevision(Game game) : base(game)
        {
        }

        public BattleRevision()
        {
        }

        public override Message Validate()
        {
            return null;
        }

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
    }
}
