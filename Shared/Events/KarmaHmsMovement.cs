/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaHmsMovement : GameEvent
    {
        public bool Passed;

        public int _targetId;

        public KarmaHmsMovement(Game game) : base(game)
        {
        }

        public KarmaHmsMovement()
        {
        }

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override string Validate()
        {
            if (!ValidLocations(Game).Contains(Target)) return "Invalid location";

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
                return Message.Express(Initiator, "Using ", TreacheryCardType.Karma, ", ", Initiator, " move the ", Game.Map.HiddenMobileStronghold, " to ", Target);
            }
            else
            {
                return Message.Express(Initiator, " pass (further) movement of the ", Game.Map.HiddenMobileStronghold, " using ", TreacheryCardType.Karma);
            }
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm);
        }
    }
}
