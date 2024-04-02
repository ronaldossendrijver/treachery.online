/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Location : IIdentifiable
{
    public virtual int Sector { get; set; }

    private Territory _territory;

    public virtual Territory Territory
    {
        get => _territory;
        set
        {
            _territory = value;

            if (value != null) _territory.AddLocation(this);
        }
    }

    public virtual bool Visible => true;

    public virtual List<Location> Neighbours { get; set; } = new();

    public string Orientation { get; set; } = "";

    public virtual int SpiceBlowAmount { get; set; } = 0;

    public virtual DiscoveryTokenType DiscoveryTokenType { get; set; } = DiscoveryTokenType.None;

    public Location(int id)
    {
        Id = id;
    }

    public int Id { get; set; }

    public bool IsStronghold => Territory.IsStronghold;

    public bool IsHomeworld => Territory.IsHomeworld;

    public bool IsProtectedFromStorm => Territory.IsProtectedFromStorm;

    public StrongholdAdvantage Advantage => Territory.Advantage;

    public override bool Equals(object obj)
    {
        return obj is Location l && l.Id == Id;
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public override string ToString()
    {
        if (Message.DefaultDescriber != null)
            return Message.DefaultDescriber.Describe(this) + "*";
        return base.ToString();
    }
}