/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class GameChatMessage : ChatMessage
    {
        public string TargetPlayerName { get; set; }

        public override Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g)
        {
            if (SourcePlayerName == receivingPlayerName)
            {
                if (TargetPlayerName == "")
                {
                    return Message.Express(Body, " ⇒ ALL");
                }
                else
                {
                    return Message.Express(Body, " ⇒ ", GetTargetFaction(g));
                }
            }
            else
            {
                if (TargetPlayerName == "")
                {
                    return Message.Express(GetSourceFaction(g), " (to ALL) ", Body);
                }
                else
                {
                    return Message.Express(GetSourceFaction(g), Body);
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
