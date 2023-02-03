/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteRevealedNoField : GameEvent
    {
        public WhiteRevealedNoField(Game g) : base(g) { }

        public WhiteRevealedNoField()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " reveal a ", FactionSpecialForce.White);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }
    }
}
