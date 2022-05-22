/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class ChatMessage
    {
        public string SourcePlayerName { get; set; }

        public string TargetPlayerName { get; set; }

        public string Body { get; set; }

        public DateTime DateTimeReceived { get; set; }

        public Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g)
        {
            if (SourcePlayerName == receivingPlayerName)
            {
                if (TargetPlayerName == "")
                {
                    return Message.Express(Body, " (to ALL)");
                }
                else
                {
                    return Message.Express(Body, " (to ", GetTargetFaction(g), ")");
                }
            }
            else
            {
                if (TargetPlayerName == "")
                {
                    return Message.Express(Body, " (from ", GetSourceFaction(g), " to ALL)");
                }
                else
                {
                    return Message.Express(Body, " (from ", GetSourceFaction(g), " to you)");
                }
            }
        }

        public Faction GetSourceFaction(Game g)
        {
            var p = g.GetPlayer(SourcePlayerName);
            return p == null ? Faction.None : p.Faction;
        }

        public Faction GetTargetFaction(Game g)
        {
            var p = g.GetPlayer(TargetPlayerName);
            return p == null ? Faction.None : p.Faction;
        }
    }


}
