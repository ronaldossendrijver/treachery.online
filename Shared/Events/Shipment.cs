/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Shipment : GameEvent
    {
        public int _toId;
        public int _fromId;

        private static List<Location> YellowSpawnLocations(Game g, Player p)
        {
            return g.Map.Locations.Where(l => g.IsNotFull(p, l) && (l == g.Map.TheGreatFlat || l == g.Map.TheGreaterFlat || l == g.Map.FuneralPlain || l.Territory == g.Map.BightOfTheCliff || l == g.Map.SietchTabr ||
                l.Territory == g.Map.PlasticBasin || l.Territory == g.Map.RockOutcroppings || l.Territory == g.Map.BrokenLand || l.Territory == g.Map.Tsimpo || l.Territory == g.Map.HaggaBasin ||
                l == g.Map.PolarSink || l.Territory == g.Map.WindPass || l.Territory == g.Map.WindPassNorth || l.Territory == g.Map.CielagoWest || l.Territory == g.Map.FalseWallWest || l.Territory == g.Map.HabbanyaErg))
                .ToList();
        }

        [JsonIgnore]
        public Location To { get { return Game.Map.LocationLookup.Find(_toId); } set { _toId = Game.Map.LocationLookup.GetId(value); } }

        [JsonIgnore]
        public Location From { get { return Game.Map.LocationLookup.Find(_fromId); } set { _fromId = Game.Map.LocationLookup.GetId(value); } }

        public int ForceAmount { get; set; }

        public int SpecialForceAmount { get; set; }

        public int SmuggledAmount { get; set; }

        public int SmuggledSpecialAmount { get; set; }

        //This is needed for compatibility with pre-exp2 game versions where _noFieldValue is 0 by default.
        public int _noFieldValue;

        [JsonIgnore]
        public int NoFieldValue
        {
            get
            {
                return _noFieldValue - 1;
            }

            set
            {
                _noFieldValue = value + 1;
            }
        }

        public bool Passed { get; set; }

        public int _karmaCardId;

        public Shipment(Game game) : base(game)
        {
        }

        public Shipment()
        {
        }

        [JsonIgnore]
        public TreacheryCard KarmaCard
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_karmaCardId);
            }

            set
            {
                if (value == null)
                {
                    _karmaCardId = -1;
                }
                else
                {
                    _karmaCardId = value.Id;
                }
            }
        }

        public bool UsingKarma(Game g)
        {
            return KarmaCard != null;
        }

        //To be removed later on
        public bool KarmaShipment { get; set; } = false;

        public int AllyContributionAmount { get; set; }

        [JsonIgnore]
        public bool IsSiteToSite => From != null;

        [JsonIgnore]
        public bool IsBackToReserves => ForceAmount + SpecialForceAmount < 0;

        public override Message Validate()
        {
            if (Passed) return null;

            var p = Player;

            if (!IsSiteToSite && Game.PreventedFromShipping(Initiator))
            {
                return Message.Express(TreacheryCardType.Karma, " prevents you from shipping");
            }

            if (IsBackToReserves && Initiator != Faction.Orange) return Message.Express("You can't ship back to reserves");
            if (IsSiteToSite && !Game.MayShipAsGuild(p)) return Message.Express("You can't site-to-site ship");
            if (IsBackToReserves && !Game.MayShipAsGuild(p) && Initiator == Faction.Orange) return Message.Express("You can't ship back to reserves");

            if (!IsBackToReserves && (ForceAmount < 0 || SpecialForceAmount < 0)) return Message.Express("Can't ship less than zero forces");
            if (ForceAmount == 0 && SpecialForceAmount == 0) return Message.Express("Select forces to ship");
            if (To == null) return Message.Express("Target location not selected");

            var cost = DetermineCost(Game, p, this);
            if (cost > p.Resources + AllyContributionAmount) return Message.Express("You can't pay that much");
            if (cost < AllyContributionAmount) return Message.Express("Your ally is paying more than needed");
            if (!ValidShipmentLocations(Game, p).Contains(To)) return Message.Express("Cannot ship there");

            bool isWhiteNoFieldShipment = p.Faction == Faction.White && NoFieldValue >= 0;
            if (NoFieldValue >= 0 && !(p.Faction == Faction.White && !Game.Prevented(FactionAdvantage.WhiteNofield)) && !(p.Ally == Faction.White && Game.WhiteAllyMayUseNoField)) return Message.Express("You can't use a No-Field");
            if (isWhiteNoFieldShipment && ForceAmount > 0) return Message.Express("You can't do both normal and No-Field shipment");
            if (isWhiteNoFieldShipment && SpecialForceAmount != 1) return Message.Express("Invalid special force value for No-Field shipment");
            if (p.Faction == Faction.White && SpecialForceAmount > 0 && !ValidNoFieldValues(Game, Player).Contains(NoFieldValue)) return Message.Express("Invalid No-Field value");

            if (From == null && ForceAmount + SmuggledAmount > p.ForcesInReserve) return Message.Express("Not enough ", p.Force, " in reserve");
            if (From == null && !isWhiteNoFieldShipment && SpecialForceAmount + SmuggledSpecialAmount > p.SpecialForcesInReserve) return Message.Express("Not enough ", p.SpecialForce, " in reserve");

            bool isSmuggling = SmuggledAmount > 0 || SmuggledSpecialAmount > 0;
            bool isShippingFromOffPlanet = !(IsBackToReserves || IsSiteToSite) && Initiator != Faction.Yellow && (ForceAmount > 0 || SpecialForceAmount > 0);
            if (isSmuggling && (!isShippingFromOffPlanet || !MaySmuggle(Game, Player, To))) return Message.Express("You can't smuggle forces here");
            if (SmuggledAmount + SmuggledSpecialAmount > 1) return Message.Express("You can't smuggle more than 1 force");

            if (From != null && ForceAmount > p.ForcesIn(From)) return Message.Express("Not enough ", p.Force, " for site-to-site shipment");
            if (From != null && SpecialForceAmount > p.SpecialForcesIn(From)) return Message.Express("Not enough ", p.SpecialForce, " for site-to-site shipment");

            if (IsNoField && p.Faction != Faction.White)
            {
                int forcesToShip = Math.Min(NoFieldValue, p.ForcesInReserve + p.SpecialForcesInReserve);
                if (ForceAmount + SpecialForceAmount != forcesToShip) return Message.Express("Using a No-Field of ", NoFieldValue, ", you must select ", forcesToShip, " forces to ship");
            }

            return null;
        }

        public static int DetermineCost(Game g, Player p, Shipment s)
        {
            if (s.To == null)
            {
                return 0;
            }

            return DetermineCost(g, p, Math.Abs(s.ForceAmount) + Math.Abs(s.SpecialForceAmount), s.To, s.UsingKarma(g), s.IsBackToReserves, s.IsNoField);
        }

        public static bool ShipsForFree(Game g, Player p, Location to)
        {
            return p.Is(Faction.Yellow) && YellowSpawnLocations(g, p).Contains(to);
        }

        public static int DetermineCost(Game g, Player p, int amount, Location to, bool karamaShipment, bool backToReserves, bool noField)
        {
            var amountToPayFor = amount;
            if (g.Version < 139 && amountToPayFor > 1 && g.SkilledAs(p, LeaderSkill.Smuggler) && !g.AnyForcesIn(to.Territory))
            {
                amountToPayFor--;
            }
            else if (noField)
            {
                amountToPayFor = 1;
            }

            if (backToReserves)
            {
                return (int)Math.Ceiling(0.5f * amountToPayFor);
            }
            else
            {
                if (ShipsForFree(g, p, to))
                {
                    return 0;
                }

                double costOfShipment = Math.Abs(amountToPayFor) * (to.Territory.IsStronghold ? 1 : 2);

                if (g.MayShipWithDiscount(p) || karamaShipment)
                {
                    costOfShipment /= 2;
                }

                return (int)Math.Ceiling(costOfShipment);
            }
        }

        public static int ValidMaxNormalShipmentForces(Player p, bool specialForces, int usedNoField)
        {
            int noFieldMax = usedNoField == -1 ? int.MaxValue : usedNoField;
            return specialForces ? Math.Min(p.SpecialForcesInReserve, noFieldMax) : Math.Min(p.ForcesInReserve, noFieldMax);
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

        public static IEnumerable<Location> ValidShipmentLocations(Game g, Player p)
        {
            IEnumerable<Location> potentialLocations;
            if (p.Is(Faction.Yellow))
            {
                if (g.MayShipAsGuild(p))
                {
                    potentialLocations = YellowSpawnLocations(g, p).Union(NormalShipmentLocations(g, p)).Distinct();
                }
                else
                {
                    potentialLocations = YellowSpawnLocations(g, p);
                }
            }
            else
            {
                potentialLocations = NormalShipmentLocations(g, p);
            }

            return potentialLocations;
        }

        private static IEnumerable<Location> NormalShipmentLocations(Game g, Player p)
        {
            return g.Map.Locations.Where(l =>
                l.Sector != g.SectorInStorm &&
                (l != g.Map.HiddenMobileStronghold || p.Is(Faction.Grey)) &&
                g.IsNotFull(p, l));
        }

        public static IEnumerable<int> ValidNoFieldValues(Game g, Player p)
        {
            var result = new List<int>();
            if (p.Faction == Faction.White && !g.Prevented(FactionAdvantage.WhiteNofield) ||
                p.Ally == Faction.White && g.WhiteAllyMayUseNoField)
            {
                if (p.Faction == Faction.White && g.LatestRevealedNoFieldValue != 0 && g.CurrentNoFieldValue != 0) result.Add(0);
                if (g.LatestRevealedNoFieldValue != 3 && g.CurrentNoFieldValue != 3) result.Add(3);
                if (g.LatestRevealedNoFieldValue != 5 && g.CurrentNoFieldValue != 5) result.Add(5);
            }
            return result;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass shipment");
            }
            else if (IsBackToReserves)
            {
                return Message.Express(Initiator, " ship from ", To, " to reserves");
            }
            else
            {
                return Message.Express(Initiator, " ship to ", To);
            }
        }

        public Message GetVerboseMessage(int cost, MessagePart orangeIncome, Player ownerOfKarma)
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass shipment");
            }
            else
            {
                if (IsBackToReserves)
                {
                    return Message.Express(Initiator, " ship from ", To, " back to reserves", CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
                }
                else if (IsSiteToSite)
                {
                    return Message.Express(Initiator, " site-to-site ship ", ForceMessage, " from ", From, " to ", To, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
                }
                else
                {
                    if (cost > 0)
                    {
                        return Message.Express(Initiator, " ship ", ForceMessage, " to ", To, NoFieldMessage, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
                    }
                    else
                    {
                        return Message.Express(Initiator, " rally ", ForceMessage, " in ", To);
                    }
                }
            }
        }

        private MessagePart NoFieldMessage => MessagePart.ExpressIf(IsNoField && Initiator != Faction.White, " (using a ", FactionSpecialForce.White, ")");

        private MessagePart ForceMessage => MessagePart.Express(
                    MessagePart.ExpressIf(ForceAmount > 0, ForceAmount, Player.Force),
                    MessagePart.ExpressIf(SpecialForceAmount > 0, SpecialForceAmount, Player.SpecialForce));

        private MessagePart CostMessage(int cost)
        {
            return MessagePart.Express(
                " for ",
                new Payment(cost),
                MessagePart.ExpressIf(AllyContributionAmount > 0, " (", new Payment(AllyContributionAmount, Player.Ally), ")"));
        }

        private MessagePart KaramaMessage(Player ownerOfKarma)
        {
            return MessagePart.ExpressIf(KarmaCard != null,
                " using ",
                MessagePart.ExpressIf(ownerOfKarma != Player, "their allies "),
                TreacheryCardType.Karma,
                MessagePart.ExpressIf(KarmaCard != null && KarmaCard.Type != TreacheryCardType.Karma, " (", KarmaCard, ")"));
        }

        [JsonIgnore]
        public bool IsNoField => NoFieldValue >= 0;

        public int DetermineOrangeProfits(Game game)
        {
            var initiator = Player;
            return
                DetermineCostToInitiator(game) +
                ((initiator.Ally != Faction.Orange || game.Applicable(Rule.OrangeShipmentContributionsFlowBack)) ? AllyContributionAmount : 0);
        }

        public int DetermineCostToInitiator(Game g)
        {
            //return DetermineCost(Game, Player, ForceAmount + SpecialForceAmount, To, UsingKarma(Game), IsBackToReserves, IsNoField) - AllyContributionAmount;
            if (g.Version <= 106)
            {
                return DetermineCost(Game, Player, ForceAmount + SpecialForceAmount, To, UsingKarma(Game), IsBackToReserves, IsNoField) - AllyContributionAmount;
            }
            else
            {
                return DetermineCost(Game, Player, this) - AllyContributionAmount;
            }
        }

        public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player p)
        {
            var result = ValidOwnKarmaCards(g, p).ToList();

            var allyCard = ValidAllyKarmaCard(g, p);
            if (allyCard != null)
            {
                result.Add(allyCard);
            }

            return result;
        }

        public static IEnumerable<TreacheryCard> ValidOwnKarmaCards(Game g, Player p) => Karma.ValidKarmaCards(g, p);

        public static TreacheryCard ValidAllyKarmaCard(Game g, Player p) => g.GetPermittedUseOfAllyKarma(p.Faction);

        public static bool CanKarma(Game g, Player p)
        {
            return ValidKarmaCards(g, p).Any();
        }

        public static bool MaySmuggle(Game g, Player p, Location l)
        {
            return l != null && !g.AnyForcesIn(l.Territory) && p.Faction != Faction.Yellow && g.SkilledAs(p, LeaderSkill.Smuggler);
        }
    }
}
