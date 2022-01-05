/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class Caravan : PlacementEvent
    {
        public Caravan(Game game) : base(game)
        {
        }

        public Caravan()
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
                return new Message(Initiator, "{0} pass {1}.", Initiator, TreacheryCardType.Caravan);
            }
            else if (location != null)
            {
                return new Message(Initiator, "{0} move from {1} to {2} by {3}.", Initiator, location.Territory, To, TreacheryCardType.Caravan);
            }
            else
            {
                return new Message(Initiator, "{0} move from ? to {1}.", Initiator, To);
            }
        }
    }
}
