/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Faction.Brown, "{0} finish their audit.", Faction.Brown);
        }

    }
}
