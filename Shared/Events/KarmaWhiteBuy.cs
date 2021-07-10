/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class KarmaWhiteBuy : GameEvent
    {
        public int _cardId;

        public KarmaWhiteBuy(Game game) : base(game)
        {
        }

        public KarmaWhiteBuy()
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
            if (Player.Resources < 3) return Skin.Current.Format("You can't pay 3 {0}", Concept.Resource);
            if (!Game.WhiteDeck.Items.Contains(Card)) return "Invalid card";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} buy one of their special Treachery cards.", Initiator);
        }
    }
}
