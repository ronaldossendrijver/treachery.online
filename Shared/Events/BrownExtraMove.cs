/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class BrownExtraMove : GameEvent
    {
        #region Construction

        public BrownExtraMove(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BrownExtraMove()
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

            Game.BrownHasExtraMove = true;
            Game.Stone(Milestone.SpecialUselessPlayed);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " gain extra movement");
        }

        #endregion Execution

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_KULON);
        }
    }
}
