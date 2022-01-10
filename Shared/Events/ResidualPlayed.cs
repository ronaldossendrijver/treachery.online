/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class ResidualPlayed : GameEvent
    {
        public ResidualPlayed(Game game) : base(game)
        {
        }

        public ResidualPlayed()
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
            return Message.Express(Initiator, " use ", TreacheryCardType.Residual, " to kill a random opponent leader");
        }

        public static bool MayPlay(Game g, Player p)
        {
            return g.CurrentBattle.IsAggressorOrDefender(p) && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Residual);
        }

    }
}
