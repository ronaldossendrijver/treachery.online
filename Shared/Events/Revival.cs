/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Revival : GameEvent
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

            var costOfRevival = DetermineCost(Game, p, Hero, AmountOfForces, AmountOfSpecialForces, ExtraForcesPaidByRed, ExtraSpecialForcesPaidByRed);
            if (costOfRevival.TotalCostForPlayer > p.Resources) return Message.Express("You can't pay that many");

            if (AssignSkill && Hero == null) return Message.Express("You must revive a leader to assign a skill to");
            if (AssignSkill && !MayAssignSkill(Game, p, Hero)) return Message.Express("You can't assign a skill to this leader");

            return null;
        }

        public static int DetermineCostOfForcesForRed(Game g, Player red, Faction ally, int forces, int specialForces)
        {
            return (int)Math.Ceiling(forces * GetPricePerForce(g, red) + specialForces * GetPricePerSpecialForce(g, red, ally));
        }

        public static RevivalCost DetermineCost(Game g, Player initiator, IHero hero, int amountOfForces, int amountOfSpecialForces, int extraForcesPaidByRed, int extraSpecialForcesPaidByRed)
        {
            return new RevivalCost(g, initiator, hero, amountOfForces, amountOfSpecialForces, extraForcesPaidByRed, extraSpecialForcesPaidByRed);
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
                var vidal = p.Leaders.FirstOrDefault(l => l.HeroType == HeroType.PinkAndCyan);
                if (vidal != null && !g.IsAlive(vidal) && !result.Contains(vidal))
                {
                    result.Add(vidal);
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

        public static int ValidMaxRevivals(Game g, Player p, bool specialForces)
        {
            if (g.Version >= 124)
            {
                if (!specialForces)
                {
                    return Math.Min(g.GetRevivalLimit(g, p), p.ForcesKilled);
                }
                else
                {
                    return Math.Min(p.Is(Faction.Grey) ? g.GetRevivalLimit(g, p) : (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1), p.SpecialForcesKilled);
                }
            }
            else
            {
                var amountPaidByEmperor = RedExtraRevivalLimit(g, p);

                if (!specialForces)
                {
                    return Math.Min(g.GetRevivalLimit(g, p) + amountPaidByEmperor, p.ForcesKilled);
                }
                else
                {
                    return Math.Min(p.Is(Faction.Grey) ? g.GetRevivalLimit(g, p) + amountPaidByEmperor : (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1), p.SpecialForcesKilled);
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
                   (p.Ally == Faction.Purple && g.PurpleAllyMayReviveAsPurple && !g.Prevented(FactionAdvantage.PurpleRevivalDiscountAlly));
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


    }

    public class RevivalCost
    {
        public int TotalCostForPlayer;
        public int CostForForceRevivalForPlayer;
        public int CostForEmperor;
        public int CostToReviveHero;
        public bool CanBePaid;

        public RevivalCost(Game g, Player initiator, IHero hero, int amountOfForces, int amountOfSpecialForces, int extraForcesPaidByRed, int extraSpecialForcesPaidByRed)
        {
            if (g.Version >= 124)
            {
                CostForEmperor = initiator.Ally == Faction.Red ? Revival.DetermineCostOfForcesForRed(g, initiator.AlliedPlayer, initiator.Faction, extraForcesPaidByRed, extraSpecialForcesPaidByRed) : 0;
                CostForForceRevivalForPlayer = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces);
            }
            else
            {
                int costForForceRevival = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces);
                var amountPaidForByEmperor = Revival.ValidMaxRevivalsByRed(g, initiator);
                var emperor = g.GetPlayer(Faction.Red);
                var emperorsSpice = emperor != null ? emperor.Resources : 0;

                CostForEmperor = DetermineCostForEmperor(g, initiator.Faction, costForForceRevival, amountOfForces, amountOfSpecialForces, emperorsSpice, amountPaidForByEmperor);
                CostForForceRevivalForPlayer = costForForceRevival - CostForEmperor;
            }

            CostToReviveHero = Revival.GetPriceOfHeroRevival(g, initiator, hero);
            TotalCostForPlayer = CostForForceRevivalForPlayer + CostToReviveHero;
            CanBePaid = initiator.Resources >= TotalCostForPlayer;
        }

        public static int GetPriceOfForceRevival(Game g, Player initiator, int amountOfForces, int amountOfSpecialForces)
        {
            int nrOfFreeRevivals = g.FreeRevivals(initiator);
            int nrOfPaidSpecialForces = Math.Max(0, amountOfSpecialForces - nrOfFreeRevivals);
            int nrOfFreeRevivalsLeft = nrOfFreeRevivals - (amountOfSpecialForces - nrOfPaidSpecialForces);
            int nrOfPaidNormalForces = Math.Max(0, amountOfForces - nrOfFreeRevivalsLeft);
            int priceOfSpecialForces = initiator.Is(Faction.Grey) ? 3 : 2;
            int priceOfNormalForces = initiator.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival) ? 1 : 2;

            var cost = nrOfPaidSpecialForces * priceOfSpecialForces + nrOfPaidNormalForces * priceOfNormalForces;

            if (Revival.MayReviveWithDiscount(g, initiator))
            {
                cost = (int)Math.Ceiling(0.5 * cost);
            }

            return cost;
        }


        public int Total => TotalCostForPlayer + CostForEmperor;

        public int TotalCostForForceRevival => CostForForceRevivalForPlayer + CostForEmperor;

        public static int DetermineCostForEmperor(Game g, Faction initiator, int totalCostForForceRevival, int amountOfForces, int amountOfSpecialForces, int emperorsSpice, int amountPaidForByEmperor)
        {
            int priceOfSpecialForces = initiator == Faction.Grey ? 3 : 2;
            int priceOfNormalForces = initiator == Faction.Brown && !g.Prevented(FactionAdvantage.BrownRevival) && g.Version >= 122 ? 1 : 2;

            int specialForcesPaidByEmperor = 0;
            while (
                (specialForcesPaidByEmperor + 1) <= amountOfSpecialForces &&
                (specialForcesPaidByEmperor + 1) * priceOfSpecialForces <= emperorsSpice &&
                specialForcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            {
                specialForcesPaidByEmperor++;
            }

            int forcesPaidByEmperor = 0;
            while (
                (forcesPaidByEmperor + 1) <= amountOfForces &&
                specialForcesPaidByEmperor * priceOfSpecialForces + (forcesPaidByEmperor + 1) * priceOfNormalForces <= emperorsSpice &&
                specialForcesPaidByEmperor + forcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            {
                forcesPaidByEmperor++;
            }

            int costForEmperor = specialForcesPaidByEmperor * priceOfSpecialForces + forcesPaidByEmperor * priceOfNormalForces;
            return Math.Min(totalCostForForceRevival, Math.Min(costForEmperor, emperorsSpice));
        }
    }
}
