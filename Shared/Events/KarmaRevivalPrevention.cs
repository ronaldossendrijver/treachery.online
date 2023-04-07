/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaRevivalPrevention : GameEvent
    {
        #region Construction

        public KarmaRevivalPrevention(Game game) : base(game)
        {
        }

        public KarmaRevivalPrevention()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!GetValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

            return null;
        }

        public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
        {
            return g.PlayersOtherThan(p);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentKarmaRevivalPrevention = this;
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Player.SpecialKarmaPowerUsed = true;
            Log();
            Game.Stone(Milestone.Karma);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent revival by ", Target);
        }

        #endregion Execution
    }
}
