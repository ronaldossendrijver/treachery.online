/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class GreySwappedCardOnBid : PassableGameEvent
    {
        #region Construction

        public GreySwappedCardOnBid(Game game) : base(game)
        {
        }

        public GreySwappedCardOnBid()
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
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                Game.GreySwappedCardOnBid = true;
                Player.TreacheryCards.Remove(Card);
                Player.TreacheryCards.Add(Game.CardsOnAuction.Draw());

                foreach (var p in Game.Players.Where(p => !Game.HasBiddingPrescience(p)))
                {
                    Game.UnregisterKnown(p, Player.TreacheryCards);
                }

                Game.CardsOnAuction.PutOnTop(Card);
                Game.RegisterKnown(Faction.Grey, Card);
                Game.Stone(Milestone.CardOnBidSwapped);
                Log();
            }

            if (!Game.BiddingRoundWasStarted)
            {
                Game.StartBiddingRound();
            }
            else
            {
                Game.Enter(IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, Game.PutNextCardOnAuction);
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " swap a card from hand with the next card on bid");
            }
            else
            {
                return Message.Express(Initiator, " don't swap a card from hand with the next card on bid");
            }
        }

        #endregion Execution
    }
}
