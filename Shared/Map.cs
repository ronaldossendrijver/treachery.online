/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Map
    {
        public const int NUMBER_OF_SECTORS = 18;

        public List<Location> Locations { get; set; }
        public readonly LocationFetcher LocationLookup;
        public readonly TerritoryFetcher TerritoryLookup;

        public Location TheGreaterFlat { get; set; }

        public Location PolarSink { get; set; }

        public Location Arrakeen { get; set; }

        public Location Carthag { get; set; }

        public Location TueksSietch { get; set; }

        public Location SietchTabr { get; set; }

        public Location HabbanyaSietch { get; set; }

        public Territory FalseWallSouth { get; set; }

        public Territory FalseWallWest { get; set; }

        public Territory ImperialBasin { get; set; }

        public Territory ShieldWall { get; set; }

        public Territory HoleInTheRock { get; set; }

        public Territory FalseWallEast { get; set; }

        public Territory TheMinorErg { get; set; }

        public Territory PastyMesa { get; set; }

        public Territory GaraKulon { get; set; }

        public Territory SihayaRidge { get; set; }

        public Location FuneralPlain { get; set; }

        public Territory BightOfTheCliff { get; set; }

        public Territory PlasticBasin { get; set; }

        public Territory RockOutcroppings { get; set; }

        public Territory BrokenLand { get; set; }

        public Territory Tsimpo { get; set; }

        public Territory HaggaBasin { get; set; }

        public Territory WindPass { get; set; }

        public Territory WindPassNorth { get; set; }

        public Territory CielagoWest { get; set; }

        public Territory HabbanyaErg { get; private set; }

        public Location TheGreatFlat { get; set; }

        public HiddenMobileStronghold HiddenMobileStronghold { get; set; }

        public Map()
        {
            LocationLookup = new LocationFetcher(this);
            TerritoryLookup = new TerritoryFetcher(this);
            Initialize();
        }

        public void Initialize()
        {
            InitializeLocations();
            InitializeLocationNeighbours();
        }

        public IEnumerable<Territory> Territories => Locations.Select(l => l.Territory).Distinct();

        public IEnumerable<Location> Strongholds => Locations.Where(l => l.Territory.IsStronghold);

        public static IEnumerable<ResourceCard> GetResourceCardsInAndOutsidePlay(Map m)
        {
            var result = new List<ResourceCard>();
            foreach (var location in m.Locations.Where(l => l.SpiceBlowAmount > 0))
            {
                result.Add(new ResourceCard(location.Territory.Id) { Location = location });
            }

            for (int i = 1; i <= 6; i++)
            {
                result.Add(new ResourceCard(98));
            }

            result.Add(new ResourceCard(99) { IsSandTrout = true });

            return result;
        }

        public static IEnumerable<ResourceCard> GetResourceCardsInPlay(Game g)
        {
            var result = new List<ResourceCard>();
            foreach (var location in g.Map.Locations.Where(l => l.SpiceBlowAmount > 0))
            {
                result.Add(new ResourceCard(location.Territory.Id) { Location = location });
            }

            for (int i = 1; i <= 6; i++)
            {
                result.Add(new ResourceCard(98));
            }

            if (g.Applicable(Rule.GreyAndPurpleExpansionSandTrout))
            {
                result.Add(new ResourceCard(99) { IsSandTrout = true });
            }

            return result;
        }

        private void InitializeLocations()
        {
            Locations = new List<Location>();
            int id = 0;

            {
                var t = new Territory(0)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                PolarSink = (new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = -1,
                    SpiceBlowAmount = 0
                });
                Locations.Add(PolarSink);
            }

            {
                ImperialBasin = new Territory(1)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {//1
                    Territory = ImperialBasin,
                    Name = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {//2
                    Territory = ImperialBasin,
                    Name = "Center",
                    Sector = 9,
                    SpiceBlowAmount = 0

                });
                Locations.Add(new Location(id++)
                {//3
                    Territory = ImperialBasin,
                    Name = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(2)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Carthag = new Location(id++)
                {//4
                    Territory = t,
                    Name = "",
                    Sector = 10,
                    SpiceBlowAmount = 0
                };
                Locations.Add(Carthag);
            }

            {
                var t = new Territory(3)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Arrakeen = new Location(id++)
                {//5
                    Territory = t,
                    Name = "",
                    Sector = 9,
                    SpiceBlowAmount = 0

                };
                Locations.Add(Arrakeen);
            }

            {
                var t = new Territory(4)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                TueksSietch = new Location(id++)
                {//6
                    Territory = t,
                    Name = "",
                    Sector = 4,
                    SpiceBlowAmount = 0
                };
                Locations.Add(TueksSietch);
            }

            {
                var t = new Territory(5)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                SietchTabr = new Location(id++)
                {//7
                    Territory = t,
                    Name = "",
                    Sector = 13,
                    SpiceBlowAmount = 0
                };
                Locations.Add(SietchTabr);
            }

            {
                var t = new Territory(6)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                HabbanyaSietch = new Location(id++)
                {//8
                    Territory = t,
                    Name = "",
                    Sector = 16,
                    SpiceBlowAmount = 0
                };
                Locations.Add(HabbanyaSietch);
            }

            {
                var t = new Territory(7)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 8
                });
            }

            {
                var t = new Territory(8)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(9)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 1,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(10)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 1,
                    SpiceBlowAmount = 12
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(11)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 2,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(12)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallSouth = new Territory(13)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Name = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Name = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallEast = new Territory(14)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Middle",
                    Sector = 6,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "North",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Far North",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                TheMinorErg = new Territory(15)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 8
                });
            }

            {
                PastyMesa = new Territory(16)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(17)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 6,
                    SpiceBlowAmount = 8
                });
            }

            {
                var t = new Territory(18)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "South",
                    Sector = 3,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Middle",
                    Sector = 4,
                    SpiceBlowAmount = 10
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "North",
                    Sector = 5,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(19)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(20)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                HoleInTheRock = new Territory(21)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HoleInTheRock,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                SihayaRidge = new Territory(22)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = SihayaRidge,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 6
                });
            }

            {
                ShieldWall = new Territory(23)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Name = "South",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Name = "North",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
            }

            {
                GaraKulon = new Territory(24)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = GaraKulon,
                    Name = "",
                    Sector = 7,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(25)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Middle",
                    Sector = 9,
                    SpiceBlowAmount = 6
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
            }

            {
                BrokenLand = new Territory(26)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Name = "West",
                    Sector = 11,
                    SpiceBlowAmount = 8
                });
            }

            {
                Tsimpo = new Territory(27)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "Middle",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "West",
                    Sector = 12,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(28)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
            }

            {
                RockOutcroppings = new Territory(29)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Name = "North",
                    Sector = 12,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Name = "South",
                    Sector = 13,
                    SpiceBlowAmount = 6
                });
            }

            {
                PlasticBasin = new Territory(30)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "North",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "Middle",
                    Sector = 12,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "South",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
            }

            {
                HaggaBasin = new Territory(31)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Name = "East",
                    Sector = 11,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Name = "West",
                    Sector = 12,
                    SpiceBlowAmount = 6
                });
            }

            {
                BightOfTheCliff = new Territory(32)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Name = "North",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Name = "South",
                    Sector = 14,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(33)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                FuneralPlain = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 14,
                    SpiceBlowAmount = 6
                };
                Locations.Add(FuneralPlain);
            }

            {
                var t = new Territory(34)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                TheGreatFlat = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 14,
                    SpiceBlowAmount = 10
                };
                Locations.Add(TheGreatFlat);
            }

            {
                WindPass = new Territory(35)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "Far North",
                    Sector = 13,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "North",
                    Sector = 14,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "South",
                    Sector = 15,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "Far South",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(36)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                TheGreaterFlat = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 15,
                    SpiceBlowAmount = 0
                };

                Locations.Add(TheGreaterFlat);
            }

            {
                HabbanyaErg = new Territory(37)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Name = "West",
                    Sector = 15,
                    SpiceBlowAmount = 8
                });
                Locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Name = "East",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
            }

            {
                FalseWallWest = new Territory(38)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "North",
                    Sector = 15,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "Middle",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
            }

            {
                WindPassNorth = new Territory(39)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Name = "North",
                    Sector = 16,
                    SpiceBlowAmount = 6
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Name = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
            }

            {
                var t = new Territory(40)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 16,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 17,
                    SpiceBlowAmount = 10
                });
            }

            {
                CielagoWest = new Territory(41)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false
                };
                Locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Name = "North",
                    Sector = 17,
                    SpiceBlowAmount = 0
                });
                Locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Name = "South",
                    Sector = 00,
                    SpiceBlowAmount = 0
                });
            }

            {
                HiddenMobileStronghold = new HiddenMobileStronghold(id++)
                {
                    SpiceBlowAmount = 0,
                };

                Locations.Add(HiddenMobileStronghold);
            }
        }

        public void InitializeLocationNeighbours()
        {
            Locations[5].Neighbours.Add(Locations[2]);
            Locations[5].Neighbours.Add(Locations[50]);
            Locations[5].Neighbours.Add(Locations[43]);
            Locations[57].Neighbours.Add(Locations[58]);
            Locations[57].Neighbours.Add(Locations[64]);
            Locations[57].Neighbours.Add(Locations[2]);
            Locations[57].Neighbours.Add(Locations[3]);
            Locations[57].Neighbours.Add(Locations[0]);
            Locations[57].Neighbours.Add(Locations[4]);
            Locations[14].Neighbours.Add(Locations[19]);
            Locations[14].Neighbours.Add(Locations[11]);
            Locations[14].Neighbours.Add(Locations[18]);
            Locations[14].Neighbours.Add(Locations[13]);
            Locations[12].Neighbours.Add(Locations[9]);
            Locations[12].Neighbours.Add(Locations[85]);
            Locations[12].Neighbours.Add(Locations[15]);
            Locations[12].Neighbours.Add(Locations[13]);
            Locations[20].Neighbours.Add(Locations[19]);
            Locations[20].Neighbours.Add(Locations[23]);
            Locations[20].Neighbours.Add(Locations[39]);
            Locations[19].Neighbours.Add(Locations[14]);
            Locations[19].Neighbours.Add(Locations[20]);
            Locations[19].Neighbours.Add(Locations[11]);
            Locations[19].Neighbours.Add(Locations[18]);
            Locations[19].Neighbours.Add(Locations[23]);
            Locations[10].Neighbours.Add(Locations[11]);
            Locations[10].Neighbours.Add(Locations[9]);
            Locations[10].Neighbours.Add(Locations[0]);
            Locations[10].Neighbours.Add(Locations[13]);
            Locations[11].Neighbours.Add(Locations[14]);
            Locations[11].Neighbours.Add(Locations[19]);
            Locations[11].Neighbours.Add(Locations[10]);
            Locations[11].Neighbours.Add(Locations[23]);
            Locations[11].Neighbours.Add(Locations[21]);
            Locations[11].Neighbours.Add(Locations[0]);
            Locations[9].Neighbours.Add(Locations[12]);
            Locations[9].Neighbours.Add(Locations[10]);
            Locations[9].Neighbours.Add(Locations[84]);
            Locations[9].Neighbours.Add(Locations[85]);
            Locations[9].Neighbours.Add(Locations[0]);
            Locations[9].Neighbours.Add(Locations[81]);
            Locations[18].Neighbours.Add(Locations[14]);
            Locations[18].Neighbours.Add(Locations[19]);
            Locations[18].Neighbours.Add(Locations[17]);
            Locations[18].Neighbours.Add(Locations[13]);
            Locations[17].Neighbours.Add(Locations[18]);
            Locations[17].Neighbours.Add(Locations[16]);
            Locations[17].Neighbours.Add(Locations[13]);
            Locations[84].Neighbours.Add(Locations[9]);
            Locations[84].Neighbours.Add(Locations[85]);
            Locations[84].Neighbours.Add(Locations[79]);
            Locations[84].Neighbours.Add(Locations[83]);
            Locations[84].Neighbours.Add(Locations[73]);
            Locations[84].Neighbours.Add(Locations[81]);
            Locations[58].Neighbours.Add(Locations[57]);
            Locations[58].Neighbours.Add(Locations[64]);
            Locations[58].Neighbours.Add(Locations[65]);
            Locations[58].Neighbours.Add(Locations[0]);
            Locations[85].Neighbours.Add(Locations[12]);
            Locations[85].Neighbours.Add(Locations[9]);
            Locations[85].Neighbours.Add(Locations[84]);
            Locations[85].Neighbours.Add(Locations[15]);
            Locations[29].Neighbours.Add(Locations[28]);
            Locations[29].Neighbours.Add(Locations[1]);
            Locations[29].Neighbours.Add(Locations[0]);
            Locations[29].Neighbours.Add(Locations[47]);
            Locations[29].Neighbours.Add(Locations[46]);
            Locations[25].Neighbours.Add(Locations[26]);
            Locations[25].Neighbours.Add(Locations[22]);
            Locations[25].Neighbours.Add(Locations[21]);
            Locations[25].Neighbours.Add(Locations[0]);
            Locations[25].Neighbours.Add(Locations[30]);
            Locations[27].Neighbours.Add(Locations[28]);
            Locations[27].Neighbours.Add(Locations[26]);
            Locations[27].Neighbours.Add(Locations[0]);
            Locations[27].Neighbours.Add(Locations[32]);
            Locations[28].Neighbours.Add(Locations[29]);
            Locations[28].Neighbours.Add(Locations[27]);
            Locations[28].Neighbours.Add(Locations[0]);
            Locations[28].Neighbours.Add(Locations[46]);
            Locations[28].Neighbours.Add(Locations[33]);
            Locations[26].Neighbours.Add(Locations[25]);
            Locations[26].Neighbours.Add(Locations[27]);
            Locations[26].Neighbours.Add(Locations[0]);
            Locations[26].Neighbours.Add(Locations[31]);
            Locations[24].Neighbours.Add(Locations[23]);
            Locations[24].Neighbours.Add(Locations[22]);
            Locations[24].Neighbours.Add(Locations[34]);
            Locations[24].Neighbours.Add(Locations[40]);
            Locations[24].Neighbours.Add(Locations[30]);
            Locations[24].Neighbours.Add(Locations[6]);
            Locations[23].Neighbours.Add(Locations[20]);
            Locations[23].Neighbours.Add(Locations[19]);
            Locations[23].Neighbours.Add(Locations[11]);
            Locations[23].Neighbours.Add(Locations[24]);
            Locations[23].Neighbours.Add(Locations[21]);
            Locations[23].Neighbours.Add(Locations[39]);
            Locations[78].Neighbours.Add(Locations[77]);
            Locations[78].Neighbours.Add(Locations[79]);
            Locations[78].Neighbours.Add(Locations[76]);
            Locations[78].Neighbours.Add(Locations[82]);
            Locations[78].Neighbours.Add(Locations[73]);
            Locations[77].Neighbours.Add(Locations[78]);
            Locations[77].Neighbours.Add(Locations[74]);
            Locations[77].Neighbours.Add(Locations[72]);
            Locations[42].Neighbours.Add(Locations[44]);
            Locations[42].Neighbours.Add(Locations[49]);
            Locations[42].Neighbours.Add(Locations[43]);
            Locations[42].Neighbours.Add(Locations[45]);
            Locations[79].Neighbours.Add(Locations[84]);
            Locations[79].Neighbours.Add(Locations[78]);
            Locations[79].Neighbours.Add(Locations[83]);
            Locations[68].Neighbours.Add(Locations[67]);
            Locations[68].Neighbours.Add(Locations[63]);
            Locations[68].Neighbours.Add(Locations[69]);
            Locations[48].Neighbours.Add(Locations[37]);
            Locations[48].Neighbours.Add(Locations[46]);
            Locations[48].Neighbours.Add(Locations[45]);
            Locations[76].Neighbours.Add(Locations[78]);
            Locations[76].Neighbours.Add(Locations[75]);
            Locations[76].Neighbours.Add(Locations[82]);
            Locations[75].Neighbours.Add(Locations[76]);
            Locations[75].Neighbours.Add(Locations[82]);
            Locations[75].Neighbours.Add(Locations[74]);
            Locations[83].Neighbours.Add(Locations[84]);
            Locations[83].Neighbours.Add(Locations[82]);
            Locations[83].Neighbours.Add(Locations[8]);
            Locations[83].Neighbours.Add(Locations[15]);
            Locations[82].Neighbours.Add(Locations[78]);
            Locations[82].Neighbours.Add(Locations[76]);
            Locations[82].Neighbours.Add(Locations[75]);
            Locations[82].Neighbours.Add(Locations[83]);
            Locations[82].Neighbours.Add(Locations[8]);
            Locations[8].Neighbours.Add(Locations[83]);
            Locations[8].Neighbours.Add(Locations[82]);
            Locations[64].Neighbours.Add(Locations[57]);
            Locations[64].Neighbours.Add(Locations[58]);
            Locations[64].Neighbours.Add(Locations[65]);
            Locations[64].Neighbours.Add(Locations[55]);
            Locations[64].Neighbours.Add(Locations[4]);
            Locations[65].Neighbours.Add(Locations[58]);
            Locations[65].Neighbours.Add(Locations[64]);
            Locations[65].Neighbours.Add(Locations[62]);
            Locations[65].Neighbours.Add(Locations[63]);
            Locations[65].Neighbours.Add(Locations[0]);
            Locations[65].Neighbours.Add(Locations[56]);
            Locations[65].Neighbours.Add(Locations[70]);
            Locations[66].Neighbours.Add(Locations[67]);
            Locations[66].Neighbours.Add(Locations[63]);
            Locations[66].Neighbours.Add(Locations[60]);
            Locations[66].Neighbours.Add(Locations[7]);
            Locations[22].Neighbours.Add(Locations[25]);
            Locations[22].Neighbours.Add(Locations[24]);
            Locations[22].Neighbours.Add(Locations[21]);
            Locations[22].Neighbours.Add(Locations[30]);
            Locations[21].Neighbours.Add(Locations[11]);
            Locations[21].Neighbours.Add(Locations[25]);
            Locations[21].Neighbours.Add(Locations[23]);
            Locations[21].Neighbours.Add(Locations[22]);
            Locations[21].Neighbours.Add(Locations[0]);
            Locations[44].Neighbours.Add(Locations[42]);
            Locations[44].Neighbours.Add(Locations[1]);
            Locations[44].Neighbours.Add(Locations[43]);
            Locations[44].Neighbours.Add(Locations[47]);
            Locations[44].Neighbours.Add(Locations[45]);
            Locations[2].Neighbours.Add(Locations[5]);
            Locations[2].Neighbours.Add(Locations[57]);
            Locations[2].Neighbours.Add(Locations[1]);
            Locations[2].Neighbours.Add(Locations[3]);
            Locations[2].Neighbours.Add(Locations[50]);
            Locations[2].Neighbours.Add(Locations[0]);
            Locations[2].Neighbours.Add(Locations[43]);
            Locations[1].Neighbours.Add(Locations[29]);
            Locations[1].Neighbours.Add(Locations[44]);
            Locations[1].Neighbours.Add(Locations[2]);
            Locations[1].Neighbours.Add(Locations[0]);
            Locations[1].Neighbours.Add(Locations[43]);
            Locations[1].Neighbours.Add(Locations[47]);
            Locations[3].Neighbours.Add(Locations[57]);
            Locations[3].Neighbours.Add(Locations[2]);
            Locations[3].Neighbours.Add(Locations[54]);
            Locations[3].Neighbours.Add(Locations[4]);
            Locations[16].Neighbours.Add(Locations[17]);
            Locations[16].Neighbours.Add(Locations[15]);
            Locations[16].Neighbours.Add(Locations[13]);
            Locations[15].Neighbours.Add(Locations[12]);
            Locations[15].Neighbours.Add(Locations[85]);
            Locations[15].Neighbours.Add(Locations[83]);
            Locations[15].Neighbours.Add(Locations[16]);
            Locations[49].Neighbours.Add(Locations[42]);
            Locations[49].Neighbours.Add(Locations[50]);
            Locations[49].Neighbours.Add(Locations[43]);
            Locations[50].Neighbours.Add(Locations[5]);
            Locations[50].Neighbours.Add(Locations[2]);
            Locations[50].Neighbours.Add(Locations[49]);
            Locations[50].Neighbours.Add(Locations[51]);
            Locations[67].Neighbours.Add(Locations[68]);
            Locations[67].Neighbours.Add(Locations[66]);
            Locations[51].Neighbours.Add(Locations[50]);
            Locations[51].Neighbours.Add(Locations[52]);
            Locations[51].Neighbours.Add(Locations[54]);
            Locations[37].Neighbours.Add(Locations[48]);
            Locations[37].Neighbours.Add(Locations[36]);
            Locations[37].Neighbours.Add(Locations[46]);
            Locations[37].Neighbours.Add(Locations[33]);
            Locations[34].Neighbours.Add(Locations[24]);
            Locations[34].Neighbours.Add(Locations[35]);
            Locations[34].Neighbours.Add(Locations[40]);
            Locations[34].Neighbours.Add(Locations[30]);
            Locations[34].Neighbours.Add(Locations[6]);
            Locations[36].Neighbours.Add(Locations[37]);
            Locations[36].Neighbours.Add(Locations[35]);
            Locations[36].Neighbours.Add(Locations[38]);
            Locations[36].Neighbours.Add(Locations[32]);
            Locations[35].Neighbours.Add(Locations[34]);
            Locations[35].Neighbours.Add(Locations[36]);
            Locations[35].Neighbours.Add(Locations[41]);
            Locations[35].Neighbours.Add(Locations[31]);
            Locations[62].Neighbours.Add(Locations[65]);
            Locations[62].Neighbours.Add(Locations[61]);
            Locations[62].Neighbours.Add(Locations[63]);
            Locations[62].Neighbours.Add(Locations[59]);
            Locations[62].Neighbours.Add(Locations[56]);
            Locations[61].Neighbours.Add(Locations[62]);
            Locations[61].Neighbours.Add(Locations[53]);
            Locations[61].Neighbours.Add(Locations[55]);
            Locations[63].Neighbours.Add(Locations[68]);
            Locations[63].Neighbours.Add(Locations[65]);
            Locations[63].Neighbours.Add(Locations[66]);
            Locations[63].Neighbours.Add(Locations[62]);
            Locations[63].Neighbours.Add(Locations[60]);
            Locations[63].Neighbours.Add(Locations[7]);
            Locations[63].Neighbours.Add(Locations[69]);
            Locations[63].Neighbours.Add(Locations[70]);
            Locations[0].Neighbours.Add(Locations[57]);
            Locations[0].Neighbours.Add(Locations[10]);
            Locations[0].Neighbours.Add(Locations[11]);
            Locations[0].Neighbours.Add(Locations[9]);
            Locations[0].Neighbours.Add(Locations[58]);
            Locations[0].Neighbours.Add(Locations[29]);
            Locations[0].Neighbours.Add(Locations[25]);
            Locations[0].Neighbours.Add(Locations[27]);
            Locations[0].Neighbours.Add(Locations[28]);
            Locations[0].Neighbours.Add(Locations[26]);
            Locations[0].Neighbours.Add(Locations[65]);
            Locations[0].Neighbours.Add(Locations[21]);
            Locations[0].Neighbours.Add(Locations[2]);
            Locations[0].Neighbours.Add(Locations[1]);
            Locations[0].Neighbours.Add(Locations[70]);
            Locations[0].Neighbours.Add(Locations[71]);
            Locations[0].Neighbours.Add(Locations[72]);
            Locations[0].Neighbours.Add(Locations[80]);
            Locations[0].Neighbours.Add(Locations[81]);
            Locations[38].Neighbours.Add(Locations[36]);
            Locations[38].Neighbours.Add(Locations[41]);
            Locations[52].Neighbours.Add(Locations[51]);
            Locations[52].Neighbours.Add(Locations[53]);
            Locations[52].Neighbours.Add(Locations[54]);
            Locations[43].Neighbours.Add(Locations[5]);
            Locations[43].Neighbours.Add(Locations[42]);
            Locations[43].Neighbours.Add(Locations[44]);
            Locations[43].Neighbours.Add(Locations[2]);
            Locations[43].Neighbours.Add(Locations[1]);
            Locations[43].Neighbours.Add(Locations[49]);
            Locations[59].Neighbours.Add(Locations[62]);
            Locations[59].Neighbours.Add(Locations[60]);
            Locations[59].Neighbours.Add(Locations[53]);
            Locations[60].Neighbours.Add(Locations[66]);
            Locations[60].Neighbours.Add(Locations[63]);
            Locations[60].Neighbours.Add(Locations[59]);
            Locations[60].Neighbours.Add(Locations[7]);
            Locations[47].Neighbours.Add(Locations[29]);
            Locations[47].Neighbours.Add(Locations[44]);
            Locations[47].Neighbours.Add(Locations[1]);
            Locations[47].Neighbours.Add(Locations[46]);
            Locations[47].Neighbours.Add(Locations[45]);
            Locations[46].Neighbours.Add(Locations[28]);
            Locations[46].Neighbours.Add(Locations[48]);
            Locations[46].Neighbours.Add(Locations[37]);
            Locations[46].Neighbours.Add(Locations[47]);
            Locations[46].Neighbours.Add(Locations[33]);
            Locations[7].Neighbours.Add(Locations[66]);
            Locations[7].Neighbours.Add(Locations[63]);
            Locations[7].Neighbours.Add(Locations[60]);
            Locations[45].Neighbours.Add(Locations[42]);
            Locations[45].Neighbours.Add(Locations[48]);
            Locations[45].Neighbours.Add(Locations[44]);
            Locations[45].Neighbours.Add(Locations[47]);
            Locations[40].Neighbours.Add(Locations[24]);
            Locations[40].Neighbours.Add(Locations[34]);
            Locations[40].Neighbours.Add(Locations[41]);
            Locations[40].Neighbours.Add(Locations[39]);
            Locations[40].Neighbours.Add(Locations[6]);
            Locations[41].Neighbours.Add(Locations[35]);
            Locations[41].Neighbours.Add(Locations[38]);
            Locations[41].Neighbours.Add(Locations[40]);
            Locations[39].Neighbours.Add(Locations[20]);
            Locations[39].Neighbours.Add(Locations[23]);
            Locations[39].Neighbours.Add(Locations[40]);
            Locations[53].Neighbours.Add(Locations[61]);
            Locations[53].Neighbours.Add(Locations[52]);
            Locations[53].Neighbours.Add(Locations[59]);
            Locations[53].Neighbours.Add(Locations[55]);
            Locations[69].Neighbours.Add(Locations[68]);
            Locations[69].Neighbours.Add(Locations[63]);
            Locations[69].Neighbours.Add(Locations[74]);
            Locations[69].Neighbours.Add(Locations[71]);
            Locations[74].Neighbours.Add(Locations[77]);
            Locations[74].Neighbours.Add(Locations[75]);
            Locations[74].Neighbours.Add(Locations[69]);
            Locations[74].Neighbours.Add(Locations[72]);
            Locations[33].Neighbours.Add(Locations[28]);
            Locations[33].Neighbours.Add(Locations[37]);
            Locations[33].Neighbours.Add(Locations[46]);
            Locations[33].Neighbours.Add(Locations[32]);
            Locations[30].Neighbours.Add(Locations[25]);
            Locations[30].Neighbours.Add(Locations[24]);
            Locations[30].Neighbours.Add(Locations[22]);
            Locations[30].Neighbours.Add(Locations[34]);
            Locations[30].Neighbours.Add(Locations[31]);
            Locations[32].Neighbours.Add(Locations[27]);
            Locations[32].Neighbours.Add(Locations[36]);
            Locations[32].Neighbours.Add(Locations[33]);
            Locations[32].Neighbours.Add(Locations[31]);
            Locations[31].Neighbours.Add(Locations[26]);
            Locations[31].Neighbours.Add(Locations[35]);
            Locations[31].Neighbours.Add(Locations[30]);
            Locations[31].Neighbours.Add(Locations[32]);
            Locations[54].Neighbours.Add(Locations[3]);
            Locations[54].Neighbours.Add(Locations[51]);
            Locations[54].Neighbours.Add(Locations[52]);
            Locations[54].Neighbours.Add(Locations[55]);
            Locations[54].Neighbours.Add(Locations[4]);
            Locations[55].Neighbours.Add(Locations[64]);
            Locations[55].Neighbours.Add(Locations[61]);
            Locations[55].Neighbours.Add(Locations[53]);
            Locations[55].Neighbours.Add(Locations[54]);
            Locations[55].Neighbours.Add(Locations[56]);
            Locations[55].Neighbours.Add(Locations[4]);
            Locations[56].Neighbours.Add(Locations[65]);
            Locations[56].Neighbours.Add(Locations[62]);
            Locations[56].Neighbours.Add(Locations[55]);
            Locations[6].Neighbours.Add(Locations[24]);
            Locations[6].Neighbours.Add(Locations[34]);
            Locations[6].Neighbours.Add(Locations[40]);
            Locations[4].Neighbours.Add(Locations[57]);
            Locations[4].Neighbours.Add(Locations[64]);
            Locations[4].Neighbours.Add(Locations[3]);
            Locations[4].Neighbours.Add(Locations[54]);
            Locations[4].Neighbours.Add(Locations[55]);
            Locations[70].Neighbours.Add(Locations[65]);
            Locations[70].Neighbours.Add(Locations[63]);
            Locations[70].Neighbours.Add(Locations[0]);
            Locations[70].Neighbours.Add(Locations[71]);
            Locations[73].Neighbours.Add(Locations[84]);
            Locations[73].Neighbours.Add(Locations[78]);
            Locations[73].Neighbours.Add(Locations[72]);
            Locations[73].Neighbours.Add(Locations[80]);
            Locations[71].Neighbours.Add(Locations[0]);
            Locations[71].Neighbours.Add(Locations[69]);
            Locations[71].Neighbours.Add(Locations[70]);
            Locations[71].Neighbours.Add(Locations[72]);
            Locations[72].Neighbours.Add(Locations[77]);
            Locations[72].Neighbours.Add(Locations[0]);
            Locations[72].Neighbours.Add(Locations[74]);
            Locations[72].Neighbours.Add(Locations[73]);
            Locations[72].Neighbours.Add(Locations[71]);
            Locations[72].Neighbours.Add(Locations[80]);
            Locations[80].Neighbours.Add(Locations[0]);
            Locations[80].Neighbours.Add(Locations[73]);
            Locations[80].Neighbours.Add(Locations[72]);
            Locations[80].Neighbours.Add(Locations[81]);
            Locations[81].Neighbours.Add(Locations[9]);
            Locations[81].Neighbours.Add(Locations[84]);
            Locations[81].Neighbours.Add(Locations[0]);
            Locations[81].Neighbours.Add(Locations[80]);
            Locations[13].Neighbours.Add(Locations[14]);
            Locations[13].Neighbours.Add(Locations[12]);
            Locations[13].Neighbours.Add(Locations[10]);
            Locations[13].Neighbours.Add(Locations[17]);
            Locations[13].Neighbours.Add(Locations[16]);
        }

        struct NeighbourCacheKey
        {
            internal Location start;
            internal int distance;
            internal Faction faction;
            internal bool ignoreStorm;

            public override bool Equals(object obj)
            {
                return
                    obj is NeighbourCacheKey c &&
                    c.start == this.start &&
                    c.distance == this.distance &&
                    c.faction == this.faction &&
                    c.ignoreStorm == this.ignoreStorm;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(start, distance, faction);
            }
        }

        private int NeighbourCacheTimestamp = -1;
        private readonly Dictionary<NeighbourCacheKey, List<Location>> NeighbourCache = new Dictionary<NeighbourCacheKey, List<Location>>();

        public List<Location> FindNeighbours(Location start, int distance, bool ignoreStorm, Faction f, Game game, bool checkForceObstacles = true)
        {
            var cacheKey = new NeighbourCacheKey() { start = start, distance = distance, faction = f, ignoreStorm = ignoreStorm };

            if (checkForceObstacles)
            {
                if (NeighbourCacheTimestamp != game.History.Count)
                {
                    NeighbourCache.Clear();
                    NeighbourCacheTimestamp = game.History.Count;
                }
                else if (NeighbourCache.ContainsKey(cacheKey))
                {
                    return NeighbourCache[cacheKey];
                }
            }

            var forceObstacles = new List<Location>();
            if (checkForceObstacles)
            {
                forceObstacles = DetermineForceObstacles(f, game);
            }

            List<Location> neighbours = new List<Location>();
            FindNeighbours(neighbours, start, null, 0, distance, f, ignoreStorm ? 99 : game.SectorInStorm, forceObstacles);

            neighbours.Remove(start);

            if (checkForceObstacles)
            {
                NeighbourCache.Add(cacheKey, neighbours);
            }

            return neighbours;
        }

        private static List<Location> DetermineForceObstacles(Faction f, Game game)
        {
            return game.ForcesOnPlanet.Where(kvp =>
                kvp.Key.IsStronghold &&
                !kvp.Value.Any(b => b.Faction == f) &&
                kvp.Value.Count(b => b.CanOccupy) >= 2)
                .Select(kvp => kvp.Key)
                .Distinct()
                .Union(game.CurrentBlockedTerritories.SelectMany(t => t.Locations))
                .ToList();
        }

        private static void FindNeighbours(
            List<Location> found, 
            Location current, 
            Location previous, 
            int currentDistance, 
            int maxDistance, 
            Faction f, 
            int sectorInStorm, 
            List<Location> forceObstacles)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours.Where(n => n != previous && n.Sector != sectorInStorm && !forceObstacles.Contains(n)))
            {
                int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                if (currentDistance + distance <= maxDistance)
                {
                    FindNeighbours(found, neighbour, current, currentDistance + distance, maxDistance, f, sectorInStorm, forceObstacles);
                }
            }
        }

        public static List<List<Location>> FindPaths(Location start, Location destination, int distance, bool ignoreStorm, Faction f, Game game)
        {
            var paths = new List<List<Location>>();
            var route = new Stack<Location>();
            var obstacles = DetermineForceObstacles(f, game);
            FindPaths(paths, route, start, destination, null, 0, distance, f, ignoreStorm ? 99 : game.SectorInStorm, obstacles);
            return paths;
        }

        private static void FindPaths(List<List<Location>> foundPaths, Stack<Location> currentPath, Location current, Location destination, Location previous, int currentDistance, int maxDistance, Faction f, int sectorInStorm, List<Location> obstacles)
        {
            currentPath.Push(current);

            if (current.Equals(destination))
            {
                foundPaths.Add(currentPath.ToList());
            }
            else
            {
                foreach (var neighbour in current.Neighbours.Where(neighbour => 
                    neighbour != previous &&
                    neighbour.Sector != sectorInStorm &&
                    !currentPath.Contains(neighbour) &&
                    !obstacles.Contains(neighbour)))
                {
                    if (neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            FindPaths(foundPaths, currentPath, neighbour, destination, current, currentDistance + distance, maxDistance, f, sectorInStorm, obstacles);
                        }
                    }
                }
            }

            currentPath.Pop();
        }

        public static List<Location> FindFirstShortestPath(Location start, Location destination, bool ignoreStorm, Faction f, Game game)
        {
            var route = new Stack<Location>();
            var obstacles = DetermineForceObstacles(f, game);
            for (int i = 0; i <= 4; i++)
            {
                var path = FindPath(route, start, destination, null, 0, i, f, ignoreStorm ? 99 : game.SectorInStorm, obstacles);
                if (path != null) return path;
            }
            return null;
        }

        private static List<Location> FindPath(Stack<Location> currentRoute, Location current, Location destination, Location previous, int currentDistance, int maxDistance, Faction f, int sectorInStorm, List<Location> obstacles)
        {
            currentRoute.Push(current);

            if (current.Equals(destination))
            {
                return currentRoute.Reverse().ToList();
            }
            else
            {
                foreach (var neighbour in current.Neighbours.Where(neighbour =>
                    neighbour != previous &&
                    neighbour.Sector != sectorInStorm &&
                    !currentRoute.Contains(neighbour) &&
                    !obstacles.Contains(neighbour)))
                {
                    if (neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            var found = FindPath(currentRoute, neighbour, destination, current, currentDistance + distance, maxDistance, f, sectorInStorm, obstacles);
                            if (found != null) return found;
                        }
                    }
                }
            }

            currentRoute.Pop();
            return null;
        }

        public static List<Location> FindNeighboursForHmsMovement(Location start, int distance, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new List<Location>();
            FindNeighboursForHmsMovement(neighbours, start, null, 0, distance, ignoreStorm, sectorInStorm);
            neighbours.Remove(start);
            return neighbours;
        }

        private static void FindNeighboursForHmsMovement(List<Location> found, Location current, Location previous, int currentDistance, int maxDistance, bool ignoreStorm, int sectorInStorm)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours)
            {
                if (neighbour != previous)
                {
                    if (ignoreStorm || neighbour.Sector != sectorInStorm)
                    {
                        int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                        if (currentDistance + distance <= maxDistance)
                        {
                            FindNeighboursForHmsMovement(found, neighbour, current, currentDistance + distance, maxDistance, ignoreStorm, sectorInStorm);
                        }
                    }
                }
            }
        }

        public static List<Location> FindNeighboursWithinTerritory(Location start, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new List<Location>();
            FindNeighboursWithinTerritory(neighbours, start, null, ignoreStorm, sectorInStorm);
            return neighbours;
        }

        private static void FindNeighboursWithinTerritory(List<Location> found, Location current, Location previous, bool ignoreStorm, int sectorInStorm)
        {
            if (!found.Contains(current))
            {
                found.Add(current);
            }

            foreach (var neighbour in current.Neighbours.Where(l => l.Territory == current.Territory))
            {
                if (neighbour != previous)
                {
                    if (ignoreStorm || neighbour.Sector != sectorInStorm)
                    {
                        FindNeighboursWithinTerritory(found, neighbour, current, ignoreStorm, sectorInStorm);
                    }
                }
            }
        }

        public class TerritoryFetcher : IFetcher<Territory>
        {
            private readonly Map _map;

            public TerritoryFetcher(Map map)
            {
                _map = map;
            }

            public Territory Find(int id)
            {
                if (id == -1)
                {
                    return null;
                }
                else
                {
                    return _map.Territories.SingleOrDefault(t => t.Id == id);
                }
            }

            public int GetId(Territory obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }

        public class LocationFetcher : IFetcher<Location>
        {
            private readonly Map _map;

            public LocationFetcher(Map map)
            {
                _map = map;
            }

            public Location Find(int id)
            {
                if (id == -1)
                {
                    return null;
                }
                else
                {
                    return _map.Locations[id];
                }
            }

            public int GetId(Location obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }
    }
}