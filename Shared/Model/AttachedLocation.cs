/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public abstract class AttachedLocation : Location
    {
        public Location AttachedToLocation { get; private set; } = null;

        //Needed to check game version
        public Game Game { get; private set; }

        public override bool Visible => AttachedToLocation != null;

        public override int Sector => Game?.Version >= 159 && AttachedToLocation != null ? AttachedToLocation.Sector : - 1;

        public AttachedLocation(int id) : base(id)
        {

        }

        public void PointAt(Game game, Location newLocation)
        {
            if (AttachedToLocation != null)
            {
                AttachedToLocation.Neighbours.Remove(this);
            }

            newLocation.Neighbours.Add(this);
            AttachedToLocation = newLocation;

            Game = Game;
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