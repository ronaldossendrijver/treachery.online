/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class LoserConcluded : GameEvent
    {
        #region Construction

        public LoserConcluded(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public LoserConcluded()
        {
        }

        #endregion Construction

        #region Properties

        public const int KARMA_NO = 0;
        public const int KARMA_KEEP = 1;
        public const int KARMA_DISCARD = 2;

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

        #endregion Properties

        #region Validation

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

        public static bool CanAssassinate(Game g, Player p) => g.LoserMayTryToAssassinate && TargetOfAssassination(g, p) != null;

        public static Leader TargetOfAssassination(Game g, Player p) => p.Traitors.FirstOrDefault(l => l is Leader && l != g.WinnerHero && (l.Faction == g.BattleWinner || g.BattleWinner == Faction.Purple && g.IsGhola(l)) && !p.RevealedTraitors.Contains(l)) as Leader;

        public static IEnumerable<TreacheryCard> CardsLoserMayKeep(Game g) => g.CardsToBeDiscardedByLoserAfterBattle;

        public static bool IsApplicable(Game g, Player p) => p.Faction == g.BattleLoser && (CardsLoserMayKeep(g).Any() || g.LoserMayTryToAssassinate);

        public static bool MayUseKarmaToForceKeptCards(Game game, Player player) =>
            player.Faction == Faction.Cyan &&
            !game.KarmaPrevented(player.Faction) &&
            !player.SpecialKarmaPowerUsed &&
            player.HasKarma(game) &&
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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (KeptCard != null)
            {
                if (Game.SecretAllyAllowsKeepingCardsAfterLosingBattle)
                {
                    Game.SecretAllyAllowsKeepingCardsAfterLosingBattle = false;
                    Game.PlayNexusCard(Player, "Secret Ally", " to keep ", KeptCard);
                }
                else
                {
                    Log(Initiator, " keep ", KeptCard);
                }
            }

            foreach (var c in Game.CardsToBeDiscardedByLoserAfterBattle.Where(c => c != KeptCard))
            {
                Game.Discard(c);
            }

            Game.CardsToBeDiscardedByLoserAfterBattle.Clear();

            Log();

            var winner = GetPlayer(Game.BattleWinner);

            if (KarmaForcedKeptCardDecision == KARMA_DISCARD || KarmaForcedKeptCardDecision == KARMA_KEEP)
            {
                Game.BattleWinnerMayChooseToDiscard = false;
                Game.Discard(Player, TreacheryCardType.Karma);
                Game.Stone(Milestone.Karma);

                foreach (var c in ForcedKeptOrDiscardedCards)
                {
                    if (KarmaForcedKeptCardDecision == KARMA_DISCARD)
                    {
                        Log("Using ", TreacheryCardType.Karma, ", ", Initiator, " force ", winner.Faction, " to discard ", c);
                        if (winner.Has(c)) Game.Discard(c);
                    }
                    else if (KarmaForcedKeptCardDecision == KARMA_KEEP)
                    {
                        Log("Using ", TreacheryCardType.Karma, ", ", Initiator, " force ", winner.Faction, " to keep ", c);
                        if (Game.TreacheryDiscardPile.Items.Contains(c))
                        {
                            Game.TreacheryDiscardPile.Items.Remove(c);
                            winner.TreacheryCards.Add(c);
                        }
                    }
                }
            }

            if (Assassinate)
            {
                Game.Stone(Milestone.Assassination);

                var assassinated = TargetOfAssassination(Game, Player);

                Game.Assassinated.Add(assassinated);
                Player.RevealedTraitors.Add(assassinated);

                if (!Game.IsAlive(assassinated) || !winner.Leaders.Contains(assassinated))
                {
                    Log(Initiator, " reveal ", assassinated, " as their target of assassination...");
                }
                else
                {
                    var price = assassinated.HeroType == HeroType.VariableValue ? 3 : assassinated.CostToRevive;
                    Log(Initiator, " get ", Payment.Of(price), " by ASSASSINATING ", assassinated, "!");
                    Player.Resources += assassinated.CostToRevive;
                    Game.KillHero(assassinated);
                }
            }

            Game.LoserMayTryToAssassinate = false;

            if (Game.BattleWasConcludedByWinner)
            {
                Game.Enter(!IsPlaying(Faction.Purple) || Game.BattleWinner == Faction.Purple, Game.FinishBattle, Game.Version <= 150, Phase.Facedancing, Phase.RevealingFacedancer);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " conclude the battle");
        }

        #endregion Execution
    }
}
