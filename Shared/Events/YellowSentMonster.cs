/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class YellowSentMonster : GameEvent
{
    #region Construction

    public YellowSentMonster(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public YellowSentMonster()
    {
    }

    #endregion Construction

    #region Properties

    public int _territoryId;

    [JsonIgnore]
    public Territory Territory
    {
        get => Game.Map.TerritoryLookup.Find(_territoryId);
        set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Territory == null) return Message.Express("No territory selected");
        if (Territory.IsProtectedFromWorm) return Message.Express("Selected territory is protected");

        return null;
    }

    public static IEnumerable<Territory> ValidTargets(Game g)
    {
        return g.Map.Territories(false).Where(t => !t.IsProtectedFromWorm);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        var monster = new MonsterAppearence(Territory, false);
        Game.Monsters.Add(monster);
        Game.PerformMonster(monster);
        Game.Enter(Game.CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
        Game.DrawResourceCard();
        Game.LetFactionsDiscardSurplusCards();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " send ", Concept.Monster, " to ", Territory);
    }

    #endregion Execution
}