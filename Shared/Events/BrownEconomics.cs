/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Economics : GameEvent
    {
        public BrownEconomicsStatus Status;

        public Economics(Game game) : base(game)
        {
        }

        public Economics()
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
            return new Message(Initiator, "{0} play a Economics: {1}.", Initiator, Status);
        }

        public static IEnumerable<BrownEconomicsStatus> ValidStates(Game g, Player p)
        {
            return new BrownEconomicsStatus[] { BrownEconomicsStatus.Double, BrownEconomicsStatus.Cancel };
        }
    }
}
