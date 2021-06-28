/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class StormDialled : GameEvent
    {
        public StormDialled(Game game) : base(game)
        {
        }

        public StormDialled()
        {
        }

        public int Amount { get; set; }

        public override string Validate()
        {
            if (!ValidAmounts(Game).Contains(Amount)) return "Invalid amount";

            return "";
        }

        public static IEnumerable<int> ValidAmounts(Game g)
        {
            if (g.CurrentTurn == 1) {
                return Enumerable.Range(0, 21);
            }
            else
            {
                return Enumerable.Range(1, 3);
            }
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} dials.", Initiator);
        }

    }
}
