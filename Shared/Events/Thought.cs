/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Thought : GameEvent
    {
        public int _cardId;

        public Thought(Game game) : base(game)
        {
        }

        public Thought()
        {
        }

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Get(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.GetId(value);
            }
        }


        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express("By ", LeaderSkill.Thinker, ", ", Initiator, " ask a question");
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return TreacheryCardManager.GetCardsInPlay(g).Where(c => c.IsWeapon);
        }

        public static bool MayBeUsed(Game game, Player player)
        {
            return game.SkilledAs(player, LeaderSkill.Thinker) && game.CurrentBattle != null && game.CurrentThought == null && game.CurrentBattle.IsAggressorOrDefender(player);
        }
    }
}
