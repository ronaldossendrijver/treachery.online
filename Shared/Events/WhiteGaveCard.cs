/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class WhiteGaveCard : GameEvent
    {
        #region Construction

        public WhiteGaveCard(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public WhiteGaveCard()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId = -1;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Lookup.Find(_cardId);
            set => _cardId = TreacheryCardManager.Lookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Player p)
        {
            return p.TreacheryCards.Where(c => c.Rules.Contains(Rule.WhiteTreacheryCards));
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var target = Player.AlliedPlayer;

            Player.TreacheryCards.Remove(Card);
            Game.RegisterKnown(Player, Card);
            target.TreacheryCards.Add(Card);

            foreach (var p in Game.Players.Where(p => p != Player && p != target))
            {
                Game.UnregisterKnown(p, Player.TreacheryCards);
                Game.UnregisterKnown(p, target.TreacheryCards);
            }

            Log();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " give a card to their ally");
        }

        #endregion Execution
    }
}
