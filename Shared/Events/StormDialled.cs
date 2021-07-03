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
            if (Amount < ValidMinAmount(Game) || Amount > ValidMaxAmount(Game)) return "Invalid amount";

            return "";
        }

        public static int ValidMinAmount(Game g)
        {
            if (g.CurrentTurn == 1)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public static int ValidMaxAmount(Game g)
        {
            if (g.CurrentTurn == 1)
            {
                return 20;
            }
            else
            {
                return 3;
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
