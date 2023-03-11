/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class ResourcesTransferred : GameEvent
    {
        public ResourcesTransferred(Game game) : base(game)
        {
        }

        public ResourcesTransferred()
        {
        }

        public int Resources { get; set; }

        public override Message Validate()
        {
            if (Resources > MaxAmount(Player)) return Message.Express("You can't transfer more than ", Game.Payment(MaxAmount(Player)));
            if (!MayTransfer(Game, Player)) return Message.Express("You currently can't transfer ", Concept.Resource);

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " transfer ", new Payment(Resources), " to their ally");
        }

        public static bool CanBePlayed(Game g, Player p) => p.HasAlly && MaxAmount(p) > 0 && MayTransfer(g, p);

        public static bool MayTransfer(Game g, Player p) => !(g.HasBidToPay(p) || g.CurrentPhaseIsUnInterruptable);

        public static int MaxAmount(Player p) => (int)Math.Min(p.TransferrableResources, p.Resources);
    }
}
