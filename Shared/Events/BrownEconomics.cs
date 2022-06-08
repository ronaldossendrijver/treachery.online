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
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play Inflation: ", Status);
        }
    }
}
