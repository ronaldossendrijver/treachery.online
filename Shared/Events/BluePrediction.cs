/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BluePrediction : GameEvent
    {
        #region Construction

        public BluePrediction(Game game) : base(game)
        {
        }

        public BluePrediction()
        {
        }

        #endregion Construction

        #region Properties

        public Faction ToWin { get; set; }

        public int Turn { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Game.IsPlaying(ToWin)) return Message.Express("Invalid target");
            if (Turn < 1 || Turn > Game.MaximumNumberOfTurns) return Message.Express("Invalid turn");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p) => g.PlayersOtherThan(p);

        public static IEnumerable<int> ValidTurns(Game g)
        {
            return Enumerable.Range(1, g.MaximumNumberOfTurns);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Player.PredictedFaction = ToWin;
            Player.PredictedTurn = Turn;
            Log();
            Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.DealStartingTreacheryCards, Game.DealTraitors);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " predict who will win and when");
        }

        #endregion Execution
    }
}
