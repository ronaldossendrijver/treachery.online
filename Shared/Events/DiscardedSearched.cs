/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class DiscardedSearched : GameEvent
    {
        #region Construction

        public DiscardedSearched(Game game) : base(game)
        {
        }

        public DiscardedSearched()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g)
        {
            return g.TreacheryDiscardPile.Items;
        }

        public static bool CanBePlayed(Player p)
        {
            return p.Has(TreacheryCardType.SearchDiscarded);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            foreach (var p in Game.Players)
            {
                Game.UnregisterKnown(p, Game.TreacheryDiscardPile.Items);
            }

            Game.TreacheryDiscardPile.Items.Remove(Card);
            Player.TreacheryCards.Add(Card);
            Game.TreacheryDiscardPile.Shuffle();
            Game.Discard(Player, TreacheryCardType.SearchDiscarded);
            Game.Enter(Game.PhaseBeforeSearchingDiscarded);
            Game.Stone(Milestone.Shuffled);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " take one card and the Treachery Discard Pile is then shuffled");
        }

        #endregion Execution
    }
}
