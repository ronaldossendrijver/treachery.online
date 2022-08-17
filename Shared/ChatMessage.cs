/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public abstract class ChatMessage
    {
        public string SourcePlayerName { get; set; }

        public string Body { get; set; }

        public DateTime DateTimeReceived { get; set; }

        public abstract Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g);
    }
}
