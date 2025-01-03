/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class Revival : GameEvent, ILocationEvent
{
    #region Construction

    public Revival(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Revival()
    {
    }

    #endregion Construction

    #region Properties

    public int AmountOfForces { get; set; } = 0;

    public int AmountOfSpecialForces { get; set; } = 0;

    public int ExtraForcesPaidByRed { get; set; } = 0;

    public int ExtraSpecialForcesPaidByRed { get; set; } = 0;

    public int NumberOfForcesInLocation { get; set; }

    public int NumberOfSpecialForcesInLocation { get; set; }

    public int _locationId = -1;

    [JsonIgnore]
    public Location Location
    {
        get => Game.Map.LocationLookup.Find(_locationId);
        set => _locationId = Game.Map.LocationLookup.GetId(value);
    }

    [JsonIgnore]
    public Location To => Location;

    public int _heroId;

    [JsonIgnore]
    public IHero Hero
    {
        get => LeaderManager.HeroLookup.Find(_heroId);
        set => _heroId = LeaderManager.HeroLookup.GetId(value);
    }

    public bool AssignSkill { get; set; } = false;

    public bool UsesRedSecretAlly { get; set; }

    [JsonIgnore]
    public int TotalAmountOfForcesAddedToLocation => NumberOfForcesInLocation + NumberOfSpecialForcesInLocation;

    [JsonIgnore]
    public int ForcesAddedToLocation => NumberOfForcesInLocation;

    [JsonIgnore]
    public int SpecialForcesAddedToLocation => NumberOfSpecialForcesInLocation;

    #endregion Properties

    #region Validation

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
            var limit = Game.GetRevivalLimit(Game, p, UsesRedSecretAlly);
            if (UsesRedSecretAlly) limit += 3;
            if (AmountOfForces + AmountOfSpecialForces > limit) return Message.Express("You can't revive more than your limit of ", limit);

            var allyLimit = ValidMaxRevivalsByRed(Game, p);
            if (ExtraForcesPaidByRed + ExtraSpecialForcesPaidByRed > ValidMaxRevivalsByRed(Game, p)) return Message.Express("Your ally won't revive more than ", allyLimit);
        }
        else
        {
            var emperorRevivals = ValidMaxRevivalsByRed(Game, p);
            var limit = Game.GetRevivalLimit(Game, p, UsesRedSecretAlly);
            if (AmountOfForces + AmountOfSpecialForces > limit + emperorRevivals) Message.Express("You can't revive that many");
        }

        var costOfRevival = DetermineCost(Game, p, Hero, AmountOfForces, AmountOfSpecialForces, ExtraForcesPaidByRed, ExtraSpecialForcesPaidByRed, UsesRedSecretAlly);
        if (costOfRevival.TotalCostForPlayer > p.Resources) return Message.Express("You can't pay that many");

        if (AssignSkill && Hero == null) return Message.Express("You must revive a leader to assign a skill to");
        if (AssignSkill && !MayAssignSkill(Game, p, Hero)) return Message.Express("You can't assign a skill to this leader");

        if (UsesRedSecretAlly && !MayUseRedSecretAlly(Game, Player)) return Message.Express("you can't use ", Faction.Red, " cunning");

        if (Location != null)
        {
            if (!ValidRevivedForceLocations(Game, Player).Contains(Location)) return Message.Express("You can't place revived forces there");

            if (NumberOfForcesInLocation > NumberOfForcesThatMayBePlacedOnPlanet(Game, Player, UsesRedSecretAlly, AmountOfForces + ExtraForcesPaidByRed) ||
                NumberOfSpecialForcesInLocation > NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, AmountOfSpecialForces + ExtraSpecialForcesPaidByRed))
                return Message.Express("You can't place that many forces directly on the planet");
        }

        return null;
    }

    public static int DetermineCostOfForcesForRed(Game g, Player red, Faction ally, int forces, int specialForces)
    {
        return (int)Math.Ceiling(forces * GetPricePerForce(g, red) + specialForces * GetPricePerSpecialForce(g, red, ally));
    }

    public static RevivalCost DetermineCost(Game g, Player initiator, IHero hero, int amountOfForces, int amountOfSpecialForces, int extraForcesPaidByRed, int extraSpecialForcesPaidByRed, bool usesRedSecretAlly)
    {
        var result = new RevivalCost();

        if (g.Version >= 124)
        {
            result.CostForEmperor = initiator.Ally == Faction.Red ? DetermineCostOfForcesForRed(g, initiator.AlliedPlayer, initiator.Faction, extraForcesPaidByRed, extraSpecialForcesPaidByRed) : 0;
            result.CostForForceRevivalForPlayer = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces, usesRedSecretAlly, out var nrOfPaidSpecialForces, out var numberOfForcesRevivedForFree);
            result.IncludesCostsForSpecialForces = nrOfPaidSpecialForces > 0;
            result.NumberOfForcesRevivedForFree = numberOfForcesRevivedForFree;
        }
        else
        {
            var costForForceRevival = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces, usesRedSecretAlly, out var nrOfPaidSpecialForces, out var numberOfForcesRevivedForFree);
            var amountPaidForByEmperor = ValidMaxRevivalsByRed(g, initiator);
            var emperor = g.GetPlayer(Faction.Red);
            var emperorsSpice = emperor != null ? emperor.Resources : 0;

            result.CostForEmperor = DetermineCostForEmperor(g, initiator.Faction, costForForceRevival, amountOfForces, amountOfSpecialForces, emperorsSpice, amountPaidForByEmperor);
            result.CostForForceRevivalForPlayer = costForForceRevival - result.CostForEmperor;
            result.IncludesCostsForSpecialForces = nrOfPaidSpecialForces > 0;
            result.NumberOfForcesRevivedForFree = numberOfForcesRevivedForFree;
        }

        result.CostToReviveHero = GetPriceOfHeroRevival(g, initiator, hero);
        result.CanBePaid = initiator.Resources >= result.TotalCostForPlayer;

        return result;
    }

    public static int GetPriceOfForceRevival(Game g, Player initiator, int amountOfForces, int amountOfSpecialForces, bool usesRedSecretAlly, out int nrOfPaidSpecialForces, out int numberOfForcesRevivedForFree)
    {
        var nrOfFreeRevivals = g.FreeRevivals(initiator, usesRedSecretAlly);
        nrOfPaidSpecialForces = initiator.Is(Faction.Red) && initiator.HasLowThreshold(World.RedStar) ? amountOfSpecialForces : Math.Max(0, amountOfSpecialForces - nrOfFreeRevivals);

        var nrOfFreeRevivalsLeft = nrOfFreeRevivals - (amountOfSpecialForces - nrOfPaidSpecialForces);
        var nrOfPaidNormalForces = Math.Max(0, amountOfForces - nrOfFreeRevivalsLeft);
        numberOfForcesRevivedForFree = amountOfForces + amountOfSpecialForces - nrOfPaidSpecialForces - nrOfPaidNormalForces;

        var priceOfSpecialForces = initiator.Is(Faction.Grey) ? 3 : 2;
        var priceOfNormalForces = initiator.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival) ? 1 : 2;
        var cost = nrOfPaidSpecialForces * priceOfSpecialForces + nrOfPaidNormalForces * priceOfNormalForces;

        if (MayReviveWithDiscount(g, initiator)) cost = (int)Math.Ceiling(0.5 * cost);

        return cost;
    }

    private static int DetermineCostForEmperor(Game g, Faction initiator, int totalCostForForceRevival, int amountOfForces, int amountOfSpecialForces, int emperorsSpice, int amountPaidForByEmperor)
    {
        var priceOfSpecialForces = initiator == Faction.Grey ? 3 : 2;
        var priceOfNormalForces = initiator == Faction.Brown && !g.Prevented(FactionAdvantage.BrownRevival) && g.Version >= 122 ? 1 : 2;

        var specialForcesPaidByEmperor = 0;
        while (
            specialForcesPaidByEmperor + 1 <= amountOfSpecialForces &&
            (specialForcesPaidByEmperor + 1) * priceOfSpecialForces <= emperorsSpice &&
            specialForcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            specialForcesPaidByEmperor++;

        var forcesPaidByEmperor = 0;
        while (
            forcesPaidByEmperor + 1 <= amountOfForces &&
            specialForcesPaidByEmperor * priceOfSpecialForces + (forcesPaidByEmperor + 1) * priceOfNormalForces <= emperorsSpice &&
            specialForcesPaidByEmperor + forcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            forcesPaidByEmperor++;

        var costForEmperor = specialForcesPaidByEmperor * priceOfSpecialForces + forcesPaidByEmperor * priceOfNormalForces;
        return Math.Min(totalCostForForceRevival, Math.Min(costForEmperor, emperorsSpice));
    }

    public static IEnumerable<IHero> ValidRevivalHeroes(Game g, Player p)
    {
        var result = new List<IHero>();

        if (p.Faction != Faction.Purple)
            result.AddRange(NormallyRevivableHeroes(g, p));
        else if (p.Leaders.Count(l => g.IsAlive(l)) < 5) result.AddRange(UnrestrictedRevivableHeroes(g, p));

        if (result.Count == 0)
        {
            var purple = g.GetPlayer(Faction.Purple);
            var gholas = purple != null ? purple.Leaders.Where(l => l.Faction == p.Faction) : Array.Empty<Leader>();

            return g.KilledHeroes(p).Union(gholas).Where(h => IsAllowedEarlyRevival(g, h));
        }

        var livingLeaders = p.Leaders.Count(l => g.IsAlive(l));
        if (g.Version >= 139) livingLeaders += g.Players.Where(player => player != p).SelectMany(player => player.Leaders.Where(l => l.Faction == p.Faction)).Count();

        if (p.Is(Faction.Purple) && g.Applicable(Rule.PurpleGholas) && livingLeaders < 5 && (g.Version < 164 || !g.Prevented(FactionAdvantage.PurpleReviveGhola)))
            result.AddRange(
                g.LeaderState.Where(leaderAndState =>
                    leaderAndState.Key.Faction != Faction.Purple &&
                    leaderAndState.Key != LeaderManager.Messiah &&
                    leaderAndState.Key.HeroType != HeroType.Auditor &&
                    !leaderAndState.Value.Alive).Select(kvp => kvp.Key));

        return result;
    }

    public static bool IsAllowedEarlyRevival(Game g, IHero h)
    {
        return g.EarlyRevivalsOffers.TryGetValue(h, out var value) && value < int.MaxValue;
    }

    public static IEnumerable<IHero> NormallyRevivableHeroes(Game g, Player p)
    {
        var result = new List<IHero>();

        if (AllAvailableLeadersHaveDiedOnce(g, p) || AtLeastFiveLeadersHaveDiedOnce(g, p))
            if (p.Leaders.Any())
            {
                var lowestDeathCount = p.Leaders.Min(l => g.DeathCount(l));

                result.AddRange(p.Leaders.Where(l => g.DeathCount(l) == lowestDeathCount && !g.IsAlive(l)));

                if (p.Is(Faction.Green) && !g.IsAlive(LeaderManager.Messiah) && g.DeathCount(LeaderManager.Messiah) == lowestDeathCount) result.Add(LeaderManager.Messiah);
            }

        if (p.Faction == Faction.Brown)
        {
            var auditor = p.Leaders.FirstOrDefault(l => l.HeroType == HeroType.Auditor);
            if (auditor != null && !g.IsAlive(auditor) && !result.Contains(auditor)) result.Add(auditor);
        }

        if (p.Faction == Faction.Pink)
            if (!g.IsAlive(g.Vidal) && !result.Contains(g.Vidal)) result.Add(g.Vidal);

        return result;

    }

    private static bool AtLeastFiveLeadersHaveDiedOnce(Game g, Player p)
    {
        return p.Leaders.Count(l => g.DeathCount(l) > 0) >= 5;
    }

    private static bool AllAvailableLeadersHaveDiedOnce(Game g, Player p)
    {
        return p.Leaders.All(l => g.DeathCount(l) > 0);
    }

    public static IEnumerable<IHero> UnrestrictedRevivableHeroes(Game g, Player p)
    {
        var result = new List<IHero>();

        result.AddRange(p.Leaders.Where(l => !g.IsAlive(l) && l.HeroType != HeroType.Auditor));

        if (p.Is(Faction.Green) && !g.IsAlive(LeaderManager.Messiah)) result.Add(LeaderManager.Messiah);

        return result;
    }

    public static int ValidMaxRevivals(Game g, Player p, bool specialForces, bool usingRedSecretAlly)
    {
        var increasedRevivalDueToRedSecretAlly = usingRedSecretAlly ? 3 : 0;
        var normalForceRevivalLimit = g.GetRevivalLimit(g, p, usingRedSecretAlly) + increasedRevivalDueToRedSecretAlly;

        if (g.Version >= 124)
        {
            if (!specialForces)
                return Math.Min(normalForceRevivalLimit, p.ForcesKilled);
            return Math.Min(p.Is(Faction.Grey) ? normalForceRevivalLimit : g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1, p.SpecialForcesKilled);
        }

        var amountPaidByEmperor = RedExtraRevivalLimit(g, p);

        if (!specialForces)
            return Math.Min(normalForceRevivalLimit + amountPaidByEmperor, p.ForcesKilled);
        
        return Math.Min(p.Is(Faction.Grey) ? normalForceRevivalLimit + amountPaidByEmperor : g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction) ? 0 : 1, p.SpecialForcesKilled);
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
            !(p.Is(Faction.Pink) && g.IsSkilled(g.Vidal)) &&
            !p.Leaders.Any(l => g.IsSkilled(l)) &&
            !capturedLeadersToConsider.Any(l => g.IsSkilled(l));
    }

    public static bool MayReviveWithDiscount(Game g, Player p)
    {
        return (p.Is(Faction.Purple) && !g.Prevented(FactionAdvantage.PurpleRevivalDiscount)) ||
               (p.Ally == Faction.Purple && g.PurpleAllowsRevivalDiscount && !g.Prevented(FactionAdvantage.PurpleRevivalDiscountAlly));
    }


    public static int GetPriceOfHeroRevival(Game g, Player initiator, IHero hero)
    {
        var purpleDiscountPrevented = g.Prevented(FactionAdvantage.PurpleRevivalDiscount);

        if (hero == null) return 0;

        var price = hero.CostToRevive;

        if (g.Version < 102)
        {
            if (initiator.Is(Faction.Purple) && !purpleDiscountPrevented) price = (int)Math.Ceiling(0.5 * price);

            if (!NormallyRevivableHeroes(g, initiator).Contains(hero) && g.EarlyRevivalsOffers.ContainsKey(hero)) price = g.EarlyRevivalsOffers[hero];
        }
        else
        {
            if (initiator.Is(Faction.Purple) && !purpleDiscountPrevented)
                price = (int)Math.Ceiling(0.5 * price);
            else if (!NormallyRevivableHeroes(g, initiator).Contains(hero) && g.EarlyRevivalsOffers.ContainsKey(hero)) price = g.EarlyRevivalsOffers[hero];
        }

        return price;
    }

    private static float GetPricePerForce(Game g, Player revivingPlayer)
    {
        return (MayReviveWithDiscount(g, revivingPlayer) ? 0.5f : 1) *
               (revivingPlayer.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival) ? 1 : 2);
    }

    private static float GetPricePerSpecialForce(Game g, Player revivingPlayer, Faction ofRevivedForces)
    {
        return (MayReviveWithDiscount(g, revivingPlayer) ? 0.5f : 1) * (ofRevivedForces == Faction.Grey ? 3 : 2);
    }

    private static int RedExtraRevivalLimit(Game g, Player p)
    {
        return p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces))
            ? g.RedWillPayForExtraRevival
            : 0;
    }

    public static int ValidMaxRevivalsByRed(Game g, Player p)
    {
        if (p.Ally != Faction.Red) return 0;

        var red = g.GetPlayer(Faction.Red);

        var potentialMaximum = p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces)) ? g.RedWillPayForExtraRevival : 0;

        if (g.Version < 115) return potentialMaximum;

        var priceOfSpecialForces = p.Faction == Faction.Grey ? 3 : 2;

        var specialForcesPaidByEmperor = 0;
        while (
            (specialForcesPaidByEmperor + 1) * priceOfSpecialForces <= red.Resources &&
            specialForcesPaidByEmperor + 1 <= potentialMaximum)
            specialForcesPaidByEmperor++;

        var forcesPaidByEmperor = 0;
        while (
            specialForcesPaidByEmperor * priceOfSpecialForces + (forcesPaidByEmperor + 1) * 2 <= red.Resources &&
            specialForcesPaidByEmperor + forcesPaidByEmperor + 1 <= potentialMaximum)
            forcesPaidByEmperor++;

        return specialForcesPaidByEmperor + forcesPaidByEmperor;
    }

    public static bool MayUseRedSecretAlly(Game game, Player player)
    {
        return player.Nexus == Faction.Red && NexusPlayed.CanUseSecretAlly(game, player);
    }

    public static int NumberOfForcesThatMayBePlacedOnPlanet(Game game, Player player, bool usesRedSecretAlly, int nrOfForcesToRevive)
    {
        return player.Is(Faction.Purple) && player.HasHighThreshold()
            ? Math.Min(nrOfForcesToRevive, game.FreeRevivals(player, usesRedSecretAlly))
            : 0;
    }

    public static int NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player player, int nrOfSpecialForcesToRevive)
    {
        return player.Is(Faction.Yellow) && player.HasHighThreshold() ? Math.Min(nrOfSpecialForcesToRevive, 1) : 0;
    }

    public static IEnumerable<Location> ValidRevivedForceLocations(Game g, Player p)
    {
        return g.Map.Locations(false).Where(l =>
            (p.Faction == Faction.Yellow || l.Sector != g.SectorInStorm) &&
            (!l.Territory.IsStronghold || g.NrOfOccupantsExcludingFaction(l, p.Faction) < 2) &&
            l.Visible &&
            (!p.HasAlly || l == g.Map.PolarSink || !p.AlliedPlayer.Occupies(l)) &&
            (p.Faction == Faction.Purple || (p.Faction == Faction.Yellow && p.AnyForcesIn(l.Territory) >= 1)));
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (UsesRedSecretAlly) Game.PlayNexusCard(Player, "Secret Ally", "revive ", 3, " additional forces beyond revival limits for free");

        //Payment
        var cost = DetermineCost(Game, Player, Hero, AmountOfForces, AmountOfSpecialForces, ExtraForcesPaidByRed, ExtraSpecialForcesPaidByRed, UsesRedSecretAlly);
        if (cost.CostForEmperor > 0)
        {
            var emperor = GetPlayer(Faction.Red);
            emperor.Resources -= cost.CostForEmperor;
        }
        Player.Resources -= cost.TotalCostForPlayer;

        var highThresholdBonus = Initiator == Faction.Grey && Player.HasHighThreshold() && (Game.Version < 169 || cost.IncludesCostsForSpecialForces) ? 
            Math.Max(0, Math.Min(2, Player.ForcesKilled - AmountOfForces - ExtraForcesPaidByRed)) : 
            0;

        //Force revival
        Player.ReviveForces(AmountOfForces + ExtraForcesPaidByRed + highThresholdBonus);
        Player.ReviveSpecialForces(AmountOfSpecialForces + ExtraSpecialForcesPaidByRed);

        if (AmountOfSpecialForces > 0) Game.FactionsThatRevivedSpecialForcesThisTurn.Add(Initiator);

        //Register free revival
        var usesFreeRevival = false;
        if (AmountOfForces + AmountOfSpecialForces > 0 && Game.FreeRevivals(Player, UsesRedSecretAlly) > 0)
        {
            usesFreeRevival = true;
            Game.FactionsThatTookFreeRevival.Add(Initiator);
        }

        //Tech token activated?
        if (usesFreeRevival && Initiator != Faction.Purple) Game.RevivalTechTokenIncome = true;

        //Purple income
        var purple = GetPlayer(Faction.Purple);
        var totalProfitsForPurple = 0;
        if (purple != null)
        {
            if (usesFreeRevival && !Game.PurpleStartedRevivalWithLowThreshold) totalProfitsForPurple += 1;

            if (Initiator != Faction.Purple)
            {
                totalProfitsForPurple += cost.TotalCostForForceRevival;
                totalProfitsForPurple += cost.CostToReviveHero;
            }

            if (totalProfitsForPurple > 0 && Game.Prevented(FactionAdvantage.PurpleReceiveRevive))
            {
                totalProfitsForPurple = 0;
                Game.LogPreventionByKarma(FactionAdvantage.PurpleReceiveRevive);
                if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.PurpleReceiveRevive);
            }

            purple.Resources += totalProfitsForPurple;

            if (totalProfitsForPurple >= 5) Game.ApplyBureaucracy(Initiator, Faction.Purple);

            if (cost.TotalCost - totalProfitsForPurple >= 4) Game.ActivateBanker(Player);
        }

        //Hero revival
        var asGhola = false;
        if (Hero != null)
        {
            if (Initiator != Hero.Faction && Hero is Leader leader)
            {
                asGhola = true;
                Game.Revive(Player, leader);
            }
            else if (purple != null && purple.Leaders.Contains(Hero) && Game.IsAlive(Hero))
            {
                //Transfer of ghola
                purple.Leaders.Remove(Hero as Leader);
                Player.Leaders.Add(Hero as Leader);
            }
            else
            {
                Game.Revive(Player, Hero);
            }

            if (AssignSkill) Game.PrepareSkillAssignmentToRevivedLeader(Player, Hero as Leader);

            Game.EarlyRevivalsOffers.Remove(Hero);
        }

        //Logging
        Game.Stone(Milestone.Revival);
        LogRevival(cost, totalProfitsForPurple, asGhola, highThresholdBonus);

        if (Location != null)
        {
            if (NumberOfSpecialForcesInLocation > 0 || NumberOfForcesInLocation > 0)
            {
                var normalForceMessage = Message.Express();
                if (NumberOfForcesInLocation > 0)
                {
                    Player.ShipForces(Location, NumberOfForcesInLocation);
                    normalForceMessage = Message.Express(NumberOfForcesInLocation, Player.Force);
                }
                
                var specialForceMessage = Message.Express();
                if (NumberOfSpecialForcesInLocation > 0)
                {
                    Player.ShipSpecialForces(Location, NumberOfSpecialForcesInLocation);
                    specialForceMessage = Message.Express(NumberOfSpecialForcesInLocation, Player.SpecialForce);
                }
                
                Log(Initiator, " place ", normalForceMessage, specialForceMessage, " in ", Location);
                Game.LastShipmentOrMovement = this;
            }

            if (Game.Version >= 161 && Game.CheckIntrusion(this))
            {
                Game.PhaseBeforeRevivalCausedIntrusion = Game.CurrentPhase;
                Game.Enter(Game.LastBlueIntrusion != null, Phase.BlueIntrudedByRevival, Game.LastTerrorTrigger != null, Phase.TerrorTriggeredByRevival, Phase.AmbassadorTriggeredByRevival);
            }
        }

        if (Initiator != Faction.Purple) Game.HasActedOrPassed.Add(Initiator);
    }

    private void LogRevival(RevivalCost cost, int purpleReceivedResources, bool asGhola, int highThresholdBonus)
    {
        var totalAmountOfForces = AmountOfForces + ExtraForcesPaidByRed + highThresholdBonus;

        Log(
            Initiator,
            " revive ",
            MessagePart.ExpressIf(Hero != null, Hero),
            MessagePart.ExpressIf(asGhola, " as Ghola"),
            MessagePart.ExpressIf(Hero != null && totalAmountOfForces + AmountOfSpecialForces + ExtraSpecialForcesPaidByRed > 0, " and "),
            MessagePart.ExpressIf(totalAmountOfForces > 0, totalAmountOfForces, Player.Force),
            MessagePart.ExpressIf(AmountOfSpecialForces + ExtraSpecialForcesPaidByRed > 0, AmountOfSpecialForces + ExtraSpecialForcesPaidByRed, Player.SpecialForce),
            " for ",
            Payment.Of(cost.TotalCostForPlayer + cost.CostForEmperor),
            MessagePart.ExpressIf(ExtraForcesPaidByRed > 0 || ExtraSpecialForcesPaidByRed > 0, " (", Payment.Of(cost.CostForEmperor, Faction.Red), ")"),
            MessagePart.ExpressIf(purpleReceivedResources > 0, " → ", Faction.Purple, " get ", Payment.Of(purpleReceivedResources)));
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " perform revival");
    }

    #endregion Execution
}