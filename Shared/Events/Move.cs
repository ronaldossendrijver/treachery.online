/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
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
            if (Passed) return "";

            var p = Player;

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > p.ForcesIn(bl.Key));
            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > p.SpecialForcesIn(bl.Key));

            if (tooManyForces) return Skin.Current.Format("Invalid amount of {0}.", p.Force);
            if (tooManySpecialForces) return Skin.Current.Format("Invalid amount of {0}.", p.SpecialForce);
            if (forceAmount == 0 && specialForceAmount == 0) return "No forces selected.";

            if (To == null) return "To not selected.";
            if (!ValidTargets(Game, p, ForceLocations).Contains(To)) return "Invalid To location.";
            if (AsAdvisors && !(p.Is(Faction.Blue) && Game.Applicable(Rule.BlueAdvisors))) return "You can't move as advisors.";

            if (Initiator == Faction.Blue)
            {
                if (AsAdvisors && p.ForcesIn(To.Territory) > 0) return "You have fighters there, so you can't move as advisors.";
                if (!AsAdvisors && p.SpecialForcesIn(To.Territory) > 0) return "You have advisors there, so you can't move as fighters.";
            }

            return "";
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
