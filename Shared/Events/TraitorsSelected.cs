/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class TraitorsSelected : GameEvent
    {
        #region Construction

        public TraitorsSelected(Game game) : base(game)
        {
        }

        public TraitorsSelected()
        {
        }

        #endregion Construction

        #region Properties

        public int _traitorId;

        [JsonIgnore]
        public IHero SelectedTraitor
        {
            get => LeaderManager.HeroLookup.Find(_traitorId);
            set => _traitorId = LeaderManager.HeroLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!Player.Traitors.Contains(SelectedTraitor)) return Message.Express("Invalid traitor");
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var toRemove = Player.Traitors.Where(l => !l.Equals(SelectedTraitor)).ToList();

            foreach (var l in toRemove)
            {
                Game.TraitorDeck.Items.Add(l);
                Player.Traitors.Remove(l);
                Player.DiscardedTraitors.Add(l);
                Player.KnownNonTraitors.Add(l);
            }

            Game.HasActedOrPassed.Add(Initiator);
            Log();

            if (Game.EveryoneActedOrPassed)
            {
                Game.AssignFaceDancers();
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pick their traitor");
        }

        #endregion Execution
    }
}
