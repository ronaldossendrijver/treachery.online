/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceOffered : GameEvent
    {
        public AllianceOffered(Game game) : base(game)
        {
        }

        public AllianceOffered()
        {
        }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " offer an alliance to ", Target);
        }
    }
}
