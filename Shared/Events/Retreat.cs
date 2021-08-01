/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Retreat : GameEvent
    {
        public int _targetId;

        public int Forces { get; set; }

        public int SpecialForces { get; set; }

        public Retreat(Game game) : base(game)
        {
        }

        public Retreat()
        {
        }

        [JsonIgnore]
        public Location Location { get { return Game.Map.LocationLookup.Find(_targetId); } set { _targetId = Game.Map.LocationLookup.GetId(value); } }

        public override string Validate()
        {
            return "";
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var plan = g.CurrentBattle.PlanOf(p);
            return g.CurrentRetreat != null && plan != null && g.SkilledAs(plan.Hero, LeaderSkill.Diplomat) && MaxForces(g, p) > 0 && ValidTargets(g, p).Any();
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            return g.CurrentBattle.Territory.Locations.SelectMany(l => l.Neighbours.Where(neighbour => !g.AnyForcesIn(neighbour.Territory) && !neighbour.IsStronghold)).Distinct();
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

        private static bool AllyPreventsAccompanyingToShipmentLocation(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return
                !g.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                g.Version >= 38 &&
                (ally != null) &&
                ally.AnyForcesIn(g.LastShippedOrMovedTo.Territory) != 0;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (SpecialForces > 0)
            {
                return new Message(Initiator, "{0} retreat {1} {2} and {3} {4} to {5}.", Initiator, Forces, Player.Force, SpecialForces, Player.SpecialForce, Location);
            }
            else
            {
                return new Message(Initiator, "{0} retreat {1} {2} to {3}.", Initiator, Forces, Player.Force, Location);
            }
        }
    }
}
