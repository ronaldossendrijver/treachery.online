/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RaiseDeadPlayed : GameEvent, ILocationEvent
    {
        public int _heroId;

        public RaiseDeadPlayed(Game game) : base(game)
        {
        }

        public RaiseDeadPlayed()
        {
        }

        public int AmountOfForces { get; set; } = 0;

        public int AmountOfSpecialForces { get; set; } = 0;

        public bool AssignSkill { get; set; } = false;

        [JsonIgnore]
        public IHero Hero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_heroId);
            }
            set
            {
                _heroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public int _locationId = -1;

        [JsonIgnore]
        public Location Location { get { return Game.Map.LocationLookup.Find(_locationId); } set { _locationId = Game.Map.LocationLookup.GetId(value); } }

        [JsonIgnore]
        public Location To => Location;

        [JsonIgnore]
        public int TotalAmountOfForces => Initiator == Faction.Yellow ? AmountOfSpecialForces : AmountOfForces;

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
                if (!Revival.MaySelectLocationForRevivedForces(Game, Player, AmountOfForces, AmountOfSpecialForces, false)) return Message.Express("You can't place revived forces directly on the planet");
                if (!Revival.ValidRevivedForceLocations(Game, Player).Contains(Location)) return Message.Express("You can't place revived forces there");
            }

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Hero != null)
            {
                if (!Game.LeaderState[Hero].IsFaceDownDead)
                {
                    return Message.Express("Using ", TreacheryCardType.RaiseDead, ", ", Initiator, " revive ", Hero);
                }
                else
                {
                    return Message.Express("Using ", TreacheryCardType.RaiseDead, ", ", Initiator, " revive a face down leader");
                }
            }
            else
            {
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
        }

        public static int ValidMaxAmount(Game g, Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    if (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction))
                    {
                        return 0;
                    }
                    else
                    {
                        return Math.Min(p.SpecialForcesKilled, 1);
                    }
                }
                else
                {
                    return Math.Min(p.SpecialForcesKilled, 5);
                }
            }
            else
            {
                return Math.Min(p.ForcesKilled, 5);
            }
        }

        public static IEnumerable<IHero> ValidHeroes(Game game, Player player) => game.KilledHeroes(player);

        public static bool MaySelectLocationForRevivedForces(Game game, Player player, int specialForces) =>
            player.Is(Faction.Yellow) && specialForces >= 1 && player.HasHighThreshold() && Revival.ValidRevivedForceLocations(game, player).Any();

    }
}
