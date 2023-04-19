/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RecruitsPlayed : GameEvent
    {
        #region Construction

        public RecruitsPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public RecruitsPlayed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool IsApplicable(Game g, Player p)
        {
            return g.CurrentPhase == Phase.BeginningOfResurrection && p.Has(TreacheryCardType.Recruits);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.CurrentRecruitsPlayed = this;
            Game.Discard(Player, TreacheryCardType.Recruits);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Recruits, " to double free revivals and set revival limits to ", 7);
        }

        #endregion Execution
    }
}
