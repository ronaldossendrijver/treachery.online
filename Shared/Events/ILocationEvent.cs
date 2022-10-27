/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface ILocationEvent
    {
        public Faction Initiator { get; }

        public Location To { get; }
    }
}
