/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Treachery.Shared
{
    public class HiddenMobileStronghold : Location
    {
        public const int DX = -271;
        public const int RADIUS = 120;

        public Location AttachedToLocation { get; private set; } = null;

        public HiddenMobileStronghold(int id) : base(id)
        {
            Territory = new Territory(42)
            {
                IsStronghold = true,
                IsProtectedFromStorm = true,
                IsProtectedFromWorm = true
            };
        }

        public bool Visible
        {
            get
            {
                return AttachedToLocation != null;
            }
        }

        public void PointAt(Location newLocation)
        {
            if (AttachedToLocation != null)
            {
                AttachedToLocation.Neighbours.Remove(this);
            }

            newLocation.Neighbours.Add(this);
            AttachedToLocation = newLocation;
        }

        public override int Sector
        {
            get
            {
                return -1;
            }
        }


        public override Point Center
        {
            get
            {
                if (AttachedToLocation != null)
                {
                    return new Point(AttachedToLocation.Center.X + DX, AttachedToLocation.Center.Y);
                }
                else
                {
                    return new Point(-200, -100);
                }
            }
        }

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