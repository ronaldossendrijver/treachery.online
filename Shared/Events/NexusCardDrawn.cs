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
            if (!Passed && !MayDraw(Game, Player)) return Message.Express("You're not allowed to draw a Nexus Card");
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

        public static bool MayDraw(Game g, Player p)
        {
            return !g.FactionsThatDrewNexusCard.Contains(p.Faction) || p.Faction == p.Nexus;
        }

        public static bool Applicable(Game g, Player p)
        {
            return g.FactionsThatMayDrawNexusCard.Contains(p.Faction);
        }
    }
}
