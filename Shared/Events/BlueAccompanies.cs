/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class BlueAccompanies : GameEvent, ILocationEvent
{
    #region Construction

    public BlueAccompanies(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BlueAccompanies()
    {
    }

    #endregion

    #region Properties

    public int _targetId;

    [JsonIgnore]
    public Location Location
    {
        get => Game.Map.LocationLookup.Find(_targetId);
        set => _targetId = Game.Map.LocationLookup.GetId(value);
    }

    [JsonIgnore]
    public Location To => Location;

    public bool Accompanies { get; set; }

    public bool ExtraAdvisor { get; set; }

    [JsonIgnore]
    public int TotalAmountOfForcesAddedToLocation => SpecialForcesAddedToLocation;

    [JsonIgnore]
    public int ForcesAddedToLocation => 0;

    [JsonIgnore]
    public int SpecialForcesAddedToLocation => (Accompanies ? 1 : 0) + (ExtraAdvisor ? 1 : 0);


    #endregion Properties

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.BlueMayAccompany = false;

        if (Accompanies)
        {
            if (Player.Occupies(Location.Territory) || !Game.IsOccupied(Location.Territory) || !Game.Applicable(Rule.BlueAdvisors))
                Player.ShipForces(Location, Game.Version >= 160 ? SpecialForcesAddedToLocation : 1);
            else
                Player.ShipAdvisors(Location, Game.Version >= 160 ? SpecialForcesAddedToLocation : 1);

            Log(Initiator, " accompany to ", Location);
            Game.LastShipmentOrMovement = this;
            Game.CheckIntrusion(this);
        }
        else
        {
            Log(Initiator, " don't accompany");
        }

        Game.DetermineNextShipmentAndMoveSubPhase();
    }

    public override Message GetMessage()
    {
        if (Accompanies)
            return Message.Express(Initiator, " accompany shipment to ", Location);
        return Message.Express(Initiator, " don't accompany shipment");
    }

    #endregion

    #region Validation

    public override Message Validate()
    {
        if (Accompanies)
        {
            if (Player.ForcesInReserve <= 0) return Message.Express("You don't have forces in reserve");
            if (!ValidTargets(Game, Player).Contains(Location)) return Message.Express("You can't accompany there");
            if (Initiator != Faction.Blue) return Message.Express("You can't accompany shipments");
            if (Location == null) return Message.Express("To not selected");
            if (Accompanies && Player.ForcesInReserve == 0) return Message.Express("No forces available");
            if (ExtraAdvisor && !MaySendExtraAdvisor(Game, Player, Location)) return Message.Express("You cannot send an extra advisor");
        }

        return null;
    }

    public static bool BlueMayAccompanyToShipmentLocation(Game g)
    {
        return g.Applicable(Rule.BlueAccompaniesToShipmentLocation) ||
               (g.Version >= 144 && g.Applicable(Rule.BlueAdvisors));
    }

    public static IEnumerable<Location> ValidTargets(Game g, Player p)
    {
        var result = new List<Location>();
        if (g.LastShipmentOrMovement.To != g.Map.PolarSink &&
            (g.Version < 167 || g.LastShipmentOrMovement.To is not Homeworld) &&
            g.LastShipmentOrMovement.To.Territory != g.AtomicsAftermath &&
            !p.Occupies(g.LastShipmentOrMovement.To.Territory) &&
            BlueMayAccompanyToShipmentLocation(g) &&
            !AllyPreventsAccompanyingToShipmentLocation(g, p))
            result.AddRange(g.LastShipmentOrMovement.To.Territory.Locations.Where(l => g.Version <= 142 || (
                l.Sector != g.SectorInStorm &&
                (g.Applicable(Rule.BlueAdvisors) || g.IsNotFull(p, l))
            )));

        result.Add(g.Map.PolarSink);
        return result;
    }

    private static bool AllyPreventsAccompanyingToShipmentLocation(Game g, Player p)
    {
        var ally = g.GetPlayer(p.Ally);

        return
            !g.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
            ally != null &&
            ally.AnyForcesIn(g.LastShipmentOrMovement.To.Territory) != 0;
    }

    public static bool MaySendExtraAdvisor(Game g, Player p, Location l)
    {
        return p.HasHighThreshold() && l == g.Map.PolarSink && p.ForcesInReserve >= 2;
    }

    #endregion Validation
}