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

public class KarmaFreeRevival : GameEvent
{
    #region Construction

    public KarmaFreeRevival(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaFreeRevival()
    {
    }

    #endregion Construction

    #region Properties

    public int _heroId;

    [JsonIgnore]
    public IHero Hero
    {
        get => LeaderManager.HeroLookup.Find(_heroId);
        set => _heroId = LeaderManager.HeroLookup.GetId(value);
    }

    public int AmountOfForces { get; set; }

    public int AmountOfSpecialForces { get; set; }

    public bool AssignSkill { get; set; } = false;

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        var p = Player;

        if (AmountOfForces > p.ForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfSpecialForces > p.SpecialForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfForces + AmountOfSpecialForces > 3) return Message.Express("You can't revive that many");
        if (AmountOfSpecialForces > 1) return Message.Express("You can only revive one ", p.SpecialForce, " per turn");
        if (AmountOfSpecialForces > 0 && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Message.Express("You already revived one ", p.SpecialForce, " this turn.");
        if (AmountOfForces + AmountOfSpecialForces > 0 && Hero != null) return Message.Express("You can't revive both forces and a leader");

        return null;
    }

    public static int ValidMaxAmount(Player p, bool specialForces)
    {
        if (specialForces)
            return Math.Min(1, p.SpecialForcesKilled);
        return Math.Min(3, p.ForcesKilled);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Stone(Milestone.Revival);
        Log();

        Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
        Player.SpecialKarmaPowerUsed = true;

        if (Hero != null)
        {
            Game.Revive(Player, Hero);

            if (AssignSkill) Game.PrepareSkillAssignmentToRevivedLeader(Player, Hero as Leader);
        }
        else
        {
            Player.ReviveForces(AmountOfForces);
            Player.ReviveSpecialForces(AmountOfSpecialForces);

            if (AmountOfSpecialForces > 0) Game.FactionsThatRevivedSpecialForcesThisTurn.Add(Initiator);
        }
    }

    public override Message GetMessage()
    {
        var p = Player;
        if (Hero != null)
        {
            if (!Game.IsFaceDownDead(Hero))
                return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " revive ", Hero);
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " revive a face down leader");
        }

        return Message.Express(
            "Using ",
            TreacheryCardType.Karma,
            ", ",
            Initiator,
            " revive ",
            MessagePart.ExpressIf(AmountOfForces > 0, AmountOfForces, p.Force),
            MessagePart.ExpressIf(AmountOfForces > 0 && AmountOfSpecialForces > 0, " and "),
            MessagePart.ExpressIf(AmountOfSpecialForces > 0, AmountOfSpecialForces, p.SpecialForce));
    }

    #endregion Execution
}