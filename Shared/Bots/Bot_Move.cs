/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public partial class Player
{
    protected virtual Move DetermineMove()
    {
        LogInfo("DetermineMove()");

        if (Game.InOrangeCunningShipment) return new Move(Game, Faction) { Passed = true };

        var moved = DetermineMovedBatallion(true);

        if (moved == null) return new Move(Game, Faction) { Passed = true };

        var forces = new Dictionary<Location, Battalion>
        {
            { moved.From, moved.Batallion }
        };

        var asAdvisors = Faction == Faction.Blue && SpecialForcesIn(moved.To.Territory) > 0;
        var result = new Move(Game, Faction) { Passed = false, To = moved.To, ForceLocations = forces, AsAdvisors = asAdvisors };

        var validationError = result.Validate();
        if (validationError != null)
        {
            LogInfo(validationError);
            return null;
        }

        return result;
    }

    protected virtual Caravan DetermineCaravan()
    {
        LogInfo("DetermineCaravan()");

        if (ForcesOnPlanet.Count(kvp => !kvp.Key.IsStronghold) <= 1) return null;

        var moved = DetermineMovedBatallion(false);

        if (moved == null) return null;

        var forces = new Dictionary<Location, Battalion>
        {
            { moved.From, moved.Batallion }
        };

        var asAdvisors = Faction == Faction.Blue && SpecialForcesIn(moved.To.Territory) > 0;
        var result = new Caravan(Game, Faction) { Passed = false, To = moved.To, ForceLocations = forces, AsAdvisors = asAdvisors };

        decidedShipmentAction = ShipmentDecision.None;

        if (!result.IsValid)
        {
            LogInfo(result.Validate());
            return null;
        }

        return result;
    }

    private MovedBatallion DetermineMovedBatallion(bool includeLowPrioMoves)
    {
        var winning = IAmWinning;
        LogInfo("DetermineMove(). AllIn: {0}, Winning: {1}.", LastTurn, winning);

        if (decidedShipmentAction == ShipmentDecision.StrongholdNearResources && ForcesInLocations.ContainsKey(decidedShipment.To))
        {
            LogInfo("Move to spice");
            var toMove = ForcesInLocations[decidedShipment.To].Take(decidedShipment.ForceAmount + decidedShipment.SpecialForceAmount, Faction == Faction.Grey);

            if (WithinRange(decidedShipment.To, finalDestination, toMove))
            {
                var move = ConstructMove(finalDestination, decidedShipment.To, toMove);
                if (move != null) return move;
            }
        }

        //Move biggest batallion threatened by ally presence
        var battalionThreatenedByAllyPresence = BattalionThatShouldBeMovedDueToAllyPresence;
        if (battalionThreatenedByAllyPresence.Key != null)
        {
            var moveTo = DetermineMostSuitableNearbyLocation(battalionThreatenedByAllyPresence, true, true);
            if (moveTo != null)
            {
                LogInfo("Move biggest batallion threatened by ally presence");
                var move = ConstructMove(moveTo, battalionThreatenedByAllyPresence.Key, battalionThreatenedByAllyPresence.Value);
                if (move != null) return move;
            }
        }

        //Move biggest batallion threatened by storm
        if (!LastTurn && !winning)
        {
            var biggestBattalionThreatenedByStorm = BiggestBattalionThreatenedByStormWithoutSpice;
            if (biggestBattalionThreatenedByStorm.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionThreatenedByStorm, true, false);
                if (moveTo != null)
                {
                    LogInfo("Move biggest batallion threatened by storm");
                    var move = ConstructMove(moveTo, biggestBattalionThreatenedByStorm.Key, biggestBattalionThreatenedByStorm.Value);
                    if (move != null) return move;
                }
            }
        }

        //Move from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock
        var biggestBattalionInSpicelessNonStronghold = BiggestBattalionInSpicelessNonStrongholdLocationOnRock;
        if (biggestBattalionInSpicelessNonStronghold.Key != null)
        {
            var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStronghold, false, false);
            if (moveTo != null)
            {
                LogInfo("Move from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock");
                var move = ConstructMove(moveTo, biggestBattalionInSpicelessNonStronghold.Key, biggestBattalionInSpicelessNonStronghold.Value);
                if (move != null) return move;
            }
        }

        //Move from useless location (if not needed for atomics) to safe stronghold or spice or rock
        var biggestBattalionInSpicelessNonStrongholdNotNearStronghold = BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold;
        if (biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key != null)
        {
            var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStrongholdNotNearStronghold, true, false);
            if (moveTo != null)
            {
                LogInfo("Move from useless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock");
                var move = ConstructMove(moveTo, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Value);
                if (move != null) return move;
            }
        }

        if (Faction == Faction.Blue)
        {
            //Move a stack of advisors from a stronghold to a nearby vacant stronghold
            var biggestStackOfAdvisorsInStrongholdNearVacantStronghold = BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold;
            if (biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Key != null)
            {
                var moveTo = VacantAndSafeNearbyStronghold(biggestStackOfAdvisorsInStrongholdNearVacantStronghold);
                if (moveTo != null)
                {
                    LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold");
                    var move = ConstructMove(moveTo, biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Key, biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Value);
                    if (move != null) return move;
                }
            }
        }

        if (!includeLowPrioMoves) return null;

        //Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold
        if (!winning)
        {
            var bigUnthreatenedBattalionNearVacantStronghold = BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold;
            if (bigUnthreatenedBattalionNearVacantStronghold.Key != null)
            {
                var moveTo = VacantAndSafeNearbyStronghold(bigUnthreatenedBattalionNearVacantStronghold);
                if (moveTo != null)
                {
                    LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold");
                    var move = ConstructMove(moveTo, bigUnthreatenedBattalionNearVacantStronghold.Key, bigUnthreatenedBattalionNearVacantStronghold.Value.TakeHalf());
                    if (move != null) return move;
                }
            }
        }

        //Move partly from unthreatened stronghold location with enough forces to nearby spice
        if (!LastTurn && !winning)
        {
            var bigUnthreatenedBattalionNearSpice = BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice;
            if (bigUnthreatenedBattalionNearSpice.Key != null)
            {
                var moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value);
                if (moveTo == null) moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value, true);

                if (moveTo != null)
                {
                    LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby spice");
                    var forcesForCollection = Math.Max(2, Math.Min(DetermineForcesNeededForCollection(moveTo), bigUnthreatenedBattalionNearSpice.Value.TotalAmountOfForces / 2));
                    var move = ConstructMove(moveTo, bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value.Take(forcesForCollection, Faction == Faction.Grey));
                    if (move != null) return move;
                }
            }
        }

        //Move towards shield wall to detonate family atomics if needed
        if (!LastTurn && !winning)
            //var locationsNearShieldWall = Map.FindNeighbours(Game.Map.ShieldWall.MiddleLocation, 1, false, Faction, Game.SectorInStorm, null);
            if (!LastTurn && Game.SectorInStorm >= 0 && Game.SectorInStorm <= 8 && TreacheryCards.Any(c => c.Type == TreacheryCardType.Metheor) && !MetheorPlayed.HasForcesAtOrNearShieldWall(Game, this))
            {
                Location from = null;
                Location to = null;
                var troopThatWillBlowShieldWall = DetermineBattalionThatWillDestroyShieldWall(ref from, ref to);

                if (troopThatWillBlowShieldWall != null)
                {
                    LogInfo("Move towards shield wall to detonate family atomics if needed");
                    var move = ConstructMove(to, from, troopThatWillBlowShieldWall);
                    if (move != null) return move;
                }
            }

        LogInfo("I'm deciding not to move");
        decidedShipmentAction = ShipmentDecision.None;
        return null;
    }

    protected virtual MovedBatallion ConstructMove(Location to, Location from, Battalion battalion)
    {
        if (Faction == Faction.White && Game.HasLowThreshold(Faction.White) && battalion.AmountOfSpecialForces != 0)
            return null;
        return new MovedBatallion { To = to, From = from, Batallion = battalion };
    }


    protected class MovedBatallion
    {
        internal Location From;
        internal Location To;
        internal Battalion Batallion;
    }

    protected virtual Battalion DetermineBattalionThatWillDestroyShieldWall(ref Location from, ref Location to)
    {
        from = Game.Map.PolarSink;
        to = Game.SectorInStorm != 8 ? Game.Map.FalseWallEast.Locations.First(l => l.Sector == 8) : Game.Map.FalseWallEast.Locations.First(l => l.Sector == 7);
        var result = FindOneTroopThatCanSafelyMove(from, to);

        if (result == null)
        {
            from = Game.Map.TueksSietch;
            to = Game.Map.PastyMesa.Locations.First(l => l.Sector == 7);
            result = FindOneTroopThatCanSafelyMove(from, to);
        }

        if (result == null)
        {
            from = Game.Map.Arrakeen;
            to = Game.SectorInStorm != 8 ? Game.Map.FalseWallEast.Locations.First(l => l.Sector == 8) : Game.Map.FalseWallEast.Locations.First(l => l.Sector == 7);
            result = FindOneTroopThatCanSafelyMove(from, to);
        }

        if (result == null)
        {
            from = Game.Map.Carthag;
            to = Game.SectorInStorm != 8 ? Game.Map.FalseWallEast.Locations.First(l => l.Sector == 8) : Game.Map.FalseWallEast.Locations.First(l => l.Sector == 7);
            result = FindOneTroopThatCanSafelyMove(from, to);
        }

        return result;
    }

    protected virtual FlightUsed DetermineFlightUsed()
    {
        LogInfo("DetermineFlightUsed()");

        if (ForcesOnPlanet.Count(kvp => !kvp.Key.IsStronghold) > 1 && ForcesOnPlanet.Where(kvp => !kvp.Key.IsStronghold).Sum(lwf => lwf.Value.TotalAmountOfForces) > 3)
            return new FlightUsed(Game, Faction) { MoveThreeTerritories = false };
        return null;
    }

    protected virtual FlightDiscoveryUsed DetermineFlightDiscoveryUsed()
    {
        LogInfo("DetermineFlightDiscoveryUsed()");

        if (!Game.HasOrnithopters(this) && (
                BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold.Key != null ||
                BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice.Key != null ||
                BiggestBattalionThreatenedByStormWithoutSpice.Key != null ||
                BiggestBattalionInSpicelessNonStrongholdLocationOnRock.Key != null
            ))
            return new FlightDiscoveryUsed(Game, Faction);
        return null;
    }

    private Location DetermineMostSuitableNearbyLocation(KeyValuePair<Location, Battalion> battalionAtLocation, bool includeSecondBestLocations, bool mustMove)
    {
        return DetermineMostSuitableNearbyLocation(battalionAtLocation.Key, battalionAtLocation.Value, includeSecondBestLocations, mustMove);
    }

    private Location DetermineMostSuitableNearbyLocation(Location location, Battalion battalion, bool includeSecondBestLocations, bool mustMove)
    {
        var result = NearbyStrongholdOfWinningOpponent(location, battalion, false);
        if (result == null) result = NearbyStrongholdOfWinningOpponent(location, battalion, true);
        LogInfo("Suitable NearbyStrongholdOfWinningOpponent: {0}", result);

        if (result == null) result = VacantAndSafeNearbyStronghold(location, battalion);
        LogInfo("Suitable VacantAndSafeNearbyStronghold: {0}", result);

        if (result == null) result = NearbyStrongholdOfAlmostWinningOpponent(location, battalion, false);
        if (result == null) result = NearbyStrongholdOfAlmostWinningOpponent(location, battalion, true);
        LogInfo("Suitable NearbyStrongholdOfAlmostWinningOpponent: {0}", result);

        if (result == null) result = WeakAndSafeNearbyStronghold(location, battalion);
        LogInfo("Suitable WeakAndSafeNearbyStronghold: {0}", result);

        if (result == null && !LastTurn) result = BestSafeAndNearbyResources(location, battalion);
        LogInfo("Suitable BestSafeAndNearbyResources without fighting: {0}", result);

        if (result == null) result = UnthreatenedAndSafeNearbyStronghold(location, battalion);
        LogInfo("Suitable UnthreatenedAndSafeNearbyStronghold: {0}", result);

        if (result == null) result = WinnableNearbyStronghold(location, battalion);
        LogInfo("Suitable WinnableNearbyStronghold: {0}", result);

        if (result == null && !LastTurn) result = BestSafeAndNearbyDiscovery(location, battalion);
        LogInfo("Nearby Discovery: {0}", result);

        if (result == null && !LastTurn) result = BestSafeAndNearbyResources(location, battalion, true);
        LogInfo("Suitable BestSafeAndNearbyResources with fighting: {0}", result);

        if (includeSecondBestLocations)
        {
            if (result == null && !LastTurn && WithinRange(location, Game.Map.PolarSink, battalion)) result = Game.Map.PolarSink;
            LogInfo("Suitable - Polar Sink nearby? {0}", result);

            if (result == null && location != Game.Map.PolarSink) result = Game.Map.Locations(false).Where(l => Game.IsProtectedFromStorm(l) && WithinRange(location, l, battalion) && NotOccupiedByOthers(l.Territory) && l.Territory != location.Territory).FirstOrDefault();
            LogInfo("Suitable nearby Rock: {0}", result);
        }

        if (result == null && mustMove) result = PlacementEvent.ValidTargets(Game, this, location, battalion).FirstOrDefault(l => AllyDoesntBlock(l.Territory) && l != location);
        LogInfo("Suitable - any location without my ally: {0}", result);

        return result;
    }
}