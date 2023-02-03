/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
            return Message.Express(Initiator, " use ", TreacheryCardType.TakeDiscarded, " to acquire the discarded ", Card);
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
