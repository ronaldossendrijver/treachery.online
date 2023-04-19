/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class Discarded : GameEvent
    {
        #region Construction

        public Discarded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Discarded()
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
            if (Card == null) return Message.Express("Choose a card to discard");
            if (!Player.Has(Card)) return Message.Express("Invalid card");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.FactionsThatMustDiscard.Remove(Initiator);
            Game.Discard(Player, Card);

            if (!Game.FactionsThatMustDiscard.Any())
            {
                Game.Enter(Game.PhaseBeforeDiscarding);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " discard ", Card);
        }

        #endregion Execution
    }
}
