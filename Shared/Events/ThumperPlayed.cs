/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ThumperPlayed : GameEvent
    {
        public ThumperPlayed(Game game) : base(game)
        {
        }

        public ThumperPlayed()
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
            return new Message(Initiator, "{0} use a {1}.", Initiator, TreacheryCardType.Thumper);
        }
    }
}
