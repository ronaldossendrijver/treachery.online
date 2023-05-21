/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Battle : GameEvent
    {
        #region Construction

        public Battle(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Battle()
        {
        }

        #endregion Construction

        #region Properties

        public int _heroId;

        [JsonIgnore]
        public IHero Hero
        {
            get => LeaderManager.HeroLookup.Find(_heroId);
            set => _heroId = LeaderManager.HeroLookup.GetId(value);
        }

        public bool Messiah { get; set; }

        public int Forces { get; set; }

        public int ForcesAtHalfStrength { get; set; }

        public int SpecialForces { get; set; }

        public int SpecialForcesAtHalfStrength { get; set; }

        public int AllyContributionAmount { get; set; }

        public int _weaponCardId;

        [JsonIgnore]
        public TreacheryCard Weapon
        {
            get => TreacheryCardManager.Lookup.Find(_weaponCardId);
            set => _weaponCardId = TreacheryCardManager.Lookup.GetId(value);
        }

        public int _defenseCardId;

        [JsonIgnore]
        public TreacheryCard Defense
        {
            get => TreacheryCardManager.Lookup.Find(_defenseCardId);
            set => _defenseCardId = TreacheryCardManager.Lookup.GetId(value);
        }

        public int BankerBonus { get; set; }

        [JsonIgnore]
        public int TotalForces => Forces + ForcesAtHalfStrength + SpecialForces + SpecialForcesAtHalfStrength;

        [JsonIgnore]
        public bool HasPoison => Weapon != null && Weapon.IsPoisonWeapon;

        [JsonIgnore]
        public bool HasProjectile => Weapon != null && Weapon.IsProjectileWeapon;

        [JsonIgnore]
        public bool HasProjectileDefense => Defense != null && Defense.IsProjectileDefense;

        [JsonIgnore]
        public bool HasShield => Defense != null && Defense.IsShield;

        [JsonIgnore]
        public bool HasNonAntidotePoisonDefense => Defense != null && Defense.IsNonAntidotePoisonDefense;

        [JsonIgnore]
        public bool HasAntidote => Defense != null && Defense.IsPoisonDefense;

        [JsonIgnore]
        public bool HasLaser => Weapon != null && Weapon.IsLaser;

        [JsonIgnore]
        public bool HasPoisonTooth => Weapon != null && Weapon.IsPoisonTooth;

        [JsonIgnore]
        public bool HasArtillery => Weapon != null && Weapon.IsArtillery;

        [JsonIgnore]
        public bool HasReinforcements => Weapon != null && Weapon.Type == TreacheryCardType.Reinforcements || Defense != null && Defense.Type == TreacheryCardType.Reinforcements;

        [JsonIgnore]
        public TreacheryCard OriginalWeapon { get; set; } = null;

        [JsonIgnore]
        public TreacheryCard OriginalDefense { get; set; } = null;

        [JsonIgnore]
        public bool HasRockMelter => Weapon != null && Weapon.IsRockmelter;

        [JsonIgnore]
        public bool HasUseless => Weapon != null && Weapon.IsUseless || Defense != null && Defense.IsUseless;

        #endregion Properties

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Initiator == Game.CurrentBattle.Aggressor)
            {
                Game.AggressorPlan = this;
            }
            else if (Initiator == Game.CurrentBattle.Defender)
            {
                Game.DefenderPlan = this;
            }

            if (Game.AggressorPlan != null && Game.DefenderPlan != null)
            {
                Game.RevealCurrentNoField(DetermineForceSupplier(Game, Game.AggressorPlan.Player), Game.CurrentBattle.Territory);
                Game.RevealCurrentNoField(DetermineForceSupplier(Game, Game.DefenderPlan.Player), Game.CurrentBattle.Territory);

                Log(Game.AggressorPlan.GetBattlePlanMessage());
                Log(Game.DefenderPlan.GetBattlePlanMessage());

                RegisterKnownCards(Game.AggressorPlan);
                RegisterKnownCards(Game.DefenderPlan);

                PassPurpleTraitorAction();

                Game.Enter(Game.AggressorPlan.HasRockMelter || Game.DefenderPlan.HasRockMelter, Phase.MeltingRock, Phase.CallTraitorOrPass);
            }
        }

        private void RegisterKnownCards(Battle battle)
        {
            Game.RegisterKnown(battle.Weapon);
            Game.RegisterKnown(battle.Defense);
        }

        private void PassPurpleTraitorAction()
        {
            if (Game.CurrentBattle.Aggressor == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Game.Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
            {
                Game.AggressorTraitorAction = new TreacheryCalled(Game, Faction.Purple) { TraitorCalled = false };
            }
            else if (Game.CurrentBattle.Defender == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Game.Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
            {
                Game.DefenderTraitorAction = new TreacheryCalled(Game, Faction.Purple) { TraitorCalled = false };
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " finalize their battle plan");
        }

        public Message GetBattlePlanMessage()
        {
            return Message.Express(
                Initiator,
                " Plan → leader: ",
                Hero,
                ", dial: ",
                Dial(Game, Game.CurrentBattle.OpponentOf(Initiator).Faction),
                MessagePart.ExpressIf(Game.Applicable(Rule.AdvancedCombat), Payment.Of(Cost(Game))),
                MessagePart.ExpressIf(AllyContributionAmount > 0, " (", Payment.Of(AllyContributionAmount, Player.Ally), ")"),
                ", weapon: ",
                Weapon,
                ", defense: ",
                Defense);
        }

        public void ActivateDynamicWeapons(TreacheryCard opponentWeapon, IHero hero, TreacheryCard opponentDefense)
        {
            if (Weapon != null && Weapon.Type == TreacheryCardType.MirrorWeapon)
            {
                OriginalWeapon = Weapon;
                Weapon = opponentWeapon;
                Log(OriginalWeapon, " becomes a ", Weapon);
            }

            if (Defense != null && Defense.IsUseless && 
                Game.SkilledAs(hero, LeaderSkill.Diplomat) && 
                opponentDefense != null && opponentWeapon != null && 
                (opponentWeapon.CounteredBy(opponentDefense, Weapon) || opponentWeapon.IsArtillery && opponentDefense.IsShield))
            {
                OriginalDefense = Defense;
                Defense = opponentDefense;
                Log(hero, " turns ", OriginalDefense, " into a ", Defense);
                Game.CardUsedByDiplomat = OriginalDefense;
            }

            if (IsUsingPortableAntidote(Game, Initiator))
            {
                OriginalDefense = Defense;
                Defense = Game.TreacheryDiscardPile.Items.FirstOrDefault(c => c.IsPortableAntidote);
            }
        }

        public void DeactivateDynamicWeapons()
        {
            if (OriginalWeapon != null)
            {
                Weapon = OriginalWeapon;
                OriginalWeapon = null;
            }

            if (OriginalDefense != null)
            {
                Defense = OriginalDefense;
                OriginalDefense = null;
            }
        }

        public float Dial(Game g, Faction opponent)
        {
            var pinkBattleContribution = !g.Prevented(FactionAdvantage.PinkOccupation) ? g.CurrentPinkBattleContribution : 0;
            if (Initiator != Faction.Pink || Game.CurrentPinkOrAllyFighter == Faction.None)
            {
                return ForceValue(g, Initiator, opponent, Forces, SpecialForces, ForcesAtHalfStrength, SpecialForcesAtHalfStrength) + pinkBattleContribution;
            }
            else
            {
                return ForceValue(g, Player.Ally, opponent, Forces, SpecialForces, ForcesAtHalfStrength, SpecialForcesAtHalfStrength) + pinkBattleContribution;
            }
        }
        public int Cost(Game g) => Cost(g, Player, Forces, SpecialForces, out int _);

        public int Cost(Game g, out int paidByArrakeen) => Cost(g, Player, Forces, SpecialForces, out paidByArrakeen);

        #endregion Execution

        #region Validation

        public override Message Validate()
        {
            var p = Player;
            if (Forces + ForcesAtHalfStrength > MaxForces(Game, p, false)) return Message.Express("Too many ", p.Force, " selected");
            if (SpecialForces + SpecialForcesAtHalfStrength > MaxForces(Game, p, true)) return Message.Express("Too many ", p.SpecialForce, " selected");
            int cost = Cost(Game, p, Forces, SpecialForces, out int _);
            if (AllyContributionAmount > cost) return Message.Express("Your ally is paying more than needed");
            if (AllyContributionAmount > MaxAllyResources(Game, p, Forces, SpecialForces)) return Message.Express("Your ally won't pay that much");
            if (cost > p.Resources + AllyContributionAmount) return Message.Express("You can't pay ", Payment.Of(cost), " to fight with ", Forces + SpecialForces, " forces at full strength");
            if (Hero == null && ValidBattleHeroes(Game, p).Any() && !Game.Applicable(Rule.BattleWithoutLeader)) return Message.Express("You must select a leader");
            if (Hero != null && !ValidBattleHeroes(Game, p).Contains(Hero)) return Message.Express("Invalid leader");
            if (Weapon != null && Weapon == Defense) return Message.Express("Can't use the same card as weapon and defense");
            if (Hero == null && (Weapon != null || Defense != null)) return Message.Express("Can't use treachery cards without a leader");
            if (Hero != null && Hero is Leader && !Game.CanFightIn(Hero, Game.CurrentBattle.Territory)) return Message.Express("Selected leader already fought in another territory");
            if (Hero == null && Messiah) return Message.Express("Can't use ", Concept.Messiah, " without a leader");
            if (Messiah && !MessiahMayBeUsedInBattle(Game, p)) return Message.Express(Concept.Messiah, " is not available");
            if (Weapon == null && Defense != null && Defense.Type == TreacheryCardType.WeirdingWay) return Message.Express("You can't use ", TreacheryCardType.WeirdingWay, " as defense without using a weapon");
            if (Defense == null && Weapon != null && Weapon.Type == TreacheryCardType.Chemistry) return Message.Express("You can't use ", TreacheryCardType.Chemistry, " as weapon without using a defense");
            if (!ValidWeapons(Game, p, Defense, Hero, Game.CurrentBattle.Territory, true).Contains(Weapon)) return Message.Express("Invalid weapon");
            if (!ValidDefenses(Game, p, Weapon, Game.CurrentBattle.Territory, true).Contains(Defense)) return Message.Express("Invalid defense");
            if (Game.IsInFrontOfShield(Hero)) return Message.Express(Hero, " is currently in front of your player shield");
            if (BankerBonus > 0 && !Game.SkilledAs(Hero, LeaderSkill.Banker)) return Message.Express("Only a leader skilled as ", LeaderSkill.Banker, " can be boosted by ", Concept.Resource);
            if (BankerBonus > MaxBankerBoost(Game, Player, Hero)) return Message.Express("You cannot boost your leader this much");
            if (cost + BankerBonus > p.Resources + AllyContributionAmount) return Message.Express("You can't pay this ", LeaderSkill.Banker, " bonus");

            if (Hero != null &&
                AffectedByVoice(Game, Player, Game.CurrentVoice) &&
                Game.CurrentVoice.Must &&
                Game.CurrentVoice.Type != TreacheryCardType.Mercenary &&
                p.TreacheryCards.Any(c => Voice.IsVoicedBy(Game, true, true, c.Type, Game.CurrentVoice.Type) || Voice.IsVoicedBy(Game, false, true, c.Type, Game.CurrentVoice.Type)) &&
                (Weapon == null || !Voice.IsVoicedBy(Game, true, true, Weapon.Type, Game.CurrentVoice.Type)) && (Defense == null || !Voice.IsVoicedBy(Game, false, true, Defense.Type, Game.CurrentVoice.Type)))
            {
                return Message.Express("You must use a ", Game.CurrentVoice.Type);
            }

            return null;
        }

        public static Player DetermineForceSupplier(Game g, Player playerThatFights) =>
            playerThatFights.Is(Faction.Pink) && g.CurrentPinkOrAllyFighter != Faction.None && !g.Prevented(FactionAdvantage.PinkOccupation) ? playerThatFights.AlliedPlayer : playerThatFights;

        public static float ForceValue(Game g, Faction player, Faction opponent, int Forces, int SpecialForces, int ForcesAtHalfStrength, int SpecialForcesAtHalfStrength)
        {
            int nrOfForcesToCountAsSpecialDueToRedCunning = g.CurrentRedCunning != null && g.CurrentRedCunning.Initiator == player ? Math.Min(5, Forces) : 0;
            int ForcesAdjustedForCunning = Forces - nrOfForcesToCountAsSpecialDueToRedCunning;
            int SpecialForcesAdjustedForCunning = SpecialForces + nrOfForcesToCountAsSpecialDueToRedCunning;

            int nrOfForcesAtHalfStrengthToCountAsSpecialDueToRedCunning = g.CurrentRedCunning != null && g.CurrentRedCunning.Initiator == player ? Math.Max(0, Math.Min(5, ForcesAtHalfStrength) - nrOfForcesToCountAsSpecialDueToRedCunning) : 0;
            int ForcesAtHalfStrengthAdjustedForCunning = ForcesAtHalfStrength - nrOfForcesAtHalfStrengthToCountAsSpecialDueToRedCunning;
            int SpecialForcesAtHalfStrengthAdjustedForCunning = SpecialForcesAtHalfStrength + nrOfForcesAtHalfStrengthToCountAsSpecialDueToRedCunning;

            float specialForceStrength = DetermineSpecialForceStrength(g, player, opponent);
            float specialForceNoSpiceFactor = DetermineSpecialForceNoSpiceFactor(g, player);
            float normalForceStrength = DetermineNormalForceStrength(g, player);
            float normalForceNoSpiceFactor = DetermineNormalForceNoSpiceFactor(player);

            return
                normalForceStrength * ForcesAdjustedForCunning +
                specialForceStrength * SpecialForcesAdjustedForCunning +
                normalForceNoSpiceFactor * normalForceStrength * ForcesAtHalfStrengthAdjustedForCunning +
                specialForceNoSpiceFactor * specialForceStrength * SpecialForcesAtHalfStrengthAdjustedForCunning;
        }

        public static bool IsUsingPortableAntidote(Game g, Faction faction) => g.CurrentPortableAntidoteUsed?.Initiator == faction;

        public static float DetermineSpecialForceStrength(Game g, Faction player, Faction opponent)
        {
            if (player == Faction.Yellow && g.Prevented(FactionAdvantage.YellowSpecialForceBonus))
            {
                return 1;
            }
            else if (player == Faction.Red && (g.Prevented(FactionAdvantage.RedSpecialForceBonus) || opponent == Faction.Yellow || g.OccupierOf(World.RedStar) != null))
            {
                return 1;
            }
            else if (player == Faction.Grey && g.Prevented(FactionAdvantage.GreySpecialForceBonus))
            {
                return 1;
            }
            else if (player == Faction.Blue)
            {
                return 0;
            }

            return 2;
        }

        private static float DetermineSpecialForceNoSpiceFactor(Game g, Faction player)
        {
            if (player == Faction.Red && g.HasHighThreshold(player, World.RedStar))
            {
                return 1;
            }
            else
            {
                return 0.5f;
            }
        }

        public static float DetermineNormalForceStrength(Game g, Faction player)
        {
            if (player == Faction.Grey && g.CurrentGreyCunning == null)
            {
                return 0.5f;
            }

            return 1;
        }

        public static float DetermineNormalForceNoSpiceFactor(Faction player)
        {
            if (player == Faction.Grey)
            {
                return 1;
            }

            return 0.5f;
        }

        public static bool MustPayForForcesInBattle(Game g, Player p) =>
            g.Applicable(Rule.AdvancedCombat) && (g.Prevented(FactionAdvantage.YellowNotPayingForBattles) || p.Faction != Faction.Yellow);

        public static bool MessiahMayBeUsedInBattle(Game g, Player p) => MessiahAvailableForBattle(g, p);

        public static int Cost(Game g, Player p, int AmountOfForcesAtFullStrength, int AmountOfSpecialForcesAtFullStrength, out int paidByArrakeen)
        {
            int cost = AmountOfForcesAtFullStrength * NormalForceCost(g, p) + AmountOfSpecialForcesAtFullStrength * SpecialForceCost(g, p);
            paidByArrakeen = Math.Min(CostReduction(g, p), cost);
            return cost - paidByArrakeen;
        }

        public static int CostReduction(Game g, Player p)
        {
            return g.HasStrongholdAdvantage(p.Faction, StrongholdAdvantage.FreeResourcesForBattles, g.CurrentBattle?.Territory) ? 2 : 0;
        }

        public static int NormalForceCost(Game g, Player p)
        {
            if (MustPayForForcesInBattle(g, p))
            {
                if (p.Faction == Faction.Grey)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }

        public static int SpecialForceCost(Game g, Player p)
        {
            if (MustPayForForcesInBattle(g, p))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        
        public static int MaxResources(Game g, Player p, int forces, int specialForces) => Math.Min(p.Resources, Math.Max(0, forces + specialForces - CostReduction(g, p)));

        public static int MaxAllyResources(Game g, Player p, int forces, int specialForces) => Math.Min(g.ResourcesYourAllyCanPay(p), Math.Max(0, forces + specialForces - CostReduction(g, p)));

        public static int MaxForces(Game g, Player p, bool specialForces)
        {
            var forceSupplier = DetermineForceSupplier(g, p);

            if (g.CurrentBattle == null || g.CurrentBattle.Territory == null)
            {
                return 0;
            }

            if (!specialForces)
            {
                if (forceSupplier.Faction == Faction.White && forceSupplier.SpecialForcesIn(g.CurrentBattle.Territory) > 0)
                {
                    return Math.Min(forceSupplier.ForcesInReserve, g.CurrentNoFieldValue) + forceSupplier.ForcesIn(g.CurrentBattle.Territory);
                }
                else
                {
                    return forceSupplier.ForcesIn(g.CurrentBattle.Territory);
                }
            }
            else
            {
                if (forceSupplier.Faction != Faction.White)
                {
                    return forceSupplier.SpecialForcesIn(g.CurrentBattle.Territory);
                }
                else
                {
                    return 0;
                }

            }
        }

        public static IEnumerable<Fight> BattlesToBeFought(Game g, Player player, bool returnOnlyOneBattle = false)
        {
            var result = new List<Fight>();

            bool mayBattleUnderStorm = g.Applicable(Rule.BattlesUnderStorm);

            foreach (var occupiedLocation in player.OccupiedLocations.Where(l => (mayBattleUnderStorm || l.Sector != g.SectorInStorm) && l != g.Map.PolarSink))
            {
                var locationsWithinRange = Map.FindNeighboursWithinTerritory(occupiedLocation, false, g.SectorInStorm);

                foreach (var battalions in g.OccupyingForcesOnPlanet.Where(l => (mayBattleUnderStorm || l.Key.Sector != g.SectorInStorm) && locationsWithinRange.Contains(l.Key)))
                {
                    var location = battalions.Key;
                    var defenders = battalions.Value.Where(b => b.Faction != player.Faction && b.Faction != player.Ally);

                    foreach (var f in defenders.Select(b => b.Faction))
                    {
                        result.Add(new Fight(occupiedLocation.Territory, f));

                        if (returnOnlyOneBattle)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static bool AffectedByVoice(Game g, Player p, Voice voice)
        {
            if (voice != null)
            {
                var opponent = g.CurrentBattle?.OpponentOf(p);
                return opponent != null && (voice.Initiator == opponent.Faction || voice.Initiator == opponent.Ally);
            }

            return false;
        }

        public static IEnumerable<IHero> ValidBattleHeroes(Game g, Player p)
        {
            bool affectedByVoice = AffectedByVoice(g, p, g.CurrentVoice);
            bool mustUseCheapHero = affectedByVoice && g.CurrentVoice.Must && g.CurrentVoice.Type == TreacheryCardType.Mercenary;
            bool mayNotUseCheapHero = affectedByVoice && g.CurrentVoice.MayNot && g.CurrentVoice.Type == TreacheryCardType.Mercenary;

            var result = new List<IHero>();

            if (mustUseCheapHero && CheapHeroes(p).Any())
            {
                result.AddRange(CheapHeroes(p));
            }
            else
            {
                result.AddRange(p.Leaders.Where(l => g.IsAlive(l) && g.CanJoinCurrentBattle(l)));
                if (!mayNotUseCheapHero)
                {
                    result.AddRange(CheapHeroes(p));
                }
            }

            return result;
        }

        public static IEnumerable<TreacheryCard> ValidWeapons(Game g, Player p, TreacheryCard selectedDefense, IHero selectedHero, Territory territoryOfBattle, bool includingNone = false)
        {
            List<TreacheryCard> result = null;

            if (!ValidBattleHeroes(g, p).Any())
            {
                result = new List<TreacheryCard>();
                if (includingNone) result.Add(null);
                return result;
            }

            var playableWeapons = CardsPlayableAsWeapon(g, p, selectedDefense, territoryOfBattle,
                selectedHero != null && g.SkilledAs(selectedHero, LeaderSkill.Planetologist));

            if (AffectedByVoice(g, p, g.CurrentVoice))
            {
                if (g.CurrentVoice.Must)
                {
                    if (selectedDefense != null && selectedDefense.Type == g.CurrentVoice.Type)
                    {
                        result = playableWeapons.ToList();
                        if (includingNone) result.Add(null);
                    }
                    else if (playableWeapons.Any(w => Voice.IsVoicedBy(g, true, true, w.Type, g.CurrentVoice.Type)))
                    {
                        result = playableWeapons.Where(w => Voice.IsVoicedBy(g, true, true, w.Type, g.CurrentVoice.Type)).ToList();
                    }
                }
                else if (g.CurrentVoice.MayNot)
                {
                    result = playableWeapons.Where(w => !Voice.IsVoicedBy(g, true, false, w.Type, g.CurrentVoice.Type)).ToList();
                    if (includingNone) result.Add(null);
                }
            }

            if (result == null)
            {
                result = playableWeapons.ToList();
                if (includingNone) result.Add(null);
            }

            return result;
        }

        public static IEnumerable<TreacheryCard> ValidDefenses(Game g, Player p, TreacheryCard selectedWeapon, Territory territoryOfBattle, bool includingNone = false)
        {
            List<TreacheryCard> result = null;

            if (!ValidBattleHeroes(g, p).Any())
            {
                result = new List<TreacheryCard>();
                if (includingNone) result.Add(null);
                return result;
            }

            var playableDefenses = CardsPlayableAsDefense(g, p, selectedWeapon, territoryOfBattle);

            if (AffectedByVoice(g, p, g.CurrentVoice))
            {
                if (g.CurrentVoice.Must)
                {
                    if (selectedWeapon != null && selectedWeapon.Type == g.CurrentVoice.Type)
                    {
                        result = playableDefenses.ToList();
                        if (includingNone) result.Add(null);
                    }
                    else if (playableDefenses.Any(w => Voice.IsVoicedBy(g, false, true, w.Type, g.CurrentVoice.Type)))
                    {
                        result = playableDefenses.Where(w => Voice.IsVoicedBy(g, false, true, w.Type, g.CurrentVoice.Type)).ToList();
                    }
                }
                else if (g.CurrentVoice.MayNot)
                {
                    result = playableDefenses.Where(w => !Voice.IsVoicedBy(g, false, false, w.Type, g.CurrentVoice.Type)).ToList();
                    if (includingNone) result.Add(null);
                }
            }

            if (result == null)
            {

                result = playableDefenses.ToList();
                if (includingNone) result.Add(null);
            }

            return result;
        }

        private static IEnumerable<TreacheryCard> CheapHeroes(Player p)
        {
            return p.TreacheryCards.Where(c => c.Type == TreacheryCardType.Mercenary);
        }

        private static IEnumerable<TreacheryCard> CardsPlayableAsWeapon(Game g, Player p, TreacheryCard withDefense, Territory territoryOfBattle, bool withPlanetologist)
        {
            var fightingOnOwnHomeworld = territoryOfBattle != null && p.IsNative(territoryOfBattle);
            var forcesToRevealUnderNoField = territoryOfBattle != null && p.Is(Faction.White) && p.SpecialForcesIn(territoryOfBattle) != 0 ? Math.Min(p.ForcesInReserve, g.CurrentNoFieldValue) : 0;

            return p.TreacheryCards.Where(c =>
            c.Type != TreacheryCardType.Chemistry && (c.IsWeapon || c.Type == TreacheryCardType.Useless) ||
            c.Type == TreacheryCardType.Chemistry && withDefense != null && withDefense.IsDefense && withDefense.Type != TreacheryCardType.WeirdingWay ||
            withPlanetologist && c.IsGreen ||
            c.Type == TreacheryCardType.Reinforcements && p.ForcesInReserve + p.SpecialForcesInReserve - forcesToRevealUnderNoField >= 3 ||
            !fightingOnOwnHomeworld && c.Type == TreacheryCardType.HarassAndWithdraw);
        }

        public static int MaxBankerBoost(Game g, Player p, IHero hero)
        {
            if (g.SkilledAs(hero, LeaderSkill.Banker))
            {
                return Math.Min(p.Resources, 3);
            }

            return 0;
        }

        private static IEnumerable<TreacheryCard> CardsPlayableAsDefense(Game g, Player p, TreacheryCard withWeapon, Territory territoryOfBattle)
        {
            var fightingOnOwnHomeworld = territoryOfBattle != null && p.IsNative(territoryOfBattle);
            var forcesToRevealUnderNoField = territoryOfBattle != null && p.Is(Faction.White) && p.SpecialForcesIn(territoryOfBattle) != 0 ? Math.Min(p.ForcesInReserve, g.CurrentNoFieldValue) : 0;

            return p.TreacheryCards.Where(c =>
            c.Type != TreacheryCardType.WeirdingWay && (c.IsDefense || c.Type == TreacheryCardType.Useless) ||
            c.Type == TreacheryCardType.WeirdingWay && withWeapon != null && withWeapon.IsWeapon && withWeapon.Type != TreacheryCardType.Chemistry ||
            c.Type == TreacheryCardType.Reinforcements && p.ForcesInReserve + p.SpecialForcesInReserve - forcesToRevealUnderNoField >= 3 ||
            !fightingOnOwnHomeworld && c.Type == TreacheryCardType.HarassAndWithdraw);
        }

        public static bool MessiahAvailableForBattle(Game g, Player p)
        {
            return p.MessiahAvailable && g.CanJoinCurrentBattle(LeaderManager.Messiah) && !g.Prevented(FactionAdvantage.GreenUseMessiah) && g.Applicable(Rule.GreenMessiah);
        }

        public static void DetermineForces(Game g, Player p, int forces, int specialForces, int resources, out int forcesFull, out int forcesHalf, out int specialForcesFull, out int specialForcesHalf)
        {
            if (MustPayForForcesInBattle(g, p))
            {
                int effectiveResources = CostReduction(g, p) + resources;
                specialForcesFull = Math.Min(specialForces, effectiveResources);
                specialForcesHalf = specialForces - specialForcesFull;

                forcesFull = Math.Min(forces, effectiveResources - specialForcesFull);
                forcesHalf = forces - forcesFull;
            }
            else
            {
                specialForcesFull = specialForces;
                specialForcesHalf = 0;

                forcesFull = forces;
                forcesHalf = 0;
            }
        }

        public static int DetermineSkillBonus(Game g, Battle plan, ref LeaderSkill activatedSkill)
        {
            return DetermineSkillBonus(g, plan.Player, plan.Hero, plan.Weapon, plan.Defense, plan.BankerBonus, ref activatedSkill);
        }

        public static int DetermineSkillBonus(Game g, Player player, IHero hero, TreacheryCard weapon, TreacheryCard defense, int bankerBonus, ref LeaderSkill activatedSkill)
        {
            if (g.SkilledAs(hero, LeaderSkill.Thinker))
            {
                activatedSkill = LeaderSkill.Thinker;
                return 2;
            }

            if (g.SkilledAs(hero, LeaderSkill.Banker))
            {
                activatedSkill = LeaderSkill.Banker;
                return bankerBonus;
            }

            if (weapon != null && weapon.IsUseless || defense != null && defense.IsUseless)
            {
                if (g.SkilledAs(hero, LeaderSkill.Warmaster))
                {
                    activatedSkill = LeaderSkill.Warmaster;
                    return 3;
                }
                else if (g.SkilledAs(player, LeaderSkill.Warmaster))
                {
                    activatedSkill = LeaderSkill.Warmaster;
                    return 1;
                }
            }

            if (defense != null && defense.IsProjectileDefense)
            {
                if (g.SkilledAs(hero, LeaderSkill.Adept))
                {
                    activatedSkill = LeaderSkill.Adept;
                    return 3;
                }
                else if (g.SkilledAs(player, LeaderSkill.Adept))
                {
                    activatedSkill = LeaderSkill.Adept;
                    return 1;
                }
            }

            if (weapon != null && weapon.IsProjectileWeapon)
            {
                if (g.SkilledAs(hero, LeaderSkill.Swordmaster))
                {

                    activatedSkill = LeaderSkill.Swordmaster;
                    return 3;
                }
                else if (g.SkilledAs(player, LeaderSkill.Swordmaster))
                {

                    activatedSkill = LeaderSkill.Swordmaster;
                    return 1;
                }
            }

            if (weapon != null && weapon.IsGreen)
            {
                if (g.SkilledAs(hero, LeaderSkill.Planetologist))
                {
                    activatedSkill = LeaderSkill.Planetologist;
                    return 2;
                }
            }

            if (defense != null && defense.IsPoisonDefense || g.CurrentPortableAntidoteUsed != null && g.CurrentPortableAntidoteUsed.Player == player)
            {
                if (g.SkilledAs(hero, LeaderSkill.KillerMedic))
                {
                    activatedSkill = LeaderSkill.KillerMedic;
                    return 3;
                }
                else if (g.SkilledAs(player, LeaderSkill.KillerMedic))
                {

                    activatedSkill = LeaderSkill.KillerMedic;
                    return 1;
                }
            }

            if (weapon != null && (weapon.IsPoisonWeapon || weapon.IsPoisonTooth))
            {
                if (g.SkilledAs(hero, LeaderSkill.MasterOfAssassins))
                {

                    activatedSkill = LeaderSkill.MasterOfAssassins;
                    return 3;
                }
                else if (g.SkilledAs(player, LeaderSkill.MasterOfAssassins))
                {
                    activatedSkill = LeaderSkill.MasterOfAssassins;
                    return 1;
                }
            }

            activatedSkill = LeaderSkill.None;
            return 0;
        }

        public static int DetermineSkillPenalty(Game g, Battle plan, Player opponent, ref LeaderSkill activatedSkill)
        {
            return DetermineSkillPenalty(g, plan.Hero, opponent, ref activatedSkill);
        }

        public static int DetermineSkillPenalty(Game g, IHero hero, Player opponent, ref LeaderSkill activatedSkill)
        {
            if (g.SkilledAs(hero, LeaderSkill.Bureaucrat))
            {
                activatedSkill = LeaderSkill.Bureaucrat;
                return g.Map.Strongholds.Count(sh => opponent.Occupies(sh));
            }

            activatedSkill = LeaderSkill.None;
            return 0;
        }

        #endregion Validation

        public static BattleOutcome DetermineBattleOutcome(Battle agg, Battle def, Territory territory, Game game)
        {
            var result = new BattleOutcome
            {
                Aggressor = agg.Player,
                Defender = def.Player
            };

            //Determine result

            agg.ActivateDynamicWeapons(def.Weapon, agg.Hero, def.Defense);
            def.ActivateDynamicWeapons(agg.Weapon, def.Hero, agg.Defense);

            bool poisonToothUsed = !game.PoisonToothCancelled && (agg.HasPoisonTooth || def.HasPoisonTooth);
            bool artilleryUsed = agg.HasArtillery || def.HasArtillery;
            bool rockMelterUsed = agg.HasRockMelter || def.HasRockMelter;
            bool rockMelterUsedToKill = game.CurrentRockWasMelted != null && game.CurrentRockWasMelted.Kill;

            result.AggHeroKilled = false;
            result.AggHeroCauseOfDeath = TreacheryCardType.None;

            DetermineCauseOfDeath(
                agg, def, agg.Hero, poisonToothUsed, artilleryUsed, rockMelterUsed && rockMelterUsedToKill, game.IsProtectedByCarthagAdvantage(agg, territory),
                ref result.AggHeroKilled, ref result.AggHeroCauseOfDeath, ref result.AggSavedByCarthag);

            result.DefHeroKilled = false;
            result.DefHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(
                def, agg, def.Hero, poisonToothUsed, artilleryUsed, rockMelterUsed && rockMelterUsedToKill, game.IsProtectedByCarthagAdvantage(def, territory),
                ref result.DefHeroKilled, ref result.DefHeroCauseOfDeath, ref result.DefSavedByCarthag);

            bool heroStrengthCountsToTotal = !artilleryUsed && !(game.Version >= 145 && rockMelterUsed && !rockMelterUsedToKill);

            result.AggHeroEffectiveStrength = agg.Hero != null && heroStrengthCountsToTotal ? agg.Hero.ValueInCombatAgainst(def.Hero) : 0;
            result.DefHeroEffectiveStrength = def.Hero != null && heroStrengthCountsToTotal ? def.Hero.ValueInCombatAgainst(agg.Hero) : 0;

            if (heroStrengthCountsToTotal)
            {
                result.AggHeroSkillBonus = Battle.DetermineSkillBonus(game, agg, ref result.AggActivatedBonusSkill);
                result.AggBattlePenalty = !result.DefHeroKilled ? Battle.DetermineSkillPenalty(game, def, result.Aggressor, ref result.DefActivatedPenaltySkill) : 0;
                result.AggMessiahContribution = agg.Messiah && agg.Hero != null ? 2 : 0;

                result.DefHeroSkillBonus = Battle.DetermineSkillBonus(game, def, ref result.DefActivatedBonusSkill);
                result.DefBattlePenalty = !result.AggHeroKilled ? Battle.DetermineSkillPenalty(game, agg, result.Defender, ref result.AggActivatedPenaltySkill) : 0;
                result.DefMessiahContribution = def.Messiah && def.Hero != null ? 2 : 0;
            }

            int aggHeroContribution = result.AggHeroKilled || (game.Version < 145 && rockMelterUsed) ? 0 : result.AggHeroEffectiveStrength + result.AggHeroSkillBonus + result.AggMessiahContribution - result.AggBattlePenalty;
            int defHeroContribution = result.DefHeroKilled || (game.Version < 145 && rockMelterUsed) ? 0 : result.DefHeroEffectiveStrength + result.DefHeroSkillBonus + result.DefMessiahContribution - result.DefBattlePenalty;

            int aggPinkKarmaContribution = agg.Initiator == Faction.Pink ? game.PinkKarmaBonus : 0;
            int defPinkKarmaContribution = def.Initiator == Faction.Pink ? game.PinkKarmaBonus : 0;

            float aggForceDial;
            float defForceDial;

            var aggForceSupplier = Battle.DetermineForceSupplier(game, result.Aggressor);
            result.AggUndialedForces = aggForceSupplier.AnyForcesIn(territory) - agg.TotalForces;

            var defForceSupplier = Battle.DetermineForceSupplier(game, result.Defender);
            result.DefUndialedForces = defForceSupplier.AnyForcesIn(territory) - def.TotalForces;

            if (!rockMelterUsed)
            {
                aggForceDial = agg.Dial(game, result.Defender.Faction);
                defForceDial = def.Dial(game, result.Aggressor.Faction);
            }
            else
            {
                aggForceDial = result.AggUndialedForces;
                if (result.Aggressor.Faction == game.CurrentPinkOrAllyFighter) aggForceDial += (int)Math.Ceiling(0.5f * game.GetPlayer(Faction.Pink).AnyForcesIn(territory));

                defForceDial = result.DefUndialedForces;
                if (result.Defender.Faction == game.CurrentPinkOrAllyFighter) defForceDial += (int)Math.Ceiling(0.5f * game.GetPlayer(Faction.Pink).AnyForcesIn(territory));
            }

            result.AggReinforcementsContribution = agg.HasReinforcements ? 2 : 0;
            result.DefReinforcementsContribution = def.HasReinforcements ? 2 : 0;

            result.AggHomeworldContribution = agg.Player.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);
            result.DefHomeworldContribution = def.Player.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);

            result.AggTotal = aggForceDial + aggHeroContribution + aggPinkKarmaContribution + result.AggHomeworldContribution + result.AggReinforcementsContribution;
            result.DefTotal = defForceDial + defHeroContribution + defPinkKarmaContribution + result.DefHomeworldContribution + result.DefReinforcementsContribution;

            agg.DeactivateDynamicWeapons();
            def.DeactivateDynamicWeapons();

            bool aggressorWinsTies = true;
            if (game.HasStrongholdAdvantage(result.Defender.Faction, StrongholdAdvantage.WinTies, territory))
            {
                aggressorWinsTies = false;
            }

            if (BattleInitiated.IsAggressorByJuice(game, result.Defender.Faction) && !game.HasStrongholdAdvantage(result.Aggressor.Faction, StrongholdAdvantage.WinTies, territory))
            {
                aggressorWinsTies = false;
            }

            if (aggressorWinsTies)
            {
                result.Winner = (result.AggTotal >= result.DefTotal) ? result.Aggressor : result.Defender;
            }
            else
            {
                result.Winner = (result.DefTotal >= result.AggTotal) ? result.Defender : result.Aggressor;
            }

            result.WinnerBattlePlan = (result.Winner == result.Aggressor) ? agg : def;
            result.Loser = result.Winner == result.Aggressor ? result.Defender : result.Aggressor;
            result.LoserBattlePlan = (result.Loser == result.Aggressor) ? agg : def;

            return result;
        }

        private static void DetermineCauseOfDeath(Battle playerPlan, Battle opponentPlan, IHero theHero, bool poisonToothUsed, bool artilleryUsed, bool rockMelterWasUsedToKill, bool isProtectedByCarthagAdvantage, ref bool heroDies, ref TreacheryCardType causeOfDeath, ref bool savedByCarthag)
        {
            heroDies = false;
            causeOfDeath = TreacheryCardType.None;
            savedByCarthag = isProtectedByCarthagAdvantage && opponentPlan.HasPoison && !playerPlan.HasAntidote;

            DetermineDeathBy(theHero, TreacheryCardType.Rockmelter, rockMelterWasUsedToKill, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.ArtilleryStrike, artilleryUsed && !playerPlan.HasShield, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.PoisonTooth, poisonToothUsed && !playerPlan.HasNonAntidotePoisonDefense, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Laser, opponentPlan.HasLaser, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Poison, opponentPlan.HasPoison && !(playerPlan.HasAntidote || isProtectedByCarthagAdvantage), ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Projectile, opponentPlan.HasProjectile && !playerPlan.HasProjectileDefense, ref heroDies, ref causeOfDeath);
        }

        private static void DetermineDeathBy(IHero hero, TreacheryCardType byWeapon, bool weaponHasEffect, ref bool heroIsKilled, ref TreacheryCardType causeOfDeath)
        {
            if (!heroIsKilled && hero != null && weaponHasEffect)
            {
                heroIsKilled = true;
                causeOfDeath = byWeapon;
            }
        }
    }
}
