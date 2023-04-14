/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RedBidSupport : GameEvent
    {
        #region Construction

        public RedBidSupport(Game game) : base(game)
        {
        }

        public RedBidSupport()
        {
        }

        #endregion Construction

        #region Properties

        public Dictionary<Faction, int> Amounts { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.PermittedUseOfRedSpice = Amounts;
            Log();
        }

        public override Message GetMessage()
        {
            if (Amounts.Sum(kvp => kvp.Value) > 0)
            {
                return Message.Express(Initiator, " supports ", Amounts.Where(kvp => kvp.Value > 0).Select(f => MessagePart.Express(f.Key, ":", Payment.Of(f.Value), " ")));
            }
            else
            {
                return Message.Express(Initiator, " doesn't support bids by other factions");
            }
        }

        #endregion Execution
    }
}
