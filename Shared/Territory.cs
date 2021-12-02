/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
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

        public int SkinId => Id;
        
        public string Name => Skin.Current.GetTerritoryName(this);

        public bool IsStronghold { get; set; }

        public bool IsProtectedFromStorm { get; set; }

        public bool IsProtectedFromWorm { get; set; }

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

        public override string ToString()
        {
            return Name;
        }
    }

}