/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DiscardedTaken : GameEvent
    {
        public int _cardId;

        public DiscardedTaken(Game game) : base(game)
        {
        }

        public DiscardedTaken()
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
            return new Message(Initiator, "{0} use {1} to take the recently discarded {2}.", Initiator, TreacheryCardType.TakeDiscarded, Card);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return g.RecentlyDiscarded.Where(kvp => kvp.Value != p.Faction).Select(kvp => kvp.Key);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return p.Has(TreacheryCardType.TakeDiscarded) && ValidCards(g, p).Any();
        }
    }
}
