/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System.Linq;

namespace Treachery.Shared
{
    public class DiscardedSearchedAnnounced : GameEvent
    {
        #region Construction

        public DiscardedSearchedAnnounced(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public DiscardedSearchedAnnounced()
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
            return !g.CurrentPhaseIsUnInterruptable && p.Resources >= 2 && p.Has(TreacheryCardType.SearchDiscarded) && DiscardedSearched.ValidCards(g).Any();
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.PhaseBeforeSearchingDiscarded = Game.CurrentPhase;
            Player.Resources -= 2;
            Game.Enter(Phase.SearchingDiscarded);
            Game.Stone(Milestone.CardWonSwapped);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.SearchDiscarded, " and pay ", Payment.Of(2), " to search a card in the Treachery Discard Pile");
        }

        #endregion Execution
    }
}
