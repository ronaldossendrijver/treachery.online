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
                    return new Message(Initiator, "Using {2}, {0} revive {1}.", Initiator, Hero, TreacheryCardType.Karma);
                }
                else
                {
                    return new Message(Initiator, "Using {1}, {0} revive a leader.", Initiator, TreacheryCardType.Karma);
                }
            }
            else
            {
                if (AmountOfSpecialForces > 0)
                {
                    return new Message(Initiator, "Using {3}, {0} revive {1} {5} and {2} {4}.", Initiator, AmountOfForces, AmountOfSpecialForces, TreacheryCardType.Karma, p.SpecialForce, p.Force);
                }
                else
                {
                    return new Message(Initiator, "Using {2}, {0} revive {1} {3}.", Initiator, AmountOfForces, TreacheryCardType.Karma, p.Force);
                }
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
