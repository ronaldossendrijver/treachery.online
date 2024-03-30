/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Leader : IHero
{
    public int Id { get; }

    public Faction Faction { get; set; }

    public int Value { get; set; }

    public Leader(int id)
    {
        Id = id;
    }

    public HeroType HeroType { get; set; }

    public bool Is(Faction f)
    {
        return Faction == f;
    }

    public int ValueInCombatAgainst(IHero opposingHero)
    {
        if (HeroType == HeroType.VariableValue)
        {
            if (opposingHero == null)
                return 0;
            return opposingHero.Value;
        }

        return Value;
    }

    public int CostToRevive
    {
        get
        {
            if (HeroType == HeroType.VariableValue)
                return 6;
            if (HeroType == HeroType.Vidal)
                return 5;
            return Value;
        }
    }

    public int SkinId => Id;

    public override bool Equals(object obj)
    {
        return obj is Leader l && l.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool IsTraitor(IHero hero)
    {
        return hero == this;
    }

    public bool IsFaceDancer(IHero hero)
    {
        return hero == this;
    }

    public override string ToString()
    {
        if (Message.DefaultDescriber != null)
            return Message.DefaultDescriber.Describe(this) + "*";
        return base.ToString();
    }
}