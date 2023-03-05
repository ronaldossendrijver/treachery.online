/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class DiscoveryRevealed : PassableGameEvent
    {
        public DiscoveryRevealed(Game game) : base(game)
        {
        }

        public DiscoveryRevealed()
        {
        }

        public int _locationId;

        [JsonIgnore]
        public Location Location { get { return Game.Map.LocationLookup.Find(_locationId); } set { _locationId = Game.Map.LocationLookup.GetId(value); } }

        public override Message Validate()
        {
            

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't reveal a discovery");
            }
            else 
            {
                return Message.Express(Initiator, " reveal a discovery in ", Location.Territory);
            }
        }

        public static bool Applicable(Game g, Player p) => g.CurrentMainPhase == MainPhase.Collection && GetLocations(g, p).Any();

        public static IEnumerable<Location> GetLocations(Game g, Player p) => g.DiscoveriesOnPlanet.Where(kvp => g.PendingDiscoveries.Contains(kvp.Value.Token) && p.Occupies(kvp.Key.Territory)).Select(kvp => kvp.Key);

    }
}
