/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BlueAccompanies : GameEvent
    {
        public int _targetId;

        public BlueAccompanies(Game game) : base(game)
        {
        }

        public BlueAccompanies()
        {
        }

        [JsonIgnore]
        public Location Location { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public bool Accompanies { get; set; }

        public override string Validate()
        {
            if (Accompanies)
            {
                if (Player.ForcesInReserve <= 0) return "You don't have forces in reserve";
                if (!ValidTargets(Game, Player).Contains(Location)) return "You can't accompany there";
                if (Initiator != Faction.Blue) return "Your faction can't accompany forces";
                if (Location == null) return "To not selected.";
                if (Accompanies && Player.ForcesInReserve == 0) return "No forces available";
            }

            return "";
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            var result = new List<Location>();
            if (g.LastShippedOrMovedTo != g.Map.PolarSink && !p.Occupies(g.LastShippedOrMovedTo.Territory) && g.Applicable(Rule.BlueAccompaniesToShipmentLocation) && !AllyPreventsAccompanyingToShipmentLocation(g, p))
            {
                result.AddRange(g.LastShippedOrMovedTo.Territory.Locations);
            }

            result.Add(g.Map.PolarSink);
            return result;
        }

        private static bool AllyPreventsAccompanyingToShipmentLocation(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return
                !g.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                g.Version >= 38 &&
                (ally != null) &&
                ally.AnyForcesIn(g.LastShippedOrMovedTo.Territory) != 0;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Accompanies)
            {
                return new Message(Initiator, "{0} accompany a shipment to {1}.", Initiator, Location);
            }
            else
            {
                return new Message(Initiator, "{0} don't accompany shipment.", Initiator);
            }
        }
    }
}
