/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Move : PlacementEvent
    {
        public Move(Game game) : base(game)
        {
        }

        public Move()
        {
        }

        public bool AsAdvisors { get; set; }

        public override Message Validate()
        {
            if (!Passed && Game.InOrangeCunningShipment) return Message.Express("You cannot move after Cunning shipment");

            return ValidateMove(AsAdvisors);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass move");
            }
            else
            {
                return Message.Express(Initiator, " move to ", To);
            }
        }
    }
}
