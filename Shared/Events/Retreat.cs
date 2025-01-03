/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class Retreat : GameEvent
{
    #region Construction

    public Retreat(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Retreat()
    {
    }

    #endregion Construction

    #region Properties

    public int Forces { get; set; }

    public int SpecialForces { get; set; }

    public int _targetId;

    [JsonIgnore]
    public Location Location
    {
        get => Game.Map.LocationLookup.Find(_targetId);
        set => _targetId = Game.Map.LocationLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidTargets(Game, Player).Contains(Location)) return Message.Express("Invalid location");
        if (Forces > MaxForces(Game, Player)) return Message.Express("You selected too many ", Player.Force);
        if (SpecialForces > MaxSpecialForces(Game, Player)) return Message.Express("You selected too many ", Player.SpecialForce);
        if (Game.Version >= 164 && Forces + SpecialForces > MaxTotalForces(Game, Player)) return Message.Express("You selected too many forces");
        return null;
    }

    public static IEnumerable<Location> ValidTargets(Game g, Player p)
    {
        var battalions = p.BattalionsIn(g.CurrentBattle.Territory);
        return PlacementEvent.ValidTargets(g, p, battalions).Where(t => !g.AnyForcesIn(t.Territory) && !t.IsStronghold);
    }
    
    public static int MaxTotalForces(Game g, Player p)
    {
        var plan = g.CurrentBattle.PlanOf(p);
        var opponentPlan = g.CurrentBattle.PlanOfOpponent(p);
        return plan.Hero.ValueInCombatAgainst(opponentPlan.Hero);
    }

    public static int MaxForces(Game g, Player p)
    {
        var plan = g.CurrentBattle.PlanOf(p);
        return p.ForcesIn(g.CurrentBattle.Territory) - plan.Forces - plan.ForcesAtHalfStrength;
    }

    public static int MaxSpecialForces(Game g, Player p)
    {
        var plan = g.CurrentBattle.PlanOf(p);
        return p.SpecialForcesIn(g.CurrentBattle.Territory) - plan.SpecialForces - plan.SpecialForcesAtHalfStrength;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var forcesToMove = Forces;
        foreach (var l in Game.CurrentBattle.Territory.Locations.Where(l => Player.ForcesIn(l) > 0).ToArray())
        {
            if (forcesToMove == 0) break;
            var toMoveFromHere = Math.Min(forcesToMove, Player.ForcesIn(l));
            Player.MoveForces(l, Location, toMoveFromHere);
            forcesToMove -= toMoveFromHere;
        }

        var specialForcesToMove = SpecialForces;
        foreach (var l in Game.CurrentBattle.Territory.Locations.Where(l => Player.SpecialForcesIn(l) > 0).ToArray())
        {
            if (specialForcesToMove == 0) break;
            var toMoveFromHere = Math.Min(specialForcesToMove, Player.SpecialForcesIn(l));
            Player.MoveSpecialForces(l, Location, toMoveFromHere);
            specialForcesToMove -= toMoveFromHere;
        }

        Log();
        Game.HandleLosses();
        Game.FlipBeneGesseritWhenAlone();
        Game.DetermineHowToProceedAfterRevealingBattlePlans();
    }

    public override Message GetMessage()
    {
        if (Forces > 0 || SpecialForces > 0)
            return Message.Express(
                Initiator,
                " retreat ",
                MessagePart.ExpressIf(Forces > 0, Forces, " ", Player.Force),
                MessagePart.ExpressIf(SpecialForces > 0, SpecialForces, " ", Player.SpecialForce),
                " to ",
                Location);
        return Message.Express(Initiator, " don't retreat");
    }

    #endregion Execution
}