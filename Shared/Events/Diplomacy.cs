/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Diplomacy : GameEvent
    {
        public int _cardId;

        public Diplomacy(Game game) : base(game)
        {
        }

        public Diplomacy()
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use Diplomacy to turn {1} into a copy of the opponent's defense.", Initiator, Card);
        }

        public Message GetDynamicMessage()
        {
            return new Message(Initiator, "{0} use Diplomacy to turn {1} into a {2}.", Initiator, Card, Game.CurrentBattle?.PlanOfOpponent(Player)?.Defense);
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            var result = new List<TreacheryCard>();
            var plan = g.CurrentBattle.PlanOf(p);
            if (plan.Weapon != null && plan.Weapon.IsUseless) result.Add(plan.Weapon);
            if (plan.Defense != null && plan.Defense.IsUseless) result.Add(plan.Defense);
            return result;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            if (g.SkilledAs(p, LeaderSkill.Diplomat) && g.CurrentDiplomacy == null)
            {
                var plan = g.CurrentBattle.PlanOf(p);
                return 
                    plan != null && 
                    (plan.Defense == null || !plan.Defense.IsDefense) && 
                    g.CurrentBattle.PlanOfOpponent(p).Defense != null &&
                    g.CurrentBattle.PlanOfOpponent(p).Defense.IsDefense &&
                    ValidCards(g, p).Any();
            }

            return false;
        }
    }
}
