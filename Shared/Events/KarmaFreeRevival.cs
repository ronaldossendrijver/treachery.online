/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

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

    private readonly int _heroId;

    [JsonIgnore]
    public IHero? Hero
    {
        get => LeaderManager.HeroLookup.Find(_heroId);
        init => _heroId = LeaderManager.HeroLookup.GetId(value);
    }

    public int AmountOfForces { get; set; }
    public int PowerCardId { get; set; } = -1;

    public int AmountOfSpecialForces { get; set; }

    public bool AssignSkill { get; set; } = false;

    #endregion Properties

    #region Validation

    public override Message? Validate()
    {
        var p = Player;
        if (AmountOfForces < 0 || AmountOfSpecialForces < 0) return Message.Express("Revival amounts cannot be negative");
        if (Game.SecondEdition && (Initiator != Faction.Red || Player.SpecialKarmaPowerUsed || Game.CurrentMainPhase != MainPhase.Resurrection || !Karma.ValidPowerCards(Game, p).Any())) return Message.Express("War Machine is not available");
        if (Game.SecondEdition && !Karma.IsValidPowerCard(Game, p, PowerCardId)) return Message.Express("Select a card to invoke War Machine");
        if (Hero != null && !Game.KilledHeroes(p).Contains(Hero)) return Message.Express("Invalid leader");
        if (AssignSkill && !Revival.MayAssignSkill(Game, p, Hero)) return Message.Express("Cannot assign this skill");

        if (AmountOfForces > p.ForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfSpecialForces > p.SpecialForcesKilled) return Message.Express("You can't revive that many");
        if (AmountOfForces + AmountOfSpecialForces > (Game.SecondEdition ? 4 : 3)) return Message.Express("You can't revive that many");
        if (!Game.SecondEdition && AmountOfSpecialForces > 1) return Message.Express("You can only revive one ", p.SpecialForce, " per turn");
        if (!Game.SecondEdition && AmountOfSpecialForces > 0 && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Message.Express("You already revived one ", p.SpecialForce, " this turn.");
        if (!Game.SecondEdition && AmountOfForces + AmountOfSpecialForces > 0 && Hero != null) return Message.Express("You can't revive both forces and a leader");

        return null;
    }

    public static int ValidMaxAmount(Player p, bool specialForces)
    {
        if (specialForces)
            return Math.Min(1, p.SpecialForcesKilled);
        return Math.Min(3, p.ForcesKilled);
    }

    public static int ValidMaxAmount(Game g, Player p, bool specialForces) =>
        g.SecondEdition ? Math.Min(4, specialForces ? p.SpecialForcesKilled : p.ForcesKilled) : ValidMaxAmount(p, specialForces);

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Stone(Milestone.Revival);
        Log();

        Game.Discard(Player, Karma.PowerCard(Game, Player, PowerCardId));
        Player.SpecialKarmaPowerUsed = true;

        if (Hero != null)
        {
            Game.Revive(Player, Hero);

            if (AssignSkill) Game.PrepareSkillAssignmentToRevivedLeader(Player, Hero as Leader);
        }
        if (Hero == null || Game.SecondEdition)
        {
            Player.ReviveForces(AmountOfForces);
            Player.ReviveSpecialForces(AmountOfSpecialForces);

            if (!Game.SecondEdition && AmountOfSpecialForces > 0) Game.FactionsThatRevivedSpecialForcesThisTurn.Add(Initiator);
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