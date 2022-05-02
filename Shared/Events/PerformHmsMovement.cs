/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
        {
            if (!Passed)
            {
                if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");
            }

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " move the ", Game.Map.HiddenMobileStronghold, " to ", Target);
            }
            else
            {
                return Message.Express(Initiator, " pass (further) movement of the ", Game.Map.HiddenMobileStronghold);
            }
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm).Where(l => !l.IsStronghold);
        }
    }
}
