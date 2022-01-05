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

        public override string Validate()
        {
            var karmaCardToUse = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            if (karmaCardToUse == null) return string.Format("You don't have a {0} card", TreacheryCardType.Karma);
            if (Cards.Contains(karmaCardToUse)) return string.Format("You can't select the {0} you need to play to use this power", TreacheryCardType.Karma);
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "Using {0}, {1} discard {2} to gain {3}.", TreacheryCardType.Karma, Initiator, Cards, Cards.Count() * 3);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            var karmaCardToUse = p.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            return p.TreacheryCards.Where(c => c != karmaCardToUse);
        }
    }
}
