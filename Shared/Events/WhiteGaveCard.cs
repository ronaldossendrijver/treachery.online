/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteGaveCard : GameEvent
    {
        public WhiteGaveCard(Game game) : base(game)
        {
        }

        public WhiteGaveCard()
        {
        }

        public int _cardId = -1;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.Lookup.GetId(value);
            }
        }

        public override string Validate()
        {
            return "";
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return p.TreacheryCards.Where(c => c.Rule == Rule.WhiteTreacheryCards);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} give a card to their ally.", Initiator);
        }
    }
}
