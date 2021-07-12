/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class BrownEconomics : GameEvent
    {
        public BrownEconomicsStatus Status;

        public BrownEconomics(Game game) : base(game)
        {
        }

        public BrownEconomics()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} play Economics: {1}.", Initiator, Status);
        }

        public static IEnumerable<BrownEconomicsStatus> ValidStates(Game g, Player p)
        {
            return new BrownEconomicsStatus[] { BrownEconomicsStatus.Double, BrownEconomicsStatus.Cancel };
        }
    }
}
