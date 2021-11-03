/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class KarmaShipmentPrevention : GameEvent
    {
        public KarmaShipmentPrevention(Game game) : base(game)
        {
        }

        public KarmaShipmentPrevention()
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
            return new Message(Initiator, "Using {2}, {0} prevent shipment by {1}.", Initiator, Target, TreacheryCardType.Karma);
        }
    }
}
