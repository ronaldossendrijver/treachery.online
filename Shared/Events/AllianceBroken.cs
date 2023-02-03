/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceBroken : GameEvent
    {
        public AllianceBroken(Game g) : base(g) { }

        public AllianceBroken()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " end their alliance");
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }
    }
}
