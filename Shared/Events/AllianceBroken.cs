/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceBroken : GameEvent
    {
        public AllianceBroken(Game g) : base(g) { }

        public AllianceBroken()
        {
        }

        public override string Validate()
        {
            return "";
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} end their alliance.", Initiator);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }
    }
}
