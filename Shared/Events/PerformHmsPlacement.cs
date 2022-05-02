/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformHmsPlacement : GameEvent
    {
        public int _targetId;

        public PerformHmsPlacement(Game game) : base(game)
        {
        }

        public PerformHmsPlacement()
        {
        }

        [JsonIgnore]
        public Location Target { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override Message Validate()
        {
            if (!ValidLocations(Game, Player).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " position the ", Game.Map.HiddenMobileStronghold, " above ", Target);
        }

        public static IEnumerable<Location> ValidLocations(Game g, Player p)
        {
            if (p.Faction != Faction.Grey)
            {
                return g.Map.Locations.Where(l => !l.Territory.IsStronghold);
            }
            else
            {
                return g.Map.Locations.Where(l => !l.Territory.IsStronghold && l.Sector != g.SectorInStorm);
            }
        }
    }
}
