/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Treachery.Shared
{
    public class Territory : IIdentifiable
    {
        public Territory(int id)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public int SkinId
        {
            get
            {
                return Id;
            }
        }

        public string Name
        {

            get
            {
                return Skin.Current.GetTerritoryName(this);
            }
        }

        public bool IsStronghold { get; set; }

        public bool IsProtectedFromStorm { get; set; }

        public bool IsProtectedFromWorm { get; set; }

        public List<PointF> _path;
        private Segment[] _shape;

        public Segment[] Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;
                _path = P(value);
            }
        }

        private readonly List<Location> _locations = new List<Location>();
        public IEnumerable<Location> Locations
        {
            get
            {
                return _locations;
            }
        }

        public void AddLocation(Location l)
        {
            _locations.Add(l);
        }

        public Location MiddleLocation
        {
            get
            {
                var locations = Locations.ToList();
                return locations[(int)(0.5 * locations.Count)];
            }
        }

        public bool IsInside(int x, int y)
        {
            if (!_path.Any())
            {
                return false;
            }
            else
            {
                return IsPointInPolygon(x, y, _path);
            }
        }

        private static List<PointF> P(IEnumerable<Segment> segments)
        {
            var result = new List<PointF>();
            foreach (var segment in segments)
            {
                segment.AddTo(result);
            }
            return result;
        }


        public static bool IsPointInPolygon(int x, int y, IList<PointF> polygon)
        {
            var intersects = new List<float>();
            var a = polygon.Last();
            foreach (var b in polygon)
            {
                if (b.X == x && b.Y == y)
                {
                    return true;
                }

                if (b.X == a.X && x == a.X && x >= Math.Min(a.Y, b.Y) && y <= Math.Max(a.Y, b.Y))
                {
                    return true;
                }

                if (b.Y == a.Y && y == a.Y && x >= Math.Min(a.X, b.X) && x <= Math.Max(a.X, b.X))
                {
                    return true;
                }

                if ((b.Y < y && a.Y >= y) || (a.Y < y && b.Y >= y))
                {
                    var px = (int)(b.X + 1.0 * (y - b.Y) / (a.Y - b.Y) * (a.X - b.X));
                    intersects.Add(px);
                }

                a = b;
            }

            intersects.Sort();
            return intersects.IndexOf(x) % 2 == 0 || intersects.Count(i => i < x) % 2 == 1;
        }

        public override string ToString()
        {
            return Name;
        }
    }

}