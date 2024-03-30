/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Diplomacy : GameEvent
    {
        #region Construction

        public Diplomacy(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Diplomacy()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            var result = new List<TreacheryCard>();
            var plan = g.CurrentBattle.PlanOf(p);
            if (plan.Weapon != null && plan.Weapon.IsUseless) result.Add(plan.Weapon);
            if (plan.Defense != null && plan.Defense.IsUseless) result.Add(plan.Defense);
            return result;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            if (g.SkilledAs(p, LeaderSkill.Diplomat) && g.CurrentDiplomacy == null)
            {
                var plan = g.CurrentBattle.PlanOf(p);
                return
                    plan != null &&
                    (plan.Defense == null || !plan.Defense.IsDefense) &&
                    g.CurrentBattle.PlanOfOpponent(p).Defense != null &&
                    g.CurrentBattle.PlanOfOpponent(p).Defense.IsDefense &&
                    ValidCards(g, p).Any();
            }

            return false;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log(Initiator, " use Diplomacy to turn ", Card, " into a ", Game.CurrentBattle?.PlanOfOpponent(Player)?.Defense);
            Game.CurrentDiplomacy = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use Diplomacy to turn ", Card, " into a copy of the opponent's defense");
        }

        #endregion Execution
    }
}
