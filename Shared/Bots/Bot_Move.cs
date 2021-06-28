/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        protected virtual Move DetermineMove()
        {
            bool winning = IAmWinning;
            LogInfo("DetermineMove(). AllIn: {0}, Winning: {1}.", LastTurn, winning);

            if (decidedShipmentAction == ShipmentDecision.StrongholdNearResources)
            {
                LogInfo("Move to spice");
                var toMove = ForcesOnPlanet[decidedShipment.To].Take(decidedShipment.ForceAmount + decidedShipment.SpecialForceAmount, false);
                return ConstructMove(finalDestination, decidedShipment.To, toMove);
            }

            //Move biggest batallion threatened by ally presence
            var battalionThreatenedByAllyPresence = BattalionThatShouldBeMovedDueToAllyPresence;
            LogInfo("BattalionThreatenedByAllyPresence: " + BattalionThatShouldBeMovedDueToAllyPresence);
            if (battalionThreatenedByAllyPresence.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(battalionThreatenedByAllyPresence, true, true);
                if (moveTo != null)
                {
                    LogInfo("Move biggest batallion threatened by ally presence");
                    return ConstructMove(moveTo, battalionThreatenedByAllyPresence.Key, battalionThreatenedByAllyPresence.Value);
                }
            }

            //Move biggest batallion threatened by storm
            if (!LastTurn && !winning)
            {
                var biggestBattalionThreatenedByStorm = BiggestBattalionThreatenedByStormWithoutSpice;
                LogInfo("BiggestBattalionThreatenedByStormWithoutSpice: " + BiggestBattalionThreatenedByStormWithoutSpice.Key);
                if (biggestBattalionThreatenedByStorm.Key != null)
                {
                    var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionThreatenedByStorm, true, false);
                    if (moveTo != null)
                    {
                        LogInfo("Move biggest batallion threatened by storm");
                        return ConstructMove(moveTo, biggestBattalionThreatenedByStorm.Key, biggestBattalionThreatenedByStorm.Value);
                    }
                }
            }

            //Move from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock
            var biggestBattalionInSpicelessNonStronghold = BiggestBattalionInSpicelessNonStrongholdLocationOnRock;
            LogInfo("BiggestBattalionInSpicelessNonStrongholdLocationOnRock: " + BiggestBattalionInSpicelessNonStrongholdLocationOnRock.Key);
            if (biggestBattalionInSpicelessNonStronghold.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStronghold, false, false);
                if (moveTo != null)
                {
                    LogInfo("Move from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock");
                    return ConstructMove(moveTo, biggestBattalionInSpicelessNonStronghold.Key, biggestBattalionInSpicelessNonStronghold.Value);
                }
            }

            //Move from useless location (if not needed for atomics) to safe stronghold or spice or rock
            var biggestBattalionInSpicelessNonStrongholdNotNearStronghold = BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold;
            LogInfo("BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold: " + BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold.Key);
            if (biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStrongholdNotNearStronghold, true, false);
                if (moveTo != null)
                {
                    LogInfo("Move from useless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock");
                    return ConstructMove(moveTo, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Value);
                }
            }

            if (Faction == Faction.Blue)
            {
                //Move a stack of advisors from a stronghold to a nearby vacant stronghold
                var biggestStackOfAdvisorsInStrongholdNearVacantStronghold = BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold;
                LogInfo("BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold: " + BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold.Key);
                if (biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Key != null)
                {
                    var moveTo = VacantAndSafeNearbyStronghold(biggestStackOfAdvisorsInStrongholdNearVacantStronghold);
                    if (moveTo != null)
                    {
                        LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold");
                        return ConstructMove(moveTo, biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Key, biggestStackOfAdvisorsInStrongholdNearVacantStronghold.Value);
                    }
                }
            }

            //Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold
            if (!winning)
            {
                var bigUnthreatenedBattalionNearVacantStronghold = BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold;
                LogInfo("bigUnthreatenedBattalionNearVacantStronghold: " + bigUnthreatenedBattalionNearVacantStronghold.Key);
                if (bigUnthreatenedBattalionNearVacantStronghold.Key != null)
                {
                    var moveTo = VacantAndSafeNearbyStronghold(bigUnthreatenedBattalionNearVacantStronghold);
                    if (moveTo != null)
                    {
                        LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby vacant stronghold");
                        return ConstructMove(moveTo, bigUnthreatenedBattalionNearVacantStronghold.Key, bigUnthreatenedBattalionNearVacantStronghold.Value.TakeHalf());
                    }
                }
            }

            //Move partly from unthreatened stronghold location with enough forces to nearby spice
            if (!LastTurn && !winning)
            {
                var bigUnthreatenedBattalionNearSpice = BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice;
                LogInfo("BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice: " + BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice.Key);
                if (bigUnthreatenedBattalionNearSpice.Key != null)
                {
                    var moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value, false);
                    if (moveTo == null) moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value, true);

                    if (moveTo != null)
                    {
                        LogInfo("Move partly from unthreatened stronghold location with enough forces to nearby spice");
                        int forcesForCollection = Math.Max(2, Math.Min(DetermineForcesNeededForCollection(moveTo), bigUnthreatenedBattalionNearSpice.Value.TotalAmountOfForces / 2));
                        return ConstructMove(moveTo, bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value.Take(forcesForCollection, Faction == Faction.Grey));
                    }
                }
            }

            //Move towards shield wall to detonate family atomics if needed
            if (!LastTurn && !winning)
            {
                //var locationsNearShieldWall = Game.Map.FindNeighbours(Game.Map.ShieldWall.MiddleLocation, 1, false, Faction, Game.SectorInStorm, null);
                if (!LastTurn && Game.SectorInStorm >= 0 && Game.SectorInStorm <= 8 && TreacheryCards.Any(c => c.Type == TreacheryCardType.Metheor) && !MetheorPlayed.HasForcesAtOrNearShieldWall(Game, this))
                {



                    Location from = null;
                    Location to = null;
                    Battalion troopThatWillBlowShieldWall = DetermineBattalionThatWillDestroyShieldWall(ref from, ref to);

                    if (troopThatWillBlowShieldWall != null)
                    {
                        LogInfo("Move towards shield wall to detonate family atomics if needed");
                        return ConstructMove(to, from, troopThatWillBlowShieldWall);
                    }
                }
            }

            LogInfo("I'm deciding not to move");
            decidedShipmentAction = ShipmentDecision.None;
            return new Move(Game) { Initiator = Faction, Passed = true };
        }

        protected virtual Caravan DetermineCaravan()
        {
            LogInfo("DetermineCaravan()");

            if (ForcesOnPlanet.Count(kvp => !kvp.Key.IsStronghold) <= 1)
            {
                return null;
            }

            if (decidedShipmentAction == ShipmentDecision.StrongholdNearResources)
            {
                decidedShipmentAction = ShipmentDecision.None;
                LogInfo("Hajr to spice: {0} -> {1}", decidedShipment.To, finalDestination);
                var toMove = ForcesOnPlanet[decidedShipment.To].Take(decidedShipment.ForceAmount + decidedShipment.SpecialForceAmount, false);
                return ConstructCaravan(finalDestination, decidedShipment.To, toMove);
            }

            bool winning = IAmWinning;

            //Move biggest batallion threatened by ally presence
            var battalionThreatenedByAllyPresence = BattalionThatShouldBeMovedDueToAllyPresence;
            if (battalionThreatenedByAllyPresence.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(battalionThreatenedByAllyPresence, true, true);
                if (moveTo != null)
                {
                    LogInfo("Move biggest batallion threatened by ally presence");
                    return ConstructCaravan(moveTo, battalionThreatenedByAllyPresence.Key, battalionThreatenedByAllyPresence.Value);
                }
            }

            //Hajr biggest batallion threatened by storm
            if (!LastTurn && !winning)
            {
                var biggestBattalionThreatenedByStorm = BiggestBattalionThreatenedByStormWithoutSpice;
                if (biggestBattalionThreatenedByStorm.Key != null)
                {
                    var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionThreatenedByStorm, true, false);
                    if (moveTo != null)
                    {
                        LogInfo("Hajr biggest batallion threatened by storm");
                        return ConstructCaravan(moveTo, biggestBattalionThreatenedByStorm.Key, biggestBattalionThreatenedByStorm.Value);
                    }
                }
            }

            //Hajr from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock
            var biggestBattalionInSpicelessNonStronghold = BiggestBattalionInSpicelessNonStrongholdLocationOnRock;
            if (biggestBattalionInSpicelessNonStronghold.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStronghold, false, false);
                if (moveTo != null)
                {
                    LogInfo("Hajr from spiceless non-stronghold location (if not needed for atomics) to safe stronghold or spice or rock");
                    return ConstructCaravan(moveTo, biggestBattalionInSpicelessNonStronghold.Key, biggestBattalionInSpicelessNonStronghold.Value);
                }
            }

            //Hajr from useless location (if not needed for atomics) to safe stronghold or spice or rock
            var biggestBattalionInSpicelessNonStrongholdNotNearStronghold = BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold;
            if (biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key != null)
            {
                var moveTo = DetermineMostSuitableNearbyLocation(biggestBattalionInSpicelessNonStrongholdNotNearStronghold, true, false);
                if (moveTo != null)
                {
                    LogInfo("Hajr from useless location (if not needed for atomics) to safe stronghold or spice or rock");
                    return ConstructCaravan(moveTo, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Key, biggestBattalionInSpicelessNonStrongholdNotNearStronghold.Value);
                }
            }

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
                        return ConstructCaravan(moveTo, bigUnthreatenedBattalionNearVacantStronghold.Key, bigUnthreatenedBattalionNearVacantStronghold.Value.TakeHalf());
                    }
                }
            }

            //Move partly from unthreatened stronghold location with enough forces to nearby spice
            if (!LastTurn && !winning)
            {
                var bigUnthreatenedBattalionNearSpice = BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice;
                if (bigUnthreatenedBattalionNearSpice.Key != null)
                {
                    var moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value, false);
                    if (moveTo == null) moveTo = BestSafeAndNearbyResources(bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value, true);
                    if (moveTo != null)
                    {
                        LogInfo("Hajr partly from unthreatened stronghold location with enough forces to nearby spice");
                        int forcesForCollection = Math.Max(2, Math.Min(DetermineForcesNeededForCollection(moveTo), bigUnthreatenedBattalionNearSpice.Value.TotalAmountOfForces / 2));
                        return ConstructCaravan(moveTo, bigUnthreatenedBattalionNearSpice.Key, bigUnthreatenedBattalionNearSpice.Value.Take(forcesForCollection, Faction == Faction.Grey));
                    }
                }
            }

            LogInfo("I'm deciding not to Hajr");
            return null;
        }


        protected virtual Move ConstructMove(Location to, Location from, Battalion battalion)
        {
            var forces = new Dictionary<Location, Battalion>
            {
                { from, battalion }
            };

            bool asAdvisors = Faction == Faction.Blue && SpecialForcesIn(to.Territory) > 0;
            var result = new Move(Game) { Initiator = Faction, Passed = false, To = to, ForceLocations = forces, AsAdvisors = asAdvisors };
            if (!result.IsValid)
            {
                LogInfo(result.Validate());
                throw new ArgumentException(result.GetMessage().ToString());
            }
            return result;
        }

        protected virtual Caravan ConstructCaravan(Location to, Location from, Battalion battalion)
        {
            var forces = new Dictionary<Location, Battalion>
            {
                { from, battalion }
            };

            bool asAdvisors = Faction == Faction.Blue && SpecialForcesIn(to) > 0;
            var result = new Caravan(Game) { Initiator = Faction, Passed = false, To = to, ForceLocations = forces, AsAdvisors = asAdvisors };
            if (!result.IsValid)
            {
                LogInfo(result.Validate());
                throw new ArgumentException(result.GetMessage().ToString());
            }
            return result;
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
    }

}
