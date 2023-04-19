/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DiscoveryRevealed : PassableGameEvent
    {
        #region Construction

        public DiscoveryRevealed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public DiscoveryRevealed()
        {
        }

        #endregion  Construction

        #region Properties

        public int _locationId;

        [JsonIgnore]
        public Location Location
        {
            get => Game.Map.LocationLookup.Find(_locationId);
            set => _locationId = Game.Map.LocationLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!GetLocations(Game, Player).Contains(Location)) return Message.Express("Invalid location");

            return null;
        }

        public static bool Applicable(Game g, Player p) => g.CurrentMainPhase == MainPhase.Collection && GetLocations(g, p).Any();

        public static IEnumerable<Location> GetLocations(Game g, Player p) => g.DiscoveriesOnPlanet.Where(kvp => g.PendingDiscoveries.Contains(kvp.Value.Token) && p.Occupies(kvp.Key.Territory)).Select(kvp => kvp.Key);

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var discovery = Game.DiscoveriesOnPlanet[Location];
            Game.PendingDiscoveries.Remove(discovery.Token);

            if (!Passed)
            {
                Game.Stone(Milestone.DiscoveryRevealed);
                Game.DiscoveriesOnPlanet.Remove(discovery.Location);

                Log(Initiator, " reveal ", discovery.Token, " in ", discovery.Location.Territory);

                switch (discovery.Token)
                {
                    case DiscoveryToken.Jacurutu:
                    case DiscoveryToken.Shrine:
                    case DiscoveryToken.TestingStation:
                    case DiscoveryToken.Cistern:
                    case DiscoveryToken.ProcessingStation:
                        var sh = Game.Map.GetDiscoveryStronghold(discovery.Token);
                        sh.PointAt(discovery.Location);
                        Game.JustRevealedDiscoveryStrongholds.Add(sh);
                        break;

                    case DiscoveryToken.CardStash:
                        var card = Game.DrawTreacheryCard();
                        Player.TreacheryCards.Add(card);
                        Log(Initiator, " draw a treachery card");
                        if (Player.HandSizeExceeded)
                        {
                            Game.LetFactionsDiscardSurplusCards();
                        }
                        break;

                    case DiscoveryToken.ResourceStash:
                        Player.Resources += 7;
                        Log(Initiator, " get ", Payment.Of(7));
                        break;

                    case DiscoveryToken.Flight:
                        Game.OwnerOfFlightDiscovery = Initiator;
                        Log(Initiator, " gain the ", discovery.Token, " discovery token");
                        break;

                    default:
                        break;
                }
            }
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't reveal a discovery");
            }
            else
            {
                return Message.Express(Initiator, " reveal a discovery in ", Location.Territory);
            }
        }

        #endregion Execution
    }
}
