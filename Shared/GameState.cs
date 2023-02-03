/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Treachery.Shared
{
    public class GameState
    {
        public int Version { get; set; }

        public GameEvent[] _events = null;

        [JsonIgnore]
        public IEnumerable<GameEvent> Events
        {
            get
            {
                return _events;
            }
            set
            {
                _events = value.ToArray();
            }
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

            var state = new GameState() { Version = g.Version, Events = g.History };
            serializer.Serialize(writer, state);
            writer.Close();
            return writer.ToString();
        }

        private static string FixGameStateString(string state)
        {
            return state.Replace("Treachery.online", "Treachery");
        }
    }
}
