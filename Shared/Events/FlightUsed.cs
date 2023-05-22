/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class FlightUsed : GameEvent
    {
        #region Construction

        public FlightUsed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public FlightUsed()
        {
        }

        #endregion Construction

        #region Properties

        public bool MoveThreeTerritories { get; set; }

        [JsonIgnore]
        public bool ExtraMove => !MoveThreeTerritories;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsAvailable(Player p)
        {
            return p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Flight);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.Discard(Player, TreacheryCardType.Flight);
            Game.CurrentFlightUsed = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use an ", TreacheryCardType.Flight, " to ", ExtraMove ? " move an additional group of forces " : " gain movement speed");
        }

        #endregion Execution
    }
}
