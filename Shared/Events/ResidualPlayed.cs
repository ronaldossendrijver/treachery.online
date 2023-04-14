/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class ResidualPlayed : GameEvent
    {
        #region Construction

        public ResidualPlayed(Game game) : base(game)
        {
        }

        public ResidualPlayed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool MayPlay(Game g, Player p)
        {
            return g.CurrentBattle.IsAggressorOrDefender(p) && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Residual);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Residual, " to kill a random opponent leader");
        }

        #endregion Execution
    }
}
