/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
            return g.Map.Locations.Where(l => IsNotFull(g, p, l) && (l == g.Map.TheGreatFlat || l == g.Map.TheGreaterFlat || l == g.Map.FuneralPlain || l.Territory == g.Map.BightOfTheCliff || l == g.Map.SietchTabr ||
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

        public TreacheryCard GetKarmaCard(Game g, Player p)
        {
            return KarmaCard;
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

        public override string Validate()
        {
            if (Passed) return "";

            var p = Player;

            if (!IsBackToReserves && (ForceAmount < 0 || SpecialForceAmount < 0)) return "Can't ship less than zero forces.";
            if (ForceAmount == 0 && SpecialForceAmount == 0) return "Select forces to ship.";
            if (To == null) return "Target location not selected";

            var cost = DetermineCost(Game, p, this);
            if (cost > p.Resources + AllyContributionAmount) return "You can't pay that much.";
            if (cost < AllyContributionAmount) return "Your ally is paying too much.";
            if (!ValidShipmentLocations(Game, p).Contains(To)) return "Cannot ship there.";

            bool isWhiteNoFieldShipment = p.Faction == Faction.White && NoFieldValue >= 0;
            if (NoFieldValue >= 0 && !(p.Faction == Faction.White && !Game.Prevented(FactionAdvantage.WhiteNofield)) && !(p.Ally == Faction.White && Game.WhiteAllyMayUseNoField)) return "You can't use a No-Field";
            if (isWhiteNoFieldShipment && ForceAmount > 0) return "You can't do both normal and No-Field shipment.";
            if (isWhiteNoFieldShipment && SpecialForceAmount != 1) return "Invalid special force value for No-Field shipment.";
            if (p.Faction == Faction.White && SpecialForceAmount > 0 && !ValidNoFieldValues(Game, Player).Contains(NoFieldValue)) return "Invalid No-Field value.";

            if (From == null && ForceAmount > p.ForcesInReserve) return Skin.Current.Format("Not enough {0} in reserve.", p.Force);
            if (From == null && !isWhiteNoFieldShipment && SpecialForceAmount > p.SpecialForcesInReserve) return Skin.Current.Format("Not enough {0} in reserve.", p.SpecialForce);

            if (From != null && ForceAmount > p.ForcesIn(From)) return Skin.Current.Format("Not enough {0} for site-to-site shipment.", p.Force);
            if (From != null && SpecialForceAmount > p.SpecialForcesIn(From)) return Skin.Current.Format("Not enough {0} for site-to-site shipment.", p.SpecialForce);

            if (IsNoField && p.Faction != Faction.White)
            {
                int forcesToShip = Math.Min(NoFieldValue, p.ForcesInReserve + p.SpecialForcesInReserve);
                if (ForceAmount + SpecialForceAmount != forcesToShip) return string.Format("Using a No-Field of {0}, you must select {1} forces to ship", NoFieldValue, forcesToShip);
            }

            return "";
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
            if (amountToPayFor > 1 && g.SkilledAs(p, LeaderSkill.Smuggler) && !g.AnyForcesIn(to.Territory))
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
                IsNotFull(g, p, l));
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

        private static bool IsNotFull(Game g, Player p, Location l)
        {
            return (!l.Territory.IsStronghold || (p.Is(Faction.Blue) && p.SpecialForcesIn(l) > 0) || g.NrOfOccupantsExcludingPlayer(l, p) < 2);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return new Message(Initiator, "{0} pass shipment.", Initiator);
            }
            else if (IsBackToReserves)
            {
                return new Message(Initiator, "{0} ship from {1} to reserves.", Initiator, To);
            }
            else
            {
                return new Message(Initiator, "{0} ship to {1}.", Initiator, To);
            }
        }

        public Message GetVerboseMessage(int cost, MessagePart orangeIncome, Player ownerOfKarma)
        {
            if (Passed)
            {
                return new Message(Initiator, "{0} pass shipment.", Initiator);
            }
            else
            {
                if (IsBackToReserves)
                {
                    return new Message(Initiator, "{0} ship from {1} back to reserves{2}.{3}{4}", Initiator, To, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
                }
                else if (IsSiteToSite)
                {
                    var baseMessage = "{0} site-to-site ship {1} from {5} to {2}{3}.{4}{6}";
                    return new Message(Initiator, baseMessage, Initiator, ForceMessage, To.ToString(), CostMessage(cost), KaramaMessage(ownerOfKarma), From, orangeIncome);
                }
                else
                {
                    var baseMessage = Initiator == Faction.Yellow ? "{0} rally {1} in {2}{3}.{4}{5}" : "{0} ship {1} to {2}{3}.{4}{5}";
                    return new Message(Initiator, baseMessage, Initiator, ForceMessage, To, CostMessage(cost), KaramaMessage(ownerOfKarma), orangeIncome);
                }
            }
        }

        private MessagePart ForceMessage
        {
            get
            {
                var p = Player;

                if (ForceAmount > 0 && SpecialForceAmount > 0)
                {
                    return new MessagePart("{0} {1} and {2} {3}", ForceAmount, p.Force, SpecialForceAmount, p.SpecialForce);
                }
                else if (ForceAmount > 0 && SpecialForceAmount == 0)
                {
                    return new MessagePart("{0} {1}", ForceAmount, p.Force);
                }
                else
                {
                    return new MessagePart("{0} {1}", SpecialForceAmount, p.SpecialForce);
                }
            }
        }

        private MessagePart CostMessage(int cost)
        {
            var p = Player;

            if (AllyContributionAmount > 0)
            {
                return new MessagePart(" for {0}, of which {1} pay {2}", cost, p.Ally, AllyContributionAmount);
            }
            else if (cost > 0)
            {
                return new MessagePart(" for {0}", cost);
            }
            else
            {
                return new MessagePart("", cost);
            }
        }

        private MessagePart KaramaMessage(Player ownerOfKarma)
        {
            if (KarmaCard != null)
            {
                var initiator = Player;
                if (ownerOfKarma != initiator)
                {
                    if (KarmaCard.Type != TreacheryCardType.Karma)
                    {
                        return new MessagePart(" They used their allies' {0} ({1}).", TreacheryCardType.Karma, KarmaCard);
                    }
                    else
                    {
                        return new MessagePart(" They used their allies' {0}.", TreacheryCardType.Karma);
                    }
                }
                else
                {
                    if (KarmaCard.Type != TreacheryCardType.Karma)
                    {
                        return new MessagePart(" They used {0} ({1}).", TreacheryCardType.Karma, KarmaCard);
                    }
                    else
                    {
                        return new MessagePart(" They used {0}.", TreacheryCardType.Karma);
                    }
                }
            }
            else
            {
                return new MessagePart("");
            }
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
            var result = Karma.ValidKarmaCards(g, p).ToList();

            if (g.GetPermittedUseOfAllyKarma(p.Faction) != null)
            {
                result.Add(g.GetPermittedUseOfAllyKarma(p.Faction));
            }

            return result;
        }

        public static bool CanKarma(Game g, Player p)
        {
            return ValidKarmaCards(g, p).Any();
        }
    }
}
