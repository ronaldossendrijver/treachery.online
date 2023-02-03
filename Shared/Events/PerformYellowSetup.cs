/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class PerformYellowSetup : PlacementEvent
    {
        public PerformYellowSetup(Game game) : base(game)
        {
        }

        public PerformYellowSetup()
        {
        }

        public override Message Validate()
        {
            var p = Player;
            int numberOfSpecialForces = ForceLocations.Sum(fl => fl.Value.AmountOfSpecialForces);
            if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("You only have ", p.SpecialForcesInReserve, " ", FactionSpecialForce.Yellow);

            int numberOfForces = ForceLocations.Sum(fl => fl.Value.AmountOfForces);
            if (numberOfForces + numberOfSpecialForces != 10) return Message.Express("Distribute 10 forces");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " have set up forces");
        }
    }
}
