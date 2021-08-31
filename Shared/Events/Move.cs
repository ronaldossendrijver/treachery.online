/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

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

        public override string Validate()
        {
            return ValidateMove(AsAdvisors);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            var location = ForceLocations.Keys.FirstOrDefault();

            if (Passed)
            {
                return new Message(Initiator, "{0} pass move.", Initiator);
            }
            else if (location != null)
            {
                return new Message(Initiator, "{0} move from {1} to {2}.", Initiator, location.Territory, To);
            }
            else
            {
                return new Message(Initiator, "{0} move from ? to {1}.", Initiator, To);
            }
        }
    }
}
