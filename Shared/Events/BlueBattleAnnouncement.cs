/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BlueBattleAnnouncement : GameEvent
    {
        public int _territoryId;

        public BlueBattleAnnouncement(Game game) : base(game)
        {
        }

        public BlueBattleAnnouncement()
        {
        }

        [JsonIgnore]
        public Territory Territory
        {
            get { return Game.Map.TerritoryLookup.Find(_territoryId); }
            set { _territoryId = Game.Map.TerritoryLookup.GetId(value); }
        }

        public override string Validate()
        {
            if (Initiator != Faction.Blue) return "Your faction can't announce battle";
            var p = Player;
            if (p.SpecialForcesIn(Territory) == 0) return "You don't have advisors there";
            var ally = Game.GetPlayer(p.Ally);
            if (ally != null && ally.AnyForcesIn(Territory) > 0) return "Can't announce battle due to ally presence";

            return "";
        }

        public static IEnumerable<Territory> ValidTerritories(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            if (g.Version >= 93)
            {
                if (ally == null)
                {
                    return g.Map.Territories.Where(t =>
                    p.SpecialForcesIn(t) > 0 &&
                    (!t.IsStronghold || g.NrOfOccupantsExcludingPlayer(t, p) <= 1) &&
                    !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
                }
                else
                {
                    return g.Map.Territories.Where(t =>
                    ally.AnyForcesIn(t) == 0 &&
                    p.SpecialForcesIn(t) > 0 &&
                    (!t.IsStronghold || g.NrOfOccupantsExcludingPlayer(t, p) <= 1) &&
                    !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
                }
            }
            else if (g.Version >= 89)
            {
                if (ally == null)
                {
                    return g.Map.Territories.Where(t =>
                    p.SpecialForcesIn(t) > 0 &&
                    !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
                }
                else
                {
                    return g.Map.Territories.Where(t =>
                    ally.AnyForcesIn(t) == 0 &&
                    p.SpecialForcesIn(t) > 0 &&
                    !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
                }
            }
            else
            {
                if (ally == null)
                {
                    return g.Map.Territories.Where(t => g.NrOfOccupantsExcludingPlayer(t, p) <= 1 && !IsStrongholdInStorm(g, t) && p.SpecialForcesIn(t) > 0);
                }
                else
                {
                    return g.Map.Territories.Where(t => g.NrOfOccupantsExcludingPlayer(t, p) <= 1 && !IsStrongholdInStorm(g, t) && p.SpecialForcesIn(t) > 0 && ally.AnyForcesIn(t) == 0);
                }
            }
        }

        public static bool IsStrongholdInStorm(Game g, Territory t)
        {
            return t.IsStronghold && t.Locations.All(l => l.Sector == g.SectorInStorm);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} flip to fighters in {1}.", Initiator, Territory);
        }
    }
}
