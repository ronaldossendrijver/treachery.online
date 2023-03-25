/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class DiscardedSearchedAnnounced : GameEvent
    {
        public DiscardedSearchedAnnounced(Game game) : base(game)
        {
        }

        public DiscardedSearchedAnnounced()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.SearchDiscarded, " and pay ", Payment.Of(2), " to search a card in the Treachery Discard Pile");
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return !g.CurrentPhaseIsUnInterruptable && p.Resources >= 2 && p.Has(TreacheryCardType.SearchDiscarded) && DiscardedSearched.ValidCards(g).Any();
        }
    }
}
