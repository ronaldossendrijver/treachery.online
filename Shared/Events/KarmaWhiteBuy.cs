/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaWhiteBuy : GameEvent
    {
        #region Construction

        public KarmaWhiteBuy(Game game) : base(game)
        {
        }

        public KarmaWhiteBuy()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Player.Resources < 3) return Message.Express("You can't pay ", Payment.Of(3));
            if (!Game.WhiteCache.Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Log();
            Player.TreacheryCards.Add(Card);
            Game.WhiteCache.Remove(Card);
            Player.Resources -= 3;
            Player.SpecialKarmaPowerUsed = true;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Karma, " to buy a card from their cache for ", Payment.Of(3));
        }

        #endregion Execution
    }
}
