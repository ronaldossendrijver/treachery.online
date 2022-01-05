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

        public override string Validate()
        {
            if (MoveAmount < 0 || MoveAmount > 10) return "Invalid number of sectors";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use {1} to move the storm {2} sectors.", Initiator, TreacheryCardType.StormSpell, MoveAmount);
        }
    }
}
