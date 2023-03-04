/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DiscoveryRevealed : PassableGameEvent
    {
        public DiscoveryRevealed(Game game) : base(game)
        {
        }

        public DiscoveryRevealed()
        {
        }

        public DiscoveryToken Token { get; set; }

        public override Message Validate()
        {
            

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't reveal a discovery");
            }
            else 
            {
                return Message.Express(Initiator, " discover ", Token);
            }
        }

        public static bool Applicable(Game g, Player p) => g.CurrentMainPhase == MainPhase.Collection && GetTokens(g, p).Any();

        public static IEnumerable<DiscoveryToken> GetTokens(Game g, Player p) => g.DiscoveriesOnPlanet.Where(kvp => g.PendingDiscoveries.Contains(kvp.Value.Token) && p.Occupies(kvp.Key.Territory)).Select(kvp => kvp.Value.Token);

    }
}
