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
    public class BrownFreeRevivalPrevention : GameEvent
    {
        #region Construction

        public BrownFreeRevivalPrevention(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BrownFreeRevivalPrevention()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_LALALA);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            if (NexusPlayed.CanUseCunning(Player))
            {
                Game.DiscardNexusCard(Player);
                Game.Stone(Milestone.NexusPlayed);
                Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
            }
            else
            {
                Game.Discard(CardToUse(Player));
            }

            Game.CurrentFreeRevivalPrevention = this;
            Game.Stone(Milestone.SpecialUselessPlayed);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " prevent ", Target, " from using free revival this phase");
        }

        #endregion Execution
    }
}
