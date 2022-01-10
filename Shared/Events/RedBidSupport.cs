/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RedBidSupport : GameEvent
    {
        public RedBidSupport(Game game) : base(game)
        {
        }

        public RedBidSupport()
        {
        }

        public Dictionary<Faction, int> Amounts { get; set; }

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
            if (Amounts.Sum(kvp => kvp.Value) > 0)
            {
                return Message.Express(Initiator, " supports ", Amounts.Where(kvp => kvp.Value > 0).Select(f => MessagePart.Express(f.Key, ":", new Payment(f.Value), " ")));
            }
            else
            {
                return Message.Express(Initiator, " doesn't support bids by other factions");
            }
        }
    }
}
