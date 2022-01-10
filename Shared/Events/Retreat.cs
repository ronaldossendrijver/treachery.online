/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
            return g.CurrentRetreat == null && plan != null && g.SkilledAs(plan.Hero, LeaderSkill.Diplomat) && MaxForces(g, p) > 0 && ValidTargets(g, p).Any();
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

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(
                Initiator, 
                " try to retreat {1} {2} and {3} {4} to {5}", 
                Forces, 
                Player.Force, 
                MessagePart.ExpressIf(SpecialForces > 0, " and ", SpecialForces, Player.SpecialForce), 
                Location);
        }
    }
}
