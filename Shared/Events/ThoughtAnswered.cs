/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class ThoughtAnswered : GameEvent
    {
        #region Construction

        public ThoughtAnswered(Game game) : base(game)
        {
        }

        public ThoughtAnswered()
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
            if (!ValidCards(Game, Player).Any()) return null;

            if (!ValidCards(Game, Player).Contains(Card)) return Message.Express("Select a valid card to show");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            if (p.TreacheryCards.Contains(g.CurrentThought.Card))
            {
                return new TreacheryCard[] { g.CurrentThought.Card };
            }
            else
            {
                return p.TreacheryCards;
            }
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Card == null)
            {
                Log(Initiator, " don't own any cards");
            }
            else
            {
                LogTo(Game.CurrentThought.Initiator, "In response, ", Initiator, " show you: ", Card);
                Game.RegisterKnown(Game.CurrentThought.Initiator, Card);
            }

            Game.Enter(Phase.BattlePhase);
        }

        public override Message GetMessage()
        {
            if (Card == null)
            {
                return Message.Express(Initiator, " don't have a card to show");
            }
            else
            {
                return Message.Express(Initiator, " show one of their cards");
            }

        }

        #endregion Execution
    }
}
