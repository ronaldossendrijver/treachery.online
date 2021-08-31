/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteKeepsUnsoldCard : GameEvent
    {
        public bool Passed;

        public WhiteKeepsUnsoldCard(Game game) : base(game)
        {
        }

        public WhiteKeepsUnsoldCard()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return new Message(Initiator, "{0} keep the card no faction bid on.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} remove the card no faction bid on from the game.", Initiator);
            }
        }
    }
}
