/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaPinkDial : GameEvent
    {
        #region Construction

        public KarmaPinkDial(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaPinkDial()
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
            Game.Stone(Milestone.Karma);
            var myLeader = Game.CurrentBattle.PlanOf(Initiator).Hero;
            var opponentLeader = Game.CurrentBattle.PlanOfOpponent(Player).Hero;

            if (myLeader != null && opponentLeader != null)
            {
                Game.PinkKarmaBonus = Math.Abs(myLeader.Value - opponentLeader.ValueInCombatAgainst(myLeader));
            }

            Log("Using ", TreacheryCardType.Karma, ", ", Initiator, " add ", Game.PinkKarmaBonus, " to their dial");
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " add the difference between leader discs to their dial");
        }

        #endregion Execution
    }
}
