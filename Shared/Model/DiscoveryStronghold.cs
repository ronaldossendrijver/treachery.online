/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class DiscoveredLocation : AttachedLocation
    {
        public DiscoveryToken Discovery { get; private set; }

        public DiscoveredLocation(Territory t, int id, DiscoveryToken discovery) : base(id)
        {
            Territory = t;
            Discovery = discovery;
        }
    }
}