/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownMovePrevention : GameEvent
    {
        public int _territoryId;

        public BrownMovePrevention(Game game) : base(game)
        {
        }

        public BrownMovePrevention()
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
            if (!ValidTerritories(Game, Player).Contains(Territory)) return "Invalid territory.";
            return "";
        }

        public static IEnumerable<Territory> ValidTerritories(Game g, Player p)
        {
            return p.TerritoriesWithForces;
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null;
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_BALISET);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use a {1} card to prevent forces moving into {2}.", Initiator, TreacheryCardType.Useless, Territory);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
