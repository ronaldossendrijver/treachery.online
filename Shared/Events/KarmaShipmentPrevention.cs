/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaShipmentPrevention : GameEvent
    {
        public KarmaShipmentPrevention(Game game) : base(game)
        {
        }

        public KarmaShipmentPrevention()
        {
        }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            if (Game.Version >= 138 && !GetValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
        {
            return g.ValidTargets(p).Where(f => f != Faction.Yellow);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent shipment by ", Target);
        }
    }
}
