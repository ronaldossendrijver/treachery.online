/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class BrownKarmaPrevention : GameEvent
    {
        #region Construction

        public BrownKarmaPrevention(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BrownKarmaPrevention()
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

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_KULLWAHAD);
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

            Game.CurrentKarmaPrevention = this;
            Game.Stone(Milestone.SpecialUselessPlayed);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " prevent ", Target, " from using ", TreacheryCardType.Karma, " this phase");
        }

        #endregion Execution
    }
}
