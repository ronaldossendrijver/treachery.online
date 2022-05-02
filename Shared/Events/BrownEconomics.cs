/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play Inflation: ", Status);
        }

        public static IEnumerable<BrownEconomicsStatus> ValidStates(Game g, Player p)
        {
            return new BrownEconomicsStatus[] { BrownEconomicsStatus.Double, BrownEconomicsStatus.Cancel };
        }
    }
}
