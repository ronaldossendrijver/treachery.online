/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Thought : GameEvent
    {
        #region Construction

        public Thought(Game game) : base(game)
        {
        }

        public Thought()
        {
        }

        #endregion Construction

        #region Properties

        public int _cardId;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Get(_cardId);
            set => _cardId = TreacheryCardManager.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

            return null;
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g)
        {
            return TreacheryCardManager.GetCardsInPlay(g).Where(c => c.IsWeapon && c.Type != TreacheryCardType.Chemistry);
        }

        public static bool MayBeUsed(Game game, Player player)
        {
            return game.SkilledAs(player, LeaderSkill.Thinker) && game.CurrentBattle != null && game.CurrentThought == null && game.CurrentBattle.IsAggressorOrDefender(player);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentThought = this;
            var opponent = Game.CurrentBattle.OpponentOf(Initiator).Faction;
            Log(Initiator, " use their ", LeaderSkill.Thinker, " skill to ask ", opponent, " if they have a ", Card);
            Game.Stone(Milestone.Prescience);
            Game.Enter(Phase.Thought);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use their ", LeaderSkill.Thinker, " skill to ask a question");
        }

        #endregion Execution

        
    }
}
