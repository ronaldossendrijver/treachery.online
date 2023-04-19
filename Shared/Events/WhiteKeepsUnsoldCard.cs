/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteKeepsUnsoldCard : PassableGameEvent
    {
        #region Construction

        public WhiteKeepsUnsoldCard(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public WhiteKeepsUnsoldCard()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            var card = Game.CardsOnAuction.Draw();
            Game.RegisterWonCardAsKnown(card);

            if (!Passed)
            {
                Player.TreacheryCards.Add(card);
                LogTo(Initiator, "You get: ", card);
                Game.FinishBid(Player, card, Game.Version < 152);
            }
            else
            {
                Game.RemovedTreacheryCards.Add(card);
                Game.FinishBid(null, card, Game.Version < 152);
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " keep the card no faction bid on");
            }
            else
            {
                return Message.Express(Initiator, " remove the card no faction bid on from the game");
            }
        }

        #endregion Execution
    }
}
