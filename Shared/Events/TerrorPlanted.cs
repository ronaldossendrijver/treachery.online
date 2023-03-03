/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TerrorPlanted : GameEvent
    {
        public TerrorPlanted(Game game) : base(game)
        {
        }

        public TerrorPlanted()
        {
        }

        public bool Passed;

        public TerrorType Type;

        public int _territoryId;

        [JsonIgnore]
        public Territory Stronghold
        {
            get { return Game.Map.TerritoryLookup.Find(_territoryId); }
            set { _territoryId = Game.Map.TerritoryLookup.GetId(value); }
        }

        public override Message Validate()
        {
            if (Stronghold == null)
            {
                if (!Game.TerrorOnPlanet.ContainsKey(Type)) return Message.Express("You can't remove a token that is not on the board");
            }
            else
            {
                if (!ValidStrongholds(Game, Player).Contains(Stronghold)) return Message.Express("Invalid Stronghold");
                if (!ValidTerrorTypes(Game, false).Contains(Type)) return Message.Express("Token not available");
            }

            return null;
        }

        public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return g.Map.Territories(false).Where(t =>
                t.IsStronghold &&
                t != g.Map.HiddenMobileStronghold.Territory &&
                (!g.TerrorIn(t).Any() || MayPlaceAtExistingToken(p)));
        }

        public static bool MayRemoveTokens(Game g, Player p) => p.HasHighThreshold(World.Cyan) && g.TerrorOnPlanet.Any();

        public static bool MayPlaceAtExistingToken(Player p) => p.HasHighThreshold(World.Cyan) || p.Nexus == Faction.Cyan && NexusPlayed.CanUseCunning(p);

        public static IEnumerable<TerrorType> ValidTerrorTypes(Game g, bool toRemove)
        {
            if (toRemove)
            {
                return g.TerrorOnPlanet.Keys;
            }
            else
            {
                return g.UnplacedTerrorTokens.Union(g.TerrorOnPlanet.Keys);
            }
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static bool IsApplicable(Game g, Player p) =>
            g.CurrentPhase == Phase.Contemplate &&
            !g.CyanHasPlantedTerror &&
            !g.Prevented(FactionAdvantage.CyanPlantingTerror) &&
            ValidTerrorTypes(g, false).Any() &&
            ValidStrongholds(g, p).Any();

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't plant terror");
            }
            else if (Stronghold != null)
            {
                return Message.Express(Initiator, " plant terror in ", Stronghold);
            }
            else
            {
                return Message.Express(Initiator, " remove a terror token to get ", Game.Payment(4));
            }
        }
    }
}
