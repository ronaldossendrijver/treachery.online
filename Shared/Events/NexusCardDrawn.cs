/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class NexusCardDrawn : GameEvent
    {
        public bool Passed { get; set; }

        public NexusCardDrawn(Game game) : base(game)
        {
        }

        public NexusCardDrawn()
        {
        }

        public override Message Validate()
        {
            if (!Passed && !MayDraw(Player)) return Message.Express("You're not allowed to draw a Nexus Card");
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(Passed, " don't"), " draw a Nexus card");
        }

        public static bool MayDraw(Player p)
        {
            return p.Nexus == Faction.None || p.Faction == p.Nexus;
        }

        public static bool Applicable(Game g, Player p)
        {
            return g.FactionsThatMayDrawNexusCard.Contains(p.Faction);
        }
    }
}
