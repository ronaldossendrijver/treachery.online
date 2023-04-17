/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class DivideResources : PassableGameEvent
    {
        #region Construction

        public DivideResources(Game game) : base(game)
        {
        }

        public DivideResources()
        {
        }

        #endregion Construction

        #region Properties

        public int PortionToFirstPlayer { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Passed) return null;

            var toBeDivided = GetResourcesToBeDivided(Game);
            if (toBeDivided == null) return Message.Express("No collections to be divided");
            if (PortionToFirstPlayer > GetResourcesToBeDivided(Game).Amount) return Message.Express("Too much assigned to ", toBeDivided.FirstFaction);

            return null;
        }

        public static ResourcesToBeDivided GetResourcesToBeDivided(Game g) => g.CollectedResourcesToBeDivided.FirstOrDefault();

        public static bool IsApplicable(Game g, Player p) => g.CurrentPhase == Phase.DividingCollectedResources && GetResourcesToBeDivided(g).FirstFaction == p.Faction;

        public static int GainedByOtherFaction(ResourcesToBeDivided tbd, bool agreed, int portionToFirstPlayer) => tbd.Amount - GainedByFirstFaction(tbd, agreed, portionToFirstPlayer);

        public static int GainedByFirstFaction(ResourcesToBeDivided tbd, bool agreed, int portionToFirstPlayer) => agreed ? portionToFirstPlayer : (int)(0.5f * tbd.Amount);

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentDivisionProposal = this;

            if (Passed)
            {
                Game.DivideResourcesFromCollection(false);
                Game.Enter(Game.CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, Game.EndCollectionMainPhase);
            }
            else
            {
                var toBeDivided = GetResourcesToBeDivided(Game);
                int gainedByOtherFaction = GainedByOtherFaction(toBeDivided, true, PortionToFirstPlayer);
                Log(Initiator, " propose that they take ", Payment.Of(PortionToFirstPlayer), " and ", toBeDivided.OtherFaction, " take ", Payment.Of(gainedByOtherFaction));
                Game.Enter(Phase.AcceptingResourceDivision);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express("Collected resources were divided");
        }

        #endregion Execution
    }
}
