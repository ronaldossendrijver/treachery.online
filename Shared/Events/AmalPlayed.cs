/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class AmalPlayed : GameEvent
    {
        #region Construction

        public AmalPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public AmalPlayed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, TreacheryCardType.Amal);
            Log();

            foreach (var p in Game.Players)
            {
                int resourcesPaid = (int)Math.Ceiling(0.5 * p.Resources);
                p.Resources -= resourcesPaid;
                Log(p.Faction, " lose ", Payment.Of(resourcesPaid));
            }

            Game.Stone(Milestone.Amal);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " perform ", TreacheryCardType.Amal);
        }

        #endregion Execution
    }
}
