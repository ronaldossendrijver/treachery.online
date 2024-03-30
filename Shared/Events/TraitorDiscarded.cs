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
    public class TraitorDiscarded : GameEvent
    {
        #region Construction

        public TraitorDiscarded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public TraitorDiscarded()
        {
        }

        #endregion Construction

        #region Properties

        public int _traitorId;

        [JsonIgnore]
        public IHero Traitor
        {
            get => LeaderManager.HeroLookup.Find(_traitorId);
            set => _traitorId = LeaderManager.HeroLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTraitors(Player).Contains(Traitor)) return Message.Express("Invalid traitor");

            return null;
        }

        public static IEnumerable<IHero> ValidTraitors(Player p) => p.Traitors;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.TraitorDeck.Items.Add(Traitor);
            Player.Traitors.Remove(Traitor);
            Game.NumberOfTraitorsToDiscard--;

            if (Game.NumberOfTraitorsToDiscard == 0)
            {
                Game.Enter(Game.PhaseBeforeDiscardingTraitor);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " shuffle a traitor into the Traitor deck");
        }

        #endregion Execution
    }
}
