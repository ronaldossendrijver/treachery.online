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
            return new Message(Initiator, "Using {2}, {0} swap cards with {1}.", Initiator, Target, TreacheryCardType.Karma);
        }
    }
}
