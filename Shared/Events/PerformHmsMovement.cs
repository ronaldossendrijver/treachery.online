/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformHmsMovement : GameEvent
    {
        public bool Passed;

        public int _targetId;

        public PerformHmsMovement(Game game) : base(game)
        {
        }

        public PerformHmsMovement()
        {
        }

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override string Validate()
        {
            if (!Passed)
            {
                if (!ValidLocations(Game).Contains(Target)) return "Invalid location";
            }

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return new Message(Initiator, "{0} move the Hidden Mobile Stronghold to {1}.", Initiator, Target);
            }
            else
            {
                return new Message(Initiator, "{0} pass (further) movement of the Hidden Mobile Stronghold.", Initiator);
            }
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            if (g.Version < 69)
            {
                return g.Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm);
            }
            else
            {
                return g.Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm).Where(l => !l.IsStronghold);
            }
        }
    }
}
