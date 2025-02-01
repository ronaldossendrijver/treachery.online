/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

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

    public static bool Applicable(Game g, Player p)
    {
        return g.CurrentMainPhase == MainPhase.Collection && GetLocations(g, p).Any();
    }

    public static IEnumerable<Location> GetLocations(Game g, Player p)
    {
        return g.DiscoveriesOnPlanet
            .Where(kvp => g.PendingDiscoveries.Contains(kvp.Value.Token) && p.Occupies(kvp.Key.Territory))
            .Select(kvp => kvp.Key);
    }

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
                    sh.PointAt(Game, discovery.Location);
                    Game.JustRevealedDiscoveryStrongholds.Add(sh);
                    break;

                case DiscoveryToken.CardStash:
                    var card = Game.DrawTreacheryCard();
                    Player.TreacheryCards.Add(card);
                    Log(Initiator, " draw a treachery card");
                    if (Player.HandSizeExceeded) Game.LetFactionsDiscardSurplusCards();
                    break;

                case DiscoveryToken.ResourceStash:
                    Player.Resources += 7;
                    Log(Initiator, " get ", Payment.Of(7));
                    break;

                case DiscoveryToken.Flight:
                    Game.OwnerOfFlightDiscovery = Initiator;
                    Log(Initiator, " gain the ", discovery.Token, " discovery token");
                    break;
            }
        }
    }

    public override Message GetMessage()
    {
        if (Passed)
            return Message.Express(Initiator, " don't reveal a discovery");
        return Message.Express(Initiator, " reveal a discovery in ", Location.Territory);
    }

    #endregion Execution
}