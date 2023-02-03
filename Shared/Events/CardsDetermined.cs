/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class CardsDetermined : GameEvent
    {
        public string _treacheryCardIds;
        public string _whiteCardIds;

        public CardsDetermined(Game game) : base(game)
        {
        }

        public CardsDetermined()
        {
        }

        [JsonIgnore]
        public IEnumerable<TreacheryCard> TreacheryCards
        {
            get
            {
                return IdStringToObjects(_treacheryCardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _treacheryCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        [JsonIgnore]
        public IEnumerable<TreacheryCard> WhiteCards
        {
            get
            {
                return IdStringToObjects(_whiteCardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _whiteCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public override Message Validate()
        {
            if (TreacheryCards.Count() + WhiteCards.Count() < Game.Players.Sum(p => p.MaximumNumberOfCards) + 1) return Message.Express("Not enough cards selected");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express("Card decks were customized.");
        }

        public Message GetVerboseMessage()
        {
            if (WhiteCards.Any())
            {
                return Message.Express("Treachery Cards: ", TreacheryCards, ". ", Faction.White, " Cards: ", WhiteCards);
            }
            else
            {
                return Message.Express("Treachery Cards: ", TreacheryCards);
            }

        }
    }
}
