/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownMovePrevention : GameEvent
    {
        #region Construction

        public BrownMovePrevention(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BrownMovePrevention()
        {
        }

        #endregion Construction

        #region Properties

        public int _territoryId;

        [JsonIgnore]
        public Territory Territory
        {
            get => Game.Map.TerritoryLookup.Find(_territoryId);
            set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTerritories(Player).Contains(Territory)) return Message.Express("Invalid territory");
            return null;
        }

        public static IEnumerable<Territory> ValidTerritories(Player p)
        {
            return p.TerritoriesWithForces;
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && ValidTerritories(p).Any() && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_BALISET);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            if (NexusPlayed.CanUseCunning(Player))
            {
                Game.DiscardNexusCard(Player);
                Game.Stone(Milestone.NexusPlayed);
                Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
            }
            else
            {
                Game.Discard(CardToUse(Player));
            }

            Game.CurrentBlockedTerritories.Add(Territory);
            Game.Stone(Milestone.SpecialUselessPlayed);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " prevent forces moving into ", Territory);
        }

        #endregion Execution
    }
}
