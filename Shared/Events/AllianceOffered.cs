/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} offer to ally with {1}.", Initiator, Target);
        }
    }
}
