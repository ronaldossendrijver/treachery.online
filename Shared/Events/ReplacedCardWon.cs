/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ReplacedCardWon : GameEvent
    {
        public ReplacedCardWon(Game game) : base(game)
        {
        }

        public ReplacedCardWon()
        {
        }

        public bool Passed { get; set; }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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
    }
}
