/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use {1} and pay 2 to search a card in the Treachery Discard Pile.", Initiator, TreacheryCardType.SearchDiscarded);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return p.Resources >= 2 && p.Has(TreacheryCardType.SearchDiscarded) && DiscardedSearched.ValidCards(g).Any();
        }
    }
}
