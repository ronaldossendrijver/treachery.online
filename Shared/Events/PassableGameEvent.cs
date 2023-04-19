/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public abstract class PassableGameEvent : GameEvent
    {

        public PassableGameEvent(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PassableGameEvent() : base()
        {
        }

        public bool Passed { get; set; }

    }
}


