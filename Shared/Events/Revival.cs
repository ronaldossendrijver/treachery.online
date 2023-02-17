/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Revival : GameEvent, ILocationEvent
    {
        public int _heroId;

        public Revival(Game game) : base(game)
        {
        }

        public Revival()
        {
        }

        public int AmountOfForces { get; set; } = 0;

        public int AmountOfSpecialForces { get; set; } = 0;

        public int ExtraForcesPaidByRed { get; set; } = 0;

        public int ExtraSpecialForcesPaidByRed { get; set; } = 0;

        public int _locationId = -1;

        [JsonIgnore]
        public Location Location { get { return Game.Map.LocationLookup.Find(_locationId); } set { _locationId = Game.Map.LocationLookup.GetId(value); } }

        [JsonIgnore]
        public Location To => Location;

        [JsonIgnore]
        public int TotalAmountOfForces => Initiator == Faction.Yellow ? AmountOfSpecialForces + ExtraForcesPaidByRed : AmountOfForces + ExtraSpecialForcesPaidByRed;


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

        public bool AssignSkill { get; set; } = false;

        public bool UsesRedSecretAlly { get; set; }

        public override Message Validate()
        {
            var p = Player;

            if (AmountOfForces + ExtraForcesPaidByRed <= 0 && AmountOfSpecialForces + ExtraSpecialForcesPaidByRed <= 0 && Hero == null) Message.Express("Select forces or a leader to revive");

            if (AmountOfForces + ExtraForcesPaidByRed > p.ForcesKilled) return Message.Express("You can't revive that many ", p.Force);
            if (AmountOfSpecialForces + ExtraSpecialForcesPaidByRed > p.SpecialForcesKilled) return Message.Express("You can't revive that many ", p.SpecialForce);

            if (Initiator != Faction.Grey && AmountOfSpecialForces + ExtraSpecialForcesPaidByRed > 1) return Message.Express("You can't revive more than one ", p.SpecialForce, " per turn");
            if (AmountOfSpecialForces + ExtraSpecialForcesPaidByRed > 0 && Initiator != Faction.Grey && Game.FactionsThatRevivedSpecialForcesThisTurn.Contains(Initiator)) return Message.Express("You already revived a ", p.SpecialForce, " this turn");

            if (Game.Version >= 124)
            {
                var limit = Game.GetRevivalLimit(Game, p);
                if (AmountOfForces + AmountOfSpecialForces > limit) return Message.Express("You can't revive more than your limit of ", limit);

                var allyLimit = ValidMaxRevivalsByRed(Game, p);
                if (ExtraForcesPaidByRed + ExtraSpecialForcesPaidByRed > ValidMaxRevivalsByRed(Game, p)) return Message.Express("Your ally won't revive more than ", allyLimit);
            }
            else
            {
                int emperorRevivals = ValidMaxRevivalsByRed(Game, p);

                int limit = Game.GetRevivalLimit(Game, p);

                if (AmountOfForces + AmountOfSpecialForces > limit + emperorRevivals) Message.Express("You can't revive that many");
            }

            var costOfRevival = DetermineCost(Game, p, Hero, AmountOfForces, AmountOfSpecialForces, ExtraForcesPaidByRed, ExtraSpecialForcesPaidByRed, UsesRedSecretAlly);
            if (costOfRevival.TotalCostForPlayer > p.Resources) return Message.Express("You can't pay that many");

            if (AssignSkill && Hero == null) return Message.Express("You must revive a leader to assign a skill to");
            if (AssignSkill && !MayAssignSkill(Game, p, Hero)) return Message.Express("You can't assign a skill to this leader");

            if (UsesRedSecretAlly && !MayUseRedSecretAlly(Game, Player)) return Message.Express("you can't use ", Faction.Red, " cunning");

            if (Location != null)
            {
                if (!MaySelectLocationForRevivedForces(Game, Player, AmountOfForces + ExtraForcesPaidByRed, AmountOfSpecialForces + ExtraSpecialForcesPaidByRed, UsesRedSecretAlly)) return Message.Express("You can't place revived forces directly on the planet");
                if (!ValidRevivedForceLocations(Game, Player).Contains(Location)) return Message.Express("You can't place revived forces there");
            }

            return null;
        }

        public static int DetermineCostOfForcesForRed(Game g, Player red, Faction ally, int forces, int specialForces)
        {
            return (int)Math.Ceiling(forces * GetPricePerForce(g, red) + specialForces * GetPricePerSpecialForce(g, red, ally));
        }

        public static RevivalCost DetermineCost(Game g, Player initiator, IHero hero, int amountOfForces, int amountOfSpecialForces, int extraForcesPaidByRed, int extraSpecialForcesPaidByRed, bool usesRedSecretAlly)
        {
            return new RevivalCost(g, initiator, hero, amountOfForces, amountOfSpecialForces, extraForcesPaidByRed, extraSpecialForcesPaidByRed, usesRedSecretAlly);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " perform revival");
        }

        public static IEnumerable<IHero> ValidRevivalHeroes(Game g, Player p)
        {
            var result = new List<IHero>();

            if (p.Faction != Faction.Purple)
            {
                result.AddRange(NormallyRevivableHeroes(g, p));
            }
            else if (p.Leaders.Count(l => g.LeaderState[l].Alive) < 5)
            {
                result.AddRange(UnrestrictedRevivableHeroes(g, p));
            }

            if (result.Count == 0)
            {
                var purple = g.GetPlayer(Faction.Purple);
                var gholas = purple != null ? purple.Leaders.Where(l => l.Faction == p.Faction) : Array.Empty<Leader>();

                return g.KilledHeroes(p).Union(gholas).Where(h => g.IsAllowedEarlyRevival(h));
            }

            int livingLeaders = p.Leaders.Count(l => g.LeaderState[l].Alive);
            if (g.Version >= 139)
            {
                livingLeaders += g.Players.Where(player => player != p).SelectMany(player => player.Leaders.Where(l => l.Faction == p.Faction)).Count();
            }

            if (p.Is(Faction.Purple) && g.Applicable(Rule.PurpleGholas) && livingLeaders < 5)
            {
                result.AddRange(
                    g.LeaderState.Where(leaderAndState =>
                    leaderAndState.Key.Faction != Faction.Purple &&
                    leaderAndState.Key != LeaderManager.Messiah &&
                    leaderAndState.Key.HeroType != HeroType.Auditor &&
                    !leaderAndState.Value.Alive).Select(kvp => kvp.Key));
            }

            return result;
        }

        public static IEnumerable<IHero> NormallyRevivableHeroes(Game g, Player p)
        {
            var result = new List<IHero>();

            if (AllAvailableLeadersHaveDiedOnce(g, p) || AtLeastFiveLeadersHaveDiedOnce(g, p))
            {
                int lowestDeathCount = p.Leaders.Min(l => g.LeaderState[l].DeathCounter);

                result.AddRange(p.Leaders.Where(l => g.LeaderState[l].DeathCounter == lowestDeathCount && !g.LeaderState[l].Alive));

                if (p.Is(Faction.Green) && !g.IsAlive(LeaderManager.Messiah) && g.LeaderState[LeaderManager.Messiah].DeathCounter == lowestDeathCount)
                {
                    result.Add(LeaderManager.Messiah);
                }
            }

            if (p.Faction == Faction.Brown)
            {
                var auditor = p.Leaders.FirstOrDefault(l => l.HeroType == HeroType.Auditor);
                if (auditor != null && !g.IsAlive(auditor) && !result.Contains(auditor))
                {
                    result.Add(auditor);
                }
            }

            if (p.Faction == Faction.Pink)
            {
                if (!g.IsAlive(g.Vidal) && !result.Contains(g.Vidal))
                {
                    result.Add(g.Vidal);
                }
            }

            return result;

        }

        private static bool AtLeastFiveLeadersHaveDiedOnce(Game g, Player p) => p.Leaders.Count(l => g.LeaderState[l].DeathCounter > 0) >= 5;

        private static bool AllAvailableLeadersHaveDiedOnce(Game g, Player p) => p.Leaders.All(l => g.LeaderState[l].DeathCounter > 0);


        public static IEnumerable<IHero> UnrestrictedRevivableHeroes(Game g, Player p)
        {
            var result = new List<IHero>();

            result.AddRange(p.Leaders.Where(l => !g.LeaderState[l].Alive && l.HeroType != HeroType.Auditor));

            if (p.Is(Faction.Green) && !g.IsAlive(LeaderManager.Messiah))
            {
                result.Add(LeaderManager.Messiah);
            }

            return result;
        }

        public static int ValidMaxRevivals(Game g, Player p, bool specialForces, bool usingRedCunning)
        {
            int increasedRevivalDueToRedCunning = usingRedCunning ? 3 : 0;
            int normalForceRevivalLimit = g.GetRevivalLimit(g, p) + increasedRevivalDueToRedCunning;

            if (g.Version >= 124)
            {
                if (!specialForces)
                {
                    return Math.Min(normalForceRevivalLimit, p.ForcesKilled);
                }
                else
                {
                    return Math.Min(p.Is(Faction.Grey) ? normalForceRevivalLimit : (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1), p.SpecialForcesKilled);
                }
            }
            else
            {
                var amountPaidByEmperor = RedExtraRevivalLimit(g, p);

                if (!specialForces)
                {
                    return Math.Min(normalForceRevivalLimit + amountPaidByEmperor, p.ForcesKilled);
                }
                else
                {
                    return Math.Min(p.Is(Faction.Grey) ? normalForceRevivalLimit + amountPaidByEmperor : (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1), p.SpecialForcesKilled);
                }
            }
        }

        public static bool MayAssignSkill(Game g, Player p, IHero h)
        {
            var capturedLeadersToConsider = g.IsPlaying(Faction.Black) ? g.GetPlayer(Faction.Black).Leaders.Where(l => l.Faction == p.Faction) : Array.Empty<Leader>();

            return
                h is Leader &&
                (g.Version < 147 || h.Faction == p.Faction) &&
                h.HeroType != HeroType.Auditor &&
                g.Applicable(Rule.LeaderSkills) &&
                !g.CapturedLeaders.Any(cl => cl.Value == p.Faction && g.IsSkilled(cl.Key)) &&
                !p.Leaders.Any(l => g.IsSkilled(l)) &&
                !capturedLeadersToConsider.Any(l => g.IsSkilled(l));
        }

        public static bool MayReviveWithDiscount(Game g, Player p)
        {
            return (p.Is(Faction.Purple) && !g.Prevented(FactionAdvantage.PurpleRevivalDiscount)) ||
                   (p.Ally == Faction.Purple && g.AllyMayReviveAsPurple && !g.Prevented(FactionAdvantage.PurpleRevivalDiscountAlly));
        }


        public static int GetPriceOfHeroRevival(Game g, Player initiator, IHero hero)
        {
            bool purpleDiscountPrevented = g.Prevented(FactionAdvantage.PurpleRevivalDiscount);

            if (hero == null) return 0;

            var price = hero.CostToRevive;

            if (g.Version < 102)
            {
                if (initiator.Is(Faction.Purple) && !purpleDiscountPrevented)
                {
                    price = (int)Math.Ceiling(0.5 * price);
                }

                if (!NormallyRevivableHeroes(g, initiator).Contains(hero) && g.EarlyRevivalsOffers.ContainsKey(hero))
                {
                    price = g.EarlyRevivalsOffers[hero];
                }
            }
            else
            {
                if (initiator.Is(Faction.Purple) && !purpleDiscountPrevented)
                {
                    price = (int)Math.Ceiling(0.5 * price);
                }
                else if (!NormallyRevivableHeroes(g, initiator).Contains(hero) && g.EarlyRevivalsOffers.ContainsKey(hero))
                {
                    price = g.EarlyRevivalsOffers[hero];
                }
            }

            return price;
        }

        public static float GetPricePerForce(Game g, Player revivingPlayer) =>
            (MayReviveWithDiscount(g, revivingPlayer) ? 0.5f : 1) * (revivingPlayer.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival) ? 1 : 2);

        public static float GetPricePerSpecialForce(Game g, Player revivingPlayer, Faction ofRevivedForces) =>
            (MayReviveWithDiscount(g, revivingPlayer) ? 0.5f : 1) * (ofRevivedForces == Faction.Grey ? 3 : 2);

        public static int RedExtraRevivalLimit(Game g, Player p) => p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces)) ? g.RedWillPayForExtraRevival : 0;

        public static int ValidMaxRevivalsByRed(Game g, Player p)
        {
            if (p.Ally != Faction.Red) return 0;

            var red = g.GetPlayer(Faction.Red);

            int potentialMaximum = p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces)) ? g.RedWillPayForExtraRevival : 0;

            if (g.Version < 115)
            {
                return potentialMaximum;
            }
            else
            {
                int priceOfSpecialForces = p.Faction == Faction.Grey ? 3 : 2;

                int specialForcesPaidByEmperor = 0;
                while (
                    (specialForcesPaidByEmperor + 1) * priceOfSpecialForces <= red.Resources &&
                    specialForcesPaidByEmperor + 1 <= potentialMaximum)
                {
                    specialForcesPaidByEmperor++;
                }

                int forcesPaidByEmperor = 0;
                while (
                    specialForcesPaidByEmperor * priceOfSpecialForces + (forcesPaidByEmperor + 1) * 2 <= red.Resources &&
                    specialForcesPaidByEmperor + forcesPaidByEmperor + 1 <= potentialMaximum)
                {
                    forcesPaidByEmperor++;
                }

                return specialForcesPaidByEmperor + forcesPaidByEmperor;
            }
        }

        public static bool MayUseRedSecretAlly(Game game, Player player) => player.Nexus == Faction.Red && NexusPlayed.CanUseSecretAlly(game, player);

        public static bool MaySelectLocationForRevivedForces(Game game, Player player, int forces, int specialForces, bool usesRedSecretAlly) => 
            (player.Is(Faction.Yellow) && specialForces >= 1 || player.Is(Faction.Purple) && forces >= 1 && game.FreeRevivals(player, usesRedSecretAlly) > 0) && 
            player.HasHighThreshold() && ValidRevivedForceLocations(game, player).Any();

        public static IEnumerable<Location> ValidRevivedForceLocations(Game g, Player p)
        {
            return g.Map.Locations(false).Where(l => 
                    (p.Faction == Faction.Yellow || l.Sector != g.SectorInStorm) && 
                    (!l.Territory.IsStronghold || g.NrOfOccupantsExcludingPlayer(l, p) < 2) &&
                    (!p.HasAlly || l == g.Map.PolarSink || !p.AlliedPlayer.Occupies(l)) &&
                    (p.Faction == Faction.Purple || p.Faction == Faction.Yellow && p.AnyForcesIn(l.Territory) >= 1));
        }
    }
}
