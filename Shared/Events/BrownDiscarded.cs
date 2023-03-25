/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownDiscarded : GameEvent
    {
        public int _cardId;

        public BrownDiscarded(Game game) : base(game)
        {
        }

        public BrownDiscarded()
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
            if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Card.Type == TreacheryCardType.Useless)
            {
                return Message.Express(Initiator, " get ", Payment.Of(2), " by discarding a ", TreacheryCardType.Useless, " card");
            }
            else
            {
                return Message.Express(Initiator, " get ", Payment.Of(3), " by discarding a duplicate ", Card);
            }
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards.Where(c =>
                 c.Type == TreacheryCardType.Useless && !p.HasHighThreshold() ||
                (c.Type != TreacheryCardType.Projectile && c.Type != TreacheryCardType.Poison) && p.TreacheryCards.Count(toCount => toCount.Type == c.Type) > 1);
        }
    }
}
