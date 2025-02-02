/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.IO;

namespace Treachery.Shared;

public class GameState
{
    public int Version { get; set; }

    public GameEvent[] _events = [];

    [JsonIgnore]
    public IEnumerable<GameEvent> Events
    {
        get => _events;
        set => _events = value.ToArray();
    }

    public static GameState Load(string data)
    {
        var fixedStateData = FixGameStateString(data);
        return Utilities.Deserialize<GameState>(fixedStateData);
    }

    public static string GetStateAsString(Game g)
    {
        var state = new GameState { Version = g.Version, Events = g.History };
        return Utilities.Serialize(state);
    }

    private static string FixGameStateString(string state)
    {
        return state.Replace("Treachery.online", "Treachery");
    }
}