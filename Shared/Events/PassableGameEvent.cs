/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public abstract class PassableGameEvent : GameEvent
    {

        public PassableGameEvent(Game game) : base(game)
        {
        }

        public PassableGameEvent() : base(null)
        {
        }

        public bool Passed { get; set; }
    }
}


