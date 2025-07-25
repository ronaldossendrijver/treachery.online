/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * Player program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. Player
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

public partial class ClassicBot
{
    protected virtual BattleClaimed DetermineBattleClaimed()
    {
        var territory = Game.BattleAboutToStart.Territory;
        var opponent = Game.GetPlayer(Game.BattleAboutToStart.Target);
        var pink = Game.GetPlayer(Faction.Pink);
        var pinkAlly = pink.AlliedPlayer;
        var pinkIsStrongest = GetDialNeeded(pinkAlly, territory, opponent, false) > GetDialNeeded(pink, territory, opponent, false);

        return Faction == Faction.Pink 
            ? new BattleClaimed(Game, Faction) { Passed = !pinkIsStrongest } 
            : new BattleClaimed(Game, Faction) { Passed = pinkIsStrongest };
    }

    protected virtual SwitchedSkilledLeader? DetermineSwitchedSkilledLeader()
    {
        var leaderToSwitch = Player.Leaders.FirstOrDefault(INeedToSwitchPlayerLeader);

        return leaderToSwitch != null 
            ? new SwitchedSkilledLeader(Game, Faction) 
            : null;
    }

    private bool INeedToSwitchPlayerLeader(Leader leader)
    {
        var heroToPlay = DetermineBattlePlan(true, true)?.Hero;
        if (heroToPlay == null) return false;
        
        return
            (Game.IsInFrontOfShield(leader) && leader == heroToPlay) ||
            (Game.IsInFrontOfShield(leader) && Game is { CurrentPhase: Phase.BattlePhase, CurrentBattle: not null } && Game.CurrentBattle.IsInvolved(Player) && Battle.ValidBattleHeroes(Game, Player).Count() < 2) ||
            (!Game.IsInFrontOfShield(leader) && Game.CurrentPhase != Phase.BattlePhase);
    }
    
    protected virtual BattleInitiated DetermineBattleInitiated()
    {
        var battle = Battle.BattlesToBeFought(Game, Player)
            .OrderBy(b => MaxDial(Game.GetPlayer(b.Faction), b.Territory, Player) - MaxDial(Player, b.Territory, Game.GetPlayer(b.Faction))).First();

        return new BattleInitiated(Game, Faction)
        {
            Target = battle.Faction,
            Territory = battle.Territory
        };
    }

    protected virtual TreacheryCalled? DetermineTreacheryCalled()
    {
        if (!Game.CurrentBattle.IsAggressorOrDefender(Player) && !TreacheryCalled.MayCallTreachery(Game, Player))
            return null;
        
        return new TreacheryCalled(Game, Faction) { TraitorCalled = TreacheryCalled.MayCallTreachery(Game, Player) };
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
        var myBattleplan = Game.CurrentBattle.PlanOf(Player);
        var opponent = Game.CurrentBattle.OpponentOf(Player);

        var discarded = new List<TreacheryCard>();

        if (BattleConcluded.MayChooseToDiscardCards(Game))
        {
            if (myBattleplan.Weapon != null &&
                Player.Has(myBattleplan.Weapon) &&
                myBattleplan.Weapon.Type == TreacheryCardType.Useless &&
                Faction != Faction.Brown &&
                !Game.SkilledAs(Player, LeaderSkill.Warmaster) &&
                !Game.OwnsStronghold(Faction, Game.Map.TueksSietch))
                discarded.Add(myBattleplan.Weapon);

            if (myBattleplan.Defense != null &&
                Player.Has(myBattleplan.Defense) &&
                myBattleplan.Defense.Type == TreacheryCardType.Useless &&
                Faction != Faction.Brown &&
                !Game.SkilledAs(Player, LeaderSkill.Warmaster) &&
                !Game.OwnsStronghold(Faction, Game.Map.TueksSietch))
                discarded.Add(myBattleplan.Defense);
        }

        var kill = Game.BlackVictim != null && !Game.IsSkilled(Game.BlackVictim) && Game.BlackVictim.Value < 4;

        var replacedSpecialForces = BattleConcluded.ValidReplacementForceAmounts(Game, Player).Max();

        IHero? newTraitor = null;
        IHero? toReplace = null;
        if (Game.TraitorsDeciphererCanLookAt.Any() && Game.DeciphererMayReplaceTraitor)
        {
            newTraitor = Game.TraitorsDeciphererCanLookAt.OrderByDescending(t => t.Value).FirstOrDefault(t => t is TreacheryCard || (t.Faction != Faction && Game.IsAlive(t)));

            if (newTraitor != null)
            {
                var replaceable = BattleConcluded.ValidTraitorsToReplace(Player).ToArray();
                toReplace = replaceable.FirstOrDefault(t => t.Faction == Faction) 
                            ?? replaceable.OrderBy(t => t.Value).FirstOrDefault(t => !(t is TreacheryCard) && !Game.IsAlive(t));
                
                toReplace ??= replaceable.OrderBy(t => t.Value)
                    .FirstOrDefault(t => !(t is TreacheryCard) && t.Value < newTraitor.Value);
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
            AddExtraForce = BattleConcluded.MayAddExtraForce(Game, Player)
        };
    }
    protected virtual Battle? DetermineBattle()
    {
        return DetermineBattlePlan(true, false);
    }

    protected virtual Battle? DetermineBattlePlan(bool waitForPrescience, bool includeLeaderInFrontOfShield)
    {
        LogInfo("DetermineBattle()");

        var opponent = Game.CurrentBattle.OpponentOf(Player);
        var prescience = MyPrescience;

        if ((waitForPrescience && Prescience.MayUsePrescience(Game, Player)) || (waitForPrescience && prescience != null && Game.CurrentBattle.PlanOf(opponent) == null)) return null; //enemy is not ready yet

        if (DecidedShipmentAction == ShipmentDecision.DummyShipment)
        {
            LogInfo("I'm spending as little as possible on Player fight because Player is a dummy shipment");
            return ConstructLostBattleMinimizingLosses(opponent, null);
        }

        var forcesAvailable = Battle.MaxForces(Game, Player, false);
        var specialForcesAvailable = Battle.MaxForces(Game, Player, true);
        var voice = VoicePlan != null && VoicePlan.Battle == Game.CurrentBattle ? VoicePlan : null;

        var dialNeeded = GetDialNeededForBattle(
            Game,
            Player,
            Param,
            IWillBeAggressorAgainst(opponent),
            opponent,
            Game.CurrentBattle.Territory,
            voice,
            prescience?.Aspect ?? PrescienceAspect.None,
            false,
            includeLeaderInFrontOfShield,
            out var defense,
            out var weapon,
            out var hero,
            out var messiah,
            out var isTraitor,
            out var lasgunShield,
            out var stoneBurner,
            out var bankerBoost);

        if (stoneBurner) dialNeeded = 0;

        LogInfo("AGAINST {0} in {1}, WITH {2} + {3} as WEAPON + {4} as DEF, I need a force dial of {5}", opponent, Game.CurrentBattle.Territory, hero, weapon, defense, dialNeeded);

        var resourcesFromAlly = Ally == Faction.Brown ? Game.ResourcesYourAllyCanPay(Player) : 0;
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
            var reinforcements = Battle.ValidWeapons(Game, Player, defense, hero, Game.CurrentBattle.Territory).FirstOrDefault(c => c.Type == TreacheryCardType.Reinforcements);
            if (reinforcements != null)
            {
                if (weapon == null) weapon = reinforcements;
                else defense = reinforcements;

                dialShortage -= 3;
            }
        }

        var predicted = WinWasPredictedByMeThisTurn(opponent.Faction);
        var totalForces = forcesAtFullStrength + forcesAtHalfStrength + specialForcesAtFullStrength + specialForcesAtHalfStrength;
        var minimizeSpendingInThisLostFight = predicted || (isTraitor && !messiah) || (Resources + ResourcesFromAlly < 10 && totalForces <= 8 && dialShortage > Param.Battle_DialShortageThresholdForThrowing);

        if (!minimizeSpendingInThisLostFight)
        {
            if (weapon == null && !MayUseUselessAsKarma && Faction != Faction.Brown) weapon = UselessAsWeapon(Game, Player, defense);
            if (defense == null && !MayUseUselessAsKarma && Faction != Faction.Brown) defense = UselessAsDefense(Game, Player, weapon);

            RemoveIllegalChoices(ref hero, ref weapon, ref defense, Game.CurrentBattle.Territory);

            AvoidLasgunShieldExplosion(ref weapon, ref defense);

            LogInfo("Leader: {0}, Weapon: {1}, Defense: {2}, Forces: {3} (supp) {4} (non-supp) {5} (spec supp) {6} (spec non-supp)", hero, weapon, defense, forcesAtFullStrength, forcesAtHalfStrength, specialForcesAtFullStrength, specialForcesAtHalfStrength);

            var cost = Battle.Cost(Game, Player, forcesAtFullStrength, specialForcesAtFullStrength, out var _ );
            return new Battle(Game, Faction)
            {
                Hero = hero,
                Messiah = messiah,
                Forces = forcesAtFullStrength,
                ForcesAtHalfStrength = forcesAtHalfStrength,
                SpecialForces = specialForcesAtFullStrength,
                SpecialForcesAtHalfStrength = specialForcesAtHalfStrength,
                AllyContributionAmount = Math.Min(cost, Math.Min(resourcesFromAlly, Battle.MaxAllyResources(Game, Player, forcesAtFullStrength, specialForcesAtFullStrength))),
                Defense = defense,
                Weapon = weapon,
                BankerBonus = bankerBoost
            };
        }

        LogInfo("I'm spending as little as possible on Player fight: predicted:{0}, isTraitor:{1} && !messiah:{2}, Resources:{3} < 10 && totalForces:{4} < 10 && dialShortage:{5} >= dialShortageToAccept:{6}",
            predicted, isTraitor, messiah, Resources, totalForces, dialShortage, Param.Battle_DialShortageThresholdForThrowing);

        return ConstructLostBattleMinimizingLosses(opponent, Game.CurrentBattle.Territory);
    }

    private static void UseDestructiveWeaponIfApplicable(Game game, Player player, BotParameters param, bool enemyCanDefendPoisonTooth, ref float myHeroSurviving, ref float enemyHeroSurviving, ref TreacheryCard? defense, ref TreacheryCard? weapon)
    {
        if (weapon != null || !(myHeroSurviving < param.Battle_MimimumChanceToAssumeMyLeaderSurvives) ||
            !(enemyHeroSurviving >= param.Battle_MimimumChanceToAssumeEnemyHeroSurvives)) return;
        
        weapon = Weapons(game, player, defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.Rockmelter) 
                 ?? Weapons(game, player, defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.ArtilleryStrike);

        if (weapon == null && !enemyCanDefendPoisonTooth) 
            weapon = Weapons(game, player, defense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.PoisonTooth);

        if (weapon != null)
        {
            enemyHeroSurviving = 0;
            myHeroSurviving = 0;
        }

        if (weapon != null && defense != null && MayPlayNoDefense(game, player, weapon) &&
            ((weapon.Type == TreacheryCardType.PoisonTooth && defense.Type != TreacheryCardType.Chemistry) || (weapon.Type == TreacheryCardType.ArtilleryStrike && !defense.IsShield))) defense = null;
    }

    private Battle ConstructLostBattleMinimizingLosses(Player opponent, Territory? territory)
    {
        var lowestAvailableHero = Battle.ValidBattleHeroes(Game, Player).FirstOrDefault(h => h is TreacheryCard);
        if (lowestAvailableHero == null) SelectHeroForBattle(Game, Player, opponent, false, true, false, null, null, out lowestAvailableHero, out _);

        var weapon = lowestAvailableHero == null || MayUseUselessAsKarma || Faction == Faction.Brown ? null : UselessAsWeapon(Game, Player, null);
        var defense = lowestAvailableHero == null || MayUseUselessAsKarma || Faction == Faction.Brown ? null : UselessAsDefense(Game, Player, weapon);

        RemoveIllegalChoices(ref lowestAvailableHero, ref weapon, ref defense, territory);

        var harass = false;
        if (Player.AnyForcesIn(territory) >= 4)
        {
            if (weapon == null && Battle.ValidWeapons(Game, Player, defense, lowestAvailableHero, territory).Any(c => c.Type == TreacheryCardType.HarassAndWithdraw))
            {
                weapon = Player.TreacheryCards.First(tc => tc.Type == TreacheryCardType.HarassAndWithdraw);
                harass = true;
            }
            else if (defense == null && Battle.ValidDefenses(Game, Player, weapon, territory).Any(c => c.Type == TreacheryCardType.HarassAndWithdraw))
            {
                defense = Player.TreacheryCards.First(tc => tc.Type == TreacheryCardType.HarassAndWithdraw);
                harass = true;
            }
        }

        var messiah = lowestAvailableHero != null && Battle.MessiahMayBeUsedInBattle(Game, Player);

        var strongholdFreeForces = Game.HasStrongholdAdvantage(Faction, StrongholdAdvantage.FreeResourcesForBattles, Game.CurrentBattle.Territory) ? 2 : 0;
        var specialAtFull = Math.Min(strongholdFreeForces, Battle.MaxForces(Game, Player, true));
        var normalAtFull = Math.Min(strongholdFreeForces - specialAtFull, Battle.MaxForces(Game, Player, false));
        
        if (Battle.MustPayForAnyForcesInBattle(Game, Player) && (specialAtFull <= 0 || Battle.MustPayForSpecialForcesInBattle(Game, Player)))
        {
            return new Battle(Game, Faction)
            {
                Hero = lowestAvailableHero,
                Forces = harass ? 0 : normalAtFull,
                ForcesAtHalfStrength = harass ? 0 : Battle.MaxForces(Game, Player, false) - normalAtFull,
                SpecialForces = harass ? 0 : specialAtFull,
                SpecialForcesAtHalfStrength = harass ? 0 : Battle.MaxForces(Game, Player, true) - specialAtFull,
                Defense = defense,
                Weapon = weapon,
                BankerBonus = 0,
                Messiah = messiah
            };
        }

        return new Battle(Game, Faction)
        {
            Hero = lowestAvailableHero,
            Forces = harass ? 0 : Battle.MaxForces(Game, Player, false),
            ForcesAtHalfStrength = 0,
            SpecialForces = harass ? 0 : Battle.MaxForces(Game, Player, true),
            SpecialForcesAtHalfStrength = 0,
            Defense = defense,
            Weapon = weapon,
            BankerBonus = 0,
            Messiah = messiah
        };
    }

    private void RemoveIllegalChoices(ref IHero? hero, ref TreacheryCard? weapon, ref TreacheryCard? defense, Territory? territory)
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

            var weaponClairvoyance = RulingWeaponClairvoyanceForPlayerBattle;
            if (weaponClairvoyance != null && !IsAllowedWithClairvoyance(weaponClairvoyance, weapon, true))
            {
                weapon = Weapons(Game, Player, defense, hero, territory).FirstOrDefault(c => IsAllowedWithClairvoyance(weaponClairvoyance, c, true));
                LogInfo($"Replaced weapon by: {weapon}");
            }

            var defClairvoyance = RulingDefenseClairvoyanceForPlayerBattle;
            if (defClairvoyance != null && !IsAllowedWithClairvoyance(defClairvoyance, defense, false))
            {
                defense = Defenses(Game, Player, weapon, territory).FirstOrDefault(c => IsAllowedWithClairvoyance(defClairvoyance, c, false));
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

            if (weapon == null && defense is { Type: TreacheryCardType.WeirdingWay })
            {
                LogInfo($"Removing illegal defense: {defense}");
                defense = null;
            }

            if (defense == null && weapon is { Type: TreacheryCardType.Chemistry })
            {
                LogInfo($"Removing illegal weapon: {weapon}");
                weapon = null;
            }

            if (defense == weapon)
            {
                LogInfo($"Removing illegal defense: {defense}");
                defense = null;
            }

            if (!Battle.ValidWeapons(Game, Player, defense, hero, territory, true).Contains(weapon))
            {
                weapon = Weapons(Game, Player, defense, hero, territory).FirstOrDefault(w => w.Type != TreacheryCardType.Chemistry);
                LogInfo($"Replaced weapon by: {weapon}");
            }

            if (!Battle.ValidDefenses(Game, Player, weapon, territory, true).Contains(defense))
            {
                defense = Defenses(Game, Player, weapon, territory).FirstOrDefault(w => w.Type != TreacheryCardType.WeirdingWay);
                LogInfo($"Replaced defense by: {defense}");
            }
        }
    }

    private void AvoidLasgunShieldExplosion(ref TreacheryCard? weapon, ref TreacheryCard? defense)
    {
        if (weapon is { Type: TreacheryCardType.Laser } && defense is { IsShield: true })
        {
            if (MayPlayNoWeapon(Player, defense))
                weapon = null;
            else
                defense = null;
        }
    }

    private float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, Territory territory, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable) 
        => DetermineDialShortageForBattle(dialNeeded, opponent, forcesAvailable, specialForcesAvailable, resourcesAvailable, territory, out _, out _, out _, out _);

    private float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable, Territory territory,
        out int forcesAtFullStrength, out int forcesAtHalfStrength, out int specialForcesAtFullStrength, out int specialForcesAtHalfStrength)
    {
        var normalStrength = Battle.DetermineNormalForceStrength(Game, Faction);
        var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
        var strongholdBonus = Game.HasStrongholdAdvantage(Faction, StrongholdAdvantage.FreeResourcesForBattles, territory) ? 2 : 0;
        var spiceLeft = resourcesAvailable + strongholdBonus;
        var costPerForce = Battle.NormalForceCost(Game, Player);
        var costPerSpecialForce = Battle.SpecialForceCost(Game, Player);
        var numberOfForcesWithCunningBonus = Game.CurrentRedCunning != null && Game.CurrentRedCunning.Initiator == Faction ? 5 : 0;

        LogInfo("DetermineValidForcesInBattle: {0} {1} {2} {3}", dialNeeded, spiceLeft, costPerSpecialForce, costPerForce);

        if (Battle.MustPayForAnyForcesInBattle(Game, Player))
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


    private static float ChanceOfMyLeaderSurviving(Game game, Player player, Player opponent, VoicePlan? voicePlan, PrescienceAspect? prescience, out TreacheryCard? mostEffectiveDefense, TreacheryCard? chosenWeapon)
    {
        if (!HeroesForBattle(game, opponent, true).Any())
        {
            LogInfo("Opponent has no leaders");
            mostEffectiveDefense = null;
            return 1;
        }

        if (voicePlan is { DefenseToUse: not null })
        {
            mostEffectiveDefense = voicePlan.DefenseToUse;
            return voicePlan.PlayerHeroWillCertainlySurvive ? 1 : 0.5f;
        }

        var availableDefenses = Defenses(game, player, chosenWeapon, game.CurrentBattle?.Territory).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
        ).ToArray();

        if (chosenWeapon is { IsArtillery: true })
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.IsShield);
            return 0;
        }

        if (chosenWeapon is { IsRockMelter: true })
        {
            mostEffectiveDefense = null;
            return 0;
        }

        if (chosenWeapon is { IsPoisonTooth: true })
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.Type == TreacheryCardType.Chemistry);
            return 0;
        }

        var opponentPlan = game.CurrentBattle?.PlanOf(opponent);
        if (prescience == PrescienceAspect.Weapon && opponentPlan != null)
        {
            if (opponentPlan.Weapon == null || opponentPlan.Weapon.Type == TreacheryCardType.Useless)
            {
                mostEffectiveDefense = MayPlayNoDefense(game, player, chosenWeapon) ? null : Defenses(game, player, chosenWeapon, null).FirstOrDefault();
                return 1;
            }

            mostEffectiveDefense = availableDefenses.FirstOrDefault(d => opponentPlan.Weapon.CounteredBy(d, chosenWeapon));
            return mostEffectiveDefense != null ? 1 : 0;
        }

        var myClairvoyance = PlayerClairvoyanceAboutEnemyWeaponInCurrentBattle(game, player);
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

        return DetermineBestDefense(game, player, opponent, chosenWeapon, out mostEffectiveDefense);
    }

    private static int NrOfUnknownOpponentCards(Game game, Player player, Player opponent)
    {
        return opponent.TreacheryCards.Count(c => !game.KnownCards(player).Contains(c));
    }

    private static float DetermineBestDefense(Game game, Player player, Player opponent, TreacheryCard? chosenWeapon, out TreacheryCard mostEffectiveDefense)
    {
        var knownEnemyWeapons = KnownOpponentWeapons(game, player, opponent).ToArray();
        var availableDefenses = Defenses(game, player, chosenWeapon, null).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
        ).ToArray();

        var defenseQuality = new ObjectCounter<TreacheryCard>();

        var unknownCards = CardsUnknownToPlayer(game, player);

        var bestDefenseAgainstUnknownCards = availableDefenses
            .LowestOrDefault(def => NumberOfUnknownWeaponsThatCouldKillMeWithPlayerDefense(unknownCards, def, chosenWeapon));

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

        return 1 - ChanceOfAnUnknownOpponentCardKillingMyLeader(game, player, unknownCards, mostEffectiveDefense, opponent, chosenWeapon);
    }

    private static float ChanceOfAnUnknownOpponentCardKillingMyLeader(Game game, Player player, List<TreacheryCard> unknownCards, TreacheryCard? usedDefense, Player opponent, TreacheryCard? chosenWeapon)
    {
        if (unknownCards.Count == 0) return 0;
        var numberOfUnknownWeaponsThatCouldKillMeWithPlayerDefense = NumberOfUnknownWeaponsThatCouldKillMeWithPlayerDefense(unknownCards, usedDefense, chosenWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(game, player, opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownWeaponsThatCouldKillMeWithPlayerDefense, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private static float ChanceOfAnUnknownOpponentCardSavingHisLeader(Game game, Player player, List<TreacheryCard> unknownCards, TreacheryCard? usedWeapon, Player opponent)
    {
        if (unknownCards.Count == 0) return 0;
        var numberOfUnknownDefensesThatCouldCounterPlayerWeapon = NumberOfUnknownDefensesThatCouldCounterPlayerWeapon(unknownCards, usedWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(game, player, opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownDefensesThatCouldCounterPlayerWeapon, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private static double CumulativeChance(double a, double b, int length)
    {
        double result = 1;
        for (var i = 0; i < length; i++) result *= (a - i) / (b - i);
        return result;
    }

    private static float NumberOfUnknownWeaponsThatCouldKillMeWithPlayerDefense(IEnumerable<TreacheryCard> unknownCards, TreacheryCard? defense, TreacheryCard? chosenWeapon)
    {
        return unknownCards.Count(c => c.IsWeapon && (defense == null || !c.CounteredBy(defense, chosenWeapon)));
    }

    private static float NumberOfUnknownDefensesThatCouldCounterPlayerWeapon(IEnumerable<TreacheryCard> unknownCards, TreacheryCard? weapon)
    {
        return unknownCards.Count(c => c.IsDefense && (weapon == null || weapon.CounteredBy(c, null)));
    }

    private ClairVoyanceQandA? RulingWeaponClairvoyanceForPlayerBattle => Game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
                                                                          (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    private ClairVoyanceQandA? RulingDefenseClairvoyanceForPlayerBattle => Game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
                                                                           (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;


    private static float ChanceOfEnemyLeaderDying(Game game, Player player, Player opponent, VoicePlan? voice, PrescienceAspect? prescience, out TreacheryCard? mostEffectiveWeapon, out bool enemyCanDefendPoisonTooth)
    {
        enemyCanDefendPoisonTooth = false;

        if (!HeroesForBattle(game, opponent, true).Any())
        {
            LogInfo("Opponent has no leaders");
            mostEffectiveWeapon = null;
            return 0f;
        }

        if (voice is { WeaponToUse: not null })
        {
            mostEffectiveWeapon = voice.WeaponToUse;
            return voice.OpponentHeroWillCertainlyBeZero ? 1 : 0.5f;
        }

        var availableWeapons = Weapons(game, player, null, null, null)
            .Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth && w.Type != TreacheryCardType.Rockmelter && w.Type != TreacheryCardType.HarassAndWithdraw && w.Type != TreacheryCardType.Recruits)
            .OrderBy(w => NumberOfUnknownDefensesThatCouldCounterPlayerWeapon(CardsUnknownToPlayer(game, player), w)).ToArray();

        var opponentPlan = game.CurrentBattle?.PlanOf(opponent);

        var opponentMayBeUsingAWeapon = true;

        //Prescience available?
        if (prescience != PrescienceAspect.None && opponentPlan != null)
        {
            if (prescience == PrescienceAspect.Defense)
            {
                enemyCanDefendPoisonTooth = opponentPlan.Defense is { IsNonAntidotePoisonDefense: true };
                mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => opponentPlan.Defense == null || !w.CounteredBy(opponentPlan.Defense, null));

                return mostEffectiveWeapon != null ? 1f : 0f;
            }

            if (prescience == PrescienceAspect.Weapon && (opponentPlan.Weapon == null || opponentPlan.Weapon.IsUseless)) opponentMayBeUsingAWeapon = false;
        }

        var usefulWeapons = opponentMayBeUsingAWeapon ? availableWeapons : availableWeapons.Where(w => w.Type != TreacheryCardType.MirrorWeapon).ToArray();

        var knownEnemyDefenses = KnownOpponentDefenses(game, player, opponent);

        //Clairvoyance available?
        var myClairvoyance = MyClairvoyanceAboutEnemyDefenseInCurrentBattle(game, player);
        if (myClairvoyance != null)
        {
            if (myClairvoyance.Question.IsAbout(TreacheryCardType.ProjectileDefense))
            {
                if (game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
            else if (myClairvoyance.Question.IsAbout(TreacheryCardType.PoisonDefense))
            {
                if (game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
        }

        var unknownOpponentCards = OpponentCardsUnknownToPlayer(game, player, opponent);

        mostEffectiveWeapon = usefulWeapons.Where(w => !knownEnemyDefenses.Any(defense => w.CounteredBy(defense, null))).RandomOrDefault();

        if (mostEffectiveWeapon != null)
        {
            if (mostEffectiveWeapon.IsMirrorWeapon)
            {
                if (prescience == PrescienceAspect.Weapon && opponentPlan != null) 
                    return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(game, player, unknownOpponentCards, opponentPlan.Weapon, opponent);
            }
            else if (!unknownOpponentCards.Any())
            {
                return 1f;
            }
            else
            {
                return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(game, player, unknownOpponentCards, mostEffectiveWeapon, opponent);
            }
        }

        mostEffectiveWeapon = usefulWeapons.Where(w => !IsKnownToOpponent(game, opponent, w)).RandomOrDefault();

        if (mostEffectiveWeapon != null) return 0.5f;

        enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);

        return 0f;
    }

    private bool IsAllowedWithClairvoyance(ClairVoyanceQandA? clairvoyance, TreacheryCard? toUse, bool asWeapon)
    {
        var inScope = toUse != null && clairvoyance != null && ClairVoyancePlayed.IsInScopeOf(asWeapon, toUse.Type, (TreacheryCardType)clairvoyance.Question.Parameter1);

        var answer = clairvoyance == null ||
                     clairvoyance.Answer.Answer == ClairVoyanceAnswer.Unknown ||
                     (clairvoyance.Answer.Answer == ClairVoyanceAnswer.Yes && toUse != null && inScope) ||
                     (clairvoyance.Answer.Answer == ClairVoyanceAnswer.No && (toUse == null || !inScope));

        return answer;
    }

    private static IEnumerable<IHero> HeroesForBattle(Game game, Player player, bool includeInFrontOfShield) 
        => Battle.ValidBattleHeroes(game, player).Where(l => includeInFrontOfShield || !game.IsInFrontOfShield(l));

    private static int SelectHeroForBattle(Game game, Player player, Player opponent, bool highest, bool forfeit, bool messiahUsed, 
        TreacheryCard? weapon, TreacheryCard? defense, out IHero? hero, out bool isTraitor, bool includeInFrontOfShield = false)
    {
        isTraitor = false;

        var ally = player.Ally != Faction.None ? player.AlliedPlayer : null;

        var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : [];
        var revealedOrToldTraitorsByNonOpponents = game.Players.Where(x => x != opponent && (x.Ally != Faction.Black || x.Faction != opponent.Ally)).SelectMany(x => x.RevealedTraitors.Union(x.ToldTraitors));
        var toldNonTraitorsByOpponent = opponent.Ally != Faction.Black ? opponent.ToldNonTraitors.AsEnumerable() : [];
        var knownNonTraitors = player.Traitors.Union(player.KnownNonTraitors).Union(knownNonTraitorsByAlly).Union(revealedOrToldTraitorsByNonOpponents).Union(toldNonTraitorsByOpponent);

        var knownTraitorsForOpponentsInBattle = game.Players.
            Where(x => x == opponent || (x.Faction == Faction.Black && x.Faction == opponent.Ally)).SelectMany(x => x.RevealedTraitors.Union(player.ToldTraitors))
            .Union(player.Leaders.Where(l => game.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && l.Faction == opponent.Faction)).ToArray();

        var hasUnknownTraitorsThatMightBeMine = game.Players.
            Where(x => x != opponent && (x.Ally != Faction.Black || x.Faction != opponent.Ally)).
            SelectMany(x => x.Traitors.Where(l => !knownNonTraitors.Contains(l))).Any();

        var highestOpponentLeader = HeroesForBattle(game, opponent, true).OrderByDescending(l => l.Value).FirstOrDefault();
        var safeLeaders = HeroesForBattle(game, player, includeInFrontOfShield).Where(l => messiahUsed || !hasUnknownTraitorsThatMightBeMine || knownNonTraitors.Contains(l));

        IHero? safeHero;
        IHero? unsafeHero;

        if (forfeit)
        {
            safeHero = null;
            unsafeHero = HeroesForBattle(game, player, includeInFrontOfShield)
                             .Where(l => !safeLeaders.Contains(l)).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader)) ??
                         HeroesForBattle(game, player, includeInFrontOfShield).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else if (highest)
        {
            safeHero = safeLeaders.HighestOrDefault(l => l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(game, player, includeInFrontOfShield).HighestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else
        {
            safeHero = safeLeaders.LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(game, player, includeInFrontOfShield).OneOfLowestNOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader), 2);
        }

        if (safeHero == null ||
            (opponent.Faction != Faction.Black && !knownTraitorsForOpponentsInBattle.Contains(unsafeHero) && safeHero.ValueInCombatAgainst(highestOpponentLeader) < unsafeHero.ValueInCombatAgainst(highestOpponentLeader) - 2))
            hero = unsafeHero;
        else
            hero = safeHero;

        isTraitor = !messiahUsed && knownTraitorsForOpponentsInBattle.Contains(hero);

        var usedSkill = LeaderSkill.None;
        return hero != null ? hero.ValueInCombatAgainst(highestOpponentLeader) + Battle.DetermineSkillBonus(game, player, hero, weapon, defense, player.Resources > 3 ? 3 : 0, ref usedSkill) : 0;
    }

    private float GetDialNeeded(Player p, Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
    {
        //var opponent = GetOpponentThatOccupies(territory);
        var voicePlan = Voice.MayUseVoice(Game, p) ? BestVoice(null, p, opponent) : null;
        var strength = MaxDial(opponent, territory, p);
        var prescience = Prescience.MayUsePrescience(Game, p) ? BestPrescience(p, opponent, strength, PrescienceAspect.None, territory) : PrescienceAspect.None;

        //More could be done with the information obtained in the below call
        return GetDialNeededForBattle(game, player, param, IWillBeAggressorAgainst(opponent), opponent, territory, 
            voicePlan, prescience, takeReinforcementsIntoAccount, true, 
            out _, out _, out _, out _, out _, out _, out _, out _);
    }

    private float GetDialNeeded(Territory territory, Player? opponent, bool takeReinforcementsIntoAccount)
    {
        if (opponent == null) return 0;
        //More could be done with the information obtained in the below call
        return GetDialNeeded(Player, territory, opponent, takeReinforcementsIntoAccount);
    }

    private static float GetDialNeededForBattle(
        Game game, Player player, BotParameters param, bool iAmAggressor, Player opponent, Territory territory, VoicePlan? voice, PrescienceAspect prescience, bool takeReinforcementsIntoAccount, bool includeInFrontOfShield,
        out TreacheryCard? bestDefense, out TreacheryCard? bestWeapon, out IHero? hero, out bool messiah, out bool isTraitor, out bool lasgunShieldDetected, out bool stoneBurnerDetected, out int bankerBoost)
    {
        var chanceOfEnemyHeroSurviving = 1 - ChanceOfEnemyLeaderDying(game, player, opponent, voice, prescience, out bestWeapon, out var enemyCanDefendPoisonTooth);

        LogInfo("Chance of enemy hero surviving: {0} with {1}", chanceOfEnemyHeroSurviving, bestWeapon);

        var chanceOfMyHeroSurviving = ChanceOfMyLeaderSurviving(game, player, opponent, voice, prescience, out bestDefense, bestWeapon);

        UseDestructiveWeaponIfApplicable(game, player, param, enemyCanDefendPoisonTooth, ref chanceOfMyHeroSurviving, ref chanceOfEnemyHeroSurviving, ref bestDefense, ref bestWeapon);

        var iAssumeMyLeaderWillDie = chanceOfMyHeroSurviving < param.Battle_MimimumChanceToAssumeMyLeaderSurvives;
        var iAssumeEnemyLeaderWillDie = chanceOfEnemyHeroSurviving < param.Battle_MimimumChanceToAssumeEnemyHeroSurvives;

        var opponentPlan = game.CurrentBattle?.PlanOf(opponent);
        lasgunShieldDetected = HasLasgunShield(
            bestWeapon,
            bestDefense,
            prescience == PrescienceAspect.Weapon && opponentPlan != null ? opponentPlan.Weapon : null,
            prescience == PrescienceAspect.Defense && opponentPlan != null ? opponentPlan.Defense : null);

        stoneBurnerDetected = bestWeapon is { IsRockMelter: true } || (prescience == PrescienceAspect.Weapon && opponentPlan is
        {
            Weapon.IsRockMelter: true
        });

        var myMessiahBonus = 0;
        if (Battle.MessiahAvailableForBattle(game, player) && !lasgunShieldDetected)
        {
            messiah = true;
            myMessiahBonus = 2;
        }
        else
        {
            messiah = false;
        }

        isTraitor = false;
        var myHeroValue = SelectHeroForBattle(game, player, opponent, !lasgunShieldDetected && !iAssumeMyLeaderWillDie, false, messiah, bestWeapon, bestDefense, out hero, out isTraitor, includeInFrontOfShield);

        var usedSkill = LeaderSkill.None;
        var opponentPenalty = Battle.DetermineSkillPenalty(game, hero, opponent, ref usedSkill);

        bankerBoost = 0;
        if (hero == null)
        {
            messiah = false;
            myMessiahBonus = 0;
        }

        if (CanOnlyUseTraitorsOrFacedancers(opponent))
        {
            LogInfo("Opponent leader only has traitors or facedancers to use!");
            bestWeapon = null;
            bestDefense = null;
            return 0.5f;
        }

        if (isTraitor)
        {
            LogInfo("My leader is a traitor: chanceOfMyHeroSurviving = 0");
            bestWeapon = null;
            bestDefense = null;
            return 99;
        }

        if (lasgunShieldDetected)
        {
            LogInfo("Lasgun/Shield detected!");
            if (bestWeapon is { IsLaser: false } && MayPlayNoWeapon(game, player, bestDefense)) bestWeapon = null;
            return 0.5f;
        }

        if (Game.SkilledAs(hero, LeaderSkill.Banker) && !iAssumeMyLeaderWillDie) bankerBoost = Math.Min(player.Resources, 3);

        if (hero is TreacheryCard && bestDefense is { IsUseless: false } && MayPlayNoDefense(game, player, bestWeapon)) bestDefense = null;

        LogInfo("Chance of my hero surviving: {0} with {1} (my weapon: {2})", chanceOfMyHeroSurviving, bestDefense, bestWeapon);

        var myHeroToFightAgainst = hero;
        var opponentLeader = prescience == PrescienceAspect.Leader && opponentPlan != null ? opponentPlan.Hero : HeroesForBattle(opponent, true).OrderByDescending(l => l.ValueInCombatAgainst(myHeroToFightAgainst)).FirstOrDefault(l => !player.Traitors.Contains(l));
        var opponentLeaderValue = opponentLeader?.ValueInCombatAgainst(hero) ?? 0;
        var opponentMessiahBonus = Battle.MessiahAvailableForBattle(game, opponent) ? 2 : 0;
        var maxReinforcements = takeReinforcementsIntoAccount ? (int)Math.Ceiling(MaxReinforcedDialTo(game, player, opponent, territory)) : 0;
        var opponentDial = prescience == PrescienceAspect.Dial && opponentPlan != null ? opponentPlan.Dial(game, player.Faction) : MaxDial(opponent, territory, player);
        var myHomeworldBonus = Player.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);
        var opponentHomeworldBonus = opponent.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);

        var result =
            opponentDial +
            opponentHomeworldBonus +
            maxReinforcements +
            (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
            (iAmAggressor ? 0 : 0.5f) -
            (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus) -
            myHomeworldBonus;


        if (MaxDial(player, territory, opponent) - result >= 5)
        {
            //I think I only need a small fraction of available forces.

            if (!iAssumeMyLeaderWillDie && chanceOfMyHeroSurviving < 0.8) iAssumeMyLeaderWillDie = true;

            if (iAssumeEnemyLeaderWillDie && chanceOfEnemyHeroSurviving > 0.1) iAssumeEnemyLeaderWillDie = false;

            result =
                opponentDial +
                maxReinforcements +
                (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
                (iAmAggressor ? 0 : 0.5f) -
                (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus);
        }

        LogInfo("{13}/{14}: opponentDial ({0}) + maxReinforcements ({8}) + (chanceOfEnemyHeroSurviving ({7}) < Battle_MinimumChanceToAssumeEnemyHeroSurvives ({10}) ? 0 : 1) * (highestLeader ({1}) + messiahBonus ({2})) + defenderPenalty ({3}) - (chanceOfMyHeroSurviving ({4}) < Battle_MinimumChanceToAssumeMyLeaderSurvives ({11}) ? 0 : 1) * (myHeroValue ({5}) + messiahBonus ({9}) + bankerBoost ({12}) => *{6}*)",
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
            Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives,
            Param.Battle_MimimumChanceToAssumeMyLeaderSurvives,
            bankerBoost,
            territory,
            opponent.Faction);

        return result;
    }

    private static bool CanOnlyUseTraitorsOrFacedancers(Player p)
    {
        return p.Leaders.All(l =>
            (!p.MessiahAvailable && p.Traitors.Contains(l)) ||
            p.FaceDancers.Contains(l) ||
            p is { MessiahAvailable: false, Ally: Faction.Black } && p.AlliedPlayer.Traitors.Contains(l) ||
            p.Ally == Faction.Purple && p.AlliedPlayer.FaceDancers.Contains(l));
    }


    private static bool HasLasgunShield(TreacheryCard? myWeapon, TreacheryCard? myDefense, TreacheryCard? enemyWeapon, TreacheryCard? enemyDefense) =>
        (myWeapon is { IsLaser: true } && enemyDefense is { IsShield: true }) ||
        (enemyWeapon is { IsLaser: true } && myDefense is { IsShield: true });

    private RockWasMelted DetermineRockWasMelted()
    {
        var outcome = Battle.DetermineBattleOutcome(Game.AggressorPlan, Game.DefenderPlan, Game.CurrentBattle.Territory, Game);
        LogInfo(outcome.GetMessage());
        return new RockWasMelted(Game, Faction) { Kill = outcome.Winner == Player };
    }

    private ResidualPlayed DetermineResidualPlayed() => new(Game, Faction);

    private PortableAntidoteUsed? DeterminePortableAntidoteUsed()
    {
        var opponent = Game.CurrentBattle.OpponentOf(Player);
        var opponentPlan = Game.CurrentBattle.PlanOf(opponent);
        var myPlan = Game.CurrentBattle.PlanOf(Player);
        var defense = Player.TreacheryCards.FirstOrDefault(c => c.IsPortableAntidote);

        if (opponentPlan.Weapon != null && opponentPlan.Weapon.CounteredBy(defense, myPlan.Weapon)) return new PortableAntidoteUsed(Game, Faction);

        return null;
    }

    private Thought? DetermineThought()
    {
        var opponent = Game.CurrentBattle.OpponentOf(Player);
        if (OpponentCardsUnknownToPlayer(opponent).Any())
        {
            var unknownWeapons = CardsUnknownToPlayer.Where(c => c.IsWeapon).OrderByDescending(c => CardQuality(c, opponent)).ToArray();
            if (unknownWeapons.Any()) return new Thought(Game, Faction) { Card = unknownWeapons.First() };
        }

        return null;
    }

    private ThoughtAnswered DetermineThoughtAnswered() 
        => new(Game, Faction) { Card = ThoughtAnswered.ValidCards(Game, Player).LowestOrDefault(c => CardQuality(c, Player)) };

    private HMSAdvantageChosen DetermineHmsAdvantageChosen()
    {
        var adv = StrongholdAdvantage.None;

        var plan = DetermineBattlePlan(false, false);
        if (plan is { HasPoison: false, HasAntidote: false }) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.CountDefensesAsAntidote);
        if (adv == StrongholdAdvantage.None && Resources < 5) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.FreeResourcesForBattles);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.CollectResourcesForDial);
        if (adv == StrongholdAdvantage.None && plan is { HasUseless: false }) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.CollectResourcesForUseless);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.FreeResourcesForBattles);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault(a => a == StrongholdAdvantage.WinTies);
        if (adv == StrongholdAdvantage.None) adv = HMSAdvantageChosen.ValidAdvantages(Game, Player).FirstOrDefault();

        return new HMSAdvantageChosen(Game, Faction) { Advantage = adv };
    }

    private Retreat? DetermineRetreat()
    {
        var forcesToRetreat = Retreat.MaxForces(Game, Player);
        var specialForcesToRetreat = Retreat.MaxSpecialForces(Game, Player);
        var maxForces = Retreat.MaxTotalForces(Game, Player);
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

        var to = (Retreat.ValidTargets(Game, Player).Where(l => ResourcesIn(l) > 0)
                      .HighestOrDefault(l => ResourcesIn(l))
                  ?? Retreat.ValidTargets(Game, Player).FirstOrDefault(l => l.IsProtectedFromStorm))
                  ?? Retreat.ValidTargets(Game, Player).FirstOrDefault();

        if (forcesToRetreat > 0 || specialForcesToRetreat > 0)
        {
            return new Retreat(Game, Faction) { Location = to, Forces = forcesToRetreat, SpecialForces = specialForcesToRetreat };
        }

        return null;
    }

    private LoserConcluded DetermineLoserConcluded()
    {
        var toKeep = LoserConcluded.CardsLoserMayKeep(Game).Where(c => CardQuality(c, Player) > 2).OrderByDescending(c => CardQuality(c, Player)).FirstOrDefault();
        return new LoserConcluded(Game, Faction) { KeptCard = toKeep, Assassinate = LoserConcluded.CanAssassinate(Game, Player) };
    }
}