/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class TerrorRevealed : GameEvent
    {
        public TerrorRevealed(Game game) : base(game)
        {
        }

        public TerrorRevealed()
        {
        }

        public bool Passed { get; set; }

        public TerrorType Type { get; set; }

        public override Message Validate()
        {
            if (!Passed && Initiator != Faction.Cyan) return Message.Express("Your faction can't reveal terror tokens");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " resort to ", Type);
            }
            else
            {
                return Message.Express(Initiator, " don't terrorize");
            }
        }

        public static Territory GetTerritory(Game g) => g.LastShipmentOrMovement.To.Territory;

        public static Faction GetVictim(Game g) => g.LastShipmentOrMovement.Initiator;

        public static IEnumerable<TerrorType> GetTypes(Game g) => g.TerrorIn(g.LastShipmentOrMovement.To.Territory);
    }
}
