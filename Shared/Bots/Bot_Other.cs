/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        private CharityClaimed DetermineCharityClaimed()
        {
            if (!(Game.EconomicsStatus == BrownEconomicsStatus.Cancel || Game.EconomicsStatus == BrownEconomicsStatus.CancelFlipped))
            {
                return new CharityClaimed(Game) { Initiator = Faction };
            }
            else
            {
                return null;
            }
        }

        protected KarmaFreeRevival DetermineKarmaFreeRevival()
        {
            int specialForcesThatCanBeRevived = Math.Min(3, Revival.ValidMaxRevivals(Game, this, true));

            if (LastTurn || ForcesKilled + specialForcesThatCanBeRevived >= 6)
            {
                int forces = Math.Max(0, Math.Min(3, ForcesKilled) - specialForcesThatCanBeRevived);
                return new KarmaFreeRevival(Game) { Initiator = Faction, Hero = null, AmountOfForces = forces, AmountOfSpecialForces = specialForcesThatCanBeRevived };
            }
            else
            {
                return null;
            }
        }

        protected KarmaHandSwap DetermineKarmaHandSwap()
        {
            var toReturn = new List<TreacheryCard>();
            foreach (var c in TreacheryCards.OrderBy(c => CardQuality(c)))
            {
                if (toReturn.Count == Game.KarmaHandSwapNumberOfCards) break;

                toReturn.Add(c);
            }

            return new KarmaHandSwap(Game) { Initiator = Faction, ReturnedCards = toReturn };
        }

        protected KarmaHandSwapInitiated DetermineKarmaHandSwapInitiated()
        {
            if (Game.CurrentPhase == Phase.BiddingReport)
            {
                if (TreacheryCards.Count(c => CardQuality(c) <= 2) >= 2)
                {
                    var bestOpponentToSwapWith = Opponents.HighestOrDefault(o => CardsPlayerHas(o).Count(c => CardQuality(c) >= 3));
                    LogInfo("opponent with most known good cards: " + bestOpponentToSwapWith);

                    if (bestOpponentToSwapWith != null && CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c) >= 3) >= 2)
                    {
                        //Swap with an opponent that 2 or more good cards that i know of
                        LogInfo("swapping, because number of good cards = " + CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c) >= 3));
                        return new KarmaHandSwapInitiated(Game) { Initiator = Faction, Target = bestOpponentToSwapWith.Faction };
                    }

                    bestOpponentToSwapWith = Opponents.FirstOrDefault(o => o.TreacheryCards.Count == 4);
                    LogInfo("opponent with 4 cards: " + bestOpponentToSwapWith);

                    if (bestOpponentToSwapWith != null && CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c) < 3) <= 2)
                    {
                        LogInfo("swapping, because number of known bad cards = " + CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c) < 3));
                        //Swap with an opponent that has 4 cards and 2 or less useless cards that i know of
                        return new KarmaHandSwapInitiated(Game) { Initiator = Faction, Target = bestOpponentToSwapWith.Faction };
                    }
                }
            }

            return null;
        }

        protected virtual HarvesterPlayed DetermineHarvesterPlayed()
        {
            if (Game.CurrentTurn > 1 &&
                (
                Game.CurrentPhase == Phase.HarvesterA && ResourcesIn(Game.LatestSpiceCardA.Location) > 6 ||
                Game.CurrentPhase == Phase.HarvesterB && ResourcesIn(Game.LatestSpiceCardB.Location) > 6
                ))
            {
                return new HarvesterPlayed(Game) { Initiator = Faction };
            }
            else
            {
                return null;
            }
        }

        protected virtual MulliganPerformed DetermineMulliganPerformed()
        {
            return new MulliganPerformed(Game) { Initiator = Faction, Passed = !MulliganPerformed.MayMulligan(this) };
        }

        protected virtual OrangeDelay DetermineDelay()
        {
            if (!Game.Prevented(FactionAdvantage.OrangeDetermineMoveMoment))
            {
                return new OrangeDelay(Game) { Initiator = Faction };
            }
            else
            {
                return null;
            }
        }

        protected virtual ClairVoyancePlayed DetermineClairvoyance()
        {
            bool imInBattle = Game.CurrentPhase == Phase.BattlePhase && Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction);

            if (imInBattle && Game.LatestClairvoyanceBattle != Game.CurrentBattle)
            {
                var opponent = Game.CurrentBattle.OpponentOf(this);

                if (NrOfUnknownOpponentCards(opponent) > 0)
                {
                    LogInfo("Start using Clairvoyance against " + opponent);

                    var myWeapons = Battle.ValidWeapons(Game, this, null, null);
                    var enemyDefenses = Battle.ValidDefenses(Game, opponent, null, false).Where(w => Game.KnownCards(this).Contains(w));

                    if (
                        (MyPrescience == null || MyPrescience.Aspect != PrescienceAspect.Defense) &&
                        !myWeapons.Any(w => w.Type == TreacheryCardType.ProjectileAndPoison))
                    {
                        if (myWeapons.Any(w => w.IsPoisonWeapon) && !OpponentMayNotUse(TreacheryCardType.PoisonDefense, false) && !enemyDefenses.Any(w => w.IsPoisonDefense)) return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsDefenseInBattle, TreacheryCardType.PoisonDefense);
                        if (myWeapons.Any(w => w.IsProjectileWeapon) && !OpponentMayNotUse(TreacheryCardType.ProjectileDefense, false) && !enemyDefenses.Any(w => w.IsProjectileDefense)) return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsDefenseInBattle, TreacheryCardType.ProjectileDefense);
                    }

                    var enemyWeapons = Battle.ValidWeapons(Game, opponent, null, null).Where(w => Game.KnownCards(this).Contains(w));
                    var myDefenses = Battle.ValidDefenses(Game, this, null, false);

                    if (
                        (MyPrescience == null || MyPrescience.Aspect != PrescienceAspect.Weapon) &&
                        !myDefenses.Any(w => w.Type == TreacheryCardType.ShieldAndAntidote) &&
                        myDefenses.Any(w => w.IsPoisonDefense) &&
                        myDefenses.Any(w => w.IsProjectileDefense) &&
                        !OpponentMayNotUse(TreacheryCardType.Poison, true) &&
                        !OpponentMayNotUse(TreacheryCardType.Projectile, true)
                        )
                    {
                        return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsWeaponInBattle, TreacheryCardType.Poison);
                    }
                }
            }

            return null;
        }

        private bool OpponentMayNotUse(TreacheryCardType type, bool asWeapon)
        {
            var voice = MyVoice;
            return voice != null && voice.MayNot && Voice.IsVoicedBy(Game, asWeapon, type, voice.Type);
        }

        private ClairVoyancePlayed UseClairvoyanceInBattle(Faction opponent, ClairvoyanceQuestion question, TreacheryCardType cardtype)
        {
            return new ClairVoyancePlayed(Game) { Initiator = Faction, Target = opponent, Question = question, QuestionParameter1 = cardtype.ToString() };
        }

        protected virtual ClairVoyanceAnswered DetermineClairVoyanceAnswered()
        {
            LogInfo("DetermineClairVoyanceAnswered() {0}", Game.LatestClairvoyance.Question);

            ClairVoyanceAnswer answer = ClairVoyanceAnswer.Unknown;

            try
            {
                switch (Game.LatestClairvoyance.Question)
                {
                    case ClairvoyanceQuestion.CardTypeAsDefenseInBattle:
                        if (Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattle(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(Game, false, plan.Defense.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.CardTypeAsWeaponInBattle:
                        if (Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattle(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(Game, true, plan.Weapon.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.CardTypeInBattle:
                        if (Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattle(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(
                                    plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(Game, false, plan.Defense.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1) ||
                                    plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(Game, true, plan.Weapon.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1) ||
                                    plan.Hero != null && plan.Hero is TreacheryCard && (TreacheryCardType)Game.LatestClairvoyance.Parameter1 == TreacheryCardType.Mercenary);
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                        if (Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattle(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Dial(Game, Game.CurrentBattle.OpponentOf(this).Faction) > (float)Game.LatestClairvoyance.Parameter1);
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.LeaderInBattle:
                        if (Game.CurrentBattle != null && (Game.CurrentBattle.Initiator == Faction || Game.CurrentBattle.Target == Faction))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattle(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Hero == (IHero)Game.LatestClairvoyance.Parameter1);
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.HasCardTypeInHand:
                        answer = Answer(TreacheryCards.Any(c => Covers(c.Type, Game.LatestClairvoyance.Parameter1)));
                        break;

                    case ClairvoyanceQuestion.LeaderAsFacedancer:
                        answer = Answer(FaceDancers.Any(f => f.IsFaceDancer((IHero)Game.LatestClairvoyance.Parameter1)));
                        break;

                    case ClairvoyanceQuestion.LeaderAsTraitor:
                        answer = Answer(Traitors.Any(f => f.IsTraitor((IHero)Game.LatestClairvoyance.Parameter1)));
                        break;

                    case ClairvoyanceQuestion.Prediction:
                        answer = Answer(PredictedFaction == (Faction)Game.LatestClairvoyance.Parameter1 && PredictedTurn == (int)Game.LatestClairvoyance.Parameter2);
                        break;

                    case ClairvoyanceQuestion.WillAttackX:
                        answer = ClairVoyanceAnswer.No;
                        break;
                }
            }
            catch (Exception e)
            {
                LogInfo(e.ToString());
            }

            return new ClairVoyanceAnswered(Game) { Initiator = Faction, Answer = answer };
        }

        private bool Covers(TreacheryCardType typeToCheck, object coveredByType)
        {
            return 
                ClairVoyanceAnswered.IsQuestionedBy(Game, true,  typeToCheck, (TreacheryCardType)coveredByType) || 
                ClairVoyanceAnswered.IsQuestionedBy(Game, false, typeToCheck, (TreacheryCardType)coveredByType);
        }

        private ClairVoyanceAnswer Answer(bool value)
        {
            return value ? ClairVoyanceAnswer.Yes : ClairVoyanceAnswer.No;
        }

        protected virtual RaiseDeadPlayed DetermineRaiseDeadPlayed()
        {
            int specialForcesThatCanBeRevived = Math.Min(5, Revival.ValidMaxRevivals(Game, this, true));

            if (Game.CurrentTurn == Game.MaximumNumberOfTurns || Game.CurrentMainPhase > MainPhase.Resurrection)
            {
                if (ForcesKilled + specialForcesThatCanBeRevived >= 7)
                {
                    int forces = Math.Max(0, Math.Min(5, ForcesKilled) - specialForcesThatCanBeRevived);
                    return new RaiseDeadPlayed(Game) { Initiator = Faction, Hero = null, AmountOfForces = forces, AmountOfSpecialForces = specialForcesThatCanBeRevived, AssignSkill = false };
                }
                else
                {
                    int nrOfLivingLeaders = Leaders.Count(l => Game.IsAlive(l));
                    int minimumValue = Faction == Faction.Purple && nrOfLivingLeaders > 2 ? 4 : 0;

                    var leaderToRevive = Revival.ValidRevivalHeroes(Game, this).Where(l =>
                        SafeLeaders.Contains(l) &&
                        l.Faction != Ally &&
                        l.Value >= minimumValue
                        ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));

                    if (leaderToRevive == null)
                    {
                        leaderToRevive = Revival.ValidRevivalHeroes(Game, this).Where(l =>
                            l.Faction != Ally &&
                            l.Value >= minimumValue
                            ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));
                    }

                    if (leaderToRevive != null)
                    {
                        var assignSkill = Revival.MayAssignSkill(Game, this, leaderToRevive);
                        return new RaiseDeadPlayed(Game) { Initiator = Faction, Hero = leaderToRevive, AmountOfForces = 0, AmountOfSpecialForces = 0, AssignSkill = assignSkill };
                    }
                }
            }

            return null;
        }

        protected virtual AmalPlayed DetermineAmalPlayed()
        {
            if (Faction == Faction.Orange)
            {
                int allyResources = Ally != Faction.None ? AlliedPlayer.Resources : 0;

                if (Game.CurrentPhase == Phase.Resurrection && Opponents.Sum(p => p.Resources) > 2 * (Resources + allyResources))
                {
                    return new AmalPlayed(Game) { Initiator = Faction };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                int allyResources = Ally != Faction.None ? AlliedPlayer.Resources : 0;

                if (Game.CurrentTurn > 1 && Game.CurrentMainPhase == MainPhase.Bidding && Opponents.Sum(p => p.Resources) > 10 && Opponents.Sum(p => p.Resources) > (Opponents.Count() + 1) * (Resources + allyResources))
                {
                    return new AmalPlayed(Game) { Initiator = Faction };
                }
                else
                {
                    return null;
                }
            }
        }

        protected virtual MetheorPlayed DetermineMetheorPlayed()
        {
            var otherForcesInArrakeen = Game.ForcesOnPlanet[Game.Map.Arrakeen].Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
            var mineAndAlliedForcesInArrakeen = Game.ForcesOnPlanet[Game.Map.Arrakeen].Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

            var otherForcesInCarthag = Game.ForcesOnPlanet[Game.Map.Carthag].Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
            var mineAndAlliedForcesInCarthag = Game.ForcesOnPlanet[Game.Map.Carthag].Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

            if (otherForcesInArrakeen + otherForcesInCarthag > 2 * (mineAndAlliedForcesInArrakeen + mineAndAlliedForcesInCarthag))
            {
                return new MetheorPlayed(Game) { Initiator = Faction };
            }
            else
            {
                return null;
            }
        }

        protected virtual StormDialled DetermineStormDialled()
        {
            var min = StormDialled.ValidMinAmount(Game);
            var max = StormDialled.ValidMaxAmount(Game);
            return new StormDialled(Game) { Initiator = Faction, Amount = min + D(1, 1 + max - min) - 1 };
        }

        protected virtual TraitorsSelected DetermineTraitorsSelected()
        {
            var traitor = Traitors.Where(l => l.Faction != Faction).HighestOrDefault(l => l.Value);
            if (traitor == null) traitor = Traitors.HighestOrDefault(l => l.Value - (l.Faction == Faction.Green && Game.Applicable(Rule.GreenMessiah) ? 2 : 0));
            return new TraitorsSelected(Game) { Initiator = Faction, SelectedTraitor = traitor };
        }

        protected virtual FactionTradeOffered DetermineFactionTradeOffered()
        {
            var match = Game.CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Target == Faction);
            if (match != null)
            {
                return new FactionTradeOffered(Game) { Initiator = Faction, Target = match.Initiator };
            }

            return null;
        }

        protected virtual StormSpellPlayed DetermineStormSpellPlayed()
        {
            int moves = 1;
            var myKills = new Dictionary<int, int>
            {
                { 0, 0 }
            };

            var enemyKills = new Dictionary<int, int>
            {
                { 0, 0 }
            };

            for (moves = 1; moves <= 10; moves++)
            {
                var affectedLocations = Game.Map.Locations.Where(l => l.Sector == (Game.SectorInStorm + moves) % Map.NUMBER_OF_SECTORS && !Game.IsProtectedFromStorm(l));
                int myAndAllyForces = affectedLocations.Sum(l => Game.ForcesOnPlanet[l].Where(bat => bat.Faction == Faction || bat.Faction == Ally).Sum(bat => bat.TotalAmountOfForces));
                int enemyForces = affectedLocations.Sum(l => Game.ForcesOnPlanet[l].Where(bat => bat.Faction != Faction && bat.Faction != Ally).Sum(bat => bat.TotalAmountOfForces));
                myKills.Add(moves, myKills[moves - 1] + myAndAllyForces);
                enemyKills.Add(moves, enemyKills[moves - 1] + enemyForces);
            }

            var mostEffectiveMove = myKills.HighestOrDefault(myKills => enemyKills[myKills.Key] - myKills.Value);
            LogInfo("StormSpellPlayed() - Most effective number of moves: {0} sectors with {1} allied and {2} enemy kills.", mostEffectiveMove.Key, myKills[mostEffectiveMove.Key], enemyKills[mostEffectiveMove.Key]);

            if (enemyKills[mostEffectiveMove.Key] - myKills[mostEffectiveMove.Key] >= 10)
            {
                var stormspell = new StormSpellPlayed(Game) { Initiator = Faction, MoveAmount = mostEffectiveMove.Key };
                return stormspell;
            }
            else
            {
                return null;
            }
        }

        protected DistransUsed DetermineDistransUsed()
        {
            var worstCard = DistransUsed.ValidCards(Game, this).LowestOrDefault(c => CardQuality(c));
            if (worstCard != null && CardQuality(worstCard) <= 1)
            {
                var target = DistransUsed.ValidTargets(Game, this)
                    .Where(f => f != Ally && (!Game.Applicable(Rule.BlueWorthlessAsKarma) || f != Faction.Blue))
                    .Select(f => Game.GetPlayer(f))
                    .HighestOrDefault(p => Game.NumberOfVictoryPoints(p, true));

                if (target != null)
                {

                    return new DistransUsed(Game) { Initiator = Faction, Card = worstCard, Target = target.Faction };
                }
            }

            return null;
        }


        private int HeroRevivalPenalty(IHero h)
        {
            if (h.HeroType == HeroType.Messiah || h.HeroType == HeroType.Auditor)
            {
                return 20;
            }
            else if (KnownNonTraitors.Contains(h))
            {
                return 10;
            }
            else
            {
                return 0;
            }
        }

        protected virtual Revival DetermineRevival()
        {
            int availableResources = Resources - 4;
            int nrOfLivingLeaders = Leaders.Count(l => Game.IsAlive(l));
            int minimumValue = Faction == Faction.Purple && nrOfLivingLeaders > 2 ? 4 : 0;
            int maxToSpendOnHeroRevival = Math.Min(availableResources, 7);

            var leaderToRevive = Revival.ValidRevivalHeroes(Game, this).Where(l =>
                SafeLeaders.Contains(l) &&
                Revival.GetPriceOfHeroRevival(Game, this, l) <= maxToSpendOnHeroRevival &&
                l.Faction != Ally &&
                l.Value >= minimumValue
                ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));

            if (leaderToRevive == null)
            {
                leaderToRevive = Revival.ValidRevivalHeroes(Game, this).Where(l =>
                    Revival.GetPriceOfHeroRevival(Game, this, l) <= maxToSpendOnHeroRevival &&
                    l.Faction != Ally &&
                    l.Value >= minimumValue
                    ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));
            }

            int specialForcesToRevive = SpecialForcesKilled > 0 && !Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Faction) ? 1 : 0;
            int normalForcesToRevive = Math.Min(ForcesKilled, Game.FreeRevivals(this) - specialForcesToRevive);
            int maxRevivals = ((Ally == Faction.Red) ? Game.RedWillPayForExtraRevival : 0) + Game.GetRevivalLimit(Game, this);

            while (
                normalForcesToRevive + 1 + specialForcesToRevive <= maxRevivals &&
                normalForcesToRevive + 1 <= ForcesKilled &&
                (ForcesKilled + SpecialForcesKilled) * 2 > ForcesInReserve + SpecialForcesInReserve &&
                Revival.DetermineCost(Game, this, leaderToRevive, normalForcesToRevive + 1, specialForcesToRevive).TotalCostForPlayer <= availableResources)
            {
                normalForcesToRevive++;
            }

            var assignSkill = leaderToRevive != null && Revival.MayAssignSkill(Game, this, leaderToRevive);

            if (leaderToRevive != null || specialForcesToRevive + normalForcesToRevive > 0)
            {
                return new Revival(Game) { Initiator = Faction, Hero = leaderToRevive, AmountOfForces = normalForcesToRevive, AmountOfSpecialForces = specialForcesToRevive, AssignSkill = assignSkill };
            }
            else
            {
                return null;
            }
        }


        public DiscardedSearchedAnnounced DetermineDiscardedSearchedAnnounced()
        {
            if (Game.CurrentMainPhase == MainPhase.Contemplate)
            {
                var cardToSearch = DiscardedSearched.ValidCards(Game).HighestOrDefault(c => CardQuality(c));
                if (cardToSearch != null && CardQuality(cardToSearch) >= 4)
                {
                    return new DiscardedSearchedAnnounced(Game) { Initiator = Faction };
                }
            }

            return null;
        }

        public DiscardedSearched DetermineDiscardedSearched()
        {
            var cardToSearch = DiscardedSearched.ValidCards(Game).HighestOrDefault(c => CardQuality(c));
            return new DiscardedSearched(Game) { Initiator = Faction, Card = cardToSearch };
        }

        public DiscardedTaken DetermineDiscardedTaken()
        {
            var cardToTake = DiscardedTaken.ValidCards(Game, this).HighestOrDefault(c => CardQuality(c));
            if (cardToTake != null && CardQuality(cardToTake) >= 4)
            {
                return new DiscardedTaken(Game) { Initiator = Faction, Card = cardToTake };
            }

            return null;
        }

        public JuicePlayed DetermineJuicePlayed()
        {
            if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && Game.CurrentMoment == MainPhaseMoment.Start && Faction != Faction.Orange && Game.ShipmentAndMoveSequence.GetPlayersInSequence().LastOrDefault()?.Player != this)
            {
                return new JuicePlayed(Game) { Initiator = Faction, Type = JuiceType.GoLast };
            }
            else if (Game.CurrentMainPhase == MainPhase.Battle && Game.CurrentMoment == MainPhaseMoment.Start && Battle.BattlesToBeFought(Game, this).Any())
            {
                return new JuicePlayed(Game) { Initiator = Faction, Type = JuiceType.GoLast };
            }

            return null;
        }

        public Bureaucracy DetermineBureaucracy()
        {
            return new Bureaucracy(Game) { Initiator = Faction, Passed = Game.TargetOfBureaucracy == Ally };
        }

        public Diplomacy DetermineDiplomacy()
        {
            var opponentPlan = Game.CurrentBattle.PlanOfOpponent(this);
            if (opponentPlan.Weapon != null && opponentPlan.Weapon.CounteredBy(opponentPlan.Defense, null))
            {
                return new Diplomacy(Game) { Initiator = Faction, Card = Diplomacy.ValidCards(Game, this).First() };
            }

            return null;
        }

        public SkillAssigned DetermineSkillAssigned()
        {
            return new SkillAssigned(Game) { Initiator = Faction, Passed = false, Leader = RandomItemFrom(SkillAssigned.ValidLeaders(Game, this)), Skill = SkillAssigned.ValidSkills(this).First() };
        }

        public T RandomItemFrom<T>(IEnumerable<T> items)
        {
            var itemsAsArray = items.ToArray();
            return itemsAsArray[D(1, itemsAsArray.Length) - 1];
        }

        protected virtual Planetology DeterminePlanetology()
        {
            return new Planetology(Game) { Initiator = Faction, AddOneToMovement = true };
        }
    }

}
