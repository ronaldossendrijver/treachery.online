/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Audited : GameEvent
    {
        public Audited(Game game) : base(game)
        {
        }

        public Audited()
        {
        }

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Faction.Brown, " finish their audit");
        }

    }
}
