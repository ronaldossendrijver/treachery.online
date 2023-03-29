/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class DiscardedSearchedAnnounced : GameEvent
    {
        #region Construction

        public DiscardedSearchedAnnounced(Game game) : base(game)
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
