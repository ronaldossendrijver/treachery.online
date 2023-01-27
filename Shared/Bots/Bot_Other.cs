/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        private DivideResources DetermineDivideResources()
        {
            var spiceIWant = Math.Max(0, DivideResources.GetResourcesToBeDivided(Game).Amount - Resources);
            return new DivideResources(Game) { Initiator = Faction, PortionToFirstPlayer = spiceIWant };
        }

        private DivideResourcesAccepted DetermineDivideResourcesAccepted()
        {
            var spiceIWant = Math.Max(0, DivideResources.GetResourcesToBeDivided(Game).Amount - Resources);
            var iWouldGet = DivideResources.GainedByOtherFaction(Game, true, Game.CurrentDivideResources.PortionToFirstPlayer);
            return new DivideResourcesAccepted(Game) { Initiator = Faction, Passed = (spiceIWant > iWouldGet + 1) };
        }

        private Discarded DetermineDiscarded()
        {
            var worstCard = TreacheryCards.OrderBy(c => CardQuality(c)).First();
            return new Discarded(Game) { Initiator = Faction, Card = worstCard };
        }

        protected TraitorDiscarded DetermineTraitorDiscarded()
        {
            var worstTraitor = DetermineWorstTraitor();

            if (worstTraitor == null)
            {
                worstTraitor = Traitors.LowestOrDefault(t => t.Value);
            }

            return new TraitorDiscarded(Game) { Initiator = Faction, Traitor = worstTraitor };
        }

        private IHero DetermineWorstTraitor()
        {
            var result = Traitors.Where(t => RevealedTraitors.Contains(t)).HighestOrDefault(t => t.Value);

            if (result == null && (Ally == Faction.Purple || !Game.IsPlaying(Faction.Purple)))
            {
                result = Traitors.Where(t => t.Faction == Faction).LowestOrDefault(t => t.Value);
            }

            if (result == null)
            {
                result = Traitors.Where(t => t is Leader && !Game.IsAlive(t)).LowestOrDefault(t => t.Value);
            }

            if (result == null)
            {
                result = Traitors.Where(t => t is Leader).LowestOrDefault(t => t.Value);
            }

            return result;
        }

        protected NexusPlayed DetermineNexusPlayed()
        {
            var result = new NexusPlayed(Game) { Initiator = Faction, Faction = Nexus };

            if (NexusPlayed.IsCunning(this))
            {
                return DetermineNexusPlayed_Cunning(result);
            }
            else if (NexusPlayed.IsSecretAlly(Game, this))
            {
                return DetermineNexusPlayed_SecretAlly(result);
            }
            else
            {
                return DetermineNexusPlayed_Betrayal(result);
            }
        }

        private bool IWishToAttack(int maxNumberOfContestedStrongholds, Faction f) => AlmostWinningOpponentsIWishToAttack(maxNumberOfContestedStrongholds, true).Select(p => p.Faction).Contains(f);

        private NexusPlayed DetermineNexusPlayed_Betrayal(NexusPlayed result)
        {
            if (Ally != result.Faction)
            {
                switch (Nexus)
                {
                    case Faction.Green: return result;
                    case Faction.Black: return result;
                    
                    case Faction.Yellow: 
                        if (Game.CurrentMainPhase == MainPhase.Blow && YellowRidesMonster.ValidSources(Game).Any() ||
                            Game.CurrentMainPhase == MainPhase.ShipmentAndMove && IWishToAttack(0, Faction.Yellow)) return result;
                        break;

                    case Faction.Red:
                        if (Game.CurrentMainPhase == MainPhase.Bidding && (Game.CurrentBid != null && Game.CurrentBid.Player.Ally == Faction.Red && Game.CurrentBid.TotalAmount > 5 && !Game.Prevented(FactionAdvantage.RedReceiveBid)) ||
                            Game.CurrentMainPhase == MainPhase.Battle && IWishToAttack(0, Faction.Red) && Game.CurrentBattle != null && Game.GetPlayer(Faction.Red).SpecialForcesIn(Game.CurrentBattle.Territory) >= 3) return result;
                        break;

                    case Faction.Orange:
                        if (Game.RecentlyPaidTotalAmount > 5) return result;
                        break;
                }
            }

            return null;
        }

        private NexusPlayed DetermineNexusPlayed_Cunning(NexusPlayed result)
        {
            switch (Nexus)
            {
                case Faction.Green:

                    if (Game.CurrentPrescience != null)
                    {
                        var opponent = Game.CurrentBattle.OpponentOf(Faction);

                        if (!(Voice.MayUseVoice(Game, opponent) && Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null))
                        {
                            if (Game.CurrentBattle.IsAggressorOrDefender(this))
                            {
                                result.GreenPrescienceAspect = BestPrescience(opponent, MaxDial(this, Game.CurrentBattle.Territory, opponent), Game.CurrentPrescience.Aspect);

                                if (result.GreenPrescienceAspect != PrescienceAspect.None)
                                {
                                    return result;
                                }
                            }
                        }
                    }
                    break;

                case Faction.Black: 
                    if (DetermineWorstTraitor() != null) return result; 
                    break;
                    
                case Faction.Yellow:
                    if (Game.Monsters.Any() && DetermineMovedBatallion(true) != null) return result;
                    break;

                case Faction.Red:
                    if (ForcesIn(Game.CurrentBattle.Territory) >= 4) return result;
                    break;

                case Faction.Orange:
                    var shipment = DetermineShipment();
                    if (shipment != null && !shipment.Passed && 
                        (decidedShipmentAction == ShipmentDecision.PreventNormalWin || decidedShipmentAction == ShipmentDecision.PreventFremenWin || decidedShipmentAction == ShipmentDecision.AttackWeakStronghold)) return result;
                    break;
            }

            return null;
        }

        private NexusPlayed DetermineNexusPlayed_SecretAlly(NexusPlayed result)
        {
            switch (Nexus)
            {
                case Faction.Green:

                    var opponent = Game.CurrentBattle.OpponentOf(Faction);
                    if (!(Voice.MayUseVoice(Game, opponent) && Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null))
                    {
                        if (Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            result.GreenPrescienceAspect = BestPrescience(opponent, MaxDial(this, Game.CurrentBattle.Territory, opponent), PrescienceAspect.None);
                            if (result.GreenPrescienceAspect != PrescienceAspect.None)
                            {
                                return result;
                            }
                        }
                    }
                    break;

                case Faction.Black:
                    if (DetermineWorstTraitor() != null) return result;
                    break;

                case Faction.Yellow:
                    if (Game.CurrentMainPhase == MainPhase.Resurrection && ForcesKilled >= 3) return result;
                    break;

                case Faction.Orange:
                    var shipment = DetermineShipment();
                    if (shipment != null && !shipment.Passed && Shipment.DetermineCost(Game, this, shipment) > 5) return result;
                    break;
            }

            return null;
        }

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
            int specialForcesThatCanBeRevived = Math.Min(3, Revival.ValidMaxRevivals(Game, this, true, false));

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

        protected KarmaShipmentPrevention DetermineKarmaShipmentPrevention()
        {
            if (Game.CurrentPhase == Phase.NonOrangeShip)
            {
                var validTargets = KarmaShipmentPrevention.GetValidTargets(Game, this).ToList();

                var winningOpponentThatCanShipMost = OpponentsToShipAndMove
                    .Where(p => validTargets.Contains(p.Faction))
                    .Where(p => IsWinningOpponent(p) && p.ForcesInReserve + p.SpecialForcesInReserve > 2 && p.Resources + p.AlliedPlayer?.Resources > 2)
                    .OrderByDescending(p => Math.Min(p.ForcesInReserve + p.SpecialForcesInReserve, p.Resources + (p.AlliedPlayer != null ? p.AlliedPlayer.Resources : 0)))
                    .FirstOrDefault();

                if (winningOpponentThatCanShipMost != null && Game.ShipmentAndMoveSequence.CurrentPlayer == winningOpponentThatCanShipMost)
                {
                    return new KarmaShipmentPrevention(Game) { Initiator = Faction, Target = winningOpponentThatCanShipMost.Faction };
                }
            }

            return null;
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
            bool imInBattle = Game.CurrentPhase == Phase.BattlePhase && Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this);

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

            if (Game.CurrentPhase == Phase.Bidding && !HasRoomForCards)
            {
                var bestLeaderToAskAbout = Leaders.Where(l => Game.IsAlive(l) && !SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);
                var bestPlayerToAsk = Opponents.Where(o => !o.ToldNonTraitors.Contains(bestLeaderToAskAbout)).HighestOrDefault(p => p.Traitors.Count - p.RevealedTraitors.Count);

                if (bestLeaderToAskAbout != null && bestPlayerToAsk != null)
                {
                    return new ClairVoyancePlayed(Game) { Initiator = Faction, Target = bestPlayerToAsk.Faction, Question = ClairvoyanceQuestion.LeaderAsTraitor, Parameter1 = bestLeaderToAskAbout.Id };
                }
            }

            return null;
        }

        private bool OpponentMayNotUse(TreacheryCardType type, bool asWeapon)
        {
            var voice = MyVoice;
            return voice != null && voice.MayNot && Voice.IsVoicedBy(Game, asWeapon, false, type, voice.Type);
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
                        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattlePlan(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(false, plan.Defense.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.CardTypeAsWeaponInBattle:
                        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattlePlan(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(true, plan.Weapon.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.CardTypeInBattle:
                        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattlePlan(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(
                                    plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(false, plan.Defense.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1) ||
                                    plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(true, plan.Weapon.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1) ||
                                    plan.Hero != null && plan.Hero is TreacheryCard && (TreacheryCardType)Game.LatestClairvoyance.Parameter1 == TreacheryCardType.Mercenary);
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattlePlan(false, true);
                            LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                            if (plan != null)
                            {
                                answer = Answer(plan.Dial(Game, Game.CurrentBattle.OpponentOf(this).Faction) > (float)Game.LatestClairvoyance.Parameter1);
                            }
                        }
                        break;

                    case ClairvoyanceQuestion.LeaderInBattle:
                        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
                        {
                            var plan = Game.CurrentBattle.PlanOf(this);
                            if (plan == null) plan = DetermineBattlePlan(false, true);
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
                ClairVoyanceAnswered.IsQuestionedBy(true, typeToCheck, (TreacheryCardType)coveredByType) ||
                ClairVoyanceAnswered.IsQuestionedBy(false, typeToCheck, (TreacheryCardType)coveredByType);
        }

        private ClairVoyanceAnswer Answer(bool value)
        {
            return value ? ClairVoyanceAnswer.Yes : ClairVoyanceAnswer.No;
        }

        protected virtual RaiseDeadPlayed DetermineRaiseDeadPlayed()
        {
            int specialForcesThatCanBeRevived = Math.Min(5, Revival.ValidMaxRevivals(Game, this, true, false));

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

                    var leaderToRevive = RaiseDeadPlayed.ValidHeroes(Game, this).Where(l =>
                        SafeOrKnownTraitorLeaders.Contains(l) &&
                        l.Faction != Ally &&
                        l.Value >= minimumValue
                        ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));

                    if (leaderToRevive == null)
                    {
                        leaderToRevive = RaiseDeadPlayed.ValidHeroes(Game, this).Where(l =>
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
            var otherForcesInArrakeen = Game.BattalionsIn(Game.Map.Arrakeen).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
            var mineAndAlliedForcesInArrakeen = Game.BattalionsIn(Game.Map.Arrakeen).Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

            var otherForcesInCarthag = Game.BattalionsIn(Game.Map.Carthag).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
            var mineAndAlliedForcesInCarthag = Game.BattalionsIn(Game.Map.Carthag).Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

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

        protected virtual ThumperPlayed DetermineThumperPlayed() => new ThumperPlayed(Game) { Initiator = Faction };

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
                var affectedLocations = Game.Map.Locations().Where(l => l.Sector == (Game.SectorInStorm + moves) % Map.NUMBER_OF_SECTORS && !Game.IsProtectedFromStorm(l));
                int myAndAllyForces = affectedLocations.Sum(l => Game.BattalionsIn(l).Where(bat => bat.Faction == Faction || bat.Faction == Ally).Sum(bat => bat.TotalAmountOfForces));
                int enemyForces = affectedLocations.Sum(l => Game.BattalionsIn(l).Where(bat => bat.Faction != Faction && bat.Faction != Ally).Sum(bat => bat.TotalAmountOfForces));
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
            if (Game.CurrentPhase == Phase.WaitingForNextBiddingRound)
            {
                var worstCard = DistransUsed.ValidCards(this).LowestOrDefault(c => CardQuality(c));
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
            if (Game.CurrentRevivalRequests.Any()) return null;

            int availableResources = Math.Max(0, (int)(0.5f * Resources - 2));
            int nrOfLivingLeaders = Leaders.Count(l => Game.IsAlive(l));
            int minimumValue = Faction == Faction.Purple && nrOfLivingLeaders > 2 ? 4 : 0;
            int maxToSpendOnHeroRevival = Math.Min(availableResources, 7);

            var leaderToRevive = Revival.ValidRevivalHeroes(Game, this).Where(l =>
                SafeOrKnownTraitorLeaders.Contains(l) &&
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

            bool useSecretAlly = Revival.MayUseRedSecretAlly(Game, this) && (ForcesKilled - Game.FreeRevivals(this, false) >= 3);

            DetermineOptimalUseOfRedRevivals(Game, this, out int forcesRevivedByRed, out int specialForcesRevivedByRed, useSecretAlly);

            int specialForcesToRevive = 0;
            while (
                //check limit of special forces
                specialForcesRevivedByRed + specialForcesToRevive + 1 <= Revival.ValidMaxRevivals(Game, this, true, useSecretAlly) &&

                //check if there are enough forces killed
                specialForcesRevivedByRed + specialForcesToRevive + 1 <= SpecialForcesKilled &&

                //check if i have enough spice
                Revival.DetermineCost(Game, this, leaderToRevive, 0, specialForcesToRevive + 1, forcesRevivedByRed, specialForcesRevivedByRed, useSecretAlly).TotalCostForPlayer <= availableResources)
            {
                specialForcesToRevive++;
            }

            int normalForcesToRevive = 0;

            while (
                //check limit of total amount of forces
                specialForcesToRevive + normalForcesToRevive + 1 <= Revival.ValidMaxRevivals(Game, this, false, useSecretAlly) &&

                //check if there are enough forces killed
                forcesRevivedByRed + normalForcesToRevive + 1 <= ForcesKilled &&

                //check if i have enough spice
                Revival.DetermineCost(Game, this, leaderToRevive, normalForcesToRevive + 1, specialForcesToRevive, forcesRevivedByRed, specialForcesRevivedByRed, useSecretAlly).TotalCostForPlayer <= availableResources)
            {
                normalForcesToRevive++;
            }

            var assignSkill = leaderToRevive != null && Revival.MayAssignSkill(Game, this, leaderToRevive);

            if (leaderToRevive != null || specialForcesToRevive + normalForcesToRevive > 0)
            {
                return new Revival(Game)
                {
                    Initiator = Faction,
                    Hero = leaderToRevive,
                    AmountOfForces = normalForcesToRevive,
                    ExtraForcesPaidByRed = forcesRevivedByRed,
                    AmountOfSpecialForces = specialForcesToRevive,
                    ExtraSpecialForcesPaidByRed = specialForcesRevivedByRed,
                    AssignSkill = assignSkill,
                    UsesRedSecretAlly = useSecretAlly
                };
            }
            else
            {
                return null;
            }
        }

        private static void DetermineOptimalUseOfRedRevivals(Game g, Player p, out int forces, out int specialForces, bool useCunning)
        {
            forces = 0;
            specialForces = 0;

            if (p.Ally != Faction.Red) return;

            var red = g.GetPlayer(Faction.Red);

            int potentialMaximumByRed = p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces)) ? g.RedWillPayForExtraRevival : 0;

            int maxSpecialForces = Revival.ValidMaxRevivals(g, p, true, useCunning);

            int freeRevivals = g.FreeRevivals(p, useCunning);

            if (maxSpecialForces > 0 && freeRevivals > 0)
            {
                maxSpecialForces--;
                freeRevivals--;
            }

            while (
                specialForces + 1 <= p.SpecialForcesKilled &&
                specialForces + 1 <= maxSpecialForces &&
                specialForces + 1 <= potentialMaximumByRed &&
                Revival.DetermineCostOfForcesForRed(g, red, p.Faction, 0, specialForces + 1) <= red.Resources)
            {
                specialForces++;
            }

            int maxForces = Revival.ValidMaxRevivals(g, p, false, useCunning);

            maxForces = Math.Max(maxForces - freeRevivals, 0);

            while (
                forces + 1 <= p.ForcesKilled &&
                forces + 1 <= maxForces &&
                specialForces + forces + 1 <= potentialMaximumByRed &&
                Revival.DetermineCostOfForcesForRed(g, red, p.Faction, forces + 1, specialForces) <= red.Resources)
            {
                forces++;
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
            LeaderSkill skill = LeaderSkill.None;
            var skills = SkillAssigned.ValidSkills(this);

            if (skill == LeaderSkill.None && TreacheryCards.Any(c => c.IsProjectileWeapon)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Swordmaster);
            if (skill == LeaderSkill.None && TreacheryCards.Any(c => c.IsPoisonWeapon)) skill = skills.FirstOrDefault(s => s == LeaderSkill.MasterOfAssassins);
            if (skill == LeaderSkill.None && TreacheryCards.Any(c => c.IsProjectileDefense)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Adept);
            if (skill == LeaderSkill.None && TreacheryCards.Any(c => c.IsPoisonDefense)) skill = skills.FirstOrDefault(s => s == LeaderSkill.KillerMedic);
            if (skill == LeaderSkill.None && TreacheryCards.Any(c => c.IsUseless)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Warmaster);

            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Graduate);
            if (skill == LeaderSkill.None && !Is(Faction.Green)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Thinker);

            if (skill == LeaderSkill.None && Is(Faction.Yellow)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Sandmaster);
            if (skill == LeaderSkill.None && !Is(Faction.Yellow)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Smuggler);

            bool isEconomicFaction = Is(Faction.Red) || Is(Faction.Orange) || Is(Faction.Brown) || Is(Faction.Purple);
            if (skill == LeaderSkill.None && !isEconomicFaction) skill = skills.FirstOrDefault(s => s == LeaderSkill.Bureaucrat);
            if (skill == LeaderSkill.None && !isEconomicFaction) skill = skills.FirstOrDefault(s => s == LeaderSkill.Banker);

            if (skill == LeaderSkill.None && !Is(Faction.Purple)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Decipherer);

            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Swordmaster);
            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.MasterOfAssassins);
            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Adept);
            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.KillerMedic);
            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Warmaster);

            if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s != LeaderSkill.Planetologist);

            if (skill == LeaderSkill.None) skill = skills.First();

            return new SkillAssigned(Game) { Initiator = Faction, Passed = false, Leader = RandomItemFrom(SkillAssigned.ValidLeaders(Game, this)), Skill = skill };
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
