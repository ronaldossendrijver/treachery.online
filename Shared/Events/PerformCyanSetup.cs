/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformCyanSetup : GameEvent
    {
        public int _targetId;

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public PerformCyanSetup(Game game) : base(game)
        {
        }

        public PerformCyanSetup()
        {
        }

        public override Message Validate()
        {
            if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g) => g.Map.Locations(false).Where(l => l != g.Map.HiddenMobileStronghold && !g.AnyForcesIn(l.Territory));

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
