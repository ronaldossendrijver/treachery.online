/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            var p = Player;

            var forceAmount = ForceLocations.Values.Sum(b => b.AmountOfForces);
            bool tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > p.ForcesIn(bl.Key));
            if (tooManyForces) return Skin.Current.Format("Invalid amount of {0}.", p.Force);

            var specialForceAmount = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            bool tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > p.SpecialForcesIn(bl.Key));
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
