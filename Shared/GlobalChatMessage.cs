/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class GlobalChatMessage : ChatMessage
    {
        public override Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g, bool contextIsGlobal)
        {
            if (contextIsGlobal)
            {
                if (SourcePlayerName == receivingPlayerName)
                {
                    return Message.Express("You: ", Body);
                }
                else
                {
                    return Message.Express(SourcePlayerName, ": ", Body);
                }
            }
            else
            {
                if (SourcePlayerName == receivingPlayerName)
                {
                    return Message.Express("You: ", Body, " ⇒ GLOBAL");
                }
                else
                {
                    return Message.Express(SourcePlayerName, ": ", Body, " ⇒ GLOBAL");
                }
            }
        }
    }
}
