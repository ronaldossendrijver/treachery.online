/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            if (Passed)
            {
                return new Message(Initiator, "{0} keep the card they just won.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} discard the card they just won and draw a new card.", Initiator);
            }
        }
    }
}
