/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class AmbassadorPlaced : GameEvent
{
    #region Construction

    public AmbassadorPlaced(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AmbassadorPlaced()
    {
    }

    #endregion Construction

    #region Properties

    public Ambassador Ambassador { get; set; }

    public int _strongholdId;

    [JsonIgnore]
    public Territory Stronghold
    {
        get => Game.Map.TerritoryLookup.Find(_strongholdId);
        set => _strongholdId = Game.Map.TerritoryLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidStrongholds(Game, Player).Contains(Stronghold)) return Message.Express("Invalid stronghold");
        if (!ValidAmbassadors(Player).Contains(Ambassador)) return Message.Express("Ambassador not available");

        return null;
    }

    public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
    {
        var ally = g.GetPlayer(p.Ally);

        return g.Map.Territories(false).Where(t =>
            t.IsVisible &&
            t.IsStronghold &&
            !g.IsInStorm(t) &&
            g.AmbassadorIn(t) == Ambassador.None);
    }

    public static IEnumerable<Ambassador> ValidAmbassadors(Player p)
    {
        return p.Ambassadors;
    }

    public static bool IsApplicable(Game g, Player p)
    {
        return p.Resources > g.AmbassadorsPlacedThisTurn &&
               g.CurrentPhase == Phase.ResurrectionReport &&
               !g.Prevented(FactionAdvantage.PinkAmbassadors) &&
               ValidAmbassadors(p).Any() &&
               ValidStrongholds(g, p).Any();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.AmbassadorsPlacedThisTurn++;
        Player.Resources -= Game.AmbassadorsPlacedThisTurn;
        Log(Initiator, " station the ", Ambassador, " ambassador in ", Stronghold, " for ", Payment.Of(Game.AmbassadorsPlacedThisTurn));
        Game.AmbassadorsOnPlanet.Add(Stronghold, Ambassador);
        Player.Ambassadors.Remove(Ambassador);
        Game.Stone(Milestone.AmbassadorPlaced);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " station the ", Ambassador, " ambassador in ", Stronghold);
    }

    #endregion Execution
}