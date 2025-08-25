/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class YellowRidesMonster : PlacementEvent
{
    #region Construction

    public YellowRidesMonster(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public YellowRidesMonster()
    {
    }

    #endregion Construction

    #region Properties

    public int ForcesFromReserves { get; set; }

    public int SpecialForcesFromReserves { get; set; }

    [JsonIgnore]
    public override int TotalAmountOfForcesAddedToLocation => base.TotalAmountOfForcesAddedToLocation + ForcesFromReserves + SpecialForcesFromReserves;

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;

        if (Game.Version >= 155 && TotalAmountOfForcesAddedToLocation == 0) return Message.Express("No forces selected");
        if (To == null) return Message.Express("Target location not selected");
        if (!ValidTargets(Game, Player).Contains(To)) return Message.Express("Invalid target location");

        var tooManyForces = ForceLocations.Any(bl => bl.Value.AmountOfForces > Player.ForcesIn(bl.Key));
        if (tooManyForces) return Message.Express("Invalid amount of forces");

        var tooManySpecialForces = ForceLocations.Any(bl => bl.Value.AmountOfSpecialForces > Player.SpecialForcesIn(bl.Key));
        if (tooManySpecialForces) return Message.Express("Invalid amount of special forces");

        if (ForcesFromReserves > MaxForcesFromReserves(Game, Player, false) ||
            SpecialForcesFromReserves > MaxForcesFromReserves(Game, Player, true)) return Message.Express("You can't ride with that many reserves");

        return null;
    }

    public static bool IsApplicable(Game g)
    {
        return ToRide(g) != null;
    }

    public static MonsterAppearence ToRide(Game g)
    {
        var yellow = g.GetPlayer(Faction.Yellow);
        if (yellow != null)
            return g.Monsters.FirstOrDefault(m =>
                (m.IsGreatMonster && yellow.AnyForcesInReserves > 0 && !g.Prevented(FactionAdvantage.YellowRidesMonster)) ||
                (!m.IsGreatMonster && LocationsWithForcesThatCanRide(g, yellow, m.Territory).Any()));

        return null;
    }

    private static IEnumerable<Location> LocationsWithForcesThatCanRide(Game g, Player yellow, Territory territory)
    {
        if (g.Version >= 136)
        {
            var mayRideFromStorm = g.Applicable(Rule.YellowMayMoveIntoStorm);

            if (NexusPlayed.CanUseCunning(yellow) && yellow.AnyForcesIn(territory) == 0)
                return yellow.ForcesOnPlanet.Keys.Where(l => !l.IsProtectedFromStorm && !l.IsStronghold && (mayRideFromStorm || !g.IsInStorm(l)));
            if (!g.Prevented(FactionAdvantage.YellowRidesMonster))
                return territory.Locations.Where(l => (mayRideFromStorm || !g.IsInStorm(l)) && yellow.AnyForcesIn(l) > 0);
            return [];
        }

        return territory.Locations.Where(l => yellow.AnyForcesIn(l) > 0);
    }

    public static IEnumerable<Location> ValidSources(Game g)
    {
        var yellow = g.GetPlayer(Faction.Yellow);
        if (yellow != null)
        {
            var toRide = ToRide(g);
            if (!toRide.IsGreatMonster) return LocationsWithForcesThatCanRide(g, yellow, toRide.Territory);
        }

        return [];
    }

    public static int MaxForcesFromReserves(Game g, Player p, bool special)
    {
        var rideFrom = ToRide(g);
        if (rideFrom != null && ToRide(g).IsGreatMonster) return special ? p.SpecialForcesInReserve : p.ForcesInReserve;

        return 0;
    }

    public static IEnumerable<Location> ValidTargets(Game g, Player p)
    {
        var mayMoveIntoStorm = p.Faction == Faction.Yellow && g.Applicable(Rule.YellowMayMoveIntoStorm) && g.Applicable(Rule.YellowStormLosses);

        return g.Map.Locations(false).Where(l =>
            (mayMoveIntoStorm || l.Sector != g.SectorInStorm) &&
            (l is not AttachedLocation al || (al.AttachedToLocation != null && al.AttachedToLocation.Sector != g.SectorInStorm)) &&
            (!l.Territory.IsStronghold || g.NrOfOccupantsExcludingFaction(l, p.Faction) < 2) &&
            (!p.HasAlly || Equals(l, g.Map.PolarSink) || !p.AlliedPlayer.Occupies(l)));
    }
    
    private static bool IsEitherValidDiscoveryOrNoDiscovery(Location l)
    {
        return l is not DiscoveredLocation ds || ds.Visible;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var toRide = ToRide(Game);

        if (Game.Version <= 150)
            Game.Monsters.RemoveAt(0);
        else
            Game.Monsters.Remove(toRide);

        if (!Passed)
        {
            if (ForceLocations.Keys.Any(l => l.Territory != toRide.Territory)) Game.PlayNexusCard(Player, "cunning", "to ride from any territory on the planet");

            var initiator = GetPlayer(Initiator);
            Game.LastShipmentOrMovement = this;
            foreach (var fl in ForceLocations)
            {
                var from = fl.Key;
                initiator.MoveForces(from, To, fl.Value.AmountOfForces);
                initiator.MoveSpecialForces(from, To, fl.Value.AmountOfSpecialForces);
                Log(
                    MessagePart.ExpressIf(fl.Value.AmountOfForces > 0, fl.Value.AmountOfForces, initiator.Force),
                    MessagePart.ExpressIf(fl.Value.AmountOfSpecialForces > 0, fl.Value.AmountOfSpecialForces, initiator.SpecialForce),
                    " ride from ",
                    from,
                    " to ",
                    To);
            }

            if (ForcesFromReserves > 0 || SpecialForcesFromReserves > 0)
            {
                if (ForcesFromReserves > 0) initiator.ShipForces(To, ForcesFromReserves);
                if (SpecialForcesFromReserves > 0) initiator.ShipSpecialForces(To, SpecialForcesFromReserves);
                Log(
                    MessagePart.ExpressIf(ForcesFromReserves > 0, ForcesFromReserves, initiator.Force),
                    MessagePart.ExpressIf(SpecialForcesFromReserves > 0, SpecialForcesFromReserves, initiator.SpecialForce),
                    " ride from their reserves to ",
                    To);
            }

            Game.FlipBlueAdvisorsWhenAlone();
            Game.CheckIntrusion(this);
        }
        else
        {
            Log(Initiator, " pass a ride on ", Concept.Monster);
        }

        Game.DetermineNextShipmentAndMoveSubPhase();
    }

    public override Message GetMessage()
    {
        if (Passed) return Message.Express(Initiator, " don't ride");

        var from = ForceLocations.Keys.FirstOrDefault()?.Territory;
        if (from != null)
            return Message.Express(Initiator, " ride from ", from, " to ", To);
        return Message.Express(Initiator, " ride from reserves to ", To);
    }

    #endregion Execution
}