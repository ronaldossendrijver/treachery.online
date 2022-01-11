/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class MetheorPlayed : GameEvent
    {
        public MetheorPlayed(Game game) : base(game)
        {
        }

        public MetheorPlayed()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Metheor, " to destroy the ", Game.Map.ShieldWall, "!");
        }

        public static bool MayPlayMetheor(Game g, Player p)
        {
            return g.CurrentTurn > 1 && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Metheor) && HasForcesAtOrNearShieldWall(g, p);
        }

        public static bool HasForcesAtOrNearShieldWall(Game g, Player p)
        {
            if (g.Version >= 100)
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
            else
            {
                return
                 p.Occupies(g.Map.ShieldWall) ||
                 p.Occupies(g.Map.HoleInTheRock) ||
                 p.Occupies(g.Map.ImperialBasin) ||
                 p.Occupies(g.Map.FalseWallEast) ||
                 p.Occupies(g.Map.TheMinorErg) ||
                 p.Occupies(g.Map.PastyMesa) ||
                 p.Occupies(g.Map.GaraKulon) ||
                 p.Occupies(g.Map.SihayaRidge);
            }
        }
    }
}
