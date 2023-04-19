/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DiscardedTaken : GameEvent
    {
        #region Construction

        public DiscardedTaken(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public DiscardedTaken()
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
            if (!ValidCards(Game, Player).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return g.RecentlyDiscarded.Where(kvp => kvp.Value != p.Faction && g.TreacheryDiscardPile.Items.Contains(kvp.Key)).Select(kvp => kvp.Key);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return !g.CurrentPhaseIsUnInterruptable && p.Has(TreacheryCardType.TakeDiscarded) && ValidCards(g, p).Any();
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.RecentlyDiscarded.Remove(Card);
            Game.TreacheryDiscardPile.Items.Remove(Card);
            Player.TreacheryCards.Add(Card);
            Game.Discard(Player, TreacheryCardType.TakeDiscarded);
            Game.Stone(Milestone.CardWonSwapped);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.TakeDiscarded, " to acquire the discarded ", Card);
        }

        #endregion Execution
    }
}
