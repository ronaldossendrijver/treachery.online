/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class PerformSetup : PlacementEvent
    {
        public PerformSetup(Game game) : base(game)
        {
        }

        public PerformSetup()
        {
        }

        public int Resources { get; set; }

        public override Message Validate()
        {
            var faction = Game.NextFactionToPerformCustomSetup;
            var p = Game.GetPlayer(faction);
            int numberOfSpecialForces = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
            if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("Too many ", p.SpecialForce);

            int numberOfForces = ForceLocations.Values.Sum(b => b.AmountOfForces);
            if (numberOfForces > p.ForcesInReserve) return Message.Express("Too many ", p.Force);

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " initial positions and ", Concept.Resource, " (", new Payment(Resources), ") determined");
        }
    }
}
