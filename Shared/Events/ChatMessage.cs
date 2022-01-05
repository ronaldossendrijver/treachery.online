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

        public string GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g)
        {
            if (SourcePlayerName == receivingPlayerName)
            {
                if (TargetPlayerName == "")
                {
                    return Skin.Current.Format("{0} (to everyone)", Body);
                }
                else
                {
                    return Skin.Current.Format("{0} (to {1})", Body, g.GetPlayer(TargetPlayerName).Faction);
                }
            }
            else
            {
                if (TargetPlayerName == "")
                {
                    return Skin.Current.Format("{0} (from {1} to everyone)", Body, GetSourceFaction(g));
                }
                else
                {
                    return Skin.Current.Format("{0} (from {1} to you)", Body, GetSourceFaction(g));
                }
            }
        }

        public Faction GetSourceFaction(Game g)
        {
            var sourcePlayer = g.GetPlayer(SourcePlayerName);
            return sourcePlayer != null ? sourcePlayer.Faction : Faction.None;
        }
    }


}
