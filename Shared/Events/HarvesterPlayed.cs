/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class HarvesterPlayed : GameEvent
    {
        public HarvesterPlayed(Game game) : base(game)
        {
        }

        public HarvesterPlayed()
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
            return Message.Express(Initiator, " use a ", TreacheryCardType.Harvester);
        }
    }
}
