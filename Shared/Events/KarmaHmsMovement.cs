/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
                return new Message(Initiator, "Using {0}, {1} move the Hidden Mobile Stronghold to {2}.", TreacheryCardType.Karma, Initiator, Target);
            }
            else
            {
                return new Message(Initiator, "{0} pass (further) movement of the Hidden Mobile Stronghold using {1}.", Initiator, TreacheryCardType.Karma);
            }
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return g.Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm);
        }
    }
}
