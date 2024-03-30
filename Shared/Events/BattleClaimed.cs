/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
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
