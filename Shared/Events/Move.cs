/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
                return Message.Express(Initiator, " pass move", Initiator);
            }
            else
            {
                return Message.Express(Initiator, " move from ", location.Territory, " to ", To);
            }
        }
    }
}
