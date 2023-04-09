/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class MulliganPerformed : PassableGameEvent
    {
        #region Construction

        public MulliganPerformed(Game game) : base(game)
        {
        }

        public MulliganPerformed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (!Passed && Game.Version >= 150 && !MayMulligan(Player)) return Message.Express("You can't take a mulligan");

            return null;
        }

        public static bool MayMulligan(Player p)
        {
            return p.Traitors.Where(l => l.Is(p.Faction)).Count() >= 2;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                Game.TraitorDeck.Items.AddRange(Player.Traitors);
                Player.Traitors.Clear();
                Game.TraitorDeck.Shuffle();
                Game.Stone(Milestone.Shuffled);
                Game.DealBlackTraitorCards();
                Game.Enter(Phase.BlackMulligan);
            }
            else
            {
                DealNonBlackTraitorCards();
                Game.EnterSelectTraitors();
            }

            Log();
        }

        private void DealNonBlackTraitorCards()
        {
            for (int i = 1; i <= 4; i++)
            {
                foreach (var p in Game.Players.Where(p => p.Faction != Faction.Black && p.Faction != Faction.Purple))
                {
                    p.Traitors.Add(Game.TraitorDeck.Draw());
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, Passed ? " pass" : " take", " a mulligan");
        }

        #endregion Execution
    }
}
