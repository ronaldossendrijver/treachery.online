/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class Planetology : GameEvent
{
    #region Construction

    public Planetology(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Planetology()
    {
    }

    #endregion Construction

    #region Properties

    public bool AddOneToMovement { get; set; }

    [JsonIgnore]
    public bool MoveFromTwoTerritories => !AddOneToMovement;

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        return g.SkilledAs(p, LeaderSkill.Planetologist) && g.CurrentPlanetology == null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.CurrentPlanetology = this;
    }

    public override Message GetMessage()
    {
        if (AddOneToMovement)
            return Message.Express(LeaderSkill.Planetologist, " adds ", 1, " to force movement");
        return Message.Express(LeaderSkill.Planetologist, " allows movement from ", 2, " different territories");
    }

    #endregion Execution
}