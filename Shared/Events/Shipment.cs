/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class Shipment : PassableGameEvent, ILocationEvent
{
    #region Construction

    public Shipment(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Shipment()
    {
    }

    #endregion Construction

    #region Properties

    public int _toId;

    [JsonIgnore]
    public Location To
    {
        get => Game.Map.LocationLookup.Find(_toId);
        set => _toId = Game.Map.LocationLookup.GetId(value);
    }

    public int _fromId;

    [JsonIgnore]
    public Location From
    {
        get => Game.Map.LocationLookup.Find(_fromId);
        set => _fromId = Game.Map.LocationLookup.GetId(value);
    }

    public int ForceAmount { get; set; }

    public int SpecialForceAmount { get; set; }

    public int SmuggledAmount { get; set; }

    public int SmuggledSpecialAmount { get; set; }

    //This is needed for compatibility with pre-exp2 game versions where _noFieldValue is 0 by default.
    public int _noFieldValue;

    [JsonIgnore]
    public int NoFieldValue
    {
        get => _noFieldValue - 1;
        set => _noFieldValue = value + 1;
    }

    //This is needed for compatibility with pre-exp2 game versions where _cunningNoFieldValue is 0 by default.
    public int _cunningNoFieldValue;

    [JsonIgnore]
    public int CunningNoFieldValue
    {
        get => _cunningNoFieldValue - 1;
        set => _cunningNoFieldValue = value + 1;
    }

    public int AllyContributionAmount { get; set; }

    public int _karmaCardId;

    [JsonIgnore]
    public TreacheryCard KarmaCard
    {
        get => TreacheryCardManager.Lookup.Find(_karmaCardId);
        set => _karmaCardId = TreacheryCardManager.GetId(value);
    }

    [JsonIgnore]
    public bool IsUsingKarma => KarmaCard != null;

    [JsonIgnore]
    public bool IsSiteToSite => From != null;

    [JsonIgnore]
    public bool IsBackToReserves => ForceAmount + SpecialForceAmount < 0;

    [JsonIgnore]
    public bool IsNoField => NoFieldValue >= 0;

    [JsonIgnore]
    public int TotalAmountOfForcesAddedToLocation => ForcesAddedToLocation + SpecialForcesAddedToLocation;


    [JsonIgnore]
    public int ForcesAddedToLocation => UseWhiteSecretAlly ? 1 : ForceAmount + SmuggledAmount;

    [JsonIgnore]
    public int SpecialForcesAddedToLocation => UseWhiteSecretAlly ? 0 : SpecialForceAmount + SmuggledSpecialAmount;

    public bool UseWhiteSecretAlly { get; set; }
    
    public string _forceLocations = "";

    [JsonIgnore]
    public Dictionary<Location, Battalion> ForceLocations
    {
        get => ParseForceLocations(Game, Player.Faction, _forceLocations);
        set => _forceLocations = ForceLocationsString(Game, value);
    }

    public int DetermineOrangeProfits(Game game)
    {
        var initiator = Player;
        return
            DetermineCostToInitiator(game) +
            (initiator.Ally != Faction.Orange || game.Applicable(Rule.OrangeShipmentContributionsFlowBack) ? AllyContributionAmount : 0);
    }

    public int DetermineCostToInitiator(Game g)
    {
        if (g.Version <= 106)
            return DetermineCost(Game, Player, ForceAmount, SpecialForceAmount, To, IsUsingKarma, IsBackToReserves, false, IsNoField, false) - AllyContributionAmount;
        return DetermineCost(Game, Player, this) - AllyContributionAmount;
    }
    
    private static string ForceLocationsString(Game g, Dictionary<Location, Battalion> forceLocations)
    {
        return string.Join(',', forceLocations.Select(x => g.Map.LocationLookup.GetId(x.Key) + ":" + x.Value.AmountOfForces + "|" + x.Value.AmountOfSpecialForces));
    }

    private static Dictionary<Location, Battalion> ParseForceLocations(Game g, Faction f, string forceLocations)
    {
        var result = new Dictionary<Location, Battalion>();
        if (forceLocations != null && forceLocations.Length > 0)
            foreach (var locationAmountPair in forceLocations.Split(','))
            {
                var locationAndAmounts = locationAmountPair.Split(':');
                var location = g.Map.LocationLookup.Find(Convert.ToInt32(locationAndAmounts[0]));
                var amounts = locationAndAmounts[1].Split('|');
                var amountOfNormalForces = Convert.ToInt32(amounts[0]);
                var amountOfNormalSpecialForces = Convert.ToInt32(amounts[1]);
                result.Add(location, new Battalion(f, amountOfNormalForces, amountOfNormalSpecialForces, location));
            }

        return result;
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;

        var p = Player;

        if (!IsSiteToSite && Game.PreventedFromShipping(Initiator)) return Message.Express(TreacheryCardType.Karma, " prevents you from shipping");

        if (IsBackToReserves && !MayShipToReserves(Game, p)) return Message.Express("You can't ship back to reserves");
        if (IsSiteToSite && !MayShipCrossPlanet(Game, p)) return Message.Express("You can't ship site-to-site");

        if (!IsBackToReserves && (ForceAmount < 0 || SpecialForceAmount < 0)) return Message.Express("Can't ship less than zero forces");
        if (ForceAmount == 0 && SpecialForceAmount == 0) return Message.Express("Select forces to ship");
        if (To == null) return Message.Express("Target location not selected");

        var cost = DetermineCost(Game, p, this);
        if (cost > p.Resources + AllyContributionAmount) return Message.Express("You can't pay that much");
        if (cost < AllyContributionAmount) return Message.Express("Your ally is paying more than needed");
        if (!ValidShipmentLocations(Game, p, IsBackToReserves || (IsSiteToSite && From is not Homeworld), UseWhiteSecretAlly).Contains(To)) return Message.Express("Cannot ship there");

        //no field checks, requires refactoring
        if (NoFieldValue >= 0 && !MayUseNoField(Game, Player)) return Message.Express("You can't use a No-Field");

        var isWhiteNoFieldShipment = p.Faction == Faction.White && NoFieldValue >= 0;
        if (isWhiteNoFieldShipment && ForceAmount > 0 && !Player.HasHighThreshold()) return Message.Express("You can't do both normal and No-Field shipment");
        if (isWhiteNoFieldShipment && SpecialForceAmount != 1) return Message.Express("Invalid special force value for No-Field shipment");

        if (p.Faction == Faction.White && SpecialForceAmount > 0 && !ValidNoFieldValues(Game, Player).Contains(NoFieldValue)) return Message.Express("Invalid No-Field value");

        if (CunningNoFieldValue >= 0 && !MayUseCunningNoField(p)) return Message.Express("You cannot use cunning to ship a second No-Field");
        if (NoFieldValue < 0 && CunningNoFieldValue >= 0) return Message.Express("You can only use cunning when you also ship a regular No-Field");
        if (CunningNoFieldValue >= 0 && ForceAmount > 0 && !Player.HasHighThreshold()) return Message.Express("You can't do both normal and No-Field shipment");
        if (CunningNoFieldValue >= 0 && !ValidCunningNoFieldValues(Game, Player, NoFieldValue).Contains(CunningNoFieldValue)) return Message.Express("Invalid Cunning No-Field value");

        if (From == null && ForceAmount + SmuggledAmount > p.ForcesInReserve) return Message.Express("Not enough ", p.Force, " in reserve");
        if (From == null && !isWhiteNoFieldShipment && SpecialForceAmount + SmuggledSpecialAmount > p.SpecialForcesInReserve) return Message.Express("Not enough ", p.SpecialForce, " in reserve");

        var isSmuggling = SmuggledAmount > 0 || SmuggledSpecialAmount > 0;
        var isShippingFromOffPlanet = !(IsBackToReserves || IsSiteToSite) && Initiator != Faction.Yellow && (ForceAmount > 0 || SpecialForceAmount > 0);
        if (isSmuggling && (!isShippingFromOffPlanet || !MaySmuggle(Game, p, To))) return Message.Express("You can't smuggle forces here");
        if (SmuggledAmount + SmuggledSpecialAmount > 1) return Message.Express("You can't smuggle more than 1 force");

        if (From != null && ForceAmount > p.ForcesIn(From)) return Message.Express("Not enough ", p.Force, " for site-to-site shipment");
        if (From != null && SpecialForceAmount > p.SpecialForcesIn(From)) return Message.Express("Not enough ", p.SpecialForce, " for site-to-site shipment");

        if (IsNoField && p.Faction != Faction.White)
        {
            var forcesToShip = Math.Min(NoFieldValue, p.ForcesInReserve + p.SpecialForcesInReserve);
            if (ForceAmount + SpecialForceAmount != forcesToShip) return Message.Express("Using a No-Field of ", NoFieldValue, ", you must select ", forcesToShip, " forces to ship");
        }

        if (KarmaCard != null && !ValidKarmaCards(Game, Player).Contains(KarmaCard)) return Message.Express("Invalid ", TreacheryCardType.Karma, " card: ", KarmaCard);

        if (UseWhiteSecretAlly && !MayUseWhiteSecretAllly(Game, Player)) return Message.Express(Faction.White, " are not your secret ally");
        if (UseWhiteSecretAlly && ForceAmount + SpecialForceAmount > 5) return Message.Express("You can't ship that much using ", Faction.White, " as your secret ally");

        if (Game.Version >= 164 && HomeworldsToShipFrom(Player, false).Count() > 1 && ForceLocations.Sum(fl => fl.Value.AmountOfForces) != ForcesAddedToLocation) 
            return Message.Express("Please specify where to ship your ", p.Force, " from");

        if (Game.Version >= 164 && HomeworldsToShipFrom(Player, true).Count() > 1 && ForceLocations.Sum(fl => fl.Value.AmountOfSpecialForces) != SpecialForcesAddedToLocation) 
            return Message.Express("Please specify where to ship your ", p.SpecialForce, " from");

        return null;
    }

    public static IEnumerable<Homeworld> HomeworldsToShipFrom(Player p, bool special)
    {
        return special ? p.Homeworlds.Where(hw => p.SpecialForcesIn(hw) > 0) : p.Homeworlds.Where(hw => p.ForcesIn(hw) > 0);
    }
        

    private static List<Location> YellowSpawnLocations(Game g, Player p)
    {
        return g.Map.Locations(g.Applicable(Rule.Homeworlds)).Where(l => g.IsNotFull(p, l) && (l == g.Map.TheGreatFlat || l == g.Map.TheGreaterFlat || l == g.Map.FuneralPlain || l.Territory == g.Map.BightOfTheCliff || l == g.Map.SietchTabr ||
                l.Territory == g.Map.PlasticBasin || l.Territory == g.Map.RockOutcroppings || l.Territory == g.Map.BrokenLand || l.Territory == g.Map.Tsimpo || l.Territory == g.Map.HaggaBasin ||
                l == g.Map.PolarSink || l.Territory == g.Map.WindPass || l.Territory == g.Map.WindPassNorth || l.Territory == g.Map.CielagoWest || l.Territory == g.Map.FalseWallWest || l.Territory == g.Map.HabbanyaErg ||
                (l is DiscoveredLocation dsPastyMesa && dsPastyMesa.Visible && dsPastyMesa.AttachedToLocation.Territory == g.Map.PastyMesa) || (l is DiscoveredLocation dsFalseWallWest && dsFalseWallWest.Visible && dsFalseWallWest.AttachedToLocation.Territory == g.Map.FalseWallWest)))
            .ToList();
    }

    public static int DetermineCost(Game g, Player p, Shipment s)
    {
        if (s.To == null) return 0;

        return DetermineCost(g, p, Math.Abs(s.ForceAmount), Math.Abs(s.SpecialForceAmount), s.To, s.IsUsingKarma, s.IsBackToReserves, s.IsSiteToSite, s.IsWhiteNoField, s.IsNonWhiteNoField);
    }
    
    private bool IsWhiteNoField => Initiator is Faction.White && IsNoField;
    private bool IsNonWhiteNoField => Initiator is not Faction.White && IsNoField || UseWhiteSecretAlly;

    public static int DetermineCost(Game g, Player p, int forces, int specialForces, Location to, bool karamaShipment, bool backToReserves, bool siteToSite, bool noFieldByWhite, bool noFieldByNonWhite)
    {
        var totalForces = forces + specialForces;
        var noFieldOrSecretAlly = noFieldByWhite || noFieldByNonWhite;

        if (g.Version < 139 && totalForces > 1 && g.SkilledAs(p, LeaderSkill.Smuggler) && !g.AnyForcesIn(to.Territory))
            totalForces--;
        else if (g.Version < 167 && noFieldOrSecretAlly || noFieldByNonWhite) 
            totalForces = Math.Min(forces + specialForces, 1);
        else if (g.Version >= 167 && noFieldByWhite) 
            totalForces = Math.Max(forces, 1);
        
        if (backToReserves) return (int)Math.Ceiling(0.5f * totalForces);

        if ((g.Version < 154 || !siteToSite) && p.Is(Faction.Yellow) && YellowSpawnLocations(g, p).Contains(to) && (g.Version < 159 || !noFieldOrSecretAlly)) return 0;

        double costOfShipment = Math.Abs(totalForces) * (to.Territory.HasReducedShippingCost ? 1 : 2);

        if (MayShipWithDiscount(g, p) || karamaShipment || siteToSite && g.HasShipmentPermission(p, ShipmentPermission.OrangeRate)) 
            costOfShipment /= 2;

        return (int)Math.Ceiling(costOfShipment);
    }

    public static int ValidMaxNormalShipmentForces(Player p, bool specialForces, int usedNoField)
    {
        var noFieldMax = usedNoField < 0 || p.Faction is Faction.White ? int.MaxValue : usedNoField;
        return specialForces ? Math.Min(p.SpecialForcesInReserve, noFieldMax) : Math.Min(p.ForcesInReserve, noFieldMax);
    }

    public static int ValidMaxSecretAllyShipmentForces(Player p, bool specialForces)
    {
        return specialForces ? Math.Min(p.SpecialForcesInReserve, 5) : Math.Min(p.ForcesInReserve, 5);
    }

    public static int ValidMaxShipmentBackForces(Player p, bool specialForces, Location source)
    {
        if (source == null) return 0;
        return specialForces ? p.SpecialForcesIn(source) : p.ForcesIn(source);
    }

    public static int ValidMaxShipmentSiteToSiteForces(Player p, bool specialForces, Location source)
    {
        if (source == null) return 0;

        return specialForces ? p.SpecialForcesIn(source) : p.ForcesIn(source);
    }

    public static IEnumerable<Location> ValidShipmentLocations(Game g, Player p, bool fromPlanet, bool secretAlly)
    {
        if (p.Is(Faction.Yellow) && !secretAlly)
        {
            if (MayShipCrossPlanet(g, p))
                return YellowSpawnLocations(g, p).Union(RegularShipmentLocations(g, p, fromPlanet)).Distinct();
            return YellowSpawnLocations(g, p);
        }

        return RegularShipmentLocations(g, p, fromPlanet);
    }

    private static IEnumerable<Location> RegularShipmentLocations(Game g, Player p, bool fromPlanet)
    {
        return g.Map.Locations(g.Applicable(Rule.Homeworlds)).Where(l =>
            l.Territory != g.AtomicsAftermath &&
            l.Sector != g.SectorInStorm &&
            (l != g.Map.HiddenMobileStronghold || p.Is(Faction.Grey)) &&
            IsEitherValidHomeworldOrNoHomeworld(g, p, l, fromPlanet) &&
            IsEitherValidDiscoveryOrNoDiscovery(l) &&
            g.IsNotFull(p, l));
    }

    public static bool IsEitherValidHomeworldOrNoHomeworld(Game g, Player p, Location l, bool fromPlanet)
    {
        return l is not Homeworld hw ||
               ((!fromPlanet || p.Is(Faction.Orange) || (p.IsNative(hw) && MayShipToReserves(g, p))) &&
                g.Applicable(Rule.Homeworlds) &&
                g.Players.Any(native => native.IsNative(hw)) &&
                (!p.HasAlly || (!p.AlliedPlayer.IsNative(hw) && p.AlliedPlayer.AnyForcesIn(hw) == 0)));
    }

    public static bool IsEitherValidDiscoveryOrNoDiscovery(Location l)
    {
        return l is not DiscoveredLocation ds || ds.Visible;
    }

    public static IEnumerable<int> ValidNoFieldValues(Game g, Player p)
    {
        var result = new List<int>();
        if ((p.Faction == Faction.White && !g.Prevented(FactionAdvantage.WhiteNofield)) ||
            (p.Ally == Faction.White && g.WhiteAllowsUseOfNoField))
        {
            if (p.Faction == Faction.White && g.LatestRevealedNoFieldValue != 0 && g.CurrentNoFieldValue != 0) result.Add(0);
            if (g.LatestRevealedNoFieldValue != 3 && g.CurrentNoFieldValue != 3) result.Add(3);
            if (g.LatestRevealedNoFieldValue != 5 && g.CurrentNoFieldValue != 5) result.Add(5);
        }
        return result;
    }

    public static IEnumerable<int> ValidCunningNoFieldValues(Game g, Player p, int valueOfNormalNoField)
    {
        var result = new List<int>();
        if ((p.Faction == Faction.White && !g.Prevented(FactionAdvantage.WhiteNofield)) ||
            (p.Ally == Faction.White && g.WhiteAllowsUseOfNoField))
        {
            if (p.Faction == Faction.White && g.LatestRevealedNoFieldValue != 0 && g.CurrentNoFieldValue != 0 && valueOfNormalNoField != 0) result.Add(0);
            if (g.LatestRevealedNoFieldValue != 3 && g.CurrentNoFieldValue != 3 && valueOfNormalNoField != 3) result.Add(3);
            if (g.LatestRevealedNoFieldValue != 5 && g.CurrentNoFieldValue != 5 && valueOfNormalNoField != 5) result.Add(5);
        }
        return result;
    }

    public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player p)
    {
        var result = ValidOwnKarmaCards(g, p).ToList();

        var allyCard = ValidAllyKarmaCard(g, p);
        if (allyCard != null) result.Add(allyCard);

        return result;
    }

    public static IEnumerable<TreacheryCard> ValidOwnKarmaCards(Game g, Player p)
    {
        return Karma.ValidKarmaCards(g, p);
    }

    public static TreacheryCard ValidAllyKarmaCard(Game g, Player p)
    {
        return g.GetPermittedUseOfAllyKarma(p.Faction);
    }

    public static bool CanKarma(Game g, Player p)
    {
        return ValidKarmaCards(g, p).Any();
    }

    public static bool MaySmuggle(Game g, Player p, Location l)
    {
        return l != null && !g.AnyForcesIn(l.Territory) && p.Faction != Faction.Yellow && g.SkilledAs(p, LeaderSkill.Smuggler);
    }

    public static bool MayUseNoField(Game g, Player p)
    {
        return (p.Faction == Faction.White && !g.Prevented(FactionAdvantage.WhiteNofield)) ||
               (p.Ally == Faction.White && g.WhiteAllowsUseOfNoField);
    }

    public static bool MayUseCunningNoField(Player p)
    {
        return p.Faction == Faction.White && NexusPlayed.CanUseCunning(p);
    }

    public static bool MayUseWhiteSecretAllly(Game g, Player p)
    {
        return p.Nexus == Faction.White && NexusPlayed.CanUseSecretAlly(g, p);
    }

    public static bool MayShipCrossPlanet(Game g, Player p)
    {
        return (p.Is(Faction.Orange) && !g.Prevented(FactionAdvantage.OrangeSpecialShipments)) ||
               (p.Ally == Faction.Orange && g.OrangeAllowsShippingDiscount) ||
               p.Initiated(g.CurrentOrangeSecretAlly) ||
               g.HasShipmentPermission(p, ShipmentPermission.Cross);
    }

    public static bool MayShipToReserves(Game g, Player p)
    {
        return (p.Is(Faction.Orange) && !g.Prevented(FactionAdvantage.OrangeSpecialShipments)) ||
               p.Initiated(g.CurrentOrangeSecretAlly) ||
               g.HasShipmentPermission(p, ShipmentPermission.ToHomeworld);
    }

    public static bool MayShipWithDiscount(Game g, Player p)
    {
        return (p.Is(Faction.Orange) && !g.Prevented(FactionAdvantage.OrangeShipmentsDiscount)) ||
               (p.Ally == Faction.Orange && g.OrangeAllowsShippingDiscount && !g.Prevented(FactionAdvantage.OrangeShipmentsDiscountAlly)) ||
               p.Initiated(g.CurrentOrangeSecretAlly);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.BeginningOfShipmentAndMovePhase = false;
        Game.CurrentBlockedTerritories.Clear();
        Game.StormLossesToTake.Clear();
        Game.ChosenDestinationsWithAllies.Clear();
        Game.BlueMayAccompany = false;

        var receivedPaymentMessage = MessagePart.Express();
        var totalCost = 0;

        if (!Passed)
        {
            var ownerOfKarma = KarmaCard != null ? Game.OwnerOf(KarmaCard) : null;
            totalCost = PayForShipment();

            if (IsNoField)
            {
                if (CunningNoFieldValue >= 0)
                {
                    Game.RevealCurrentNoField(GetPlayer(Faction.White));
                    Player.ShipSpecialForces(To, 1);
                    Game.CurrentNoFieldValue = CunningNoFieldValue;
                    Game.PlayNexusCard(Player, "Cunning", "ship and reveal a second ", FactionSpecialForce.White);
                }

                Game.RevealCurrentNoField(GetPlayer(Faction.White));
                Game.CurrentNoFieldValue = NoFieldValue;

                if (Initiator != Faction.White)
                {
                    //Immediately reveal No-Field
                    Game.LatestRevealedNoFieldValue = NoFieldValue;
                    Game.CurrentNoFieldValue = -1;
                }
            }

            if (Game.ContainsConflictingAlly(Player, To)) Game.ChosenDestinationsWithAllies.Add(To.Territory);

            Game.LastShipmentOrMovement = this;
            var mustBeAdvisors = (Player.Is(Faction.Blue) && Player.SpecialForcesIn(To) > 0) || (Game.Version >= 148 && Player.SpecialForcesIn(To.Territory) > 0);

            if (IsSiteToSite)
                PerformSiteToSiteShipment();
            else if (IsBackToReserves)
                Player.ShipForces(To, ForceAmount);
            else if (IsNoField && Initiator == Faction.White)
                PerformNormalShipment(Player, To, ForceAmount + SmuggledAmount, 1, false);
            else
                PerformNormalShipment(Player, To, ForceAmount + SmuggledAmount, SpecialForceAmount + SmuggledSpecialAmount, IsSiteToSite);

            if (UseWhiteSecretAlly) Game.PlayNexusCard(Player, "ship from reserves as if shipping ", 1);

            if (!By(Faction.Yellow)) Game.Stone(Milestone.Shipment);

            Player.FlipForces(To, mustBeAdvisors);

            if (!IsBackToReserves) Game.CheckIntrusion(this);

            Game.DetermineNextShipmentAndMoveSubPhase();

            var receivedPayment = HandleReceivedShipmentPayment(ref receivedPaymentMessage);
            Log(GetVerboseMessage(totalCost, receivedPaymentMessage, ownerOfKarma));

            if (totalCost - receivedPayment >= 4) Game.ActivateBanker(Player);

            Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
            Game.DetermineOccupation(To);
        }
        else
        {
            Log(GetVerboseMessage(totalCost, receivedPaymentMessage, null));
            Game.DetermineNextShipmentAndMoveSubPhase();
        }
    }

    private int PayForShipment()
    {
        var costToInitiator = DetermineCostToInitiator(Game);
        Player.Resources -= costToInitiator;

        if (IsUsingKarma)
        {
            var karmaCard = KarmaCard;
            Game.Discard(karmaCard);
            Game.Stone(Milestone.Karma);
        }

        if (AllyContributionAmount > 0)
        {
            Game.GetPlayer(Player.Ally).Resources -= AllyContributionAmount;
            Game.DecreasePermittedUseOfAllySpice(Initiator, AllyContributionAmount);
        }

        return costToInitiator + AllyContributionAmount;
    }

    private int HandleReceivedShipmentPayment(ref MessagePart profitMessage)
    {
        var totalCost = DetermineCost(Game, Player, this);

        var receiverProfit = 0;
        var orange = GetPlayer(Faction.Orange);

        if (orange != null && !By(Faction.Orange) && !IsUsingKarma)
        {
            receiverProfit = DetermineOrangeProfits(Game);

            if (receiverProfit > 0)
            {
                if (!Game.Prevented(FactionAdvantage.OrangeReceiveShipment))
                {
                    Game.ModifyIncomeBecauseOfLowThresholdOrOccupation(orange, ref receiverProfit);
                    orange.Resources += receiverProfit;

                    if (receiverProfit > 0)
                    {
                        profitMessage = MessagePart.Express(" → ", Faction.Orange, " get ", Payment.Of(receiverProfit));

                        if (receiverProfit >= 5) Game.ApplyBureaucracy(Initiator, Faction.Orange);
                    }
                }
                else
                {
                    profitMessage = MessagePart.Express(" → ", TreacheryCardType.Karma, " prevents ", Faction.Orange, " from receiving", Payment.Of(receiverProfit));
                    if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.OrangeReceiveShipment);
                }
            }
        }

        Game.SetRecentPayment(receiverProfit, Initiator, Faction.Orange, this);
        Game.SetRecentPayment(totalCost - receiverProfit, Initiator, this);

        return receiverProfit;
    }

    private void PerformSiteToSiteShipment()
    {
        Player.MoveForces(From, To, ForceAmount);
        Player.MoveSpecialForces(From, To, SpecialForceAmount);
    }

    private void PerformNormalShipment(Player initiator, Location to, int forceAmount, int specialForceAmount, bool isSiteToSite)
    {
        Game.BlueMayAccompany = (forceAmount > 0 || specialForceAmount > 0) && initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Blue;

        if (HomeworldsToShipFrom(Player, false).Count() > 1 && ForceLocations.Any())
        {
            foreach (var fl in ForceLocations.Where(x => x.Value.AmountOfForces > 0))
            {
                if (fl.Key is Homeworld hw)
                    initiator.ShipForces(to, hw, fl.Value.AmountOfForces);
            }
        }
        else
        {
            initiator.ShipForces(to, forceAmount);
        }
    
        if (HomeworldsToShipFrom(Player, true).Count() > 1 && ForceLocations.Any())
        {
            foreach (var fl in ForceLocations.Where(x => x.Value.AmountOfSpecialForces > 0))
            {
                if (fl.Key is Homeworld hw)
                {
                    initiator.ShipSpecialForces(to, hw, fl.Value.AmountOfSpecialForces);
                }
            }
        }
        else
        {
            initiator.ShipSpecialForces(to, specialForceAmount);
        }

        if (initiator.Is(Faction.Yellow) && Game.IsInStorm(to))
        {
            int killCount;
            if (!Game.Prevented(FactionAdvantage.YellowProtectedFromStorm) && Game.Applicable(Rule.YellowStormLosses))
            {
                killCount = 0;
                Game.StormLossesToTake.Add(new LossToTake { Location = to, Amount = TakeLosses.HalfOf(forceAmount, specialForceAmount), Faction = Faction.Yellow });
            }
            else
            {
                killCount = initiator.KillForces(to, forceAmount, specialForceAmount, false);
            }

            if (killCount > 0) Log(killCount, initiator.Faction, " forces are killed by the storm");
        }

        if (initiator.Faction != Faction.Yellow && initiator.Faction != Faction.Orange && !isSiteToSite) Game.ShipsTechTokenIncome = true;
    }

    public override Message GetMessage()
    {
        if (Passed)
            return Message.Express(Initiator, " pass shipment");
        if (IsBackToReserves)
            return Message.Express(Initiator, " ship from ", To, " to reserves");
        return Message.Express(Initiator, " ship to ", To);
    }

    public Message GetVerboseMessage(int cost, MessagePart orangeIncome, Player ownerOfKarma)
    {
        if (Passed) return Message.Express(Initiator, " pass shipment");

        if (IsBackToReserves) return Message.Express(Initiator, " ship from ", To, " back to reserves", CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);

        if (IsSiteToSite) return Message.Express(Initiator, " site-to-site ship ", ForceMessage, " from ", From, " to ", To, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);

        if (cost > 0)
            return Message.Express(Initiator, " ship ", ForceMessage, " to ", To, NoFieldMessage, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
        return Message.Express(Initiator, " rally ", ForceMessage, " in ", To);
    }

    private MessagePart NoFieldMessage => MessagePart.ExpressIf(IsNoField && Initiator != Faction.White, " (using a ", FactionSpecialForce.White, ")");

    private MessagePart ForceMessage => MessagePart.Express(
        MessagePart.ExpressIf(ForceAmount > 0, ForceAmount, Player.Force),
        MessagePart.ExpressIf(SpecialForceAmount > 0, SpecialForceAmount, Player.SpecialForce));

    private MessagePart CostMessage(int cost)
    {
        return MessagePart.Express(
            " for ",
            Payment.Of(cost),
            MessagePart.ExpressIf(AllyContributionAmount > 0, " (", Payment.Of(AllyContributionAmount, Player.Ally), ")"));
    }

    private MessagePart KaramaMessage(Player ownerOfKarma)
    {
        return MessagePart.ExpressIf(KarmaCard != null,
            " using ",
            MessagePart.ExpressIf(ownerOfKarma != Player, "their allies "),
            TreacheryCardType.Karma,
            MessagePart.ExpressIf(KarmaCard != null && KarmaCard.Type != TreacheryCardType.Karma, " (", KarmaCard, ")"));
    }

    #endregion Execution
}