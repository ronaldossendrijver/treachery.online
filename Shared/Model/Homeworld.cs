/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Homeworld : Location
{
    public World World { get; }

    public Faction Faction { get; private set; }

    public int Threshold { get; private set; }

    public int ResourceAmount { get; private set; }

    public bool IsHomeOfNormalForces { get; private set; }

    public bool IsHomeOfSpecialForces { get; private set; }

    public int BattleBonusAndLasgunShieldLimitAtHighThreshold { get; private set; }

    public int BattleBonusAndLasgunShieldLimitAtLowThreshold { get; private set; }

    public Homeworld(World world, Faction faction, Territory t, bool isHomeOfNormalForces, bool isHomeOfSpecialForces, int treshold, int battleBonusAtHighThreshold, int battleBonusAtLowThreshold, int resourceAmount, int id) : base(id)
    {
        World = world;
        Faction = faction;
        Territory = t;
        IsHomeOfNormalForces = isHomeOfNormalForces;
        IsHomeOfSpecialForces = isHomeOfSpecialForces;
        Threshold = treshold;
        BattleBonusAndLasgunShieldLimitAtHighThreshold = battleBonusAtHighThreshold;
        BattleBonusAndLasgunShieldLimitAtLowThreshold = battleBonusAtLowThreshold;
        ResourceAmount = resourceAmount;
    }

    public override int Sector => -1;

    public override string ToString()
    {
        if (Message.DefaultDescriber != null)
            return Message.DefaultDescriber.Describe(World) + "*";
        return "Homeworld";
    }
}

public class HomeworldStatus
{
    public bool IsHigh { get; }

    public bool IsLow => !IsHigh;

    public Faction Occupant { get; private set; }

    public HomeworldStatus(bool isHigh, Faction occupant)
    {
        IsHigh = isHigh;
        Occupant = occupant;
    }
}