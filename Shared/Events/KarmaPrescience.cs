/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class KarmaPrescience : GameEvent
    {
        #region Construction

        public KarmaPrescience(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaPrescience()
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
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Player.SpecialKarmaPowerUsed = true;
            Log();
            Game.Stone(Milestone.Karma);
            Game.GreenKarma = true;
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, Initiator, " see the entire enemy battle plan");
        }

        #endregion Execution
    }
}
