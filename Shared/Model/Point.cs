/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public struct PointD
{
    public double X;
    public double Y;

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        return obj is PointD other && Equals(other);
    }

    public bool Equals(PointD p)
    {
        return X == p.X && Y == p.Y;
    }

    public override int GetHashCode()
    {
        return (X, Y).GetHashCode();
    }

    public static bool operator ==(PointD lhs, PointD rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(PointD lhs, PointD rhs)
    {
        return !(lhs == rhs);
    }

    public PointD Translate(double dX, double dY)
    {
        return new PointD(X + dX, Y + dY);
    }
}