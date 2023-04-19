/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaShipmentPrevention : GameEvent
    {
        #region Construction

        public KarmaShipmentPrevention(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaShipmentPrevention()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Game.Version >= 138 && !GetValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

            return null;
        }

        public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
        {
            return g.PlayersOtherThan(p).Where(f => f != Faction.Yellow);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentKarmaShipmentPrevention = this;
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Player.SpecialKarmaPowerUsed = true;
            Log();
            Game.Stone(Milestone.Karma);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent shipment by ", Target);
        }

        #endregion Execution
    }
}
