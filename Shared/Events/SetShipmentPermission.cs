/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SetShipmentPermission : GameEvent
    {
        public SetShipmentPermission(Game game) : base(game)
        {
        }

        public SetShipmentPermission()
        {
        }

        public Faction Target { get; set; }

        public ShipmentPermission Permission { get; set; }

        public override Message Validate()
        {
            if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        public static bool IsApplicable(Game g, Player p) => g.CurrentMainPhase == MainPhase.ShipmentAndMove && p.Is(Faction.Orange) && p.HasHighThreshold();

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Permission switch
            {
                ShipmentPermission.None => Message.Express(Initiator, " deny ", Target, " shipping cross/from planet"),
                ShipmentPermission.CrossAtNormalRates => Message.Express(Initiator, " allow ", Target, " to ship cross/from planet"),
                ShipmentPermission.CrossAtOrangeRates => Message.Express(Initiator, " allow ", Target, " to ship cross/from planet at half price"),
                _ => Message.Express("unknown"),
            };
        }
    }
}
