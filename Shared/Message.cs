/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Message
    {
        private static int Counter = 0;

        public string UnformattedBody { private get; set; }

        public object[] Parameters { get; set; }

        public int Nr { get; } = Counter++;

        public Faction Initiator { get; set; }

        public Faction Target { get; set; }

        public override string ToString()
        {
            return Skin.Current.Format(UnformattedBody, Parameters);
        }

        public Message()
        {

        }

        public Message(string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = Faction.None;
        }

        public Message(Faction f, string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = f;
        }

        public Message(Faction from, Faction to, string m, params object[] list)
        {
            UnformattedBody = m;
            Parameters = list;
            Initiator = from;
            Target = to;
        }
    }

    public class MessagePart
    {
        public string UnformattedBody { private get; set; }

        public object[] Parameters { get; set; }

        public MessagePart(string unformattedBody, params object[] parameters)
        {
            UnformattedBody = unformattedBody;
            Parameters = parameters;
        }

        public override string ToString()
        {
            return Skin.Current.Format(UnformattedBody, Parameters);
        }
    }
}
