/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public abstract class AttachedLocation : Location
{
    public Location AttachedToLocation { get; private set; }

    //Needed to check game version
    public Game Game { get; private set; }

    public override bool Visible => AttachedToLocation != null;

    public override int Sector => Game?.Version >= 159 && AttachedToLocation != null ? AttachedToLocation.Sector : -1;

    public AttachedLocation(int id) : base(id)
    {

    }

    public void PointAt(Game game, Location newLocation)
    {
        if (AttachedToLocation != null) AttachedToLocation.Neighbours.Remove(this);

        newLocation.Neighbours.Add(this);
        AttachedToLocation = newLocation;

        if (game.Version >= 163) Game = game;
    }

    public override List<Location> Neighbours
    {
        get
        {
            var result = new List<Location>();
            if (AttachedToLocation != null) result.Add(AttachedToLocation);
            return result;
        }
    }
}