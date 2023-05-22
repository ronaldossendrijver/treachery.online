/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class CardsDetermined : GameEvent
    {
        #region Construction

        public CardsDetermined(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public CardsDetermined()
        {
        }

        #endregion Construction

        #region Properties

        public string _treacheryCardIds;

        [JsonIgnore]
        public IEnumerable<TreacheryCard> TreacheryCards
        {
            get => IdStringToObjects(_treacheryCardIds, TreacheryCardManager.Lookup);
            set => _treacheryCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
        }

        public string _whiteCardIds;

        [JsonIgnore]
        public IEnumerable<TreacheryCard> WhiteCards
        {
            get => IdStringToObjects(_whiteCardIds, TreacheryCardManager.Lookup);
            set => _whiteCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (TreacheryCards.Count() + WhiteCards.Count() < Game.Players.Sum(p => p.MaximumNumberOfCards) + 1) return Message.Express("Not enough cards selected");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.TreacheryDeck = new Deck<TreacheryCard>(TreacheryCards, Game.Random);
            Game.TreacheryDeck.Shuffle();
            Game.Stone(Milestone.Shuffled);
            Game.WhiteCache = new List<TreacheryCard>(WhiteCards);
            Log(GetVerboseMessage());
            Game.Enter(Game.Version < 134, Game.EnterPhaseTradingFactions, Game.EnterSetupPhase);
        }

        public override Message GetMessage()
        {
            return Message.Express("Card decks were customized.");
        }

        private Message GetVerboseMessage()
        {
            if (WhiteCards.Any())
            {
                return Message.Express("Treachery Cards: ", TreacheryCards, ". ", Faction.White, " Cards: ", WhiteCards);
            }
            else
            {
                return Message.Express("Treachery Cards: ", TreacheryCards);
            }

        }

        #endregion Execution
    }
}
