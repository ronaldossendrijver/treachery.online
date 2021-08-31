/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Battle : GameEvent
    {
        public int _heroId;

        [JsonIgnore]
        public IHero Hero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_heroId);
            }

            set
            {
                _heroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public bool Messiah { get; set; }

        public int Forces { get; set; }
        public int ForcesAtHalfStrength { get; set; }

        public int SpecialForces { get; set; }
        public int SpecialForcesAtHalfStrength { get; set; }

        [JsonIgnore]
        public int TotalForces => Forces + ForcesAtHalfStrength + SpecialForces + SpecialForcesAtHalfStrength;

        public int AllyContributionAmount { get; set; }

        public int _weaponCardId;

        [JsonIgnore]
        public TreacheryCard Weapon
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_weaponCardId);
            }
            set
            {
                _weaponCardId = TreacheryCardManager.Lookup.GetId(value);
            }
        }

        public int _defenseCardId;

        public Battle(Game game) : base(game)
        {
        }

        public Battle()
        {
        }

        [JsonIgnore]
        public TreacheryCard Defense
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_defenseCardId);
            }
            set
            {
                _defenseCardId = TreacheryCardManager.Lookup.GetId(value);
            }
        }

        public int BankerBonus { get; set; }

        [JsonIgnore]
        public bool HasPoison
        {
            get
            {
                return Weapon != null && Weapon.IsPoisonWeapon;
            }
        }

        [JsonIgnore]
        public bool HasProjectile
        {
            get
            {
                return Weapon != null && Weapon.IsProjectileWeapon;
            }
        }

        [JsonIgnore]
        public bool HasProjectileDefense
        {
            get
            {
                return Defense != null && Defense.IsProjectileDefense;
            }
        }

        [JsonIgnore]
        public bool HasShield
        {
            get
            {
                return Defense != null && Defense.IsShield;
            }
        }

        [JsonIgnore]
        public bool HasNonAntidotePoisonDefense
        {
            get
            {
                return Defense != null && Defense.IsNonAntidotePoisonDefense;
            }
        }

        [JsonIgnore]
        public bool HasAntidote
        {
            get
            {
                return Defense != null && Defense.IsPoisonDefense;
            }
        }

        [JsonIgnore]
        public bool HasLaser
        {
            get
            {
                return Weapon != null && Weapon.IsLaser;
            }
        }

        [JsonIgnore]
        public bool HasPoisonTooth
        {
            get
            {
                return Weapon != null && Weapon.IsPoisonTooth;
            }
        }

        [JsonIgnore]
        public bool HasArtillery
        {
            get
            {
                return Weapon != null && Weapon.IsArtillery;
            }
        }

        [JsonIgnore]
        public TreacheryCard OriginalWeapon { get; set; } = null;

        [JsonIgnore]
        public TreacheryCard OriginalDefense { get; set; } = null;

        public void ActivateDynamicWeapons(TreacheryCard mirroredWeapon, TreacheryCard mirroredDefense)
        {
            if (Weapon != null && Weapon.Type == TreacheryCardType.MirrorWeapon)
            {
                OriginalWeapon = Weapon;
                Weapon = mirroredWeapon;
            }

            if (Game.CurrentDiplomacy?.Initiator == Initiator)
            {
                OriginalDefense = Defense;
                Defense = mirroredDefense;
            }

            if (Game.CurrentPortableAntidoteUsed?.Initiator == Initiator)
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

            if (Game.CurrentDiplomacy?.Initiator == Initiator)
            {
                Defense = OriginalDefense;
                OriginalDefense = null;
            }

            if (Game.CurrentPortableAntidoteUsed?.Initiator == Initiator)
            {
                Defense = OriginalDefense;
                OriginalDefense = null;
            }
        }

        public bool HasRockMelter => Weapon != null && Weapon.IsRockmelter;

        public static float DetermineSpecialForceStrength(Game g, Faction player, Faction opponent)
        {
            if (player == Faction.Yellow && g.Prevented(FactionAdvantage.YellowSpecialForceBonus))
            {
                return 1;
            }
            else if (player == Faction.Red && (g.Prevented(FactionAdvantage.RedSpecialForceBonus) || opponent == Faction.Yellow))
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

        public static float DetermineSpecialForceNoSpiceFactor()
        {
            return 0.5f;
        }

        public static float DetermineNormalForceStrength(Faction player)
        {
            if (player == Faction.Grey)
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

        public float Dial(Game g, Faction opponent)
        {
            return ForceValue(g, Initiator, opponent, Forces, SpecialForces, ForcesAtHalfStrength, SpecialForcesAtHalfStrength);
        }

        public static float MaxDial(Game g, Faction initiator, Battalion battalion, Faction opponent)
        {
            return ForceValue(g, initiator, opponent, battalion.AmountOfForces, battalion.AmountOfSpecialForces, 0, 0);
        }

        public static float ForceValue(Game g, Faction player, Faction opponent, int Forces, int SpecialForces, int ForcesAtHalfStrength, int SpecialForcesAtHalfStrength)
        {
            float specialForceStrength = DetermineSpecialForceStrength(g, player, opponent);
            float specialForceNoSpiceFactor = DetermineSpecialForceNoSpiceFactor();
            float normalForceStrength = DetermineNormalForceStrength(player);
            float normalForceNoSpiceFactor = DetermineNormalForceNoSpiceFactor(player);

            return
                normalForceStrength * Forces +
                specialForceStrength * SpecialForces +
                normalForceNoSpiceFactor * normalForceStrength * ForcesAtHalfStrength +
                specialForceNoSpiceFactor * specialForceStrength * SpecialForcesAtHalfStrength;
        }

        public static bool MustPayForForcesInBattle(Game g, Player p)
        {
            return g.Applicable(Rule.AdvancedCombat) && (g.Prevented(FactionAdvantage.YellowNotPayingForBattles) || p.Faction != Faction.Yellow);
        }

        public static bool KwisatzHaderachMayBeUsedInBattle(Game g, Player p)
        {
            return MessiahAvailableForBattle(g, p);
        }

        public int Cost(Game g)
        {
            return Cost(g, Player, Forces, SpecialForces);
        }

        public static int Cost(Game g, Player p, int AmountOfForcesAtFullStrength, int AmountOfSpecialForcesAtFullStrength)
        {
            int cost = AmountOfForcesAtFullStrength * NormalForceCost(g, p) + AmountOfSpecialForcesAtFullStrength * SpecialForceCost(g, p);
            return cost - Math.Min(CostReduction(g, p), cost);
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

        public override string Validate()
        {
            var p = Player;
            if (Forces < 0 || ForcesAtHalfStrength < 0 || SpecialForces < 0 || SpecialForcesAtHalfStrength < 0) return string.Format("Invalid number of forces {0} {1} {2} {3}.", Forces, ForcesAtHalfStrength, SpecialForces, SpecialForcesAtHalfStrength);
            if (Forces + ForcesAtHalfStrength > MaxForces(Game, p, false)) return Skin.Current.Format("Too many {0} selected.", p.Force);
            if (SpecialForces + SpecialForcesAtHalfStrength > MaxForces(Game, p, true)) return Skin.Current.Format("Too many {0} selected.", p.SpecialForce);
            if (AllyContributionAmount > MaxAllyResources(Game, p, Forces, SpecialForces)) return "Your ally won't pay that much";
            int cost = Cost(Game, p, Forces, SpecialForces);
            if (cost > p.Resources + AllyContributionAmount) return Skin.Current.Format("You can't pay {0} {1} to fight with {2} forces at full strength.", cost, Concept.Resource, Forces + SpecialForces);
            if (Hero == null && ValidBattleHeroes(Game, p).Any()) return "You must select a hero.";
            if (Hero != null && !ValidBattleHeroes(Game, p).Contains(Hero)) return "Invalid hero.";
            if (Weapon != null && Weapon == Defense) return "Can't use the same card as weapon and defense.";
            if (Hero == null && (Weapon != null || Defense != null)) return "Can't use treachery cards without a hero.";
            if (Hero != null && Hero is Leader && Game.LeaderState[Hero as Leader].CurrentTerritory != null && Game.LeaderState[Hero as Leader].CurrentTerritory != Game.CurrentBattle.Territory) return "Selected hero already fought in another territory.";
            if (Hero == null && Messiah) return Skin.Current.Format("Can't use {0} without a hero.", Concept.Messiah);
            if (Messiah && !KwisatzHaderachMayBeUsedInBattle(Game, p)) return Skin.Current.Format("{0} is not available.", Concept.Messiah);
            if (Weapon == null && Defense != null && Defense.Type == TreacheryCardType.WeirdingWay) return Skin.Current.Format("You can't use {0} as defense without using a weapon.", TreacheryCardType.WeirdingWay);
            if (Defense == null && Weapon != null && Weapon.Type == TreacheryCardType.Chemistry) return Skin.Current.Format("You can't use {0} as weapon without using a defense.", TreacheryCardType.Chemistry);
            if (!ValidWeapons(Game, p, Defense, Hero, true).Contains(Weapon)) return "Invalid weapon";
            if (!ValidDefenses(Game, p, Weapon, true).Contains(Defense)) return "Invalid defense";
            if (Game.IsInFrontOfShield(Hero)) return Skin.Current.Format("{0} is currently in front of your player shield", Hero);
            if (BankerBonus > 0 && !Game.SkilledAs(Hero, LeaderSkill.Banker)) return Skin.Current.Format("Only a leader skilled as {0} can be boosted by {1}", LeaderSkill.Banker, Concept.Resource);
            if (BankerBonus > MaxBankerBoost(Game, Player, Hero)) return Skin.Current.Format("You cannot boost your leader this much");
            if (cost + BankerBonus > p.Resources) return Skin.Current.Format("You can't pay this {0} bonus", LeaderSkill.Banker);

            if (Hero != null &&
                AffectedByVoice(Game, Player, Game.CurrentVoice) &&
                Game.CurrentVoice.Must &&
                Game.CurrentVoice.Type != TreacheryCardType.Mercenary &&
                p.TreacheryCards.Any(c => Voice.IsVoicedBy(Game, true, c.Type, Game.CurrentVoice.Type) || Voice.IsVoicedBy(Game, false, c.Type, Game.CurrentVoice.Type)) &&
                (Weapon == null || !Voice.IsVoicedBy(Game, true, Weapon.Type, Game.CurrentVoice.Type)) && (Defense == null || !Voice.IsVoicedBy(Game, false, Defense.Type, Game.CurrentVoice.Type)))
            {
                return Skin.Current.Format("You must use a {0} card.", Game.CurrentVoice.Type);
            }

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} finalize their battle plan.", Initiator);
        }

        public Message GetBattlePlanMessage()
        {
            if (Game.Applicable(Rule.AdvancedCombat))
            {
                return new Message(Initiator, "{0} leader: {1}, dial: {2}, weapon: {3}, defense: {4}, {5}: {6}{7}.",
                    Initiator,
                    Hero != null ? Hero.ToString() : "none",
                    Dial(Game, Game.CurrentBattle.OpponentOf(Initiator).Faction),
                    Weapon != null ? Weapon.ToString() : "none", Defense != null ? Defense.ToString() : "none",
                    Concept.Resource,
                    Cost(Game),
                    AllyContributionAmount > 0 ? string.Format(" ({0} from ally)", AllyContributionAmount) : "");
            }
            else
            {
                return new Message(Initiator, "{0} leader: {1}, dial: {2}, weapon: {3}, defense: {4}.",
                    Initiator,
                    Hero != null ? Hero.ToString() : "none",
                    Dial(Game, Game.CurrentBattle.OpponentOf(Initiator).Faction),
                    Weapon != null ? Weapon.ToString() : "none",
                    Defense != null ? Defense.ToString() : "none");
            }
        }

        public static int MaxResources(Game g, Player p, int forces, int specialForces) => Math.Min(p.Resources, Math.Max(0, forces + specialForces - CostReduction(g, p)));

        public static int MaxAllyResources(Game g, Player p, int forces, int specialForces) => Math.Min(g.SpiceYourAllyCanPay(p), Math.Max(0, forces + specialForces - CostReduction(g, p)));

        public static int MaxForces(Game g, Player p, bool specialForces)
        {
            if (g.CurrentBattle == null || g.CurrentBattle.Territory == null)
            {
                return 0;
            }

            if (!specialForces)
            {
                if (p.Faction == Faction.White && p.SpecialForcesIn(g.CurrentBattle.Territory) > 0)
                {
                    return Math.Min(p.ForcesInReserve, g.CurrentNoFieldValue) + p.ForcesIn(g.CurrentBattle.Territory);
                }
                else
                {
                    return p.ForcesIn(g.CurrentBattle.Territory);
                }
            }
            else
            {
                if (p.Faction != Faction.White)
                {
                    return p.SpecialForcesIn(g.CurrentBattle.Territory);
                }
                else
                {
                    return 0;
                }

            }
        }

        public static IEnumerable<Tuple<Territory, Faction>> BattlesToBeFought(Game g, Player player)
        {
            var result = new List<Tuple<Territory, Faction>>();

            bool mayBattleUnderStorm = g.Applicable(Rule.BattlesUnderStorm);

            foreach (var occupiedLocation in player.OccupiedLocations.Where(l => (mayBattleUnderStorm || l.Sector != g.SectorInStorm) && l != g.Map.PolarSink))
            {
                var locationsWithinRange = Map.FindNeighboursWithinTerritory(occupiedLocation, false, g.SectorInStorm);

                foreach (var battalions in g.OccupyingForcesOnPlanet.Where(l => (mayBattleUnderStorm || l.Key.Sector != g.SectorInStorm) && locationsWithinRange.Contains(l.Key)))
                {
                    var location = battalions.Key;
                    var defenders = battalions.Value.Where(b => b.Faction != player.Faction);

                    foreach (var f in defenders.Select(b => b.Faction))
                    {
                        result.Add(new Tuple<Territory, Faction>(occupiedLocation.Territory, f));
                    }
                }
            }

            return result;
        }

        public static bool MustFight(Game g, Player player)
        {
            bool mayBattleUnderStorm = g.Applicable(Rule.BattlesUnderStorm);

            foreach (var occupiedLocation in player.OccupiedLocations.Where(l => (mayBattleUnderStorm || l.Sector != g.SectorInStorm) && l != g.Map.PolarSink))
            {
                var locationsWithinRange = Map.FindNeighboursWithinTerritory(occupiedLocation, false, g.SectorInStorm);

                foreach (var battalions in g.OccupyingForcesOnPlanet.Where(l => (mayBattleUnderStorm || l.Key.Sector != g.SectorInStorm) && locationsWithinRange.Contains(l.Key)))
                {
                    var location = battalions.Key;
                    var defenders = battalions.Value.Where(b => b.Faction != player.Faction);

                    if (defenders.Any())
                    {
                        return true;
                    }
                }
            }

            return false;
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
                result.AddRange(p.Leaders.Where(l => g.LeaderState[l].Alive && g.CanJoinCurrentBattle(l)));
                if (!mayNotUseCheapHero)
                {
                    result.AddRange(CheapHeroes(p));
                }
            }

            return result;
        }


        public static IEnumerable<TreacheryCard> ValidWeapons(Game g, Player p, TreacheryCard selectedDefense, IHero selectedHero, bool includingNone = false)
        {
            List<TreacheryCard> result = null;

            if (!ValidBattleHeroes(g, p).Any())
            {
                result = new List<TreacheryCard>();
                if (includingNone) result.Add(null);
                return result;
            }

            bool isPlanetologist = selectedHero != null && g.SkilledAs(selectedHero, LeaderSkill.Planetologist);

            if (AffectedByVoice(g, p, g.CurrentVoice))
            {
                if (g.CurrentVoice.Must)
                {
                    if (selectedDefense != null && selectedDefense.Type == g.CurrentVoice.Type)
                    {
                        result = CardsPlayableAsWeapon(g, p, selectedDefense, isPlanetologist).ToList();
                        if (includingNone) result.Add(null);
                    }
                    else if (CardsPlayableAsWeapon(g, p, selectedDefense, isPlanetologist).Any(w => Voice.IsVoicedBy(g, true, w.Type, g.CurrentVoice.Type)))
                    {
                        result = CardsPlayableAsWeapon(g, p, selectedDefense, isPlanetologist).Where(w => Voice.IsVoicedBy(g, true, w.Type, g.CurrentVoice.Type)).ToList();
                    }
                }
                else if (g.CurrentVoice.MayNot)
                {
                    result = CardsPlayableAsWeapon(g, p, selectedDefense, isPlanetologist).Where(w => !Voice.IsVoicedBy(g, true, w.Type, g.CurrentVoice.Type)).ToList();
                    if (includingNone) result.Add(null);
                }
            }

            if (result == null)
            {
                result = CardsPlayableAsWeapon(g, p, selectedDefense, isPlanetologist).ToList();
                if (includingNone) result.Add(null);
            }

            return result;
        }

        public static IEnumerable<TreacheryCard> ValidDefenses(Game g, Player p, TreacheryCard selectedWeapon, bool includingNone = false)
        {
            List<TreacheryCard> result = null;

            if (!ValidBattleHeroes(g, p).Any())
            {
                result = new List<TreacheryCard>();
                if (includingNone) result.Add(null);
                return result;
            }

            if (AffectedByVoice(g, p, g.CurrentVoice))
            {
                if (g.CurrentVoice.Must)
                {
                    if (selectedWeapon != null && selectedWeapon.Type == g.CurrentVoice.Type)
                    {
                        result = CardsPlayableAsDefense(g, p, selectedWeapon).ToList();
                        if (includingNone) result.Add(null);
                    }
                    else if (CardsPlayableAsDefense(g, p, selectedWeapon).Any(w => Voice.IsVoicedBy(g, false, w.Type, g.CurrentVoice.Type)))
                    {
                        result = CardsPlayableAsDefense(g, p, selectedWeapon).Where(w => Voice.IsVoicedBy(g, false, w.Type, g.CurrentVoice.Type)).ToList();
                    }
                }
                else if (g.CurrentVoice.MayNot)
                {
                    result = CardsPlayableAsDefense(g, p, selectedWeapon).Where(w => !Voice.IsVoicedBy(g, false, w.Type, g.CurrentVoice.Type)).ToList();
                    if (includingNone) result.Add(null);
                }
            }

            if (result == null)
            {

                result = CardsPlayableAsDefense(g, p, selectedWeapon).ToList();
                if (includingNone) result.Add(null);
            }

            return result;
        }

        private static IEnumerable<TreacheryCard> CheapHeroes(Player p)
        {
            return p.TreacheryCards.Where(c => c.Type == TreacheryCardType.Mercenary);
        }

        private static IEnumerable<TreacheryCard> CardsPlayableAsWeapon(Game g, Player p, TreacheryCard withDefense, bool withPlanetologist)
        {
            if (g.Version <= 91)
            {
                return p.TreacheryCards.Where(c => c.IsWeapon || c.Type == TreacheryCardType.Useless);
            }
            else
            {
                return p.TreacheryCards.Where(c =>
                c.Type != TreacheryCardType.Chemistry && (c.IsWeapon || c.Type == TreacheryCardType.Useless) ||
                c.Type == TreacheryCardType.Chemistry && withDefense != null && withDefense.IsDefense && withDefense.Type != TreacheryCardType.WeirdingWay ||
                withPlanetologist && !(c.IsWeapon || c.IsDefense || c.IsUseless));
            }
        }

        public static int MaxBankerBoost(Game g, Player p, IHero hero)
        {
            if (g.SkilledAs(hero, LeaderSkill.Banker))
            {
                return Math.Min(p.Resources, 3);
            }

            return 0;
        }

        private static IEnumerable<TreacheryCard> CardsPlayableAsDefense(Game g, Player p, TreacheryCard withWeapon)
        {
            if (g.Version <= 91)
            {
                return p.TreacheryCards.Where(c => c.IsDefense || c.Type == TreacheryCardType.Useless);
            }
            else
            {
                return p.TreacheryCards.Where(c =>
                c.Type != TreacheryCardType.WeirdingWay && (c.IsDefense || c.Type == TreacheryCardType.Useless) ||
                c.Type == TreacheryCardType.WeirdingWay && withWeapon != null && withWeapon.IsWeapon && withWeapon.Type != TreacheryCardType.Chemistry);
            }
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

        public static int DetermineSkillBonus(Game g, Battle plan, out LeaderSkill activatedSkill)
        {
            return DetermineSkillBonus(g, plan.Player, plan.Hero, plan.Weapon, plan.Defense, plan.BankerBonus, out activatedSkill);
        }

        public static int DetermineSkillBonus(Game g, Player player, IHero hero, TreacheryCard weapon, TreacheryCard defense, int bankerBonus, out LeaderSkill activatedSkill)
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

            if (weapon != null && !(weapon.IsWeapon || weapon.IsDefense || weapon.IsUseless))
            {
                if (g.SkilledAs(hero, LeaderSkill.Planetologist))
                {
                    activatedSkill = LeaderSkill.Planetologist;
                    return 2;
                }
            }

            if (defense != null && defense.IsPoisonDefense)
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

        public static int DetermineSkillPenalty(Game g, Battle plan, Player opponent, out LeaderSkill activatedSkill)
        {
            return DetermineSkillPenalty(g, plan.Hero, opponent, out activatedSkill);
        }

        public static int DetermineSkillPenalty(Game g, IHero hero, Player opponent, out LeaderSkill activatedSkill)
        {
            if (g.SkilledAs(hero, LeaderSkill.Bureaucrat))
            {
                activatedSkill = LeaderSkill.Bureaucrat;
                return g.Map.Strongholds.Count(sh => opponent.Occupies(sh));
            }

            activatedSkill = LeaderSkill.None;
            return 0;
        }
    }
}
