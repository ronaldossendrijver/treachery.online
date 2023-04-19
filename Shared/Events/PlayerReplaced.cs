/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PlayerReplaced : GameEvent
    {
        #region Construction

        public PlayerReplaced(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PlayerReplaced()
        {
        }

        #endregion Construction

        #region Properties

        public Faction ToReplace { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g)
        {
            return g.Players.Select(p => p.Faction);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var player = GetPlayer(ToReplace);
            player.IsBot = !player.IsBot;
            Log(ToReplace, " will now be played by a ", player.IsBot ? "Bot" : "Human");
        }

        public override Message GetMessage()
        {
            return Message.Express(ToReplace, " player has been replaced");
        }

        #endregion Execution
    }
}
