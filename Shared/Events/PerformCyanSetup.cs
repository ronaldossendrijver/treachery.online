/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformCyanSetup : PlacementEvent
    {
        public PerformCyanSetup(Game game) : base(game)
        {
        }

        public PerformCyanSetup()
        {
        }

        public override Message Validate()
        {
            if (ForceLocations.Sum(fl => fl.Value.AmountOfForces) != 6) return Message.Express("Place 6 forces");
            if (ForceLocations.Keys.Select(l => l.Territory).Distinct().Count() > 1) Message.Express("Place forces within 1 territory");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g) => g.Map.Locations().Where(l => l != g.Map.HiddenMobileStronghold && !g.AnyForcesIn(l.Territory));

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
