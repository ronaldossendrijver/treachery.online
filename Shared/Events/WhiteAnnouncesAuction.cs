/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteAnnouncesAuction : GameEvent
    {
        #region Construction

        public WhiteAnnouncesAuction(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public WhiteAnnouncesAuction()
        {
        }

        #endregion Construction

        #region Properties

        public bool First { get; set; }

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
            Game.WhiteOccupierSpecifiedCard = false;
            Log();

            int threshold = Game.Version > 150 ? 0 : 1;
            if (Game.Version > 150)
            {
                Game.NumberOfCardsOnAuction--;
            }

            if (!First && Game.NumberOfCardsOnAuction > threshold)
            {
                Game.WhiteAuctionShouldStillHappen = true;
            }

            if (Game.NumberOfCardsOnAuction == threshold)
            {
                Game.RegularBiddingIsDone = true;
            }

            Game.Enter(First || Game.NumberOfCardsOnAuction == threshold, Phase.WhiteSpecifyingAuction, Game.StartRegularBidding);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " will auction a card from their cache ", First ? "first" : "last");
        }

        #endregion Execution
    }
}
