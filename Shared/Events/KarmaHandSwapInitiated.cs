/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class KarmaHandSwapInitiated : GameEvent
    {
        public KarmaHandSwapInitiated(Game game) : base(game)
        {
        }

        public KarmaHandSwapInitiated()
        {
        }

        public Faction Target { get; set; }

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
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " swap cards with ", Target);
        }
    }
}
