/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Retreat : GameEvent
    {
        #region Construction

        public Retreat(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Retreat()
        {
        }

        #endregion Construction

        #region Properties

        public int Forces { get; set; }

        public int SpecialForces { get; set; }

        public int _targetId;

        [JsonIgnore]
        public Location Location
        {
            get => Game.Map.LocationLookup.Find(_targetId);
            set => _targetId = Game.Map.LocationLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTargets(Game, Player).Contains(Location)) return Message.Express("Invalid location");
            if (Forces > MaxForces(Game, Player)) return Message.Express("You selected too many ", Player.Force);
            if (SpecialForces > MaxSpecialForces(Game, Player)) return Message.Express("You selected too many ", Player.SpecialForce);

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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            int forcesToMove = Forces;
            foreach (var l in Game.CurrentBattle.Territory.Locations.Where(l => Player.ForcesIn(l) > 0).ToArray())
            {
                if (forcesToMove == 0) break;
                int toMoveFromHere = Math.Min(forcesToMove, Player.ForcesIn(l));
                Player.MoveForces(l, Location, toMoveFromHere);
                forcesToMove -= toMoveFromHere;
            }

            int specialForcesToMove = SpecialForces;
            foreach (var l in Game.CurrentBattle.Territory.Locations.Where(l => Player.SpecialForcesIn(l) > 0).ToArray())
            {
                if (specialForcesToMove == 0) break;
                int toMoveFromHere = Math.Min(specialForcesToMove, Player.SpecialForcesIn(l));
                Player.MoveSpecialForces(l, Location, toMoveFromHere);
                specialForcesToMove -= toMoveFromHere;
            }

            Log();
            Game.HandleLosses();
            Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
            Game.DetermineHowToProceedAfterRevealingBattlePlans();
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

        #endregion Execution
    }
}
