/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class FlightUsed : GameEvent
    {
        public bool MoveThreeTerritories { get; set; }

        public FlightUsed(Game game) : base(game)
        {
        }

        [JsonIgnore]
        public bool ExtraMove => !MoveThreeTerritories;

        public FlightUsed()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use an ", TreacheryCardType.Flight);
        }

        public static bool IsAvailable(Player p)
        {
            return p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Flight);
        }

    }
}
