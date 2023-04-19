/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class FlightDiscoveryUsed : GameEvent
    {
        #region Construction

        public FlightDiscoveryUsed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public FlightDiscoveryUsed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsAvailable(Game g, Player p)
        {
            return g.OwnerOfFlightDiscovery == p.Faction;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.OwnerOfFlightDiscovery = Faction.None;
            Game.CurrentFlightDiscoveryUsed = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use the ", DiscoveryToken.Flight, " discovert token to gain movement speed");
        }

        #endregion Execution
    }
}
