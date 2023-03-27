/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownDiscarded : GameEvent
    {
        #region Construction

        public BrownDiscarded(Game game) : base(game)
        {
        }

        public BrownDiscarded()
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
            if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards.Where(c =>
                 c.Type == TreacheryCardType.Useless && !p.HasHighThreshold() ||
                (c.Type != TreacheryCardType.Projectile && c.Type != TreacheryCardType.Poison) && p.TreacheryCards.Count(toCount => toCount.Type == c.Type) > 1);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Card);
            Log();

            if (Card.Type == TreacheryCardType.Useless)
            {
                Player.Resources += 2;
            }
            else
            {
                Player.Resources += 3;
            }

            Game.Stone(Milestone.ResourcesReceived);
        }

        public override Message GetMessage()
        {
            if (Card.Type == TreacheryCardType.Useless)
            {
                return Message.Express(Initiator, " get ", Payment.Of(2), " by discarding a ", TreacheryCardType.Useless, " card");
            }
            else
            {
                return Message.Express(Initiator, " get ", Payment.Of(3), " by discarding a duplicate ", Card);
            }
        }

        #endregion Execution
    }
}
