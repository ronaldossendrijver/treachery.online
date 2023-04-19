/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ReplacedCardWon : PassableGameEvent
    {
        #region Construction

        public ReplacedCardWon(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ReplacedCardWon()
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
            if (!Passed)
            {
                Game.Discard(Game.CardJustWon);
                var initiator = GetPlayer(Initiator);
                var newCard = Game.DrawTreacheryCard();
                initiator.TreacheryCards.Add(newCard);
                Game.Stone(Milestone.CardWonSwapped);

                if (Game.ReplacingBoughtCardUsingNexus)
                {
                    Game.PlayNexusCard(Player, "Secret Ally", "to replace the card they just bought");
                    Game.ReplacingBoughtCardUsingNexus = false;
                }
                else
                {
                    Log();
                }

                LogTo(initiator.Faction, "You replaced your ", Game.CardJustWon, " with a ", newCard);
            }

            if (Game.CardJustWon == Game.CardSoldOnBlackMarket)
            {
                Game.EnterWhiteBidding();
            }
            else
            {
                Game.DetermineNextStepAfterCardWasSold();
            }
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " keep the card they just won");
            }
            else
            {
                return Message.Express(Initiator, " discard the card they just won and draw a new card");
            }
        }

        #endregion Execution
    }
}
