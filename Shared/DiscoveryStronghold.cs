/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class DiscoveredLocation : Location
    {
        public DiscoveryToken Discovery { get; private set; }

        public Location AttachedToLocation { get; private set; } = null;

        public DiscoveredLocation(Territory t, int id, DiscoveryToken discovery) : base(id)
        {
            Territory = t;
            Discovery = discovery;
        }

        public override bool Visible => AttachedToLocation != null;

        public void PointAt(Location newLocation)
        {
            if (AttachedToLocation != null)
            {
                AttachedToLocation.Neighbours.Remove(this);
            }

            newLocation.Neighbours.Add(this);
            AttachedToLocation = newLocation;
        }

        public override int Sector => -1;

        public override List<Location> Neighbours
        {
            get
            {
                var result = new List<Location>();
                if (AttachedToLocation != null) result.Add(AttachedToLocation);
                return result;
            }
        }
    }
}