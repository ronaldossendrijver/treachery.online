/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaHandSwap : GameEvent
    {
        #region Construction

        public KarmaHandSwap(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaHandSwap()
        {
        }

        #endregion Construction

        #region Properties

        public string _cardIds;

        [JsonIgnore]
        public IEnumerable<TreacheryCard> ReturnedCards
        {
            get => IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
            set => _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (ReturnedCards.Count() != Game.KarmaHandSwapNumberOfCards) return Message.Express("Select ", Game.KarmaHandSwapNumberOfCards, " cards to return");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var victim = GetPlayer(Game.KarmaHandSwapTarget);

            foreach (var p in Game.Players.Where(p => p != Player && p != victim))
            {
                Game.UnregisterKnown(p, Player.TreacheryCards);
                Game.UnregisterKnown(p, victim.TreacheryCards);
            }

            foreach (var returned in ReturnedCards)
            {
                victim.TreacheryCards.Add(returned);
                Player.TreacheryCards.Remove(returned);
            }

            foreach (var returned in ReturnedCards)
            {
                Game.RegisterKnown(Player, returned);
            }

            Log();
            Game.Enter(Game.KarmaHandSwapPausedPhase);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " return ", ReturnedCards.Count(), " cards");
        }

        #endregion Execution
    }
}
