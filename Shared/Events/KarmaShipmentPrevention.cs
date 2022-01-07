/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent shipment by ", Target);
        }
    }
}
