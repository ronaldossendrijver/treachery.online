/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class BrownFreeRevivalPrevention : GameEvent
    {
        public Faction Target { get; set; }

        public BrownFreeRevivalPrevention(Game game) : base(game)
        {
        }

        public BrownFreeRevivalPrevention()
        {
        }

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use a ", TreacheryCardType.Useless, " card to prevent ", Target, " from using free revival this phase");
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null;
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_LALALA);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
