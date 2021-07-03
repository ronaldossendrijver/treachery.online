/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TakeLosses : GameEvent
    {
        public TakeLosses(Game game) : base(game)
        {
        }

        public TakeLosses()
        {
        }

        public int ForceAmount { get; set; }

        public int SpecialForceAmount { get; set; }

        public override string Validate()
        {
            int valueToBeKilled = LossesToTake(Game);

            if (ForceAmount + 2 * SpecialForceAmount < valueToBeKilled) return string.Format("Select a total value of at least {0} to be killed.", valueToBeKilled);

            return "";
        }

        public static int LossesToTake(Game g)
        {
            return g.StormLossesToTake[0].Item2;
        }

        public static Location LossLocation(Game g)
        {
            return g.StormLossesToTake[0].Item1;
        }

        public static int ValidMaxForceAmount(Game g, Player p)
        {
            return p.ForcesIn(LossLocation(g));
        }

        public static int ValidMaxSpecialForceAmount(Game g, Player p)
        {
            return p.SpecialForcesIn(LossLocation(g));
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (SpecialForceAmount > 0)
            {
                var p = Player;
                return new Message(Initiator, "The storm kills {0} {1} forces and {2} {3}.", ForceAmount, Initiator, SpecialForceAmount, p.SpecialForce);
            }
            else
            {
                return new Message(Initiator, "The storm kills {0} {1} forces.", ForceAmount, Initiator);
            }
        }
    }
}
