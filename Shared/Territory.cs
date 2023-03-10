/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public Territory(bool isHomeworld, bool isStronghold, bool hasReducedShippingCost, bool isDiscovery, bool isProtectedFromStorm, bool isProtectedFromWorm, int id) : this(id)
        {
            IsHomeworld = isHomeworld;
            IsStronghold = isStronghold;
            IsProtectedFromStorm = isProtectedFromStorm;
            IsProtectedFromWorm = isProtectedFromWorm;
            IsDiscovery = isDiscovery;
            HasReducedShippingCost = hasReducedShippingCost;
        }

        public int Id { get; private set; }

        public int SkinId => Id;

        public bool IsHomeworld { get; set; }

        public bool HasReducedShippingCost { get; set; }

        public bool IsDiscovery { get; set; }

        public bool IsDiscoveredStronghold => IsStronghold && IsDiscovery;

        public bool IsVisible => Locations.Any(l => l.Visible);

        public bool IsStronghold { get; set; }

        public bool IsProtectedFromStorm { get; set; }

        public bool IsProtectedFromWorm { get; set; }

        public StrongholdAdvantage Advantage { get; set; }

        public List<Location> Locations { get; private set; } = new List<Location>();

        public void AddLocation(Location l)
        {
            Locations.Add(l);
        }

        public Location MiddleLocation
        {
            get
            {
                var locations = Locations.ToList();
                return locations[(int)(0.5 * locations.Count)];
            }
        }

        public Location ResourceBlowLocation => Locations.FirstOrDefault(l => l.SpiceBlowAmount > 0);

        public Location DiscoveryTokenLocation => Locations.FirstOrDefault(l => l.DiscoveryTokenType != DiscoveryTokenType.None);

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return Message.DefaultDescriber.Describe(this) + "*";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}