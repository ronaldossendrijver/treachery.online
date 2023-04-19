/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class GreySelectedStartingCard : GameEvent
    {
        #region Construction
        
        public GreySelectedStartingCard(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public GreySelectedStartingCard()
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
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            GetPlayer(Initiator).TreacheryCards.Add(Card);
            Game.StartingTreacheryCards.Items.Remove(Card);
            Log();
            Game.StartingTreacheryCards.Shuffle();
            Game.Stone(Milestone.Shuffled);
            Game.DealRemainingStartingTreacheryCardsToNonGrey();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pick their starting treachery card");
        }

        #endregion Execution
    }
}
