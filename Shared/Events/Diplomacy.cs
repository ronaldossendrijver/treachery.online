/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Diplomacy : GameEvent
    {
        #region Construction

        public Diplomacy(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Diplomacy()
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
            return null;
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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log(Initiator, " use Diplomacy to turn ", Card, " into a ", Game.CurrentBattle?.PlanOfOpponent(Player)?.Defense);
            Game.CurrentDiplomacy = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use Diplomacy to turn ", Card, " into a copy of the opponent's defense");
        }

        #endregion Execution
    }
}
