/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class CardGiven : PassableGameEvent
    {
        public CardGiven(Game game) : base(game)
        {
        }

        public CardGiven()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        public static bool IsApplicable(Game game, Player p) => p.TreacheryCards.Contains(game.CardThatMustBeKeptOrGivenToAlly);

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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
    }
}
