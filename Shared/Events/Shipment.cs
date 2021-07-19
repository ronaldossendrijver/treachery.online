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
            private get
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
            if (g.Version >= 38)
            {
                return KarmaCard;
            }
            else
            {
                //needed for older versions (<38) that did not have the karmaCardId
                if (p.Is(Faction.Blue))
                {
                    return p.TreacheryCards.FirstOrDefault(x => x.Type == TreacheryCardType.Karma || x.Type == TreacheryCardType.Useless);
                }
                else
                {
                    return p.TreacheryCards.FirstOrDefault(x => x.Type == TreacheryCardType.Karma);
                }
            }
        }

        public bool UsingKarma(Game g)
        {
            if (g.Version >= 38)
            {
                return KarmaCard != null;
            }
            else
            {
                //To be removed later on
                return KarmaShipment;
            }
        }

        //To be removed later on
        public bool KarmaShipment { get; set; } = false;

        public int AllyContributionAmount { get; set; }

        [JsonIgnore]
        public bool IsSiteToSite
        {
            get
            {
                return From != null;
            }
        }

        [JsonIgnore]
        public bool IsBackToReserves
        {
            get
            {
                return ForceAmount + SpecialForceAmount < 0;
            }
        }

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
            if (From == null && ForceAmount > p.ForcesInReserve) return Skin.Current.Format("Not enough {0} in reserve.", p.Force);
            if (From == null && SpecialForceAmount > p.SpecialForcesInReserve) return Skin.Current.Format("Not enough {0} in reserve.", p.SpecialForce);
            if (From != null && ForceAmount > p.ForcesIn(From)) return Skin.Current.Format("Not enough {0} for site-to-site shipment.", p.Force);
            if (From != null && SpecialForceAmount > p.SpecialForcesIn(From)) return Skin.Current.Format("Not enough {0} for site-to-site shipment.", p.SpecialForce);

            return "";
        }

        public static int DetermineCost(Game g, Player p, Shipment s)
        {
            if (s.To == null)
            {
                return 0;
            }

            return DetermineCost(g, p, Math.Abs(s.ForceAmount) + Math.Abs(s.SpecialForceAmount), s.To, s.UsingKarma(g), s.IsBackToReserves);
        }

        public static bool ShipsForFree(Game g, Player p, Location to)
        {
            return p.Is(Faction.Yellow) && YellowSpawnLocations(g, p).Contains(to);
        }

        public static int DetermineCost(Game g, Player p, int amount, Location to, bool karamaShipment, bool backToReserves)
        {
            if (backToReserves)
            {
                return (int)Math.Ceiling(0.5 * amount);
            }
            else
            {
                if (ShipsForFree(g, p, to))
                {
                    return 0;
                }

                double costOfShipment = Math.Abs(amount) * (to.Territory.IsStronghold ? 1 : 2);

                if (g.MayShipWithDiscount(p) || karamaShipment)
                {
                    costOfShipment /= 2;
                }

                return (int)Math.Ceiling(costOfShipment);
            }
        }

        public static IEnumerable<int> ValidNormalShipmentForces(Player p, bool specialForces)
        {
            return Enumerable.Range(0, 1 + (specialForces ? p.SpecialForcesInReserve : p.ForcesInReserve));
        }

        public static int ValidMaxNormalShipmentForces(Player p, bool specialForces)
        {
            return specialForces ? p.SpecialForcesInReserve : p.ForcesInReserve;
        }

        public static IEnumerable<int> ValidShipmentBackForces(Player p, bool specialForces, Location source)
        {
            if (source == null) return new int[] { 0 };
            return Enumerable.Range(0, 1 + (specialForces ? p.SpecialForcesIn(source) : p.ForcesIn(source)));
        }

        public static int ValidMaxShipmentBackForces(Player p, bool specialForces, Location source)
        {
            if (source == null) return 0;
            return specialForces ? p.SpecialForcesIn(source) : p.ForcesIn(source);
        }

        public static IEnumerable<int> ValidShipmentSiteToSiteForces(Player p, bool specialForces, Location source)
        {
            if (source == null) return new int[] { 0 };
            return Enumerable.Range(0, 1 + (specialForces ? p.SpecialForcesIn(source) : p.ForcesIn(source)));
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

        public Message GetVerboseMessage(MessagePart orangeIncome)
        {
            if (Passed)
            {
                return new Message(Initiator, "{0} pass shipment.", Initiator);
            }
            else
            {
                if (IsBackToReserves)
                {
                    return new Message(Initiator, "{0} ship from {1} back to reserves{2}.{3}{4}", Initiator, To, CostMessage, KaramaMessage, orangeIncome);
                }
                else if (IsSiteToSite)
                {
                    var baseMessage = "{0} site-to-site ship {1} from {5} to {2}{3}.{4}{6}";
                    return new Message(Initiator, baseMessage, Initiator, ForceMessage, To.ToString(), CostMessage, KaramaMessage, From, orangeIncome);
                }
                else
                {
                    var baseMessage = Initiator == Faction.Yellow ? "{0} rally {1} in {2}{3}.{4}{5}" : "{0} ship {1} to {2}{3}.{4}{5}";
                    return new Message(Initiator, baseMessage, Initiator, ForceMessage, To, CostMessage, KaramaMessage, orangeIncome);
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

        private MessagePart CostMessage
        {
            get
            {
                var p = Player;
                var cost = DetermineCost(Game, p, this);

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
        }

        private MessagePart KaramaMessage
        {
            get
            {
                if (KarmaCard != null)
                {
                    var cardOwner = Game.OwnerOf(KarmaCard);
                    var initiator = Player;
                    if (cardOwner != initiator)
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
        }

        public int DetermineOrangeProfits(Game game)
        {
            var initiator = Player;
            return
                DetermineCostToInitiator() +
                ((initiator.Ally != Faction.Orange || game.Applicable(Rule.OrangeShipmentContributionsFlowBack)) ? AllyContributionAmount : 0);
        }

        public int DetermineCostToInitiator()
        {
            return DetermineCost(Game, Player, ForceAmount + SpecialForceAmount, To, UsingKarma(Game), IsBackToReserves) - AllyContributionAmount;
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
