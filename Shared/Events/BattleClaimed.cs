/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class BattleClaimed : PassableGameEvent
    {
        #region Construction

        public BattleClaimed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BattleClaimed()
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
            Log();
            Game.CurrentPinkOrAllyFighter = Passed ? Game.GetAlly(Faction.Pink) : Faction.Pink;
            Game.Enter(Phase.BattlePhase);
            Game.InitiateBattle();
            DeterminePinkContribution();
        }

        private void DeterminePinkContribution()
        {
            var fighter = Game.GetPlayer(Game.CurrentPinkOrAllyFighter);

            if (Game.CurrentBattle != null && (fighter.Is(Faction.Pink) || fighter.Ally == Faction.Pink))
            {
                var pink = GetPlayer(Faction.Pink);
                if (Game.Version < 159)
                {
                    Game.CurrentPinkBattleContribution = (int)(0.5f * pink.AnyForcesIn(Game.CurrentBattle.Territory));
                }
                else
                {
                    Game.CurrentPinkBattleContribution = (int)Math.Ceiling(0.5f * pink.AnyForcesIn(Game.CurrentBattle.Territory));
                }
            }
            else
            {
                Game.CurrentPinkBattleContribution = 0;
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Faction.Pink, " will fight this battle");
            }
            else
            {
                return Message.Express(Faction.Pink, "'s ally will fight this battle");
            }
        }

        #endregion Execution
    }
}
