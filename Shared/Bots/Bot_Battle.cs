/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared.Model;

public partial class Player
{
    protected virtual BattleClaimed DetermineBattleClaimed()
    {
        var territory = Game.BattleAboutToStart.Territory;
        var opponent = Game.GetPlayer(Game.BattleAboutToStart.Target);
        var pink = Game.GetPlayer(Faction.Pink);
        var pinkAlly = pink.AlliedPlayer;
        var pinkIsStrongest = GetDialNeeded(pinkAlly, territory, opponent, false) > GetDialNeeded(pink, territory, opponent, false);

        if (Faction == Faction.Pink)
            return new BattleClaimed(Game, Faction) { Passed = !pinkIsStrongest };
        return new BattleClaimed(Game, Faction) { Passed = pinkIsStrongest };
    }

    protected virtual SwitchedSkilledLeader DetermineSwitchedSkilledLeader()
    {
        var leaderToSwitch = Leaders.FirstOrDefault(l => INeedToSwitchThisLeader(l));

        if (leaderToSwitch != null) return new SwitchedSkilledLeader(Game, Faction);

        return null;
    }

    private bool INeedToSwitchThisLeader(Leader leader)
    {
        if (leader == null) return false;

        return
            (Game.IsInFrontOfShield(leader) && leader == DetermineBattlePlan(true, true)?.Hero) ||
            (Game.IsInFrontOfShield(leader) && Game.CurrentPhase == Phase.BattlePhase && Game.CurrentBattle != null && Game.CurrentBattle.IsInvolved(this) && Battle.ValidBattleHeroes(Game, this).Count() < 2) ||
            (!Game.IsInFrontOfShield(leader) && Game.CurrentPhase != Phase.BattlePhase);
    }

    public Dictionary<Location, Battalion> BattalionsIn(Territory territory)
    {
        var result = new Dictionary<Location, Battalion>();
        foreach (var kvp in ForcesInLocations.Where(kvp => kvp.Key.Territory == territory)) result.Add(kvp.Key, kvp.Value);
        return result;
    }

    public bool BattalionIn(Location location, out Battalion battalion)
    {
        return ForcesInLocations.TryGetValue(location, out battalion);
    }

    public Battalion BattalionIn(Location location)
    {
        if (BattalionIn(location, out var result))
            return result;
        return new Battalion(Faction, 0, 0, location);
    }

    protected virtual BattleInitiated DetermineBattleInitiated()
    {
        var battle = Battle.BattlesToBeFought(Game, this).OrderBy(b => MaxDial(Game.GetPlayer(b.Faction), b.Territory, this) - MaxDial(this, b.Territory, Game.GetPlayer(b.Faction))).FirstOrDefault();

        return new BattleInitiated(Game, Faction)
        {
            Target = battle.Faction,
            Territory = battle.Territory
        };
    }

    protected virtual TreacheryCalled DetermineTreacheryCalled()
    {
        if (!Game.CurrentBattle.IsAggressorOrDefender(this) && !TreacheryCalled.MayCallTreachery(Game, this))
            return null;
        return new TreacheryCalled(Game, Faction) { TraitorCalled = TreacheryCalled.MayCallTreachery(Game, this) };
    }

    protected virtual AuditCancelled DetermineAuditCancelled()
    {
        return new AuditCancelled(Game, Faction) { Cancelled = false };
    }

    protected virtual Audited DetermineAudited()
    {
        return new Audited(Game, Faction);
    }

    protected virtual BattleConcluded DetermineBattleConcluded()
    {
        var myBattleplan = Game.CurrentBattle.PlanOf(this);
        var opponent = Game.CurrentBattle.OpponentOf(this);

        var discarded = new List<TreacheryCard>();

        if (BattleConcluded.MayChooseToDiscardCards(Game))
        {
            if (myBattleplan.Weapon != null &&
                Has(myBattleplan.Weapon) &&
                myBattleplan.Weapon.Type == TreacheryCardType.Useless &&
                Faction != Faction.Brown &&
                !Game.SkilledAs(this, LeaderSkill.Warmaster) &&
                !Game.OwnsStronghold(Faction, Game.Map.TueksSietch))
                discarded.Add(myBattleplan.Weapon);

            if (myBattleplan.Defense != null &&
                Has(myBattleplan.Defense) &&
                myBattleplan.Defense.Type == TreacheryCardType.Useless &&
                Faction != Faction.Brown &&
                !Game.SkilledAs(this, LeaderSkill.Warmaster) &&
                !Game.OwnsStronghold(Faction, Game.Map.TueksSietch))
                discarded.Add(myBattleplan.Defense);
        }

        var kill = Game.BlackVictim != null && !Game.IsSkilled(Game.BlackVictim) && Game.BlackVictim.Value < 4;

        var replacedSpecialForces = BattleConcluded.ValidReplacementForceAmounts(Game, this).Max();

        IHero newTraitor = null;
        IHero toReplace = null;
        if (Game.TraitorsDeciphererCanLookAt.Any() && Game.DeciphererMayReplaceTraitor)
        {
            newTraitor = Game.TraitorsDeciphererCanLookAt.OrderByDescending(t => t.Value).FirstOrDefault(t => t is TreacheryCard || (t.Faction != Faction && Game.IsAlive(t)));

            if (newTraitor != null)
            {
                var replacable = BattleConcluded.ValidTraitorsToReplace(this);
                toReplace = replacable.FirstOrDefault(t => t.Faction == Faction);
                if (toReplace == null) toReplace = replacable.OrderBy(t => t.Value).FirstOrDefault(t => !(t is TreacheryCard) && !Game.IsAlive(t));
                if (toReplace == null) toReplace = replacable.OrderBy(t => t.Value).FirstOrDefault(t => !(t is TreacheryCard) && t.Value < newTraitor.Value);
            }
        }

        if (toReplace == null) newTraitor = null;

        return new BattleConcluded(Game, Faction)
        {
            DiscardedCards = discarded,
            StolenToken = opponent.TechTokens.FirstOrDefault(),
            Kill = kill,
            SpecialForceLossesReplaced = replacedSpecialForces,
            NewTraitor = newTraitor,
            TraitorToReplace = toReplace,
            AddExtraForce = BattleConcluded.MayAddExtraForce(Game, this)
        };
    }
    protected virtual Battle DetermineBattle()
    {
        return DetermineBattlePlan(true, false);
    }

    protected virtual Battle DetermineBattlePlan(bool waitForPrescience, bool includeLeaderInFrontOfShield)
    {
        LogInfo("DetermineBattle()");

        var opponent = Game.CurrentBattle.OpponentOf(this);
        var prescience = MyPrescience;

        if ((waitForPrescience && Prescience.MayUsePrescience(Game, this)) || (waitForPrescience && prescience != null && Game.CurrentBattle.PlanOf(opponent) == null)) return null; //enemy is not ready yet

        if (decidedShipmentAction == ShipmentDecision.DummyShipment)
        {
            LogInfo("I'm spending as little as possible on this fight because this is a dummy shipment");
            return ConstructLostBattleMinimizingLosses(opponent, null);
        }

        var forcesAvailable = Battle.MaxForces(Game, this, false);
        var specialForcesAvailable = Battle.MaxForces(Game, this, true);
        var voice = voicePlan != null && voicePlan.battle == Game.CurrentBattle ? voicePlan : null;

        var dialNeeded = GetDialNeededForBattle(
            this,
            IWillBeAggressorAgainst(opponent),
            opponent,
            Game.CurrentBattle.Territory,
            voice,
            prescience != null ? prescience.Aspect : PrescienceAspect.None,
            false,
            includeLeaderInFrontOfShield,
            out var defense,
            out var weapon,
            out var hero,
            out var messiah,
            out var isTraitor,
            out var lasgunShield,
            out var stoneBurner,
            out var bankerBoost,
            out _,
            out _);

        if (stoneBurner) dialNeeded = 0;

        LogInfo("AGAINST {0} in {1}, WITH {2} + {3} as WEAP + {4} as DEF, I need a force dial of {5}", opponent, Game.CurrentBattle.Territory, hero, weapon, defense, dialNeeded);

        var resourcesFromAlly = Ally == Faction.Brown ? Game.ResourcesYourAllyCanPay(this) : 0;
        var resourcesForBattle = Resources + resourcesFromAlly;

        var dialShortage = DetermineDialShortageForBattle(
            lasgunShield ? 0.5f : dialNeeded,
            opponent.Faction,
            forcesAvailable,
            specialForcesAvailable,
            resourcesForBattle - bankerBoost,
            Game.CurrentBattle.Territory,
            out var forcesAtFullStrength,
            out var forcesAtHalfStrength,
            out var specialForcesAtFullStrength,
            out var specialForcesAtHalfStrength);

        if (dialShortage <= 3 && (weapon == null || defense == null))
        {
            var reinforcements = Battle.ValidWeapons(Game, this, defense, hero, Game.CurrentBattle.Territory).FirstOrDefault(c => c.Type == TreacheryCardType.Reinforcements);
            if (reinforcements != null)
            {
                if (weapon == null) weapon = reinforcements;
                else defense = reinforcements;

                dialShortage -= 3;
            }
        }

        var predicted = WinWasPredictedByMeThisTurn(opponent.Faction);
        var totalForces = forcesAtFullStrength + forcesAtHalfStrength + specialForcesAtFullStrength + specialForcesAtHalfStrength;
        var minimizeSpendingsInThisLostFight = predicted || (isTraitor && !messiah) || (Resources + ResourcesFromAlly < 10 && totalForces <= 8 && dialShortage > Param.Battle_DialShortageThresholdForThrowing);

        if (!minimizeSpendingsInThisLostFight)
        {
            if (weapon == null && !MayUseUselessAsKarma && Faction != Faction.Brown) weapon = UselessAsWeapon(defense);
            if (defense == null && !MayUseUselessAsKarma && Faction != Faction.Brown) defense = UselessAsDefense(weapon);

            RemoveIllegalChoices(ref hero, ref weapon, ref defense, Game.CurrentBattle.Territory);

            AvoidLasgunShieldExplosion(ref weapon, ref defense);

            LogInfo("Leader: {0}, Weapon: {1}, Defense: {2}, Forces: {3} (supp) {4} (non-supp) {5} (spec supp) {6} (spec non-supp)", hero, weapon, defense, forcesAtFullStrength, forcesAtHalfStrength, specialForcesAtFullStrength, specialForcesAtHalfStrength);

            var cost = Battle.Cost(Game, this, forcesAtFullStrength, specialForcesAtFullStrength, out var _ );
            return new Battle(Game, Faction)
            {
                Hero = hero,
                Messiah = messiah,
                Forces = forcesAtFullStrength,
                ForcesAtHalfStrength = forcesAtHalfStrength,
                SpecialForces = specialForcesAtFullStrength,
                SpecialForcesAtHalfStrength = specialForcesAtHalfStrength,
                AllyContributionAmount = Math.Min(cost, Math.Min(resourcesFromAlly, Battle.MaxAllyResources(Game, this, forcesAtFullStrength, specialForcesAtFullStrength))),
                Defense = defense,
                Weapon = weapon,
                BankerBonus = bankerBoost
            };
        }

        LogInfo("I'm spending as little as possible on this fight: predicted:{0}, isTraitor:{1} && !messiah:{2}, Resources:{3} < 10 && totalForces:{4} < 10 && dialShortage:{5} >= dialShortageToAccept:{6}",
            predicted, isTraitor, messiah, Resources, totalForces, dialShortage, Param.Battle_DialShortageThresholdForThrowing);

        return ConstructLostBattleMinimizingLosses(opponent, Game.CurrentBattle.Territory);
    }

    private void UseDestructiveWeaponIfApplicable(bool enemyCanDefendPoisonTooth, ref float myHeroSurviving, ref float enemyHeroSurviving, ref TreacheryCard defense, ref TreacheryCard weapon)
    {
        if (weapon == null && myHeroSurviving < Param.Battle_MimimumChanceToAssumeMyLeaderSurvives && enemyHeroSurviving >= Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives)
        {
            weapon = Weapons(defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.Rockmelter);

            if (weapon == null) weapon = Weapons(defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.ArtilleryStrike);

            if (weapon == null && !enemyCanDefendPoisonTooth) weapon = Weapons(defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.PoisonTooth);

            if (weapon != null)
            {
                enemyHeroSurviving = 0;
                myHeroSurviving = 0;
            }

            if (weapon != null && defense != null && MayPlayNoDefense(weapon) &&
                ((weapon.Type == TreacheryCardType.PoisonTooth && defense.Type != TreacheryCardType.Chemistry) || (weapon.Type == TreacheryCardType.ArtilleryStrike && !defense.IsShield))) defense = null;
        }
    }

    private Battle ConstructLostBattleMinimizingLosses(Player opponent, Territory territory)
    {
        var lowestAvailableHero = Battle.ValidBattleHeroes(Game, this).FirstOrDefault(h => h is TreacheryCard);
        if (lowestAvailableHero == null) SelectHeroForBattle(opponent, false, true, false, null, null, out lowestAvailableHero, out _);

        var weapon = lowestAvailableHero == null || MayUseUselessAsKarma || Faction == Faction.Brown ? null : UselessAsWeapon(null);
        var defense = lowestAvailableHero == null || MayUseUselessAsKarma || Faction == Faction.Brown ? null : UselessAsDefense(weapon);

        RemoveIllegalChoices(ref lowestAvailableHero, ref weapon, ref defense, territory);

        TreacheryCard harassAndWithdraw = null;
        var harass = false;
        if (territory != null && AnyForcesIn(territory) >= 4)
        {
            if (weapon == null && Battle.ValidWeapons(Game, this, defense, lowestAvailableHero, territory).Any(c => c.Type == TreacheryCardType.HarassAndWithdraw))
            {
                harassAndWithdraw = TreacheryCards.First(tc => tc.Type == TreacheryCardType.HarassAndWithdraw);
                weapon = harassAndWithdraw;
                harass = true;
            }
            else if (defense == null && Battle.ValidDefenses(Game, this, weapon, territory).Any(c => c.Type == TreacheryCardType.HarassAndWithdraw))
            {
                harassAndWithdraw = TreacheryCards.First(tc => tc.Type == TreacheryCardType.HarassAndWithdraw);
                defense = harassAndWithdraw;
                harass = true;
            }
        }

        var messiah = lowestAvailableHero != null && Battle.MessiahMayBeUsedInBattle(Game, this);

        var strongholdFreeForces = Game.HasStrongholdAdvantage(Faction, StrongholdAdvantage.FreeResourcesForBattles, Game.CurrentBattle.Territory) ? 2 : 0;
        var specialAtFull = Math.Min(strongholdFreeForces, Battle.MaxForces(Game, this, true));
        var normalAtFull = Math.Min(strongholdFreeForces - specialAtFull, Battle.MaxForces(Game, this, false));
        
        if (Battle.MustPayForAnyForcesInBattle(Game, this) && (specialAtFull <= 0 || Battle.MustPayForSpecialForcesInBattle(Game, this)))
        {
            return new Battle(Game, Faction)
            {
                Hero = lowestAvailableHero,
                Forces = harass ? 0 : normalAtFull,
                ForcesAtHalfStrength = harass ? 0 : Battle.MaxForces(Game, this, false) - normalAtFull,
                SpecialForces = harass ? 0 : specialAtFull,
                SpecialForcesAtHalfStrength = harass ? 0 : Battle.MaxForces(Game, this, true) - specialAtFull,
                Defense = defense,
                Weapon = weapon,
                BankerBonus = 0,
                Messiah = messiah
            };
        }

        return new Battle(Game, Faction)
        {
            Hero = lowestAvailableHero,
            Forces = harass ? 0 : Battle.MaxForces(Game, this, false),
            ForcesAtHalfStrength = 0,
            SpecialForces = harass ? 0 : Battle.MaxForces(Game, this, true),
            SpecialForcesAtHalfStrength = 0,
            Defense = defense,
            Weapon = weapon,
            BankerBonus = 0,
            Messiah = messiah
        };
    }

    private void RemoveIllegalChoices(ref IHero hero, ref TreacheryCard weapon, ref TreacheryCard defense, Territory territory)
    {
        LogInfo($"Removing Illegal Choices: hero: {hero}, weapon: {weapon}, defense: {defense}...");

        for (var check = 0; check < 3; check++)
        {
            if (hero == null)
            {
                defense = null;
                weapon = null;
                LogInfo("Removed weapon and defense because no leader available");
            }

            var weapClairvoyance = RulingWeaponClairvoyanceForThisBattle;
            if (weapClairvoyance != null && !IsAllowedWithClairvoyance(weapClairvoyance, weapon, true))
            {
                weapon = Weapons(defense, hero, territory).FirstOrDefault(c => IsAllowedWithClairvoyance(weapClairvoyance, c, true));
                LogInfo($"Replaced weapon by: {weapon}");
            }

            var defClairvoyance = RulingDefenseClairvoyanceForThisBattle;
            if (defClairvoyance != null && !IsAllowedWithClairvoyance(defClairvoyance, defense, false))
            {
                defense = Defenses(weapon, territory).FirstOrDefault(c => IsAllowedWithClairvoyance(defClairvoyance, c, false));
                LogInfo($"Replaced defense by: {defense}");
            }

            if (weapon != null && defense == weapon && weapon.Type == TreacheryCardType.Chemistry)
            {
                LogInfo($"Removing illegal weapon: {weapon}");
                weapon = null;
            }

            if (weapon != null && defense == weapon && weapon.Type == TreacheryCardType.WeirdingWay)
            {
                LogInfo($"Removing illegal defense: {defense}");
                defense = null;
            }

            if (weapon == null && defense != null && defense.Type == TreacheryCardType.WeirdingWay)
            {
                LogInfo($"Removing illegal defense: {defense}");
                defense = null;
            }

            if (defense == null && weapon != null && weapon.Type == TreacheryCardType.Chemistry)
            {
                LogInfo($"Removing illegal weapon: {weapon}");
                weapon = null;
            }

            if (defense == weapon)
            {
                LogInfo($"Removing illegal defense: {defense}");
                defense = null;
            }

            if (!Battle.ValidWeapons(Game, this, defense, hero, territory, true).Contains(weapon))
            {
                weapon = Weapons(defense, hero, territory).FirstOrDefault(w => w.Type != TreacheryCardType.Chemistry);
                LogInfo($"Replaced weapon by: {weapon}");
            }

            if (!Battle.ValidDefenses(Game, this, weapon, territory, true).Contains(defense))
            {
                defense = Defenses(weapon, territory).FirstOrDefault(w => w.Type != TreacheryCardType.WeirdingWay);
                LogInfo($"Replaced defense by: {defense}");
            }
        }
    }

    private void AvoidLasgunShieldExplosion(ref TreacheryCard weapon, ref TreacheryCard defense)
    {
        if (weapon != null && weapon.Type == TreacheryCardType.Laser && defense != null && defense.IsShield)
        {
            if (MayPlayNoWeapon(defense))
                weapon = null;
            else
                defense = null;
        }
    }

    protected float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, Territory territory, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable)
    {
        return DetermineDialShortageForBattle(dialNeeded, opponent, forcesAvailable, specialForcesAvailable, resourcesAvailable, territory, out _, out _, out _, out _);
    }

    protected float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable, Territory territory,
        out int forcesAtFullStrength, out int forcesAtHalfStrength, out int specialForcesAtFullStrength, out int specialForcesAtHalfStrength)
    {
        var normalStrength = Battle.DetermineNormalForceStrength(Game, Faction);
        var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
        var strongholdBonus = Game.HasStrongholdAdvantage(Faction, StrongholdAdvantage.FreeResourcesForBattles, territory) ? 2 : 0;
        var spiceLeft = resourcesAvailable + strongholdBonus;
        var costPerForce = Battle.NormalForceCost(Game, this);
        var costPerSpecialForce = Battle.SpecialForceCost(Game, this);
        var numberOfForcesWithCunningBonus = Game.CurrentRedCunning != null && Game.CurrentRedCunning.Initiator == Faction ? 5 : 0;

        LogInfo("DetermineValidForcesInBattle: {0} {1} {2} {3}", dialNeeded, spiceLeft, costPerSpecialForce, costPerForce);

        if (Battle.MustPayForAnyForcesInBattle(Game, this))
        {
            specialForcesAtFullStrength = 0;
            while (dialNeeded > normalStrength && specialForcesAvailable >= 1 && spiceLeft >= costPerSpecialForce)
            {
                dialNeeded -= specialStrength;
                specialForcesAtFullStrength++;
                specialForcesAvailable--;
                spiceLeft -= costPerSpecialForce;
            }

            forcesAtFullStrength = 0;
            while (dialNeeded >= normalStrength && forcesAvailable >= 1 && spiceLeft >= costPerForce)
            {
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= specialStrength;
                }
                else
                {
                    dialNeeded -= normalStrength;
                }

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
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= 0.5f * specialStrength;
                }
                else
                {
                    dialNeeded -= 0.5f * normalStrength;
                }

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
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= specialStrength;
                }
                else
                {
                    dialNeeded -= normalStrength;
                }

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


    protected float ChanceOfMyLeaderSurviving(Player opponent, VoicePlan voicePlan, PrescienceAspect prescience, out TreacheryCard mostEffectiveDefense, TreacheryCard chosenWeapon)
    {
        if (!HeroesForBattle(opponent, true).Any())
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

        var availableDefenses = Defenses(chosenWeapon, Game.CurrentBattle?.Territory).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
        ).ToArray();

        if (chosenWeapon != null && chosenWeapon.IsArtillery)
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.IsShield);
            return 0;
        }

        if (chosenWeapon != null && chosenWeapon.IsRockMelter)
        {
            mostEffectiveDefense = null;
            return 0;
        }

        if (chosenWeapon != null && chosenWeapon.IsPoisonTooth)
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.Type == TreacheryCardType.Chemistry);
            return 0;
        }

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
        if (prescience == PrescienceAspect.Weapon && opponentPlan != null)
        {
            if (opponentPlan.Weapon == null || opponentPlan.Weapon.Type == TreacheryCardType.Useless)
            {
                if (MayPlayNoDefense(chosenWeapon))
                    mostEffectiveDefense = null;
                else
                    mostEffectiveDefense = Defenses(chosenWeapon, null).FirstOrDefault();

                return 1;
            }

            mostEffectiveDefense = availableDefenses.FirstOrDefault(d => opponentPlan.Weapon.CounteredBy(d, chosenWeapon));
            return mostEffectiveDefense != null ? 1 : 0;
        }

        var myClairvoyance = MyClairVoyanceAboutEnemyWeaponInCurrentBattle;
        if (myClairvoyance != null)
        {
            LogInfo("Clairvoyance detected!");

            if (myClairvoyance.Question.IsAbout(TreacheryCardType.Projectile))
            {
                if (myClairvoyance.Answer.IsYes)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsProjectileDefense);
                    return mostEffectiveDefense != null ? 1 : 0;
                }

                if (myClairvoyance.Answer.IsNo)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                    if (mostEffectiveDefense != null) return 0.5f;
                }
            }

            if (myClairvoyance.Question.IsAbout(TreacheryCardType.Poison))
            {
                if (myClairvoyance.Answer.IsYes)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                    return mostEffectiveDefense != null ? 1 : 0;
                }

                if (myClairvoyance.Answer.IsNo)
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
        var availableDefenses = Defenses(chosenWeapon, null).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
        ).ToArray();

        var defenseQuality = new ObjectCounter<TreacheryCard>();

        var unknownCards = CardsUnknownToMe;

        var bestDefenseAgainstUnknownCards = availableDefenses
            .LowestOrDefault(def => NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, def, chosenWeapon));

        foreach (var def in availableDefenses)
        {
            defenseQuality.Count(def);

            if (def == bestDefenseAgainstUnknownCards) defenseQuality.Count(def);

            foreach (var knownWeapon in knownEnemyWeapons)
                if (knownWeapon.CounteredBy(def, chosenWeapon))
                {
                    LogInfo("potentialWeapon " + knownWeapon + " is countered by " + def);
                    defenseQuality.Count2(def);
                }
        }

        mostEffectiveDefense = defenseQuality.Highest;

        var defenseToCheck = mostEffectiveDefense;

        if ((mostEffectiveDefense == null && knownEnemyWeapons.Any()) || knownEnemyWeapons.Any(w => !w.CounteredBy(defenseToCheck, chosenWeapon))) return 0;

        return 1 - ChanceOfAnUnknownOpponentCardKillingMyLeader(unknownCards, mostEffectiveDefense, opponent, chosenWeapon);
    }

    private float ChanceOfAnUnknownOpponentCardKillingMyLeader(List<TreacheryCard> unknownCards, TreacheryCard usedDefense, Player opponent, TreacheryCard chosenWeapon)
    {
        if (unknownCards.Count == 0) return 0;
        var numberOfUnknownWeaponsThatCouldKillMeWithThisDefense = NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, usedDefense, chosenWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownWeaponsThatCouldKillMeWithThisDefense, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private float ChanceOfAnUnknownOpponentCardSavingHisLeader(List<TreacheryCard> unknownCards, TreacheryCard usedWeapon, Player opponent)
    {
        if (unknownCards.Count == 0) return 0;
        var numberOfUnknownDefensesThatCouldCounterThisWeapon = NumberOfUnknownDefensesThatCouldCounterThisWeapon(unknownCards, usedWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownDefensesThatCouldCounterThisWeapon, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private double CumulativeChance(double a, double b, int length)
    {
        double result = 1;
        for (var i = 0; i < length; i++) result *= (a - i) / (b - i);
        return result;
    }

    private float NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(IEnumerable<TreacheryCard> unknownCards, TreacheryCard defense, TreacheryCard chosenWeapon)
    {
        return unknownCards.Count(c => c.IsWeapon && (defense == null || !c.CounteredBy(defense, chosenWeapon)));
    }

    private float NumberOfUnknownDefensesThatCouldCounterThisWeapon(IEnumerable<TreacheryCard> unknownCards, TreacheryCard weapon)
    {
        return unknownCards.Count(c => c.IsDefense && (weapon == null || weapon.CounteredBy(c, null)));
    }

    private ClairVoyanceQandA RulingWeaponClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
                                                                       (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    private ClairVoyanceQandA RulingDefenseClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
                                                                        (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    public Homeworld PrimaryHomeworld => Homeworlds.First();

    public bool HasTooManyCards => TreacheryCards.Count > MaximumNumberOfCards;

    protected float ChanceOfEnemyLeaderDying(Player opponent, VoicePlan voicePlan, PrescienceAspect prescience, out TreacheryCard mostEffectiveWeapon, out bool enemyCanDefendPoisonTooth)
    {
        enemyCanDefendPoisonTooth = false;

        if (!HeroesForBattle(opponent, true).Any())
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

        var availableWeapons = Weapons(null, null, null).Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth && w.Type != TreacheryCardType.Rockmelter && w.Type != TreacheryCardType.HarassAndWithdraw && w.Type != TreacheryCardType.Recruits)
            .OrderBy(w => NumberOfUnknownDefensesThatCouldCounterThisWeapon(CardsUnknownToMe, w)).ToArray();

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);

        var opponentMayBeUsingAWeapon = true;

        //Prescience available?
        if (prescience != PrescienceAspect.None && opponentPlan != null)
        {
            if (prescience == PrescienceAspect.Defense)
            {
                enemyCanDefendPoisonTooth = opponentPlan.Defense != null && opponentPlan.Defense.IsNonAntidotePoisonDefense;
                mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => opponentPlan.Defense == null || !w.CounteredBy(opponentPlan.Defense, null));

                return mostEffectiveWeapon != null ? 1f : 0f;
            }

            if (prescience == PrescienceAspect.Weapon && (opponentPlan.Weapon == null || opponentPlan.Weapon.IsUseless)) opponentMayBeUsingAWeapon = false;
        }

        var usefulWeapons = opponentMayBeUsingAWeapon ? availableWeapons : availableWeapons.Where(w => w.Type != TreacheryCardType.MirrorWeapon).ToArray();

        var knownEnemyDefenses = KnownOpponentDefenses(opponent);

        //Clairvoyance available?
        var myClairvoyance = MyClairVoyanceAboutEnemyDefenseInCurrentBattle;
        if (myClairvoyance != null)
        {
            if (myClairvoyance.Question.IsAbout(TreacheryCardType.ProjectileDefense))
            {
                if (Game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (Game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
            else if (myClairvoyance.Question.IsAbout(TreacheryCardType.PoisonDefense))
            {
                if (Game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (Game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
        }

        var unknownOpponentCards = OpponentCardsUnknownToMe(opponent);

        mostEffectiveWeapon = usefulWeapons.Where(w => !knownEnemyDefenses.Any(defense => w.CounteredBy(defense, null))).RandomOrDefault();

        if (mostEffectiveWeapon != null)
        {
            if (mostEffectiveWeapon.IsMirrorWeapon)
            {
                if (prescience == PrescienceAspect.Weapon && opponentPlan != null) return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(unknownOpponentCards, opponentPlan.Weapon, opponent);
            }
            else if (!unknownOpponentCards.Any())
            {
                return 1f;
            }
            else
            {
                return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(unknownOpponentCards, mostEffectiveWeapon, opponent);
            }
        }

        mostEffectiveWeapon = usefulWeapons.Where(w => !IsKnownToOpponent(opponent, w)).RandomOrDefault();

        if (mostEffectiveWeapon != null) return 0.5f;

        enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);

        return 0f;
    }

    private bool IsAllowedWithClairvoyance(ClairVoyanceQandA clairvoyance, TreacheryCard toUse, bool asWeapon)
    {
        var inScope = toUse != null && ClairVoyancePlayed.IsInScopeOf(asWeapon, toUse.Type, (TreacheryCardType)clairvoyance.Question.Parameter1);

        var answer = clairvoyance == null ||
                     clairvoyance.Answer.Answer == ClairVoyanceAnswer.Unknown ||
                     (clairvoyance.Answer.Answer == ClairVoyanceAnswer.Yes && toUse != null && inScope) ||
                     (clairvoyance.Answer.Answer == ClairVoyanceAnswer.No && (toUse == null || !inScope));

        //LogInfo("IsAllowedWithClairvoyance(): in scope: {0}, answer: {1}.", inScope, answer);

        return answer;
    }

    protected IEnumerable<IHero> HeroesForBattle(Player player, bool includeInFrontOfShield)
    {
        return Battle.ValidBattleHeroes(Game, player).Where(l => includeInFrontOfShield || !Game.IsInFrontOfShield(l));
    }

    protected int SelectHeroForBattle(Player opponent, bool highest, bool forfeit, bool messiahUsed, TreacheryCard weapon, TreacheryCard defense, out IHero hero, out bool isTraitor, bool includeInFrontOfShield = false)
    {
        isTraitor = false;

        var ally = Ally != Faction.None ? AlliedPlayer : null;
        var purple = Game.GetPlayer(Faction.Purple);

        var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : Array.Empty<IHero>();
        var revealedOrToldTraitorsByNonOpponents = Game.Players.Where(p => p != opponent && (p.Ally != Faction.Black || p.Faction != opponent.Ally)).SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors));
        var toldNonTraitorsByOpponent = opponent.Ally != Faction.Black ? opponent.ToldNonTraitors.AsEnumerable() : Array.Empty<IHero>();
        var knownNonTraitors = Traitors.Union(KnownNonTraitors).Union(knownNonTraitorsByAlly).Union(revealedOrToldTraitorsByNonOpponents).Union(toldNonTraitorsByOpponent);

        var knownTraitorsForOpponentsInBattle = Game.Players.
            Where(p => p == opponent || (p.Faction == Faction.Black && p.Faction == opponent.Ally)).SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors))
            .Union(Leaders.Where(l => Game.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && l.Faction == opponent.Faction));

        var hasUnknownTraitorsThatMightBeMine = Game.Players.
            Where(p => p != opponent && (p.Ally != Faction.Black || p.Faction != opponent.Ally)).
            SelectMany(p => p.Traitors.Where(l => !knownNonTraitors.Contains(l))).Any();

        var highestOpponentLeader = HeroesForBattle(opponent, true).OrderByDescending(l => l.Value).FirstOrDefault();
        var safeLeaders = HeroesForBattle(this, includeInFrontOfShield).Where(l => messiahUsed || !hasUnknownTraitorsThatMightBeMine || knownNonTraitors.Contains(l));

        IHero safeHero = null;
        IHero unsafeHero = null;

        if (forfeit)
        {
            safeHero = null;
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).Where(l => !safeLeaders.Contains(l)).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
            if (unsafeHero == null) unsafeHero = HeroesForBattle(this, includeInFrontOfShield).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else if (highest)
        {
            safeHero = safeLeaders.HighestOrDefault(l => l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).HighestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else
        {
            safeHero = safeLeaders.LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).OneOfLowestNOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader), 2);
        }

        if (safeHero == null ||
            (opponent.Faction != Faction.Black && !knownTraitorsForOpponentsInBattle.Contains(unsafeHero) && safeHero.ValueInCombatAgainst(highestOpponentLeader) < unsafeHero.ValueInCombatAgainst(highestOpponentLeader) - 2))
            hero = unsafeHero;
        else
            hero = safeHero;

        isTraitor = !messiahUsed && knownTraitorsForOpponentsInBattle.Contains(hero);

        var usedSkill = LeaderSkill.None;
        return hero != null ? hero.ValueInCombatAgainst(highestOpponentLeader) + Battle.DetermineSkillBonus(Game, this, hero, weapon, defense, Resources > 3 ? 3 : 0, ref usedSkill) : 0;
    }

    protected float GetDialNeeded(Player inBattle, Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
    {
        //var opponent = GetOpponentThatOccupies(territory);
        var voicePlan = Voice.MayUseVoice(Game, inBattle) ? inBattle.BestVoice(null, inBattle, opponent) : null;
        var strength = inBattle.MaxDial(opponent, territory, inBattle);
        var prescience = Prescience.MayUsePrescience(Game, inBattle) ? inBattle.BestPrescience(opponent, strength, PrescienceAspect.None, territory) : PrescienceAspect.None;

        //More could be done with the information obtained in the below call
        return GetDialNeededForBattle(inBattle, inBattle.IWillBeAggressorAgainst(opponent), opponent, territory, voicePlan, prescience, takeReinforcementsIntoAccount, true, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
    }

    protected float GetDialNeeded(Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
    {
        //More could be done with the information obtained in the below call
        return GetDialNeeded(this, territory, opponent, takeReinforcementsIntoAccount);
    }

    protected float GetDialNeededForBattle(
        Player inBattle, bool iAmAggressor, Player opponent, Territory territory, VoicePlan voicePlan, PrescienceAspect prescience, bool takeReinforcementsIntoAccount, bool includeInFrontOfShield,
        out TreacheryCard bestDefense, out TreacheryCard bestWeapon, out IHero hero, out bool messiah, out bool isTraitor, out bool lasgunShieldDetected, out bool stoneBurnerDetected, out int bankerBoost,
        out float chanceOfMyHeroSurviving, out float chanceOfEnemyHeroSurviving)
    {

        chanceOfEnemyHeroSurviving = 1 - inBattle.ChanceOfEnemyLeaderDying(opponent, voicePlan, prescience, out bestWeapon, out var enemyCanDefendPoisonTooth);

        LogInfo("Chance of enemy hero surviving: {0} with {1}", chanceOfEnemyHeroSurviving, bestWeapon);

        chanceOfMyHeroSurviving = inBattle.ChanceOfMyLeaderSurviving(opponent, voicePlan, prescience, out bestDefense, bestWeapon);

        inBattle.UseDestructiveWeaponIfApplicable(enemyCanDefendPoisonTooth, ref chanceOfMyHeroSurviving, ref chanceOfEnemyHeroSurviving, ref bestDefense, ref bestWeapon);

        var iAssumeMyLeaderWillDie = chanceOfMyHeroSurviving < inBattle.Param.Battle_MimimumChanceToAssumeMyLeaderSurvives;
        var iAssumeEnemyLeaderWillDie = chanceOfEnemyHeroSurviving < inBattle.Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives;

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
        lasgunShieldDetected = HasLasgunShield(
            bestWeapon,
            bestDefense,
            prescience == PrescienceAspect.Weapon && opponentPlan != null ? opponentPlan.Weapon : null,
            prescience == PrescienceAspect.Defense && opponentPlan != null ? opponentPlan.Defense : null);

        stoneBurnerDetected = (bestWeapon != null && bestWeapon.IsRockMelter) || (prescience == PrescienceAspect.Weapon && opponentPlan != null && opponentPlan.Weapon != null && opponentPlan.Weapon.IsRockMelter);

        var myMessiahBonus = 0;
        if (Battle.MessiahAvailableForBattle(Game, inBattle) && !lasgunShieldDetected)
        {
            messiah = true;
            myMessiahBonus = 2;
        }
        else
        {
            messiah = false;
        }

        isTraitor = false;
        var myHeroValue = inBattle.SelectHeroForBattle(opponent, !lasgunShieldDetected && !iAssumeMyLeaderWillDie, false, messiah, bestWeapon, bestDefense, out hero, out isTraitor, includeInFrontOfShield);

        var usedSkill = LeaderSkill.None;
        var opponentPenalty = Battle.DetermineSkillPenalty(Game, hero, opponent, ref usedSkill);

        bankerBoost = 0;
        if (hero == null)
        {
            messiah = false;
            myMessiahBonus = 0;
        }

        if (inBattle.CanOnlyUseTraitorsOrFacedancers(opponent))
        {
            LogInfo("Opponent leader only has traitors or facedancers to use!");
            bestWeapon = null;
            bestDefense = null;
            chanceOfMyHeroSurviving = 1;
            iAssumeMyLeaderWillDie = false;
            chanceOfEnemyHeroSurviving = 0;
            iAssumeEnemyLeaderWillDie = true;
            return 0.5f;
        }

        if (isTraitor)
        {
            LogInfo("My leader is a traitor: chanceOfMyHeroSurviving = 0");
            bestWeapon = null;
            bestDefense = null;
            chanceOfMyHeroSurviving = 0;
            iAssumeMyLeaderWillDie = true;
            chanceOfEnemyHeroSurviving = 1;
            iAssumeEnemyLeaderWillDie = false;
            return 99;
        }

        if (lasgunShieldDetected)
        {
            LogInfo("Lasgun/Shield detected!");

            if (bestWeapon != null && !bestWeapon.IsLaser && inBattle.MayPlayNoWeapon(bestDefense)) bestWeapon = null;

            chanceOfMyHeroSurviving = 0;
            iAssumeMyLeaderWillDie = true;
            chanceOfEnemyHeroSurviving = 0;
            iAssumeEnemyLeaderWillDie = true;
            return 0.5f;
        }

        if (Game.SkilledAs(hero, LeaderSkill.Banker) && !iAssumeMyLeaderWillDie) bankerBoost = Math.Min(inBattle.Resources, 3);

        if (hero is TreacheryCard && bestDefense != null && !bestDefense.IsUseless && inBattle.MayPlayNoDefense(bestWeapon)) bestDefense = null;

        LogInfo("Chance of my hero surviving: {0} with {1} (my weapon: {2})", chanceOfMyHeroSurviving, bestDefense, bestWeapon);

        var myHeroToFightAgainst = hero;
        var opponentLeader = prescience == PrescienceAspect.Leader && opponentPlan != null ? opponentPlan.Hero : inBattle.HeroesForBattle(opponent, true).OrderByDescending(l => l.ValueInCombatAgainst(myHeroToFightAgainst)).FirstOrDefault(l => !inBattle.Traitors.Contains(l));
        var opponentLeaderValue = opponentLeader == null ? 0 : opponentLeader.ValueInCombatAgainst(hero);
        var opponentMessiahBonus = Battle.MessiahAvailableForBattle(Game, opponent) ? 2 : 0;
        var maxReinforcements = takeReinforcementsIntoAccount ? (int)Math.Ceiling(inBattle.MaxReinforcedDialTo(opponent, territory)) : 0;
        var opponentDial = prescience == PrescienceAspect.Dial && opponentPlan != null ? opponentPlan.Dial(Game, inBattle.Faction) : inBattle.MaxDial(opponent, territory, inBattle);
        var myHomeworldBonus = GetHomeworldBattleContributionAndLasgunShieldLimit(territory);
        var opponentHomeworldBonus = opponent.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);

        var result =
            opponentDial +
            opponentHomeworldBonus +
            maxReinforcements +
            (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
            (iAmAggressor ? 0 : 0.5f) -
            (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus) -
            myHomeworldBonus;


        if (inBattle.MaxDial(inBattle, territory, opponent) - result >= 5)
        {
            //I think I only need a small fraction of available forces. Am I really sure amout this?

            if (!iAssumeMyLeaderWillDie && chanceOfMyHeroSurviving < 0.8) iAssumeMyLeaderWillDie = true;

            if (iAssumeEnemyLeaderWillDie && chanceOfEnemyHeroSurviving > 0.1) iAssumeEnemyLeaderWillDie = false;

            result =
                opponentDial +
                maxReinforcements +
                (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
                (iAmAggressor ? 0 : 0.5f) -
                (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus);
        }

        LogInfo("{13}/{14}: opponentDial ({0}) + maxReinforcements ({8}) + (chanceOfEnemyHeroSurviving ({7}) < Battle_MimimumChanceToAssumeEnemyHeroSurvives ({10}) ? 0 : 1) * (highestleader ({1}) + messiahbonus ({2})) + defenderpenalty ({3}) - (chanceOfMyHeroSurviving ({4}) < Battle_MimimumChanceToAssumeMyLeaderSurvives ({11}) ? 0 : 1) * (myHeroValue ({5}) + messiahbonus ({9}) + bankerBoost ({12}) => *{6}*)",
            opponentDial,
            opponentLeaderValue,
            opponentMessiahBonus,
            iAmAggressor ? 0 : 0.5f,
            chanceOfMyHeroSurviving,
            myHeroValue,
            result,
            chanceOfEnemyHeroSurviving,
            maxReinforcements,
            myMessiahBonus,
            inBattle.Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives,
            inBattle.Param.Battle_MimimumChanceToAssumeMyLeaderSurvives,
            bankerBoost,
            territory,
            opponent.Faction);

        return result;
    }

    private bool CanOnlyUseTraitorsOrFacedancers(Player player)
    {
        return player.Leaders.All(l =>
            (!player.MessiahAvailable && Traitors.Contains(l)) ||
            FaceDancers.Contains(l) ||
            (!player.MessiahAvailable && Ally == Faction.Black && AlliedPlayer.Traitors.Contains(l)) ||
            (Ally == Faction.Purple && AlliedPlayer.FaceDancers.Contains(l)));
    }


    private bool HasLasgunShield(TreacheryCard myWeapon, TreacheryCard myDefense, TreacheryCard enemyWeapon, TreacheryCard enemyDefense)
    {
        return
            (myWeapon != null && myWeapon.IsLaser && enemyDefense != null && enemyDefense.IsShield) ||
            (enemyWeapon != null && enemyWeapon.IsLaser && myDefense != null && myDefense.IsShield);
    }

    protected RockWasMelted DetermineRockWasMelted()
    {
        var outcome = Battle.DetermineBattleOutcome(Game.AggressorPlan, Game.DefenderPlan, Game.CurrentBattle.Territory, Game);
        LogInfo(outcome.GetMessage());
        return new RockWasMelted(Game, Faction) { Kill = outcome.Winner == this };
    }

    protected ResidualPlayed DetermineResidualPlayed()
    {
        return new ResidualPlayed(Game, Faction);
    }

    protected PortableAntidoteUsed DeterminePortableAntidoteUsed()
    {
        var opponent = Game.CurrentBattle.OpponentOf(this);
        var opponentPlan = Game.CurrentBattle.PlanOf(opponent);
        var myPlan = Game.CurrentBattle.PlanOf(this);
        var defense = TreacheryCards.FirstOrDefault(c => c.IsPortableAntidote);

        if (opponentPlan.Weapon != null && opponentPlan.Weapon.CounteredBy(defense, myPlan.Weapon)) return new PortableAntidoteUsed(Game, Faction);

        return null;
    }

    protected Thought DetermineThought()
    {
        var opponent = Game.CurrentBattle.OpponentOf(this);
        if (OpponentCardsUnknownToMe(opponent).Any())
        {
            var unknownWeapons = CardsUnknownToMe.Where(c => c.IsWeapon).OrderByDescending(c => CardQuality(c, opponent));
            if (unknownWeapons.Any()) return new Thought(Game, Faction) { Card = unknownWeapons.First() };
        }

        return null;
    }

    protected ThoughtAnswered DetermineThoughtAnswered()
    {
        return new ThoughtAnswered(Game, Faction) { Card = ThoughtAnswered.ValidCards(Game, this).LowestOrDefault(c => CardQuality(c, this)) };
    }

    protected HMSAdvantageChosen DetermineHMSAdvantageChosen()
    {
        var adv = StrongholdAdvantage.None;

        var plan = DetermineBattlePlan(false, false);
        if (adv == StrongholdAdvantage.None && !plan.HasPoison && !plan.HasAntidote) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.CountDefensesAsAntidote);
        if (adv == StrongholdAdvantage.None && Resources < 5) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.FreeResourcesForBattles);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.CollectResourcesForDial);
        if (adv == StrongholdAdvantage.None && !plan.HasUseless) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.CollectResourcesForUseless);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.FreeResourcesForBattles);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault(a => a == StrongholdAdvantage.WinTies);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, this).FirstOrDefault();

        return new HMSAdvantageChosen(Game, Faction) { Advantage = adv };
    }

    protected Retreat DetermineRetreat()
    {
        int forcesToRetreat = Retreat.MaxForces(Game, this);
        int specialForcesToRetreat = Retreat.MaxSpecialForces(Game, this);
        int maxForces = Retreat.MaxTotalForces(Game, this);
        while (forcesToRetreat + specialForcesToRetreat > maxForces)
        {
            if (forcesToRetreat > 0)
            {
                forcesToRetreat--;
            }
            else
            {
                specialForcesToRetreat--;
            }
        }
            
        var to = Retreat.ValidTargets(Game, this).Where(l => ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));

        if (to == null) to = Retreat.ValidTargets(Game, this).FirstOrDefault(l => l.IsProtectedFromStorm);
        if (to == null) to = Retreat.ValidTargets(Game, this).FirstOrDefault();

        if (forcesToRetreat > 0 || specialForcesToRetreat > 0)
        {
            return new Retreat(Game, Faction) { Location = to, Forces = forcesToRetreat, SpecialForces = specialForcesToRetreat };
        }

        return null;
    }

    protected LoserConcluded DetermineLoserConcluded()
    {
        var toKeep = LoserConcluded.CardsLoserMayKeep(Game).Where(c => CardQuality(c, this) > 2).OrderByDescending(c => CardQuality(c, this)).FirstOrDefault();
        return new LoserConcluded(Game, Faction) { KeptCard = toKeep, Assassinate = LoserConcluded.CanAssassinate(Game, this) };
    }

}