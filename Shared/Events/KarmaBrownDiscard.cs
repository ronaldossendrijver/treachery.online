/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaBrownDiscard : GameEvent
    {
        #region Construction

        public KarmaBrownDiscard(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaBrownDiscard()
        {
        }

        #endregion Construction

        #region Properties

        public string _cardIds;

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

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            var karmaCardToUse = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            if (karmaCardToUse == null) return Message.Express("You don't have a ", TreacheryCardType.Karma, " card");
            if (Cards.Contains(karmaCardToUse)) return Message.Express("You can't select the ", TreacheryCardType.Karma, " you need to play to use this power");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            var karmaCardToUse = p.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
            return p.TreacheryCards.Where(c => c != karmaCardToUse);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Log();

            foreach (var card in Cards)
            {
                Game.Discard(Player, card);
            }

            Player.Resources += Cards.Count() * 3;
            Player.SpecialKarmaPowerUsed = true;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, "Using ", TreacheryCardType.Karma, ", ", Initiator, " discard ", Cards.Select(c => MessagePart.Express(" ", c, " ")), "to get ", Payment.Of(Cards.Count() * 3));
        }

        #endregion Execution
    }
}
