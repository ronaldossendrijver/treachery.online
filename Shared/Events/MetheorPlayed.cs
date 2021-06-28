/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Initiator, "{0} use {1} to destroy the {2}! All forces there are destroyed.", Initiator, TreacheryCardType.Metheor, Game.Map.ShieldWall);
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
                    if (g.Map.FindNeighbours(shieldwallLocation, 1, false, p.Faction, g.SectorInStorm, null).Any(l => p.Occupies(l)))
                    {
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
