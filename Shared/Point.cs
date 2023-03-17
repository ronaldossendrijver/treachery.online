/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct PointD
    {
        public double X;
        public double Y;

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj) => obj is PointD other && Equals(other);

        public bool Equals(PointD p) => X == p.X && Y == p.Y;

        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(PointD lhs, PointD rhs) => lhs.Equals(rhs);

        public static bool operator !=(PointD lhs, PointD rhs) => !(lhs == rhs);
    }
}
