/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class MetheorPlayed : GameEvent
    {
        #region Construction

        public MetheorPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public MetheorPlayed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (!MayPlayMetheor(Game, Player)) return Message.Express("You cannot use ", TreacheryCardType.Metheor);

            return null;
        }

        public static bool MayPlayMetheor(Game g, Player p)
        {
            return g.CurrentTurn > 1 && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Metheor) && HasForcesAtOrNearShieldWall(g, p);
        }

        public static bool HasForcesAtOrNearShieldWall(Game g, Player p)
        {
            if (p.Occupies(g.Map.ShieldWall)) return true;

            foreach (var shieldwallLocation in g.Map.ShieldWall.Locations)
            {
                if (shieldwallLocation.Neighbours.Any(l => p.Occupies(l)))
                {
                    //checks locations that are immediately adjacent to the shield wall
                    return true;
                }

                if (g.Map.FindNeighbours(shieldwallLocation, 1, false, p.Faction, g, false).Any(l => p.Occupies(l)))
                {
                    //checks locations that are in a territory adjacent to the shield wall (but may be further away and separated by storm)
                    return true;
                }
            }

            return false;
        }

        #endregion Construction

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var card = Player.Card(TreacheryCardType.Metheor);

            Game.Stone(Milestone.MetheorUsed);
            Game.ShieldWallDestroyed = true;
            Player.TreacheryCards.Remove(card);
            Game.RemovedTreacheryCards.Add(card);
            Log();

            foreach (var p in Game.Players)
            {
                foreach (var location in Game.Map.ShieldWall.Locations.Where(l => p.AnyForcesIn(l) > 0))
                {
                    Game.RevealCurrentNoField(p, location);
                    p.KillAllForces(location, false);
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Metheor, " to destroy the ", Game.Map.ShieldWall, "!");
        }

        #endregion Execution
    }
}
