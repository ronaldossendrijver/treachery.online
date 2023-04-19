/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TraitorDiscarded : GameEvent
    {
        #region Construction

        public TraitorDiscarded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public TraitorDiscarded()
        {
        }

        #endregion Construction

        #region Properties

        public int _traitorId;

        [JsonIgnore]
        public IHero Traitor
        {
            get => LeaderManager.HeroLookup.Find(_traitorId);
            set => _traitorId = LeaderManager.HeroLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTraitors(Player).Contains(Traitor)) return Message.Express("Invalid traitor");

            return null;
        }

        public static IEnumerable<IHero> ValidTraitors(Player p) => p.Traitors;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.TraitorDeck.Items.Add(Traitor);
            Player.Traitors.Remove(Traitor);
            Game.NumberOfTraitorsToDiscard--;

            if (Game.NumberOfTraitorsToDiscard == 0)
            {
                Game.Enter(Game.PhaseBeforeDiscardingTraitor);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " shuffle a traitor into the Traitor deck");
        }

        #endregion Execution
    }
}
