/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class BrownExtraMove : GameEvent
    {
        public BrownExtraMove(Game game) : base(game)
        {
        }

        public BrownExtraMove()
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
            return new Message(Initiator, "{0} use a {1} card for extra movement.", Initiator, TreacheryCardType.Useless);
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null;
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_KULON);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
