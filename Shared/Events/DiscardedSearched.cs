/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class DiscardedSearched : GameEvent
    {
        public int _cardId;

        public DiscardedSearched(Game game) : base(game)
        {
        }

        public DiscardedSearched()
        {
        }

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Get(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.GetId(value);
            }
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
            return new Message(Initiator, "{0} take one card from the treachery discard pile and shuffle it.", Initiator, TreacheryCardType.SearchDiscarded);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g)
        {
            return g.TreacheryDiscardPile.Items;
        }

        public static bool CanBePlayed(Player p)
        {
            return p.Has(TreacheryCardType.SearchDiscarded);
        }
    }
}
