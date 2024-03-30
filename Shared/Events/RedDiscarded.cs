/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class RedDiscarded : GameEvent
    {
        #region Construction

        public RedDiscarded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public RedDiscarded()
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
            if (Card == null) return Message.Express("Choose a card to discard");
            if (!Player.Has(Card)) return Message.Express("Invalid card");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Player.Resources -= 2;
            Game.Discard(Player, Card);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pay ", Payment.Of(2), " to discard ", Card);
        }

        #endregion Execution
    }
}
