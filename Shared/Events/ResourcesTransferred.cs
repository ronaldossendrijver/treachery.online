/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class ResourcesTransferred : GameEvent
    {
        #region Construction

        public ResourcesTransferred(Game game) : base(game)
        {
        }

        public ResourcesTransferred()
        {
        }

        #endregion Construction

        #region Properties

        public int Resources { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Resources > MaxAmount(Player)) return Message.Express("You can't transfer more than ", Payment.Of(MaxAmount(Player)));
            if (!MayTransfer(Game, Player)) return Message.Express("You currently can't transfer ", Concept.Resource);

            return null;
        }

        public static bool CanBePlayed(Game g, Player p) => p.HasAlly && MaxAmount(p) > 0 && MayTransfer(g, p);

        public static bool MayTransfer(Game g, Player p) => !(g.HasBidToPay(p) || g.CurrentPhaseIsUnInterruptable);

        public static int MaxAmount(Player p) => (int)Math.Min(p.TransferrableResources, p.Resources);


        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Player.Resources -= Resources;
            Player.TransferrableResources -= Resources;
            Player.AlliedPlayer.Resources += Resources;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " transfer ", Payment.Of(Resources), " to their ally");
        }

        #endregion Execution
    }
}
