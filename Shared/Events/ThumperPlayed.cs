/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ThumperPlayed : GameEvent
    {
        #region Construction

        public ThumperPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ThumperPlayed()
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
            Game.Discard(Player, TreacheryCardType.Thumper);
            Log();
            Game.Stone(Milestone.Thumper);
            Game.ThumperUsed = true;
            Game.EnterBlowA();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use a ", TreacheryCardType.Thumper, " to attract ", Concept.Monster);
        }

        #endregion Execution
    }
}
