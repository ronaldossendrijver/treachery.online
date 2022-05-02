/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            return Message.Express(Initiator, " perform ", TreacheryCardType.Amal);
        }
    }
}
