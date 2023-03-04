/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Discovery
    {
        public DiscoveryToken Token { get; private set; }

        public DiscoveryTokenType TokenType { get; private set; }

        public Discovery(DiscoveryToken token, DiscoveryTokenType tokenType)
        {
            Token = token;
            TokenType = tokenType;
        }
    }
}
