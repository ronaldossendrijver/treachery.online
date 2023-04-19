/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class GreyRemovedCardFromAuction : GameEvent
    {
        #region Construction

        public GreyRemovedCardFromAuction(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public GreyRemovedCardFromAuction()
        {
        }

        #endregion Construction

        #region Properties

        public bool PutOnTop { get; set; }

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
            Game.CardsOnAuction.Items.Remove(Card);

            if (PutOnTop)
            {
                Game.TreacheryDeck.PutOnTop(Card);
            }
            else
            {
                Game.TreacheryDeck.PutOnBottom(Card);
            }

            Game.RegisterKnown(Faction.Grey, Card);
            Game.CardsOnAuction.Shuffle();
            Game.Stone(Milestone.Shuffled);
            Log();

            if (Game.GreyMaySwapCardOnBid)
            {
                if (Game.Version < 113 || !Game.Prevented(FactionAdvantage.GreySwappingCard))
                {
                    Game.Enter(Phase.GreySwappingCard);
                }
                else
                {
                    Game.LogPreventionByKarma(FactionAdvantage.GreySwappingCard);
                    if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreySwappingCard);
                    Game.StartBiddingRound();
                }
            }
            else
            {
                Game.StartBiddingRound();
            }
        }

        public override Message GetMessage()
        {
            if (PutOnTop)
            {
                return Message.Express(Initiator, " put a card on top of the Treachery Card deck");
            }
            else
            {
                return Message.Express(Initiator, " put a card at the bottom of the Treachery Card deck");
            }
        }

        #endregion Execution
    }
}
