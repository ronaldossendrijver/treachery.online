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

        public static IEnumerable<Location> ValidTargets(Game g)
        {
            var neighbouringEmptyNonStrongholdTerritories = g.CurrentBattle.Territory.Locations.SelectMany(l => l.Neighbours).Select(l => l.Territory).Distinct().Where(t => !g.AnyForcesIn(t) && !t.IsStronghold);
            return neighbouringEmptyNonStrongholdTerritories.SelectMany(t => t.Locations);
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
            if (Forces > 0 || SpecialForces > 0)
            {
                return Message.Express(
                    Initiator,
                    " retreat ",
                    MessagePart.ExpressIf(Forces > 0, Forces, " ", Player.Force),
                    MessagePart.ExpressIf(SpecialForces > 0, SpecialForces, " ", Player.SpecialForce),
                    " to ",
                    Location);
            }
            else
            {
                return Message.Express(Initiator, " don't retreat");
            }
        }
    }
}
