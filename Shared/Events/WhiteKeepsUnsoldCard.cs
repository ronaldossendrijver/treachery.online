/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
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
                return Message.Express(Initiator, " keep the card no faction bid on");
            }
            else
            {
                return Message.Express(Initiator, " remove the card no faction bid on from the game");
            }
        }
    }
}
