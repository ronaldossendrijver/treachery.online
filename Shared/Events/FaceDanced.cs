/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FaceDanced : PlacementEvent
    {
        #region Construction

        public FaceDanced(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public FaceDanced()
        {
        }

        #endregion Construction

        #region Properties

        public string _targetForceLocations = "";

        [JsonIgnore]
        public Dictionary<Location, Battalion> TargetForceLocations
        {
            get => ParseForceLocations(Game, Player.Faction, _targetForceLocations);
            set => _targetForceLocations = ForceLocationsString(Game, value);
        }

        public int ForcesFromReserve { get; set; }

        //Needed for game version <= 150
        public bool FaceDancerCalled { get; set; }

        #endregion Properties

        #region Validation

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
                if (!g.CurrentBattle.Territory.IsHomeworld || p.IsNative(g.CurrentBattle.Territory))
                {
                    var winnerHero = g.WinnerHero;
                    if (winnerHero != null && !g.IsOccupiedByFactionOrTheirAlly(World.Purple, g.BattleWinner))
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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Game.Version > 150 || Game.Version <= 150 && FaceDancerCalled)
            {
                var facedancer = Player.FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));

                if (Game.Version <= 150)
                {
                    Log(Initiator, " reveal ", facedancer, " as one of their Face Dancers!");
                    Game.Stone(Milestone.FaceDanced);
                }

                if (facedancer is Leader && Game.IsAlive(facedancer))
                {
                    Game.KillHero(facedancer);
                }

                foreach (var p in Game.Players)
                {
                    if (!p.KnownNonTraitors.Contains(facedancer)) p.KnownNonTraitors.Add(facedancer);
                }

                if (!Player.RevealedDancers.Contains(facedancer))
                {
                    Player.RevealedDancers.Add(facedancer);
                }

                if (!Player.UnrevealedFaceDancers.Any())
                {
                    ReplaceFacedancers();
                }

                ReplaceForces();
                Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
            }
            else
            {
                Log(Initiator, " don't reveal a Face Dancer");
            }

            Game.FinishBattle();
        }

        private void ReplaceForces()
        {
            var winner = GetPlayer(Game.BattleWinner);
            int nrOfRemovedForces = winner.AnyForcesIn(Game.CurrentBattle.Territory);

            var coocupyingPlayer = (Game.BattleWinner == Game.CurrentPinkOrAllyFighter) ? winner.AlliedPlayer : null;
            if (coocupyingPlayer != null) nrOfRemovedForces += coocupyingPlayer.AnyForcesIn(Game.CurrentBattle.Territory);

            if (nrOfRemovedForces > 0)
            {
                winner.ForcesToReserves(Game.CurrentBattle.Territory);
                coocupyingPlayer?.ForcesToReserves(Game.CurrentBattle.Territory);

                Player.RemoveForcesFromReserves(ForcesFromReserve);
                foreach (var fl in ForceLocations)
                {
                    var location = fl.Key;
                    Player.RemoveForces(location, fl.Value.AmountOfForces);
                    Player.RemoveSpecialForces(location, fl.Value.AmountOfSpecialForces);
                }

                foreach (var fl in TargetForceLocations)
                {
                    var location = fl.Key;
                    Player.AddForces(location, fl.Value.AmountOfForces, false);
                    Player.AddSpecialForces(location, fl.Value.AmountOfSpecialForces, false);
                }

                Log(nrOfRemovedForces, " ", winner.Faction,
                    MessagePart.ExpressIf(coocupyingPlayer != null, coocupyingPlayer?.Faction),
                    " forces go back to reserves and are replaced by ",
                    TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces),
                    Player.Force,
                    " (", ForcesFromReserve, " from reserves", DetermineSourceLocations(), ")");
            }
        }

        private void ReplaceFacedancers()
        {
            Game.TraitorDeck.Items.AddRange(Player.FaceDancers);
            Player.FaceDancers.Clear();
            Player.RevealedDancers.Clear();
            Game.TraitorDeck.Shuffle();
            Game.Stone(Milestone.Shuffled);
            for (int i = 0; i < 3; i++)
            {
                Player.FaceDancers.Add(Game.TraitorDeck.Draw());
            }
            Log(Initiator, " draw 3 new Face Dancers.");
        }

        private MessagePart DetermineSourceLocations()
        {
            return MessagePart.ExpressIf(ForceLocations.Count > 0, ForceLocations.Where(fl => fl.Value.TotalAmountOfForces > 0).Select(fl => MessagePart.Express(", ", fl.Value.AmountOfForces, " from ", fl.Key)));
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(!FaceDancerCalled, " don't"), " reveal a Face Dancer!");
        }

        #endregion Execution
    }
}
