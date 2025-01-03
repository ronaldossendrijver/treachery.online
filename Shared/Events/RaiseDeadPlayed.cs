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

public class RaiseDeadPlayed : GameEvent, ILocationEvent
{
    #region Construction

    public RaiseDeadPlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public RaiseDeadPlayed()
    {
    }

    #endregion Construction

    #region Properties

    public int AmountOfForces { get; set; } = 0;

    public int AmountOfSpecialForces { get; set; } = 0;

    public bool AssignSkill { get; set; } = false;

    public int _heroId;

    [JsonIgnore]
    public IHero Hero
    {
        get => LeaderManager.HeroLookup.Find(_heroId);
        set => _heroId = LeaderManager.HeroLookup.GetId(value);
    }

    public int NumberOfSpecialForcesInLocation { get; set; }

    public int _locationId = -1;

    [JsonIgnore]
    public Location Location
    {
        get => Game.Map.LocationLookup.Find(_locationId);
        set => _locationId = Game.Map.LocationLookup.GetId(value);
    }

    [JsonIgnore]
    public Location To => Location;

    [JsonIgnore]
    public int TotalAmountOfForcesAddedToLocation => NumberOfSpecialForcesInLocation;

    [JsonIgnore]
    public int ForcesAddedToLocation => 0;

    [JsonIgnore]
    public int SpecialForcesAddedToLocation => NumberOfSpecialForcesInLocation;

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        var p = Player;
        if (AmountOfForces < 0 || AmountOfSpecialForces < 0) return Message.Express("You can't revive a negative amount of forces");
        if (AmountOfForces > p.ForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfSpecialForces > p.SpecialForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfForces + AmountOfSpecialForces > 5) return Message.Express("You can't revive that many");
        if (Initiator != Faction.Grey && AmountOfSpecialForces > 1) return Message.Express("You can only revive one ", p.SpecialForce, " per turn");
        if (AmountOfSpecialForces > 0 && Initiator != Faction.Grey && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Message.Express("You already revived one ", p.SpecialForce, " this turn");
        if (AmountOfForces + AmountOfSpecialForces > 0 && Hero != null) return Message.Express("You can't revive both forces and a leader");
        if (Hero != null && !ValidHeroes(Game, p).Contains(Hero)) return Message.Express("Invalid leader");

        if (AssignSkill && Hero == null) return Message.Express("You must revive a leader to assign a skill to");
        if (AssignSkill && !Revival.MayAssignSkill(Game, p, Hero)) return Message.Express("You can't assign a skill to this leader");

        if (Location != null)
        {
            if (!Revival.ValidRevivedForceLocations(Game, Player).Contains(Location)) return Message.Express("You can't place revived forces there");

            if (NumberOfSpecialForcesInLocation > Revival.NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, AmountOfSpecialForces)) return Message.Express("You can't place that many forces directly on the planet");
        }

        return null;
    }

    public static IEnumerable<IHero> ValidHeroes(Game game, Player player)
    {
        return game.KilledHeroes(player);
    }

    public static bool MaySelectLocationForRevivedForces(Game game, Player player, int specialForces)
    {
        return player.Is(Faction.Yellow) && specialForces >= 1 && player.HasHighThreshold() &&
               Revival.ValidRevivedForceLocations(game, player).Any();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Stone(Milestone.RaiseDead);
        Log();
        Game.Discard(Player, TreacheryCardType.RaiseDead);

        var purple = GetPlayer(Faction.Purple);
        if (purple != null)
        {
            purple.Resources += 1;
            Log(Faction.Purple, " get ", Payment.Of(1), " for revival by ", TreacheryCardType.RaiseDead);
        }

        if (Hero != null)
        {
            if (Initiator != Hero.Faction && Hero is Leader)
                Game.Revive(Player, Hero as Leader);
            else
                Game.Revive(Player, Hero);

            if (AssignSkill) Game.PrepareSkillAssignmentToRevivedLeader(Player, Hero as Leader);
        }
        else
        {
            Player.ReviveForces(AmountOfForces);
            Player.ReviveSpecialForces(AmountOfSpecialForces);

            if (AmountOfSpecialForces > 0) Game.FactionsThatRevivedSpecialForcesThisTurn.Add(Initiator);
        }

        if (Location != null && Initiator == Faction.Yellow)
        {
            Player.ShipSpecialForces(Location, 1);
            Log(Initiator, " place ", FactionSpecialForce.Yellow, " in ", Location);
        }
    }

    public override Message GetMessage()
    {
        if (Hero != null)
        {
            if (!Game.IsFaceDownDead(Hero))
                return Message.Express("Using ", TreacheryCardType.RaiseDead, ", ", Initiator, " revive ", Hero);
            return Message.Express("Using ", TreacheryCardType.RaiseDead, ", ", Initiator, " revive a face down leader");
        }

        return Message.Express(
            "Using ",
            TreacheryCardType.RaiseDead,
            ", ",
            Initiator,
            " revive ",
            MessagePart.ExpressIf(AmountOfForces > 0, AmountOfForces, " ", Player.Force),
            MessagePart.ExpressIf(AmountOfForces > 0 && AmountOfSpecialForces > 0, " and "),
            MessagePart.ExpressIf(AmountOfSpecialForces > 0, AmountOfSpecialForces, " ", Player.SpecialForce));
    }

    public static int ValidMaxAmount(Game g, Player p, bool specialForces)
    {
        if (specialForces)
        {
            if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
            {
                if (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction))
                    return 0;
                return Math.Min(p.SpecialForcesKilled, 1);
            }

            return Math.Min(p.SpecialForcesKilled, 5);
        }

        return Math.Min(p.ForcesKilled, 5);
    }

    #endregion Execution
}