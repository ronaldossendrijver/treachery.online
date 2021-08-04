/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return new Message(Initiator, "{0} use {1} to kill a random opponent leader.", Initiator, TreacheryCardType.Residual);
        }

        public static bool MayPlay(Game g, Player p)
        {
            var opponentBattlePlan = g.CurrentBattle?.PlanOfOpponent(p);
            return p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Residual) && opponentBattlePlan == null;
        }

    }
}
