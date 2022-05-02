/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class StormSpellPlayed : GameEvent
    {
        public StormSpellPlayed(Game game) : base(game)
        {
        }

        public StormSpellPlayed()
        {
        }

        public int MoveAmount { get; set; }

        public override Message Validate()
        {
            if (MoveAmount < 0 || MoveAmount > 10) return Message.Express("Invalid number of sectors");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.StormSpell, " to move the storm ", MoveAmount, " sectors");
        }
    }
}
