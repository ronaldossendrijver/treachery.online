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
        protected virtual BattleInitiated DetermineBattleInitiated()
        {
            var battle = Battle.BattlesToBeFought(Game, this).OrderBy(b => MaxDial(Game.GetPlayer(b.Item2), b.Item1, Faction) - MaxDial(this, b.Item1, b.Item2)).FirstOrDefault();

            return new BattleInitiated(Game)
            {
                Initiator = Faction,
                Target = battle.Item2,
                Territory = battle.Item1
            };
        }

        protected virtual TreacheryCalled DetermineTreacheryCalled()
        {
            if (Faction != Game.CurrentBattle.Initiator && Faction != Game.CurrentBattle.Target && !TreacheryCalled.MayCallTreachery(Game, this))
            {
                return null;
            }
            else
            {
                return new TreacheryCalled(Game) { Initiator = Faction, TraitorCalled = TreacheryCalled.MayCallTreachery(Game, this) };
            }
        }

        public bool HasNoFieldIn(Territory territory)
        {
            return Faction == Faction.White && SpecialForcesIn(territory) > 0;
        }

        protected virtual AuditCancelled DetermineAuditCancelled()
        {
            return new AuditCancelled(Game) { Initiator = Faction, Cancelled = false };
        }

        protected virtual Audited DetermineAudited()
        {
            return new Audited(Game) { Initiator = Faction };
        }

        protected virtual BattleConcluded DetermineBattleConcluded()
        {
            var myBattleplan = Game.CurrentBattle.PlanOf(this);
            var opponent = Game.CurrentBattle.OpponentOf(this);

            var discarded = new List<TreacheryCard>();

            if (myBattleplan.Weapon != null && TreacheryCards.Contains(myBattleplan.Weapon) && myBattleplan.Weapon.Type == TreacheryCardType.Useless)
            {
                discarded.Add(myBattleplan.Weapon);
            }

            if (myBattleplan.Defense != null && TreacheryCards.Contains(myBattleplan.Defense) && myBattleplan.Defense.Type == TreacheryCardType.Useless)
            {
                discarded.Add(myBattleplan.Defense);
            }

            bool kill = Game.BlackVictim != null && Game.BlackVictim.Value < 4;

            int replacedSpecialForces = BattleConcluded.ValidReplacementForceAmounts(Game, this).Max();

            return new BattleConcluded(Game) { Initiator = Faction, DiscardedCards = discarded, StolenToken = opponent.TechTokens.FirstOrDefault(), Kill = kill, SpecialForceLossesReplaced = replacedSpecialForces };
        }

        protected virtual Battle DetermineBattle(bool waitForPrescience)
        {
            LogInfo("DetermineBattle()");

            var opponent = Game.CurrentBattle.OpponentOf(this);
            var prescience = MyPrescience;

            if (waitForPrescience && prescience != null && Game.CurrentBattle.PlanOf(opponent) == null)
            {
                return null; //enemy is not ready yet
            }

            if (decidedShipmentAction == ShipmentDecision.DummyShipment)
            {
                return ConstructLostBattleMinimizingLosses(opponent);
            }

            int forcesAvailable = ForcesIn(Game.CurrentBattle.Territory);
            int specialForcesAvailable = SpecialForcesIn(Game.CurrentBattle.Territory);

            var dialNeeded = GetDialNeeded(
                IWillBeAggressorAgainst(opponent),
                opponent,
                Game.CurrentBattle.Territory,
                voicePlan != null && voicePlan.battle == Game.CurrentBattle ? voicePlan : null,
                prescience,
                false,
                out TreacheryCard defense,
                out TreacheryCard weapon,
                out IHero hero,
                out bool messiah,
                out bool isTraitor,
                out bool lasgunShield);

            LogInfo("AGAINST {0} in {1}, WITH {2} + {3} as WEAP + {4} as DEF, I need a force dial of {5}", opponent, Game.CurrentBattle.Territory, hero, weapon, defense, dialNeeded);

            var remainingDial = DetermineRemainingDialInBattle(
                lasgunShield ? 0.5f : dialNeeded, 
                opponent.Faction, 
                forcesAvailable, 
                specialForcesAvailable, 
                out int forcesAtFullStrength, 
                out int forcesAtHalfStrength, 
                out int specialForcesAtFullStrength, 
                out int specialForcesAtHalfStrength);

            bool predicted = WinWasPredictedByMeThisTurn(opponent.Faction);
            float dialShortageToAccept = prescience != null && prescience.Aspect == PrescienceAspect.Dial ? 0 : Param.Battle_DialShortageThresholdForThrowing;
            bool minimizeSpendingsInThisLostFight = predicted || isTraitor && !messiah || remainingDial >= dialShortageToAccept;

            if (!minimizeSpendingsInThisLostFight)
            {
                if (weapon == null && !MayUseUselessAsKarma) weapon = UselessAsWeapon(defense);
                if (defense == null && !MayUseUselessAsKarma) defense = UselessAsDefense(weapon);

                RemoveIllegalChoices(ref hero, ref weapon, ref defense);

                //Avoid Lasgun/Shield suicide
                if (weapon != null && weapon.Type == TreacheryCardType.Laser && defense != null && defense.IsShield)
                {
                    if (MayPlayNoWeapon(defense))
                    {
                        weapon = null;
                    }
                    else
                    {
                        defense = null;
                    }
                }

                LogInfo("Leader: {0}, Weapon: {1}, Defense: {2}, Forces: {3} {4} {5} {6}", hero, weapon, defense, forcesAtFullStrength, forcesAtHalfStrength, specialForcesAtFullStrength, specialForcesAtHalfStrength);
                return new Battle(Game)
                {
                    Initiator = Faction,
                    Hero = hero,
                    Messiah = messiah,
                    Forces = forcesAtFullStrength,
                    ForcesAtHalfStrength = forcesAtHalfStrength,
                    SpecialForces = specialForcesAtFullStrength,
                    SpecialForcesAtHalfStrength = specialForcesAtHalfStrength,
                    Defense = defense,
                    Weapon = weapon
                };
            }
            else
            {
                LogInfo("I'm spending as little as possible on this fight.");
                return ConstructLostBattleMinimizingLosses(opponent);
            }
        }

        private void UseArtilleryStrikeOrPoisonToothIfApplicable(bool enemyCanDefendPoisonTooth, ref float myHeroSurviving, ref float enemyHeroSurviving, ref TreacheryCard defense, ref TreacheryCard weapon)
        {
            if (weapon == null && myHeroSurviving < Param.Battle_MimimumChanceToAssumeMyLeaderSurvives && enemyHeroSurviving >= Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives)
            {
                weapon = Weapons(defense).FirstOrDefault(c => c.Type == TreacheryCardType.ArtilleryStrike);

                if (weapon == null && !enemyCanDefendPoisonTooth)
                {
                    weapon = Weapons(defense).FirstOrDefault(c => c.Type == TreacheryCardType.PoisonTooth);
                }

                if (weapon != null)
                {
                    enemyHeroSurviving = 0;
                    myHeroSurviving = 0;
                }

                if (weapon != null && defense != null && MayPlayNoDefense(weapon) &&
                   (weapon.Type == TreacheryCardType.PoisonTooth && defense.Type != TreacheryCardType.Chemistry || weapon.Type == TreacheryCardType.ArtilleryStrike && !defense.IsShield)) defense = null;
            }
        }

        private Battle ConstructLostBattleMinimizingLosses(Player opponent)
        {
            SelectHeroForBattle(opponent, false, false, out IHero lowestAvailableHero, out _);

            var uselessAsWeapon = lowestAvailableHero == null || MayUseUselessAsKarma ? null : UselessAsWeapon(null);
            var uselessAsDefense = lowestAvailableHero == null || MayUseUselessAsKarma ? null : UselessAsDefense(uselessAsWeapon);

            RemoveIllegalChoices(ref lowestAvailableHero, ref uselessAsWeapon, ref uselessAsDefense);
            
            if (Battle.MustPayForForcesInBattle(Game, this))
            {
                return new Battle(Game)
                {
                    Initiator = Faction,
                    Hero = lowestAvailableHero,
                    Forces = 0,
                    ForcesAtHalfStrength = Battle.MaxForces(Game, this, false),
                    SpecialForces = 0,
                    SpecialForcesAtHalfStrength = Battle.MaxForces(Game, this, true),
                    Defense = uselessAsDefense,
                    Weapon = uselessAsWeapon
                };
            }
            else
            {
                return new Battle(Game)
                {
                    Initiator = Faction,
                    Hero = lowestAvailableHero,
                    Forces = Battle.MaxForces(Game, this, false),
                    ForcesAtHalfStrength = 0,
                    SpecialForces = Battle.MaxForces(Game, this, true),
                    SpecialForcesAtHalfStrength = 0,
                    Defense = uselessAsDefense,
                    Weapon = uselessAsWeapon
                };
            }
        }

        private void RemoveIllegalChoices(ref IHero hero, ref TreacheryCard weapon, ref TreacheryCard defense)
        {
            var weapClairvoyance = RulingWeaponClairvoyanceForThisBattle;
            if (weapClairvoyance != null && !IsAllowedWithClairvoyance(weapClairvoyance, weapon, true))
            {
                weapon = Weapons(defense).FirstOrDefault(c => IsAllowedWithClairvoyance(weapClairvoyance, c, true));
            }
            
            var defClairvoyance = RulingDefenseClairvoyanceForThisBattle;
            if (defClairvoyance != null && !IsAllowedWithClairvoyance(defClairvoyance, defense, false))
            {
                defense = Defenses(weapon).FirstOrDefault(c => IsAllowedWithClairvoyance(defClairvoyance, c, false));
            }

            if (defense == weapon && weapon != null && weapon.Type == TreacheryCardType.Chemistry) weapon = null;
            if (defense == weapon && weapon != null && weapon.Type == TreacheryCardType.WeirdingWay) defense = null;
            if (defense == weapon) defense = null;
            if (weapon == null && defense != null && defense.Type == TreacheryCardType.WeirdingWay) defense = null;
            if (defense == null && weapon != null && weapon.Type == TreacheryCardType.Chemistry) weapon = null;

            if (!Battle.ValidWeapons(Game, this, defense, true).Contains(weapon))
            {
                weapon = Weapons(defense).FirstOrDefault(w => w.Type != TreacheryCardType.Chemistry);
            }

            if (!Battle.ValidDefenses(Game, this, weapon, true).Contains(defense))
            {
                defense = Defenses(weapon).FirstOrDefault(w => w.Type != TreacheryCardType.WeirdingWay);
            }

            if (hero == null)
            {
                defense = null;
                weapon = null;
            }
        }

        protected float DetermineRemainingDialInBattle(float dialNeeded, Faction opponent, int forcesAvailable, int specialForcesAvailable)
        {
            return DetermineRemainingDialInBattle(dialNeeded, opponent, forcesAvailable, specialForcesAvailable, out _, out _, out _, out _);
        }

        protected float DetermineRemainingDialInBattle(float dialNeeded, Faction opponent, int forcesAvailable, int specialForcesAvailable, out int forcesAtFullStrength, out int forcesAtHalfStrength, out int specialForcesAtFullStrength, out int specialForcesAtHalfStrength)
        {
            var normalStrength = Battle.DetermineNormalForceStrength(Faction);
            var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
            int spiceLeft = Resources;
            int costPerForce = Battle.NormalForceCost(Game, this);
            int costPerSpecialForce = Battle.SpecialForceCost(Game, this);

            LogInfo("DetermineValidForcesInBattle: {0} {1} {2} {3}", dialNeeded, spiceLeft, costPerSpecialForce, costPerForce);

            if (Battle.MustPayForForcesInBattle(Game, this))
            {
                specialForcesAtFullStrength = 0;
                while (dialNeeded >= specialStrength && specialForcesAvailable >= 1 && spiceLeft >= costPerSpecialForce)
                {
                    dialNeeded -= specialStrength;
                    specialForcesAtFullStrength++;
                    specialForcesAvailable--;
                    spiceLeft -= costPerSpecialForce;
                }

                forcesAtFullStrength = 0;
                while (dialNeeded >= normalStrength && forcesAvailable >= 1 && spiceLeft >= costPerForce)
                {
                    dialNeeded -= normalStrength;
                    forcesAtFullStrength++;
                    forcesAvailable--;
                    spiceLeft -= costPerForce;
                }

                specialForcesAtHalfStrength = 0;
                while (dialNeeded > 0 && specialForcesAvailable >= 1)
                {
                    dialNeeded -= 0.5f * specialStrength;
                    specialForcesAtHalfStrength++;
                    specialForcesAvailable--;
                }

                forcesAtHalfStrength = 0;
                while (dialNeeded > 0 && forcesAvailable >= 1)
                {
                    dialNeeded -= 0.5f * normalStrength;
                    forcesAtHalfStrength++;
                    forcesAvailable--;
                }
            }
            else
            {
                specialForcesAtFullStrength = 0;
                while (dialNeeded >= specialStrength && specialForcesAvailable >= 1 && spiceLeft >= costPerSpecialForce)
                {
                    dialNeeded -= specialStrength;
                    specialForcesAtFullStrength++;
                    specialForcesAvailable--;
                    spiceLeft -= costPerSpecialForce;
                }

                forcesAtFullStrength = 0;
                while (dialNeeded > 0 && forcesAvailable >= 1 && spiceLeft >= costPerForce)
                {
                    dialNeeded -= normalStrength;
                    forcesAtFullStrength++;
                    forcesAvailable--;
                    spiceLeft -= costPerForce;
                }

                while (dialNeeded > 0 && specialForcesAvailable >= 1 && spiceLeft >= costPerSpecialForce)
                {
                    dialNeeded -= specialStrength;
                    specialForcesAtFullStrength++;
                    specialForcesAvailable--;
                    spiceLeft -= costPerSpecialForce;
                }

                specialForcesAtHalfStrength = 0;
                forcesAtHalfStrength = 0;
            }

            return dialNeeded;
        }


        protected float ChanceOfMyLeaderSurviving(Player opponent, VoicePlan voicePlan, Prescience prescience, out TreacheryCard mostEffectiveDefense, TreacheryCard chosenWeapon)
        {
            if (!Battle.ValidBattleHeroes(Game, opponent).Any())
            {
                LogInfo("Opponent has no leaders");
                mostEffectiveDefense = null;
                return 1;
            }

            if (voicePlan != null && voicePlan.defenseToUse != null)
            {
                mostEffectiveDefense = voicePlan.defenseToUse;
                return voicePlan.playerHeroWillCertainlySurvive ? 1 : 0.5f;
            }

            var availableDefenses = Defenses(chosenWeapon).Where(def =>
                def != chosenWeapon &&
                (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
                (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
                def.Type != TreacheryCardType.Useless
                ).ToArray();

            if (chosenWeapon != null && chosenWeapon.Type == TreacheryCardType.ArtilleryStrike)
            {
                mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.IsShield);
                return 0;
            }

            if (chosenWeapon != null && chosenWeapon.Type == TreacheryCardType.PoisonTooth)
            {
                mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.Type == TreacheryCardType.Chemistry);
                return 0;
            }

            var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
            if (prescience != null && prescience.Aspect == PrescienceAspect.Weapon && opponentPlan != null)
            {
                if (opponentPlan.Weapon == null || opponentPlan.Weapon.Type == TreacheryCardType.Useless)
                {
                    if (MayPlayNoDefense(chosenWeapon))
                    {
                        mostEffectiveDefense = null;
                    }
                    else
                    {
                        mostEffectiveDefense = Defenses(chosenWeapon).FirstOrDefault();
                    }

                    return 1;
                }
                else
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => opponentPlan.Weapon.CounteredBy(d));
                    return mostEffectiveDefense != null ? 1 : 0;
                }
            }

            var myClairvoyance = MyClairVoyanceAboutEnemyWeaponInCurrentBattle;
            if (myClairvoyance != null)
            {
                LogInfo("Clairvoyance detected!");

                if (myClairvoyance.Question.IsAbout(TreacheryCardType.Projectile))
                {
                    if (myClairvoyance.Answer.IsYes())
                    {
                        mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsProjectileDefense);
                        return (mostEffectiveDefense != null) ? 1 : 0;
                    }
                    else if (myClairvoyance.Answer.IsNo())
                    {
                        mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                        if (mostEffectiveDefense != null) return 0.5f;
                    }
                }

                if (myClairvoyance.Question.IsAbout(TreacheryCardType.Poison))
                {
                    if (myClairvoyance.Answer.IsYes())
                    {
                        mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                        return (mostEffectiveDefense != null) ? 1 : 0;
                    }
                    else if (myClairvoyance.Answer.IsNo())
                    {
                        mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsProjectileDefense);
                        if (mostEffectiveDefense != null) return 0.5f;
                    }
                }
            }

            return DetermineBestDefense(opponent, chosenWeapon, out mostEffectiveDefense);
        }

        private int NrOfUnknownOpponentCards(Player opponent)
        {
            return opponent.TreacheryCards.Count(c => !Game.KnownCards(this).Contains(c));
        }

        private float DetermineBestDefense(Player opponent, TreacheryCard chosenWeapon, out TreacheryCard mostEffectiveDefense)
        {
            var knownEnemyWeapons = KnownOpponentWeapons(opponent).ToArray();
            var availableDefenses = Defenses(chosenWeapon).Where(def =>
                def != chosenWeapon &&
                (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
                (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
                def.Type != TreacheryCardType.Useless
                ).ToArray();

            LogInfo("availableDefenses: " + Skin.Current.Join(availableDefenses));

            var defenseQuality = new ObjectCounter<TreacheryCard>();

            var unknownCards = CardsUnknownToMe;

            var bestDefenseAgainstUnknownCards = availableDefenses
                    .OrderBy(def => NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, def))
                    .FirstOrDefault();

            foreach (var def in availableDefenses)
            {
                defenseQuality.Count(def);

                if (def == bestDefenseAgainstUnknownCards)
                {
                    defenseQuality.Count(def);
                }

                foreach (var knownWeapon in knownEnemyWeapons)
                {
                    if (knownWeapon.CounteredBy(def))
                    {
                        LogInfo("potentialWeapon " + knownWeapon + " is countered by " + def);
                        defenseQuality.Count2(def);
                    }
                }
            }

            mostEffectiveDefense = defenseQuality.Highest;

            var defenseToCheck = mostEffectiveDefense;
            
            if (mostEffectiveDefense == null && knownEnemyWeapons.Any() || knownEnemyWeapons.Any(w => !w.CounteredBy(defenseToCheck))) return 0;

            return 1 - ChanceOfAnUnknownOpponentCardKillingMyLeader(unknownCards, mostEffectiveDefense, opponent);
        }

        private float ChanceOfAnUnknownOpponentCardKillingMyLeader(List<TreacheryCard> unknownCards, TreacheryCard usedDefense, Player opponent)
        {
            if (unknownCards.Count == 0) return 0;
            float numberOfUnknownWeaponsThatCouldKillMeWithThisDefense = NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, usedDefense);
            var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

            var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownWeaponsThatCouldKillMeWithThisDefense, unknownCards.Count, nrOfUnknownOpponentCards);
            
            LogInfo("ChanceOfAnUnknownOpponentCardKillingMyLeader: unknownCards.Length {0}, numberOfUnknownWeaponsThatCouldKillMeWithThisDefense {1}, NrOfUnknownOpponentCards {2} = {3}",
                unknownCards.Count,
                numberOfUnknownWeaponsThatCouldKillMeWithThisDefense,
                nrOfUnknownOpponentCards,
                result);

            return result;
        }

        private float ChanceOfAnUnknownOpponentCardSavingHisLeader(List<TreacheryCard> unknownCards, TreacheryCard usedWeapon, Player opponent)
        {
            if (unknownCards.Count == 0) return 0;
            float numberOfUnknownDefensesThatCouldCounterThisWeapon = NumberOfUnknownDefensesThatCouldCounterThisWeapon(unknownCards, usedWeapon);
            var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

            var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownDefensesThatCouldCounterThisWeapon, unknownCards.Count, nrOfUnknownOpponentCards);

            LogInfo("ChanceOfAnUnknownOpponentCardSavingHisLeader: unknownCards.Length {0}, numberOfUnknownDefensesThatCouldCounterThisWeapon {1}, NrOfUnknownOpponentCards {2} = {3}",
                unknownCards.Count,
                numberOfUnknownDefensesThatCouldCounterThisWeapon,
                nrOfUnknownOpponentCards,
                result);

            return result;
        }

        private double CumulativeChance(double a, double b, int length)
        {
            double result = 1;
            for (int i = 0; i < length; i++)
            {
                result *= (a - i) / (b - i);
            }
            return result;
        }

        private float NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(IEnumerable<TreacheryCard> unknownCards, TreacheryCard defense)
        {
            return unknownCards.Count(c => c.IsWeapon && (defense == null || !c.CounteredBy(defense)));
        }

        private float NumberOfUnknownDefensesThatCouldCounterThisWeapon(IEnumerable<TreacheryCard> unknownCards, TreacheryCard weapon)
        {
            return unknownCards.Count(c => c.IsDefense && (weapon == null || weapon.CounteredBy(c)));
        }

        private ClairVoyanceQandA RulingWeaponClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle && 
            (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle ) ? Game.LatestClairvoyanceQandA : null;

        private ClairVoyanceQandA RulingDefenseClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle && 
            (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;


        protected float ChanceOfEnemyLeaderDying(Player opponent, VoicePlan voicePlan, Prescience prescience, out TreacheryCard mostEffectiveWeapon, out bool enemyCanDefendPoisonTooth)
        {
            enemyCanDefendPoisonTooth = false;

            if (!Battle.ValidBattleHeroes(Game, opponent).Any())
            {
                LogInfo("Opponent has no leaders");
                mostEffectiveWeapon = null;
                return 0f;
            }

            if (voicePlan != null && voicePlan.weaponToUse != null)
            {
                mostEffectiveWeapon = voicePlan.weaponToUse;
                return voicePlan.opponentHeroWillCertainlyBeZero ? 1 : 0.5f;
            }

            var availableWeapons = Weapons(null).Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth)
                .OrderBy(w => NumberOfUnknownDefensesThatCouldCounterThisWeapon(CardsUnknownToMe, w)).ToArray();

            var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
            
            //Prescience available?
            if (prescience != null && prescience.Aspect == PrescienceAspect.Defense && opponentPlan != null)
            {
                enemyCanDefendPoisonTooth = opponentPlan.Defense != null && opponentPlan.Defense.IsNonAntidotePoisonDefense;
                mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => opponentPlan.Defense == null || !w.CounteredBy(opponentPlan.Defense));

                return mostEffectiveWeapon != null ? 1f : 0f;
            }

            var knownEnemyDefenses = KnownOpponentDefenses(opponent);

            //Clairvoyance available?
            var myClairvoyance = MyClairVoyanceAboutEnemyDefenseInCurrentBattle;
            if (myClairvoyance != null)
            {
                if (myClairvoyance.Question.IsAbout(TreacheryCardType.ProjectileDefense))
                {
                    if (Game.LatestClairvoyanceQandA.Answer.IsNo())
                    {
                        enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                        mostEffectiveWeapon = availableWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                        if (mostEffectiveWeapon != null) return 1f;
                    }
                    else if (Game.LatestClairvoyanceQandA.Answer.IsYes())
                    {
                        mostEffectiveWeapon = availableWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                        if (mostEffectiveWeapon != null) return 1f;
                    }
                }
                else if (myClairvoyance.Question.IsAbout(TreacheryCardType.PoisonDefense))
                {
                    if (Game.LatestClairvoyanceQandA.Answer.IsNo())
                    {
                        mostEffectiveWeapon = availableWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                        if (mostEffectiveWeapon != null) return 1f;
                    }
                    else if (Game.LatestClairvoyanceQandA.Answer.IsYes())
                    {
                        enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                        mostEffectiveWeapon = availableWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                        if (mostEffectiveWeapon != null) return 1f;
                    }
                }
            }
            
            var unknownOpponentCards = OpponentCardsUnknownToMe(opponent);

            mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => !knownEnemyDefenses.Any(defense => w.CounteredBy(defense)));
            LogInfo("ChanceOfLeaderDying(): {0} is a weapon without a known defense.", mostEffectiveWeapon);
            if (mostEffectiveWeapon != null)
            {
                if (!unknownOpponentCards.Any())
                {
                    return 1f;
                }
                else
                {
                    return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(unknownOpponentCards, mostEffectiveWeapon, opponent);
                }
            }

            mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => !IsKnownToOpponent(opponent, w));
            LogInfo("ChanceOfLeaderDying(): {0} is weapon unknown to my enemy.", mostEffectiveWeapon);
            if (mostEffectiveWeapon != null)
            {
                return 0.5f;
            }
            
            enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);

            return 0f;
        }

        private bool IsAllowedWithClairvoyance(ClairVoyanceQandA clairvoyance, TreacheryCard toUse, bool asWeapon)
        {
            bool inScope = toUse != null && ClairVoyancePlayed.IsInScopeOf(asWeapon, toUse.Type, (TreacheryCardType)clairvoyance.Question.Parameter1);

            var answer = clairvoyance == null ||
                    clairvoyance.Answer.Answer == ClairVoyanceAnswer.Unknown ||
                    clairvoyance.Answer.Answer == ClairVoyanceAnswer.Yes && toUse != null && inScope ||
                    clairvoyance.Answer.Answer == ClairVoyanceAnswer.No && (toUse == null || !inScope);

            LogInfo("IsAllowedWithClairvoyance(): in scope: {0}, answer: {1}.",
                inScope,
                answer);

            return answer;
        }

        protected void SelectHeroForBattle(Player opponent, bool highest, bool messiahUsed, out IHero hero, out bool isTraitor)
        {
            isTraitor = false;

            var ally = Ally != Faction.None ? AlliedPlayer : null;
            var purple = Game.GetPlayer(Faction.Purple);

            var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : Array.Empty<IHero>();
            var revealedTraitorsByNonOpponents = Game.Players.Where(p => p != opponent && (p.Ally != Faction.Black || p.Faction != opponent.Ally)).SelectMany(p => p.RevealedTraitors);
            var knownNonTraitors = Traitors.Union(KnownNonTraitors).Union(knownNonTraitorsByAlly).Union(revealedTraitorsByNonOpponents);
            var revealedTraitorsByOpponentsInBattle = Game.Players.Where(p => p == opponent || (p.Ally == Faction.Black && p.Faction == opponent.Ally)).SelectMany(p => p.RevealedTraitors);
            var highestOpponentLeader = Battle.ValidBattleHeroes(Game, opponent).OrderByDescending(l => l.Value).FirstOrDefault();
            var safeLeaders = Battle.ValidBattleHeroes(Game, this).Where(l => messiahUsed || (knownNonTraitors.Contains(l) && !revealedTraitorsByOpponentsInBattle.Contains(l)));

            IHero safeHero = null;
            IHero unsafeHero = null;

            if (highest)
            {
                safeHero = safeLeaders.OrderByDescending(l => l.ValueInCombatAgainst(highestOpponentLeader)).FirstOrDefault();
                unsafeHero = Battle.ValidBattleHeroes(Game, this).OrderByDescending(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader)).FirstOrDefault();
            }
            else
            {
                safeHero = safeLeaders.OrderBy(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader)).FirstOrDefault();
                unsafeHero = Battle.ValidBattleHeroes(Game, this).OrderBy(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader)).FirstOrDefault();
            }

            if (safeHero == null ||
                opponent.Faction != Faction.Black && !revealedTraitorsByOpponentsInBattle.Contains(unsafeHero) && safeHero.ValueInCombatAgainst(highestOpponentLeader) < unsafeHero.ValueInCombatAgainst(highestOpponentLeader) - 2)
            {
                hero = unsafeHero;
            }
            else
            {
                hero = safeHero;
            }

            isTraitor = !messiahUsed && revealedTraitorsByOpponentsInBattle.Contains(hero);
        }

        protected float GetDialNeeded(Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
        {
            //var opponent = GetOpponentThatOccupies(territory);
            var voicePlan = Voice.MayUseVoice(Game, this) ? BestVoice(null, this, opponent) : null;
            var strength = MaxDial(opponent, territory, Faction);
            var prescience = Prescience.MayUsePrescience(Game, this) ? BestPrescience(opponent, strength) : null;

            //More could be done with the information obtained in the below call
            return GetDialNeeded(IWillBeAggressorAgainst(opponent), opponent, territory, voicePlan, prescience, takeReinforcementsIntoAccount, out _, out _, out _, out _, out _, out _);
        }

        protected float GetDialNeeded(
            bool iAmAggressor, Player opponent, Territory territory, VoicePlan voicePlan, Prescience prescience, bool takeReinforcementsIntoAccount,
            out TreacheryCard bestDefense, out TreacheryCard bestWeapon, out IHero hero, out bool messiah, out bool isTraitor, out bool lasgunShieldDetected)
        {
            bool enemyCanDefendPoisonTooth = false;
            float chanceOfMyHeroSurviving;
            float chanceOfEnemyHeroSurviving;

            chanceOfEnemyHeroSurviving = 1 - ChanceOfEnemyLeaderDying(opponent, voicePlan, prescience, out bestWeapon, out enemyCanDefendPoisonTooth);
 
            LogInfo("Chance of enemy hero surviving: {0} with {1}", chanceOfEnemyHeroSurviving, bestWeapon);

            chanceOfMyHeroSurviving = ChanceOfMyLeaderSurviving(opponent, voicePlan, prescience, out bestDefense, bestWeapon);

            UseArtilleryStrikeOrPoisonToothIfApplicable(enemyCanDefendPoisonTooth, ref chanceOfMyHeroSurviving, ref chanceOfEnemyHeroSurviving, ref bestDefense, ref bestWeapon);

            var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
            lasgunShieldDetected = HasLasgunShield(
                bestWeapon,
                bestDefense,
                (prescience != null && prescience.Aspect == PrescienceAspect.Weapon && opponentPlan != null) ? opponentPlan.Weapon : null,
                (prescience != null && prescience.Aspect == PrescienceAspect.Defense && opponentPlan != null) ? opponentPlan.Defense : null);

            int myMessiahBonus = 0;
            if (Battle.MessiahAvailableForBattle(Game, this) && !lasgunShieldDetected)
            {
                messiah = true;
                myMessiahBonus = 2;
            }
            else
            {
                messiah = false;
            }

            isTraitor = false;
            SelectHeroForBattle(opponent, !lasgunShieldDetected && chanceOfMyHeroSurviving > 0, messiah, out hero, out isTraitor);

            if (hero == null)
            {
                messiah = false;
                myMessiahBonus = 0;
            }

            if (isTraitor)
            {
                LogInfo("My leader is a traitor: chanceOfMyHeroSurviving = 0");
                chanceOfMyHeroSurviving = 0;
                chanceOfEnemyHeroSurviving = 1;
                return 99;
            }

            if (opponent.Leaders.All(l =>
                !opponent.MessiahAvailable && Traitors.Contains(l) ||
                FaceDancers.Contains(l) ||
                !opponent.MessiahAvailable && (Ally == Faction.Black && AlliedPlayer.Traitors.Contains(l)) ||
                (Ally == Faction.Purple && AlliedPlayer.FaceDancers.Contains(l))))
            {
                LogInfo("Opponent leader only has traitors or facedancers to use!");
                bestWeapon = null;
                bestDefense = null;
                return 0.5f;
            }

            if (lasgunShieldDetected)
            {
                LogInfo("Lasgun/Shield detected!");

                if (bestWeapon != null && !bestWeapon.IsLaser && MayPlayNoWeapon(bestDefense))
                {
                    bestWeapon = null;
                }

                chanceOfMyHeroSurviving = 0;
                chanceOfEnemyHeroSurviving = 0;
                return 0.5f;
            }

            if (hero is TreacheryCard && bestDefense != null && !bestDefense.IsUseless && MayPlayNoDefense(bestWeapon))
            {
                bestDefense = null;
            }

            LogInfo("Chance of my hero surviving: {0} with {1} (my weapon: {2})", chanceOfMyHeroSurviving, bestDefense, bestWeapon);

            var myHeroToFightAgainst = hero;
            var opponentLeader = (prescience != null && prescience.Aspect == PrescienceAspect.Leader && opponentPlan != null) ? opponentPlan.Hero : Battle.ValidBattleHeroes(Game, opponent).OrderByDescending(l => l.ValueInCombatAgainst(myHeroToFightAgainst)).FirstOrDefault(l => !Traitors.Contains(l));
            int opponentLeaderValue = opponentLeader == null ? 0 : opponentLeader.ValueInCombatAgainst(hero);

            int opponentMessiahBonus = Battle.MessiahAvailableForBattle(Game, opponent) ? 2 : 0;
            int maxReinforcements = takeReinforcementsIntoAccount ? (int)Math.Ceiling(MaxReinforcedDialTo(opponent, territory)) : 0;
            int myHeroValue = hero == null ? 0 : hero.Value;

            var opponentDial = (prescience != null && prescience.Aspect == PrescienceAspect.Dial && opponentPlan != null) ? opponentPlan.Dial(Game, Faction) : MaxDial(opponent, territory, Faction);

            var result = 
                opponentDial + 
                maxReinforcements + 
                (chanceOfEnemyHeroSurviving < Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) + 
                (iAmAggressor ? 0 : 0.5f) - 
                (chanceOfMyHeroSurviving < Param.Battle_MimimumChanceToAssumeMyLeaderSurvives ? 0 : 1) * (myHeroValue + myMessiahBonus);

            LogInfo("opponentDial ({0}) + maxReinforcements ({8}) + (chanceOfEnemyHeroSurviving ({7}) < Battle_MimimumChanceToAssumeEnemyHeroSurvives ({10}) ? 0 : 1) * (highestleader ({1}) + messiahbonus ({2})) + defenderpenalty ({3}) - (chanceOfMyHeroSurviving ({4}) < Battle_MimimumChanceToAssumeMyLeaderSurvives ({11}) ? 0 : 1) * (myHeroValue ({5}) + messiahbonus ({9}) = ({6}))",
                opponentDial,
                opponentLeaderValue,
                opponentMessiahBonus,
                (iAmAggressor ? 0 : 0.5f),
                chanceOfMyHeroSurviving,
                myHeroValue,
                result,
                chanceOfEnemyHeroSurviving,
                maxReinforcements,
                myMessiahBonus,
                Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives,
                Param.Battle_MimimumChanceToAssumeMyLeaderSurvives);

            return result;
        }

        private bool HasLasgunShield(TreacheryCard myWeapon, TreacheryCard myDefense, TreacheryCard enemyWeapon, TreacheryCard enemyDefense)
        {
            return
                (myWeapon != null && myWeapon.IsLaser && enemyDefense != null && enemyDefense.IsShield) ||
                (enemyWeapon != null && enemyWeapon.IsLaser && myDefense != null && myDefense.IsShield);
        }
    }

}
