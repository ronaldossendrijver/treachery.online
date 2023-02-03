/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<Location> ValidTargets(Game g, Player p)
        {
            var battalions = p.BattalionsIn(g.CurrentBattle.Territory);
            return PlacementEvent.ValidTargets(g, p, battalions).Where(t => !g.AnyForcesIn(t.Territory) && !t.IsStronghold);
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
