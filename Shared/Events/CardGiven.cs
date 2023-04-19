/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class CardGiven : PassableGameEvent
    {
        #region Construction

        public CardGiven(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public CardGiven()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsApplicable(Game game, Player p) => p.Has(game.CardThatMustBeKeptOrGivenToAlly);

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            if (!Passed)
            {
                Player.TreacheryCards.Remove(Game.CardThatMustBeKeptOrGivenToAlly);
                Player.AlliedPlayer.TreacheryCards.Add(Game.CardThatMustBeKeptOrGivenToAlly);
            }

            Game.CardThatMustBeKeptOrGivenToAlly = null;
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't give the card to their ally");
            }
            else
            {
                return Message.Express(Initiator, " give the card to their ally");
            }
        }

        #endregion Execution
    }
}
