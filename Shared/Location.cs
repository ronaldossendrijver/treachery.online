/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Location : IIdentifiable
    {
        public virtual int Sector { get; set; }

        private Territory _territory = null;
        public virtual Territory Territory
        {
            get
            {
                return _territory;
            }
            set
            {
                _territory = value;

                if (value != null)
                {
                    _territory.AddLocation(this);
                }
            }
        }

        public int SkinId => Id;

        public virtual Point Center => Skin.Current.LocationCenter_Point[SkinId];

        public virtual Point SpiceLocation => SpiceBlowAmount != 0 ? Skin.Current.LocationSpice_Point[SkinId] : null;

        public virtual List<Location> Neighbours { get; set; } = new List<Location>();

        public virtual string Name { get; set; } = "";

        public virtual int SpiceBlowAmount { get; set; } = 0;

        public Location(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            if (Name != "")
            {
                return string.Format("{0} ({1} Sector)", Territory.Name, Name);
            }
            else
            {
                return Territory.Name;
            }
        }

        public int Id { get; set; }

        public bool IsStronghold => Territory.IsStronghold;

        public bool IsProtectedFromStorm => Territory.IsProtectedFromStorm;

        public override bool Equals(object obj)
        {
            return obj is Location l && l.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}