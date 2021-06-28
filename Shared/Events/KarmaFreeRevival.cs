/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override string Validate()
        {
            var p = Player;

            if (AmountOfForces > p.ForcesKilled) return "You can't revive that much.";
            if (AmountOfSpecialForces > p.SpecialForcesKilled) return "You can't revive that much.";
            if (Game.Version >= 23 && AmountOfForces + AmountOfSpecialForces > 3) return "You can't revive that much.";
            if (Game.Version >= 32 && AmountOfSpecialForces > 1) return Skin.Current.Format("You can only revive one {0} per turn.", p.SpecialForce);
            if (Game.Version >= 32 && AmountOfSpecialForces > 0 && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Skin.Current.Format("You already revived one {0} this turn.", p.SpecialForce);
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
                    return Enumerable.Range(0, Math.Min(p.SpecialForcesKilled, 3) + 1);
                }
            }
            else
            {
                return Enumerable.Range(0, Math.Min(p.ForcesKilled, 3) + 1);
            }
        }
    }
}
