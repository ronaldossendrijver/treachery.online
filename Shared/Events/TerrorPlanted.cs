/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class TerrorPlanted : PassableGameEvent
{
    #region Construction

    public TerrorPlanted(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public TerrorPlanted()
    {
    }

    #endregion Construction

    #region Properties

    public TerrorType Type { get; set; }

    public int _territoryId;

    [JsonIgnore]
    public Territory Stronghold
    {
        get => Game.Map.TerritoryLookup.Find(_territoryId);
        set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;
        
        if (Stronghold == null)
        {
            if (!Game.TerrorOnPlanet.ContainsKey(Type)) return Message.Express("You can't remove a token that is not on the board");
        }
        else
        {
            if (!ValidStrongholds(Game, Player).Contains(Stronghold)) return Message.Express("Invalid Stronghold");
            if (!ValidTerrorTypes(Game, false).Contains(Type)) return Message.Express(Type, " not available");
        }

        return null;
    }

    public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
    {
        if (g.CurrentMainPhase is not MainPhase.Contemplate)
            return [];
        
        return g.Map.Territories(false).Where(t =>
            t.IsVisible &&
            t.IsStronghold &&
            t != g.Map.HiddenMobileStronghold.Territory &&
            (!g.TerrorIn(t).Any() || MayPlaceAtExistingToken(p)));
    }

    public static bool MayRemoveTokens(Game g, Player p)
    {
        return g.CurrentMainPhase is MainPhase.Collection && p.HasHighThreshold(World.Cyan) && g.TerrorOnPlanet.Any();
    }

    public static bool MayPlaceAtExistingToken(Player p)
    {
        return p.HasHighThreshold(World.Cyan) || (p.Nexus == Faction.Cyan && NexusPlayed.CanUseCunning(p));
    }

    public static IEnumerable<TerrorType> ValidTerrorTypes(Game g, bool toRemove)
    {
        if (toRemove)
            return g.TerrorOnPlanet.Keys;
        
        return g.UnplacedTerrorTokens.Union(g.TerrorOnPlanet.Keys);
    }

    public static bool IsApplicable(Game g, Player p)
    {
        return MayRemoveTokens(g, p)
               || 
               g.CurrentPhase is Phase.Contemplate &&
               ValidTerrorTypes(g, false).Any() &&
               ValidStrongholds(g, p).Any() &&
               !g.CyanHasPerformedTerror &&
               !g.Prevented(FactionAdvantage.CyanPlantingTerror);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log(GetVerboseMessage());

        Game.CyanHasPerformedTerror = true;
        
        if (!Passed)
        {
            Game.Stone(Milestone.TerrorPlanted);

            if (Stronghold == null)
            {
                Game.TerrorOnPlanet.Remove(Type);
                Game.UnplacedTerrorTokens.Add(Type);
                Player.Resources += 4;
            }
            else
            {
                if (Game.TerrorIn(Stronghold).Any() && !Player.HasHighThreshold(World.Cyan)) Game.PlayNexusCard(Player, "Cunning", "to plant additional Terror in ", Stronghold);

                if (Game.UnplacedTerrorTokens.Contains(Type))
                {
                    Game.TerrorOnPlanet.Add(Type, Stronghold);
                    Game.UnplacedTerrorTokens.Remove(Type);
                }
                else
                {
                    Game.TerrorOnPlanet.Remove(Type);
                    Game.TerrorOnPlanet.Add(Type, Stronghold);
                }
            }
        }
    }

    private Message GetVerboseMessage()
    {
        if (Passed) return Message.Express(Initiator, " don't plant terror");

        if (Stronghold != null)
        {
            if (Game.TerrorOnPlanet.TryGetValue(Type, out var territory))
                return Message.Express(Initiator, " move terror from ", territory, " to ", Stronghold);
            return Message.Express(Initiator, " plant terror in ", Stronghold);
        }

        return Message.Express(Initiator, " remove a terror token to get ", Payment.Of(4));
    }

    public override Message GetMessage()
    {
        if (Passed)
            return Message.Express(Initiator, " don't plant terror");
        if (Stronghold != null)
            return Message.Express(Initiator, " plant terror in ", Stronghold);
        return Message.Express(Initiator, " remove a terror token to get ", Payment.Of(4));
    }

    #endregion Execution
}