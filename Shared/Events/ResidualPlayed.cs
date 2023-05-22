/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class ResidualPlayed : GameEvent
    {
        #region Construction

        public ResidualPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ResidualPlayed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool MayPlay(Game g, Player p)
        {
            return g.CurrentBattle.IsAggressorOrDefender(p) && p.TreacheryCards.Any(c => c.Type == TreacheryCardType.Residual);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, TreacheryCardType.Residual);

            var opponent = Game.CurrentBattle.OpponentOf(Initiator);
            var leadersToKill = new Deck<IHero>(opponent.Leaders.Where(l => Game.IsAlive(l) && Game.CanJoinCurrentBattle(l)), Game.Random);
            leadersToKill.Shuffle();

            if (!leadersToKill.IsEmpty)
            {
                var toKill = leadersToKill.Draw();
                var opponentPlan = Game.CurrentBattle.PlanOf(opponent);
                if (opponentPlan != null && opponentPlan.Hero == toKill)
                {
                    Game.RevokePlan(opponentPlan);
                }

                Game.KillHero(toKill);
                Log(TreacheryCardType.Residual, " kills ", toKill);
            }
            else
            {
                Log(opponent.Faction, " have no available leaders to kill");
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Residual, " to kill a random opponent leader");
        }

        #endregion Execution
    }
}
