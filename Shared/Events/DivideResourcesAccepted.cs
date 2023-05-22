/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class DivideResourcesAccepted : PassableGameEvent
    {
        #region Construction

        public DivideResourcesAccepted(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public DivideResourcesAccepted()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsApplicable(Game g, Player p) => g.CurrentPhase == Phase.AcceptingResourceDivision && GetResourcesToBeDivided(g).OtherFaction == p.Faction;

        public static ResourcesToBeDivided GetResourcesToBeDivided(Game g) => g.CollectedResourcesToBeDivided.FirstOrDefault();

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.DivideResourcesFromCollection(!Passed);
            Game.Enter(Game.CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, Game.EndCollectionMainPhase);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, !Passed ? "" : " don't", " agree with the proposed division");
        }

        #endregion Execution
    }
}
