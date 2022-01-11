/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RaiseDeadPlayed : GameEvent
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

        public override string Validate()
        {
            var p = Player;
            if (AmountOfForces < 0 || AmountOfSpecialForces < 0) return "You can't revive a negative amount of forces.";
            if (AmountOfForces > p.ForcesKilled) return "You can't revive that much.";
            if (AmountOfSpecialForces > p.SpecialForcesKilled) return "You can't revive that much.";
            if (AmountOfForces + AmountOfSpecialForces > 5) return "You can't revive that much.";
            if (Initiator != Faction.Grey && AmountOfSpecialForces > 1) return Skin.Current.Format("You can only revive one {0} per turn.", p.SpecialForce);
            if (AmountOfSpecialForces > 0 && Initiator != Faction.Grey && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Skin.Current.Format("You already revived one {0} this turn.", p.SpecialForce);
            if (AmountOfForces + AmountOfSpecialForces > 0 && Hero != null) return "You can't revive both forces and a leader";
            return "";
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
                    MessagePart.ExpressIf(AmountOfForces > 0, " and ", AmountOfForces, Player.Force),
                    MessagePart.ExpressIf(AmountOfSpecialForces > 0, " and ", AmountOfSpecialForces, Player.SpecialForce));
            }
        }

        public static IEnumerable<int> ValidAmounts(Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    return Enumerable.Range(0, Math.Min(p.SpecialForcesKilled, 1) + 1);
                }
                else
                {
                    return Enumerable.Range(0, Math.Min(p.SpecialForcesKilled, 5) + 1);
                }
            }
            else
            {
                return Enumerable.Range(0, Math.Min(p.ForcesKilled, 5) + 1);
            }
        }

        public static int ValidMaxAmount(Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    return Math.Min(p.SpecialForcesKilled, 1);
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
    }
}
