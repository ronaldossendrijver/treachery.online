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
            return new Message(Initiator, "{0} use the {1} stronghold advantage for this battle.", Initiator, Advantage);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return g.ChosenHMSAdvantage == StrongholdAdvantage.None && g.CurrentBattle?.Territory == g.Map.HiddenMobileStronghold.Territory && g.OwnsStronghold(p.Faction, g.Map.HiddenMobileStronghold) && ValidAdvantages(g, p).Any();
        }

        public static IEnumerable<StrongholdAdvantage> ValidAdvantages(Game g, Player p)
        {
            var result = new List<StrongholdAdvantage>();
            if (g.OwnerOf(g.Map.Arrakeen) == p) result.Add(StrongholdAdvantage.FreeResourcesForBattles);
            if (g.OwnerOf(g.Map.Carthag) == p) result.Add(StrongholdAdvantage.CountDefensesAsAntidote);
            if (g.OwnerOf(g.Map.SietchTabr) == p) result.Add(StrongholdAdvantage.CollectResourcesForDial);
            if (g.OwnerOf(g.Map.TueksSietch) == p) result.Add(StrongholdAdvantage.CollectResourcesForUseless);
            if (g.OwnerOf(g.Map.HabbanyaSietch) == p) result.Add(StrongholdAdvantage.WinTies);
            return result;
        }

        public static bool MayBeUsed(Game game, Player player)
        {
            return game.SkilledAs(player, LeaderSkill.Thinker) && game.CurrentBattle != null && game.CurrentThought == null && game.CurrentBattle.IsAggressorOrDefender(player);
        }
    }
}
