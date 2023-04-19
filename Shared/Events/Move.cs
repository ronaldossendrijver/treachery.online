/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Move : PlacementEvent
    {
        #region Construction

        public Move(Game game) : base(game)
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
                Game.CurrentOrangeNexus = null;
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
