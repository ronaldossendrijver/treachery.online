/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class GlobalChatMessage : ChatMessage
    {
        public override Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g)
        {
            if (SourcePlayerName == receivingPlayerName)
            {
                return Message.Express(Body, " ⇒ GLOBAL");
            }
            else
            {
                return Message.Express(SourcePlayerName, ": ", Body, " ⇒ GLOBAL");
            }
        }
    }
}
