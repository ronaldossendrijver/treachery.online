/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */


using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Homeworld : Location
    {
        public World World { get; private set; }

        public Faction Faction { get; private set; }

        public int Threshold { get; private set; }

        public bool IsHomeOfNormalForces { get; private set; }

        public bool IsHomeOfSpecialForces { get; private set; }

        public Homeworld(World world, Faction faction, bool isHomeOfNormalForces, bool isHomeOfSpecialForces, int treshold, int id) : base(id)
        {
            World = world;
            Faction = faction;
            IsHomeOfNormalForces = isHomeOfNormalForces;
            IsHomeOfNormalForces = isHomeOfSpecialForces;
            Threshold = treshold;
        }
    }

    public class HomeworldStatus
    {
        public bool IsHigh { get; private set; }

        public bool IsLow => !IsHigh;

        public Faction Occupant { get; private set; }

        public HomeworldStatus(bool isHigh, Faction occupant)
        {
            IsHigh = isHigh;
            Occupant = occupant;
        }
    }
}