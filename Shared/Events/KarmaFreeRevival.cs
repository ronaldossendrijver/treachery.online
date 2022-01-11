/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;

namespace Treachery.Shared
{
    public class KarmaFreeRevival : GameEvent
    {
        public int _heroId;

        public KarmaFreeRevival(Game game) : base(game)
        {
        }

        public KarmaFreeRevival()
        {
        }

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

        public int AmountOfForces { get; set; }

        public int AmountOfSpecialForces { get; set; }

        public bool AssignSkill { get; set; } = false;

        public override string Validate()
        {
            var p = Player;

            if (AmountOfForces > p.ForcesKilled) return "You can't revive that much.";
            if (AmountOfSpecialForces > p.SpecialForcesKilled) return "You can't revive that much.";
            if (AmountOfForces + AmountOfSpecialForces > 3) return "You can't revive that much.";
            if (AmountOfSpecialForces > 1) return Skin.Current.Format("You can only revive one {0} per turn.", p.SpecialForce);
            if (AmountOfSpecialForces > 0 && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Skin.Current.Format("You already revived one {0} this turn.", p.SpecialForce);
            if (AmountOfForces + AmountOfSpecialForces > 0 && Hero != null) return "You can't revive both forces and a leader";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            var p = Player;
            if (Hero != null)
            {
                if (!Game.LeaderState[Hero].IsFaceDownDead)
                {
                    return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " revive ", Hero);
                }
                else
                {
                    return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " revive a face down leader");
                }
            }
            else
            {
                return Message.Express(
                    "Using ",
                    TreacheryCardType.Karma,
                    ", ",
                    Initiator,
                    " revive ",
                    MessagePart.ExpressIf(AmountOfForces > 0, " and ", AmountOfForces, p.Force),
                    MessagePart.ExpressIf(AmountOfSpecialForces > 0, " and ", AmountOfSpecialForces, p.SpecialForce));
            }
        }

        public static int ValidMaxAmount(Player p, bool specialForces)
        {
            if (specialForces)
            {
                return Math.Min(1, p.SpecialForcesKilled);
            }
            else
            {
                return Math.Min(3, p.ForcesKilled);
            }
        }
    }
}
