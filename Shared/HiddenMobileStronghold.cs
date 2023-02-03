/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class HiddenMobileStronghold : Location
    {
        public Location AttachedToLocation { get; private set; } = null;

        public HiddenMobileStronghold(Territory t, int id) : base(id)
        {
            Territory = t;
        }

        public bool Visible => AttachedToLocation != null;

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