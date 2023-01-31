/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FaceDanced : PlacementEvent
    {
        //Needed for game version <= 150
        public bool FaceDancerCalled { get; set; }

        public int ForcesFromReserve { get; set; }

        public string _targetForceLocations = "";

        public FaceDanced(Game game) : base(game)
        {
        }

        public FaceDanced()
        {
        }

        [JsonIgnore]
        public Dictionary<Location, Battalion> TargetForceLocations
        {
            get
            {
                return ParseForceLocations(Game, Player.Faction, _targetForceLocations);
            }
            set
            {
                _targetForceLocations = ForceLocationsString(Game, value);
            }
        }

        public override Message Validate()
        {
            if (!FaceDancerCalled) return null;

            var p = Player;
            if (!MayCallFaceDancer(Game, p)) return Message.Express("You can't reveal a Face Dancer");

            int amountOfForces = ForcesFromReserve + ForceLocations.Values.Sum(b => b.TotalAmountOfForces);
            int maximumForces = MaximumNumberOfForces(Game, p);
            if (amountOfForces > maximumForces) return Message.Express("Place ", maximumForces, " or less forces");

            int amountOfTargetForces = TargetForceLocations.Values.Sum(b => b.TotalAmountOfForces);
            if (amountOfForces != amountOfTargetForces) return Message.Express("The amount of forces you selected from the planet and from reserves (", amountOfForces, ") should equal the amount you wish to put in ", Game.CurrentBattle.Territory, " (", amountOfTargetForces, ")");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(!FaceDancerCalled, " don't"), " reveal a Face Dancer!");
        }

        public static int MaximumNumberOfForces(Game g, Player p)
        {
            var ally = p.AlliedPlayer;
            if (ally == null || g.BattleWinner == p.Ally || ally.AnyForcesIn(g.CurrentBattle.Territory) == 0)
            {
                var winner = g.GetPlayer(g.BattleWinner);
                var coocupyingPlayer = (g.BattleWinner == g.CurrentPinkOrAllyFighter) ? winner.AlliedPlayer : null;

                int nrOfForces = winner.AnyForcesIn(g.CurrentBattle.Territory);
                if (coocupyingPlayer != null) nrOfForces += coocupyingPlayer.AnyForcesIn(g.CurrentBattle.Territory);

                return nrOfForces;
            }
            else
            {
                return 0;
            }
        }

        public static bool MayCallFaceDancer(Game g, Player p)
        {
            if (g.BattleWinner != Faction.None)
            {
                var winnerHero = g.WinnerHero;
                if (winnerHero != null)
                {
                    return p.FaceDancers.Any(t => t.IsFaceDancer(winnerHero) && !p.RevealedDancers.Contains(t));
                }
            }

            return false;
        }

        public static IEnumerable<Location> ValidSourceLocations(Game g, Player p)
        {
            return g.LocationsWithAnyForcesNotInStorm(p);
        }

        public static IEnumerable<int> ValidForcesFromReserves(Player p)
        {
            return Enumerable.Range(0, 1 + p.ForcesInReserve);
        }

        public static IEnumerable<Location> ValidTargetLocations(Game g)
        {
            return g.CurrentBattle.Territory.Locations.Where(l => (g.Applicable(Rule.BattlesUnderStorm) || l.Sector != g.SectorInStorm));
        }
    }
}
