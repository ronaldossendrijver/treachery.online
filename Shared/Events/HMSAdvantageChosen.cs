/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class HMSAdvantageChosen : GameEvent
{
    #region Construction

    public HMSAdvantageChosen(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public HMSAdvantageChosen()
    {
    }

    #endregion Construction

    #region Properties

    public StrongholdAdvantage Advantage { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        return g.ChosenHMSAdvantage == StrongholdAdvantage.None && g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p) && g.CurrentBattle.Territory == g.Map.HiddenMobileStronghold.Territory && g.OwnsStronghold(p.Faction, g.Map.HiddenMobileStronghold) && ValidAdvantages(g, p).Any();
    }

    public static IEnumerable<StrongholdAdvantage> ValidAdvantages(Game g, Player p)
    {
        var result = new List<StrongholdAdvantage>();
        if (g.OwnerOf(g.Map.Arrakeen) == p) result.Add(StrongholdAdvantage.FreeResourcesForBattles);
        if (g.OwnerOf(g.Map.Carthag) == p) result.Add(StrongholdAdvantage.CountDefensesAsAntidote);
        if (g.OwnerOf(g.Map.SietchTabr) == p) result.Add(StrongholdAdvantage.CollectResourcesForDial);
        if (g.OwnerOf(g.Map.TueksSietch) == p) result.Add(StrongholdAdvantage.CollectResourcesForUseless);
        if (g.OwnerOf(g.Map.HabbanyaSietch) == p) result.Add(StrongholdAdvantage.WinTies);
        return result;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.ChosenHMSAdvantage = Advantage;
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use the ", Advantage, " stronghold advantage for this battle");
    }

    #endregion Execution
}