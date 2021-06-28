/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AmalPlayed : GameEvent
    {
        public AmalPlayed(Game game) : base(game)
        {
        }

        public AmalPlayed()
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
            return new Message(Initiator, "{0} perform {1}.", Initiator, TreacheryCardType.Amal);
        }
    }
}
