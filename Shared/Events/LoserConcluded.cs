/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class LoserConcluded : GameEvent
    {
        public const int KARMA_NO = 0;
        public const int KARMA_KEEP = 1;
        public const int KARMA_DISCARD = 2;

        public LoserConcluded(Game game) : base(game)
        {
        }

        public LoserConcluded()
        {
        }

        public int KarmaForcedKeptCardDecision { get; set; }

        public string _forcedKeptOrDiscardedCardIds;
        [JsonIgnore]
        public IEnumerable<TreacheryCard> ForcedKeptOrDiscardedCards
        {
            get
            {
                return IdStringToObjects(_forcedKeptOrDiscardedCardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _forcedKeptOrDiscardedCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public bool Assassinate { get; set; }

        public int _keptCardId;

        [JsonIgnore]
        public TreacheryCard KeptCard
        {
            get
            {
                return TreacheryCardManager.Get(_keptCardId);
            }
            set
            {
                _keptCardId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (Assassinate && !CanAssassinate(Game, Player)) return Message.Express("You can't assassinate");
            if (KeptCard != null && !CardsLoserMayKeep(Game).Contains(KeptCard)) return Message.Express("You can't keep ", KeptCard);
            var karmaUsed = KarmaForcedKeptCardDecision == KARMA_KEEP || KarmaForcedKeptCardDecision == KARMA_DISCARD;
            if (karmaUsed && !MayUseKarmaToForceKeptCards(Game, Player)) return Message.Express("You can't use ", TreacheryCardType.Karma, " to force keeping or discarding cards");
            if (!(KarmaForcedKeptCardDecision == KARMA_NO || karmaUsed)) return Message.Express("Invalid decision", KeptCard);
            if (!karmaUsed && ForcedKeptOrDiscardedCards.Any()) return Message.Express("You need to decide to either force keeping or discarding cards", KeptCard);
            
            return null;
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static bool CanAssassinate(Game g, Player p) => g.LoserMayTryToAssassinate && TargetOfAssassination(g,p) != null;

        public static Leader TargetOfAssassination(Game g, Player p) => p.Traitors.FirstOrDefault(l => l is Leader && l != g.WinnerHero && (l.Faction == g.BattleWinner || g.BattleWinner == Faction.Purple && g.IsGhola(l)) && !p.RevealedTraitors.Contains(l)) as Leader;

        public static IEnumerable<TreacheryCard> CardsLoserMayKeep(Game g) => g.CardsToBeDiscardedByLoserAfterBattle;

        public static bool IsApplicable(Game g, Player p) => p.Faction == g.BattleLoser && (CardsLoserMayKeep(g).Any() || g.LoserMayTryToAssassinate);

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " conclude the battle");
        }

        public static bool MayUseKarmaToForceKeptCards(Game game, Player player) =>
            player.Faction == Faction.Cyan &&
            !game.KarmaPrevented(player.Faction) &&
            !player.SpecialKarmaPowerUsed &&
            player.HasKarma &&
            game.CurrentPhase == Phase.BattleConclusion &&
            game.BattleLoser == player.Faction &&
            game.Applicable(Rule.AdvancedKarama);

        public static IEnumerable<TreacheryCard> CardsThatMayBeKeptOrDiscarded(Game game)
        {

            var result = new List<TreacheryCard>();
            var winnerPlan = game.CurrentBattle.PlanOf(game.BattleWinner);

            if (winnerPlan.Weapon != null) result.Add(winnerPlan.Weapon);
            if (winnerPlan.Defense != null) result.Add(winnerPlan.Defense);
            if (winnerPlan.Hero is TreacheryCard cheaphero) result.Add(cheaphero);

            return result;
        }
    }
}
