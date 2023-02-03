/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class BrownKarmaPrevention : GameEvent
    {
        public Faction Target { get; set; }

        public BrownKarmaPrevention(Game game) : base(game)
        {
        }

        public BrownKarmaPrevention()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " prevent ", Target, " from using ", TreacheryCardType.Karma, " this phase");
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_KULLWAHAD);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
