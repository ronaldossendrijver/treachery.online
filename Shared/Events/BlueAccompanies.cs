/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BlueAccompanies : GameEvent, ILocationEvent
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

        [JsonIgnore]
        public Location To => Location;

        public bool Accompanies { get; set; }

        public override Message Validate()
        {
            if (Accompanies)
            {
                if (Player.ForcesInReserve <= 0) return Message.Express("You don't have forces in reserve");
                if (!ValidTargets(Game, Player).Contains(Location)) return Message.Express("You can't accompany there");
                if (Initiator != Faction.Blue) return Message.Express("You can't accompany shipments");
                if (Location == null) return Message.Express("To not selected");
                if (Accompanies && Player.ForcesInReserve == 0) return Message.Express("No forces available");
            }

            return null;
        }

        public static bool BlueMayAccompanyToShipmentLocation(Game g) => g.Applicable(Rule.BlueAccompaniesToShipmentLocation) || g.Version >= 144 && g.Applicable(Rule.BlueAdvisors);

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            var result = new List<Location>();
            if (g.LastShipmentOrMovement != g.Map.PolarSink &&
                !p.Occupies(g.LastShipmentOrMovement.To.Territory) &&
                BlueMayAccompanyToShipmentLocation(g) &&
                !AllyPreventsAccompanyingToShipmentLocation(g, p))
            {
                result.AddRange(g.LastShipmentOrMovement.To.Territory.Locations.Where(l => g.Version <= 142 || (
                    l.Sector != g.SectorInStorm &&
                    (g.Applicable(Rule.BlueAdvisors) || g.IsNotFull(p, l))
                    )));
            }

            result.Add(g.Map.PolarSink);
            return result;
        }

        private static bool AllyPreventsAccompanyingToShipmentLocation(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return
                !g.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                (ally != null) &&
                ally.AnyForcesIn(g.LastShipmentOrMovement.To.Territory) != 0;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Accompanies)
            {
                return Message.Express(Initiator, " accompany shipment to ", Location);
            }
            else
            {
                return Message.Express(Initiator, " don't accompany shipment");
            }
        }
    }
}
