/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class NexusCardDrawn : PassableGameEvent
    {
        #region Construction

        public NexusCardDrawn(Game game) : base(game)
        {
        }

        public NexusCardDrawn()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (!Passed && !MayDraw(Game, Player)) return Message.Express("You're not allowed to draw a Nexus Card");
            return null;
        }

        public static bool MayDraw(Game g, Player p)
        {
            return !g.FactionsThatDrewNexusCard.Contains(p.Faction) || p.Faction == p.Nexus;
        }

        public static bool Applicable(Game g, Player p)
        {
            return g.FactionsThatMayDrawNexusCard.Contains(p.Faction);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                DealNexusCard();
                Game.FactionsThatDrewNexusCard.Add(Initiator);
            }

            if (Passed)
            {
                Game.FactionsThatMayDrawNexusCard.Remove(Initiator);
            }

            if (!Game.FactionsThatMayDrawNexusCard.Any())
            {
                Game.EndBlowPhase();
            }
        }


        private void DealNexusCard()
        {
            Game.DiscardNexusCard(Player);

            if (Game.NexusCardDeck.IsEmpty)
            {
                Game.NexusCardDeck.Items.AddRange(Game.NexusDiscardPile);
                Game.NexusDiscardPile.Clear();
                Game.NexusCardDeck.Shuffle();
                Game.Stone(Milestone.Shuffled);
                Log("The Nexus Card discard pile was shuffled into a new Nexus Card deck");
            }

            Log(Player.Faction, " draw a Nexus Card");
            Player.Nexus = Game.NexusCardDeck.Draw();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(Passed, " don't"), " draw a Nexus card");
        }

        #endregion Execution

    }
}
