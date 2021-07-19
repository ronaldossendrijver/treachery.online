/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class WhiteAnnouncesAuction : GameEvent
    {
        public bool First;

        public WhiteAnnouncesAuction(Game game) : base(game)
        {
        }

        public WhiteAnnouncesAuction()
        {
        }

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
            return new Message(Initiator, "{0} will auction a card from their cache {1}", Initiator, First ? "first" : "last");
        }
    }
}
