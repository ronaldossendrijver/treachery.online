/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class Move : PlacementEvent
    {
        #region Construction

        public Move(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Move()
        {
        }

        #endregion Construction

        #region Properties

        public bool AsAdvisors { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Passed && Game.InOrangeCunningShipment) return Message.Express("You cannot move after Cunning shipment");

            return ValidateMove(AsAdvisors);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.StormLossesToTake.Clear();

            Game.CurrentPlayerMayPerformExtraMove = (Game.CurrentFlightUsed != null && Game.CurrentFlightUsed.Initiator == Initiator && Game.CurrentFlightUsed.ExtraMove);

            if (!Game.CurrentPlayerMayPerformExtraMove && !Game.InOrangeCunningShipment)
            {
                Game.HasActedOrPassed.Add(Initiator);

                if (Game.CurrentPhase == Phase.NonOrangeMove)
                {
                    Game.ShipmentAndMoveSequence.NextPlayer();

                    if (Game.ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && Game.OrangeMayShipOutOfTurnOrder)
                    {
                        Game.ShipmentAndMoveSequence.NextPlayer();
                    }
                }
            }

            if (Game.InOrangeCunningShipment)
            {
                Game.CurrentOrangeCunning = null;
                Game.InOrangeCunningShipment = false;
            }

            if (!Passed)
            {
                Game.RecentMoves.Add(this);

                if (Game.ContainsConflictingAlly(Player, To))
                {
                    Game.ChosenDestinationsWithAllies.Add(To.Territory);
                }

                Game.PerformMoveFromLocations(Player, ForceLocations, this, Initiator != Faction.Blue || AsAdvisors, false);
                Game.CheckIntrusion(this);
            }
            else
            {
                Log(Initiator, " pass movement");
            }

            Game.DetermineNextShipmentAndMoveSubPhase();
            Game.CheckIfForcesShouldBeDestroyedByAllyPresence(Player);
            Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();

            if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.YellowExtraMove);
            if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreyCyborgExtraMove);

            Game.CurrentFlightUsed = null;
            Game.CurrentFlightDiscoveryUsed = null;
            Game.CurrentPlanetology = null;
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass move");
            }
            else
            {
                return Message.Express(Initiator, " move to ", To);
            }
        }

        #endregion Execution
    }
}
