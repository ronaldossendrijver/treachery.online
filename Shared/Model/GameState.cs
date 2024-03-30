/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class GameState
{
    public int Version { get; set; }

    public GameEvent[] _events;

    [JsonIgnore]
    public IEnumerable<GameEvent> Events
    {
        get => _events;
        set => _events = value.ToArray();
    }

    public static GameState Load(string data)
    {
        var fixedStateData = FixGameStateString(data);
        var serializer = JsonSerializer.CreateDefault();
        serializer.TypeNameHandling = TypeNameHandling.All;
        var textReader = new StringReader(fixedStateData);
        var jsonReader = new JsonTextReader(textReader);
        var result = serializer.Deserialize<GameState>(jsonReader);
        return result;
    }

    public static string GetStateAsString(Game g)
    {
        //https://wellsb.com/csharp/aspnet/blazor-jsinterop-save-file/
        var serializer = JsonSerializer.CreateDefault();
        serializer.TypeNameHandling = TypeNameHandling.All;
        var writer = new StringWriter();

        /*
         * Check if there are problems serializing one of the GameEvents
         *
        var testlist = new List<GameEvent>();
        foreach (var e in g.History)
        {
            var testwriter = new StringWriter();
            testlist.Add(e);
            var teststate = new GameState() { Version = g.Version, Events = testlist };
            serializer.Serialize(testwriter, teststate);
        }*/

        var state = new GameState { Version = g.Version, Events = g.History };
        serializer.Serialize(writer, state);
        writer.Close();
        return writer.ToString();
    }

    private static string FixGameStateString(string state)
    {
        return state.Replace("Treachery.online", "Treachery");
    }
}