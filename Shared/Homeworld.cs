/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Homeworld : Location
    {
        public World World { get; private set; }

        public Faction Faction { get; private set; }

        public int Threshold { get; private set; }

        public bool IsHomeOfNormalForces { get; private set; }

        public bool IsHomeOfSpecialForces { get; private set; }

        public Homeworld(World world, Faction faction, Territory t, bool isHomeOfNormalForces, bool isHomeOfSpecialForces, int treshold, int id) : base(id)
        {
            World = world;
            Faction = faction;
            Territory = t;
            IsHomeOfNormalForces = isHomeOfNormalForces;
            IsHomeOfSpecialForces = isHomeOfSpecialForces;
            Threshold = treshold;
        }

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return Message.DefaultDescriber.Describe(World) + "*";
            }
            else
            {
                return "Homeworld";
            }
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