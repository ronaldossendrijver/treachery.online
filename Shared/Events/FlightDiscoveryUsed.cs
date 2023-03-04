/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class FlightDiscoveryUsed : GameEvent
    {
        public FlightDiscoveryUsed(Game game) : base(game)
        {
        }

        public FlightDiscoveryUsed()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use the ", DiscoveryToken.Flight, " discovert token to gain movement speed");
        }

        public static bool IsAvailable(Game g, Player p)
        {
            return g.OwnerOfFlightDiscovery == p.Faction;
        }

    }
}
