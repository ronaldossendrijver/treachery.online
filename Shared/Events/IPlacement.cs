/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public interface IPlacement
    {
        public Faction Initiator { get; }

        public Dictionary<Location, Battalion> ForceLocations { get; }

        public Location To { get; }
    }
}
