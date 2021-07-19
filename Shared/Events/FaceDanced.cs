/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FaceDanced : PlacementEvent
    {
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

        public override string Validate()
        {
            if (!FaceDancerCalled) return "";

            var p = Player;
            if (!MayCallFaceDancer(Game, p)) return "You can't reveal a Face Dancer";

            int amountOfForces = ForcesFromReserve + ForceLocations.Values.Sum(b => b.TotalAmountOfForces);
            int maximumForces = MaximumNumberOfForces(Game, p);
            if (amountOfForces > maximumForces) return string.Format("Place {0} or less forces.", maximumForces);

            int amountOfTargetForces = TargetForceLocations.Values.Sum(b => b.TotalAmountOfForces);
            if (amountOfForces != amountOfTargetForces) return string.Format("The amount of forces you selected from the planet and from reserves ({0}) should equal the amount you wish to put in {1} ({2}).", amountOfForces, Game.CurrentBattle.Territory, amountOfTargetForces);

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (FaceDancerCalled)
            {
                return new Message(Initiator, "{0} reveal a Face Dancer!", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} don't reveal a Face Dancer.", Initiator);
            }
        }

        public static int MaximumNumberOfForces(Game g, Player p)
        {
            if (g.Version < 80)
            {
                return g.Players.Select(p => p.AnyForcesIn(g.CurrentBattle.Territory)).DefaultIfEmpty(0).Sum();
            }
            else if (g.Version < 83)
            {
                var winner = g.GetPlayer(g.BattleWinner);
                return winner.AnyForcesIn(g.CurrentBattle.Territory);
            }
            else
            {
                var ally = p.AlliedPlayer;
                if (ally == null || g.BattleWinner == p.Ally || ally.AnyForcesIn(g.CurrentBattle.Territory) == 0)
                {
                    var winner = g.GetPlayer(g.BattleWinner);
                    return winner.AnyForcesIn(g.CurrentBattle.Territory);
                }
                else
                {
                    return 0;
                }
            }
        }

        public static bool MayCallFaceDancer(Game g, Player p)
        {
            if (g.BattleWinner != Faction.None)
            {
                var winnerHero = g.WinnerHero;
                if (winnerHero != null)
                {
                    if (g.Version <= 98)
                    {
                        return p.FaceDancers.Any(t => t.IsFaceDancer(winnerHero));
                    }
                    else
                    {
                        return p.FaceDancers.Any(t => t.IsFaceDancer(winnerHero) && !p.RevealedDancers.Contains(t));
                    }
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
