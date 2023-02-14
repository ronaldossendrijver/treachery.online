/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface ILocationEvent
    {
        public Faction Initiator { get; }

        public Location To { get; }

        public int TotalAmountOfForces { get; }
    }
}
