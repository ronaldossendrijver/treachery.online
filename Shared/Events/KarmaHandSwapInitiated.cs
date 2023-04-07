/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class KarmaHandSwapInitiated : GameEvent
    {
        #region Construction

        public KarmaHandSwapInitiated(Game game) : base(game)
        {
        }

        public KarmaHandSwapInitiated()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

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
            Game.KarmaHandSwapPausedPhase = Game.CurrentPhase;
            Game.Enter(Phase.PerformingKarmaHandSwap);

            var victim = GetPlayer(Target);

            Player.SpecialKarmaPowerUsed = true;
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());

            Game.KarmaHandSwapNumberOfCards = victim.TreacheryCards.Count;
            Game.KarmaHandSwapTarget = Target;

            var cardsToDrawFrom = new Deck<TreacheryCard>(victim.TreacheryCards, Game.Random);
            Game.Stone(Milestone.Shuffled);
            cardsToDrawFrom.Shuffle();
            for (int i = 0; i < Game.KarmaHandSwapNumberOfCards; i++)
            {
                var card = cardsToDrawFrom.Draw();
                Game.RegisterKnown(Player, card);
                victim.TreacheryCards.Remove(card);
                Player.TreacheryCards.Add(card);
            }

            Log();
            Game.Stone(Milestone.Karma);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " swap cards with ", Target);
        }

        #endregion Execution
    }
}
