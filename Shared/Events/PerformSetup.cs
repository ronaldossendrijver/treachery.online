/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class PerformSetup : PlacementEvent
{
    #region Construction

    public PerformSetup(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public PerformSetup()
    {
    }

    #endregion Construction

    #region Properties

    public int Resources { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        var faction = Game.NextFactionToPerformCustomSetup;
        var p = Game.GetPlayer(faction);
        var numberOfSpecialForces = ForceLocations.Values.Sum(b => b.AmountOfSpecialForces);
        if (numberOfSpecialForces > p.SpecialForcesInReserve) return Message.Express("Too many ", p.SpecialForce);

        var numberOfForces = ForceLocations.Values.Sum(b => b.AmountOfForces);
        if (numberOfForces > p.ForcesInReserve) return Message.Express("Too many ", p.Force);

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var faction = Game.NextFactionToPerformCustomSetup;
        var player = GetPlayer(faction);

        foreach (var fl in ForceLocations)
        {
            var location = fl.Key;
            player.ShipForces(location, fl.Value.AmountOfForces);
            player.ShipSpecialForces(location, fl.Value.AmountOfSpecialForces);
        }

        player.Resources = Resources;

        Log(faction, " initial positions set, starting with ", Payment.Of(Resources));
        Game.HasActedOrPassed.Add(faction);

        if (Game.Players.Count == Game.HasActedOrPassed.Count) Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " initial positions and ", Concept.Resource, " (", Payment.Of(Resources), ") determined");
    }

    #endregion Execution
}