/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class HMSAdvantageChosen : GameEvent
    {
        public StrongholdAdvantage Advantage;

        public HMSAdvantageChosen(Game game) : base(game)
        {
        }

        public HMSAdvantageChosen()
        {
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
            return new Message(Initiator, "{0} choose {1} for the {2}.", Initiator, Advantage, Game.Map.HiddenMobileStronghold);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return g.CurrentBattle?.Territory == g.Map.HiddenMobileStronghold.Territory && g.HasStronghold(p.Faction, g.Map.HiddenMobileStronghold) && ValidAdvantages(g, p).Any();
        }

        public static IEnumerable<StrongholdAdvantage> ValidAdvantages(Game g, Player p)
        {
            return Enumerations.GetValuesExceptDefault(typeof(StrongholdAdvantage), StrongholdAdvantage.None).Where(a => g.HasStrongholdAdvantage(p.Faction, a));
        }

        public static bool MayBeUsed(Game game, Player player)
        {
            return game.SkilledAs(player, LeaderSkill.Thinker) && game.CurrentBattle != null && game.CurrentThought == null && game.CurrentBattle.IsAggressorOrDefender(player);
        }
    }
}
