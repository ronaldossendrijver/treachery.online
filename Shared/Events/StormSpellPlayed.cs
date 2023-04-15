/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class StormSpellPlayed : GameEvent
    {
        #region Construction

        public StormSpellPlayed(Game game) : base(game)
        {
        }

        public StormSpellPlayed()
        {
        }

        #endregion Construction

        #region Properties

        public int MoveAmount { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (MoveAmount < 0 || MoveAmount > 10) return Message.Express("Invalid number of sectors");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, TreacheryCardType.StormSpell);
            Log();
            Game.MoveStormAndDetermineNext(MoveAmount);
            Game.EndStormPhase();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.StormSpell, " to move the storm ", MoveAmount, " sectors");
        }

        #endregion Execution
    }
}
