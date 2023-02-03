/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class GameChatMessage : ChatMessage
    {
        public string TargetPlayerName { get; set; }

        public override Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g, bool contextIsGlobal)
        {
            if (SourcePlayerName == receivingPlayerName)
            {
                if (TargetPlayerName == "")
                {
                    return Message.Express("You: ", Body, " ⇒ ALL");
                }
                else
                {
                    return Message.Express("You: ", Body, " ⇒ ", GetTargetFaction(g));
                }
            }
            else
            {
                var sourceFaction = GetSourceFaction(g);

                if (TargetPlayerName == "")
                {
                    if (sourceFaction != Faction.None)
                    {
                        return Message.Express(GetSourceFaction(g), " (to ALL) ", Body);
                    }
                    else
                    {
                        return Message.Express(SourcePlayerName, " (to ALL) ", Body);
                    }
                }
                else
                {
                    if (sourceFaction != Faction.None)
                    {
                        return Message.Express(GetSourceFaction(g), Body);
                    }
                    else
                    {
                        return Message.Express(SourcePlayerName, Body);
                    }
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
