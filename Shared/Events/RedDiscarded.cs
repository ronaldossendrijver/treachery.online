/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class RedDiscarded : GameEvent
    {
        #region Construction

        public RedDiscarded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public RedDiscarded()
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
            Log();
            Player.Resources -= 2;
            Game.Discard(Player, Card);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pay ", Payment.Of(2), " to discard ", Card);
        }

        #endregion Execution
    }
}
