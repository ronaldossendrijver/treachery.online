/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteRevealedNoField : GameEvent
    {
        public WhiteRevealedNoField(Game g) : base(g) { }

        public WhiteRevealedNoField()
        {
        }

        public override string Validate()
        {
            return "";
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} reveal a No-Field.", Initiator);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }
    }
}
