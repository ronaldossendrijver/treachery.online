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
                    return Message.Express(Body, " (to everyone)");
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
                    return Message.Express("{0} (from {1} to everyone)", Body, GetSourceFaction(g));
                }
                else
                {
                    return Message.Express("{0} (from {1} to you)", Body, GetSourceFaction(g));
                }
            }
        }

        public Faction GetSourceFaction(Game g)
        {
            var p = g.GetPlayer(SourcePlayerName);
            return p != null ? p.Faction : Faction.None;
        }

        public Faction GetTargetFaction(Game g)
        {
            var p = g.GetPlayer(TargetPlayerName);
            return p != null ? p.Faction : Faction.None;
        }
    }


}
