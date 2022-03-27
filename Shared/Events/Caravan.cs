/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class Caravan : PlacementEvent
    {
        public Caravan(Game game) : base(game)
        {
        }

        public Caravan()
        {
        }

        public bool AsAdvisors { get; set; }

        public override string Validate()
        {
            return ValidateMove(AsAdvisors);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass ", TreacheryCardType.Caravan);
            }
            else
            {
                return Message.Express(Initiator, " move to ", To, " by ", TreacheryCardType.Caravan);
            }
        }
    }
}
