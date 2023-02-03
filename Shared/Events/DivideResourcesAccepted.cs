/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class DivideResourcesAccepted : PassableGameEvent
    {
        public DivideResourcesAccepted(Game game) : base(game)
        {
        }

        public DivideResourcesAccepted()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, !Passed ? "" : " don't", " agree with the proposed division");
        }

        public static bool IsApplicable(Game g, Player p) => g.CurrentPhase == Phase.AcceptingResourceDivision && GetResourcesToBeDivided(g).OtherFaction == p.Faction;

        public static ResourcesToBeDivided GetResourcesToBeDivided(Game g) => g.CollectedResourcesToBeDivided.FirstOrDefault();
    }
}
