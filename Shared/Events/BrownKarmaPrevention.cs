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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use a {1} card to prevent {2} from using {3} this phase.", Initiator, TreacheryCardType.Useless, Target, TreacheryCardType.Karma);
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null;
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_KULLWAHAD);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
