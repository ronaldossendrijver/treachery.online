/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaBrownDiscard : GameEvent
    {
        public string _cardIds;

        public KarmaBrownDiscard(Game game) : base(game)
        {
        }

        public KarmaBrownDiscard()
        {
        }

        [JsonIgnore]
        public IEnumerable<TreacheryCard> Cards
        {
            get
            {
                return IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public override Message Validate()
        {
            var karmaCardToUse = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            if (karmaCardToUse == null) return Message.Express("You don't have a ", TreacheryCardType.Karma, " card");
            if (Cards.Contains(karmaCardToUse)) return Message.Express("You can't select the ", TreacheryCardType.Karma, " you need to play to use this power");
            
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, "Using ", TreacheryCardType.Karma, ", ", Initiator, " discard ", Cards.Select(c => MessagePart.Express(" ", c, " ")), "to get ", new Payment(Cards.Count() * 3));
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            var karmaCardToUse = p.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            return p.TreacheryCards.Where(c => c != karmaCardToUse);
        }
    }
}
