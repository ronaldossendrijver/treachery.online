/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
    }
}
