/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Treachery.Shared
{
    public class Map
    {
        public static bool CorrectForPreVersion46IssueWithHMS = false;

        public const int NUMBER_OF_SECTORS = 18;

        public List<Location> Locations { get; set; }
        public readonly LocationFetcher LocationLookup;
        public readonly TerritoryFetcher TerritoryLookup;

        public Point TurnMarkerPosition { get; set; }

        public IDictionary<MainPhase, Point> PhaseMarkerPositions { get; set; }

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
            InitializePositions();
        }

        public IEnumerable<Territory> Territories
        {
            get
            {
                return Locations.Select(l => l.Territory).Distinct();
            }
        }

        public int FindSector(int x, int y)
        {
            int radialPosition = (int)Math.Floor(Math.Atan2(y - Skin.Current.PlanetCenter.Y, x - Skin.Current.PlanetCenter.X) * NUMBER_OF_SECTORS / (-2 * Math.PI));
            if (radialPosition >= 0)
            {
                return radialPosition + 6;
            }
            else
            {
                return (radialPosition + 24) % NUMBER_OF_SECTORS;
            }
        }

        public IEnumerable<Location> Strongholds
        {
            get
            {
                return Locations.Where(l => l.Territory.IsStronghold);
            }
        }

        public Location FindLocation(int x, int y)
        {
            if (Arrakeen.IsInside(this, x, y)) return Arrakeen;
            if (Carthag.IsInside(this, x, y)) return Carthag;
            if (SietchTabr.IsInside(this, x, y)) return SietchTabr;
            if (HabbanyaSietch.IsInside(this, x, y)) return HabbanyaSietch;
            if (TueksSietch.IsInside(this, x, y)) return TueksSietch;
            if (HiddenMobileStronghold.IsInside(this, x, y)) return HiddenMobileStronghold;

            int sector = FindSector(x, y);
            return Locations.FirstOrDefault(l => !l.Territory.IsStronghold && (l.Sector == sector || l.Sector == -1) && l.Territory.IsInside(x, y));
        }

        public Location Get(string locationName)
        {
            return Locations.SingleOrDefault(l => l.ToString() == locationName);
        }

        private void InitializePositions()
        {
            TurnMarkerPosition = new Point(500, 495);

            PhaseMarkerPositions = new Dictionary<MainPhase, Point>()
            {
                [MainPhase.Storm] = new Point(1050, 222),
                [MainPhase.Blow] = new Point(1305, 222),
                [MainPhase.Charity] = new Point(1560, 222),
                [MainPhase.Bidding] = new Point(1820, 222),
                [MainPhase.Resurrection] = new Point(2325, 222),
                [MainPhase.ShipmentAndMove] = new Point(2580, 222),
                [MainPhase.Battle] = new Point(2835, 222),
                [MainPhase.Collection] = new Point(3090, 222),
                [MainPhase.Contemplate] = new Point(3090, 222)
            };
        }

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

        public Point GetPhaseMarkerPosition(Phase phase)
        {
            switch (phase)
            {
                case Phase.AwaitingPlayers:
                case Phase.TradingFactions:
                case Phase.BluePredicting:
                case Phase.SelectingTraitors:
                case Phase.YellowSettingUp:
                case Phase.BlueSettingUp:
                case Phase.MetheorAndStormSpell:
                    return PhaseMarkerPositions[MainPhase.Storm];

                case Phase.BlowA:
                case Phase.BlowB:
                case Phase.YellowSendingMonsterA:
                case Phase.AllianceA:
                case Phase.YellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.YellowSendingMonsterB:
                case Phase.AllianceB:
                case Phase.YellowRidingMonsterB:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.BlowReport:
                    return PhaseMarkerPositions[MainPhase.Blow];

                case Phase.ClaimingCharity:
                    return PhaseMarkerPositions[MainPhase.Charity];

                case Phase.Bidding:
                    return PhaseMarkerPositions[MainPhase.Bidding];

                case Phase.Resurrection:
                    return PhaseMarkerPositions[MainPhase.Resurrection];

                case Phase.NonOrangeShip:
                case Phase.OrangeShip:
                case Phase.BlueAccompaniesNonOrange:
                case Phase.BlueAccompaniesOrange:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.NonOrangeMove:
                case Phase.OrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.ShipmentAndMoveConcluded:
                    return PhaseMarkerPositions[MainPhase.ShipmentAndMove];

                case Phase.BattlePhase:
                case Phase.CallTraitorOrPass:
                case Phase.BattleConclusion:
                    return PhaseMarkerPositions[MainPhase.Battle];

                case Phase.TurnConcluded:
                case Phase.GameEnded:
                    return PhaseMarkerPositions[MainPhase.Contemplate];

                default:
                    return PhaseMarkerPositions[MainPhase.Storm];
            }
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
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory0
                };
                PolarSink = (new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = -1,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2040, Y = 2311 }
                });
                Locations.Add(PolarSink);
            }

            {
                ImperialBasin = new Territory(1)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory1
                };
                Locations.Add(new Location(id++)
                {//1
                    Territory = ImperialBasin,
                    Name = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2405, Y = 1785 }
                });
                Locations.Add(new Location(id++)
                {//2
                    Territory = ImperialBasin,
                    Name = "Center",
                    Sector = 9,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2375, Y = 1500 }

                });
                Locations.Add(new Location(id++)
                {//3
                    Territory = ImperialBasin,
                    Name = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2140, Y = 1500 }
                });
            }

            {
                var t = new Territory(2)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory2
                };
                Carthag = new Location(id++)
                {//4
                    Territory = t,
                    Name = "",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2000, Y = 1200 }
                };
                Locations.Add(Carthag);
            }

            {
                var t = new Territory(3)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory3
                };
                Arrakeen = new Location(id++)
                {//5
                    Territory = t,
                    Name = "",
                    Sector = 9,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2565, Y = 1070 }

                };
                Locations.Add(Arrakeen);
            }

            {
                var t = new Territory(4)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory4
                };
                TueksSietch = new Location(id++)
                {//6
                    Territory = t,
                    Name = "",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3320, Y = 2990 }
                };
                Locations.Add(TueksSietch);
            }

            {
                var t = new Territory(5)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory5
                };
                SietchTabr = new Location(id++)
                {//7
                    Territory = t,
                    Name = "",
                    Sector = 13,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 810, Y = 1475 }

                };
                Locations.Add(SietchTabr);
            }

            {
                var t = new Territory(6)
                {
                    IsStronghold = true,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory6
                };
                HabbanyaSietch = new Location(id++)
                {//8
                    Territory = t,
                    Name = "",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 835, Y = 3021 }

                };
                Locations.Add(HabbanyaSietch);
            }

            {
                var t = new Territory(7)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory7
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1775, Y = 3066 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2055, Y = 3096 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 8,
                    SpiceLocation = new Point { X = 2275, Y = 2885 },
                    Center = new Point { X = 2330, Y = 3056 }
                });
            }

            {
                var t = new Territory(8)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory8
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1740, Y = 3361 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Center",
                    Sector = 1,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2040, Y = 3336 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2385, Y = 3391 }
                });
            }

            {
                var t = new Territory(9)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory9
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 0,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1585, Y = 3741 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 1,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1860, Y = 3841 }
                });
            }

            {
                var t = new Territory(10)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory10
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 1,
                    SpiceBlowAmount = 12,
                    SpiceLocation = new Point { X = 2138, Y = 3960 },
                    Center = new Point { X = 2180, Y = 3661 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 2,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2400, Y = 3651 }
                });
            }

            {
                var t = new Territory(11)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory11
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 2,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2600, Y = 3396 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 3,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2800, Y = 3441 }
                });
            }

            {
                var t = new Territory(12)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory12
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2300, Y = 2611 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2455, Y = 2546 }
                });
            }

            {
                FalseWallSouth = new Territory(13)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory13
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Name = "West",
                    Sector = 3,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2860, Y = 3191 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallSouth,
                    Name = "East",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2900, Y = 2891 }
                });
            }

            {
                FalseWallEast = new Territory(14)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory14
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2350, Y = 2451 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2400, Y = 2351 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Middle",
                    Sector = 6,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2445, Y = 2225 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "North",
                    Sector = 7,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2390, Y = 2090 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallEast,
                    Name = "Far North",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2325, Y = 2025 }
                });
            }

            {
                TheMinorErg = new Territory(15)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory15
                };
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2700, Y = 2606 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2655, Y = 2396 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2790, Y = 2165 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = TheMinorErg,
                    Name = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 8,
                    SpiceLocation = new Point { X = 2665, Y = 1996 },
                    Center = new Point { X = 2730, Y = 1970 }
                });
            }

            {
                PastyMesa = new Territory(16)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory16
                };
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "Far South",
                    Sector = 4,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3005, Y = 2686 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "South",
                    Sector = 5,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3190, Y = 2471 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "North",
                    Sector = 6,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3235, Y = 2045 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PastyMesa,
                    Name = "Far North",
                    Sector = 7,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3170, Y = 1720 }
                });
            }

            {
                var t = new Territory(17)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory17
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 6,
                    SpiceBlowAmount = 8,
                    SpiceLocation = new Point { X = 3755, Y = 2145 },
                    Center = new Point { X = 3600, Y = 2050 }
                });
            }

            {
                var t = new Territory(18)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory18
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "South",
                    Sector = 3,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3285, Y = 3476 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Middle",
                    Sector = 4,
                    SpiceBlowAmount = 10,
                    SpiceLocation = new Point { X = 3618, Y = 2945 },
                    Center = new Point { X = 3385, Y = 3301 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "North",
                    Sector = 5,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3730, Y = 2546 }
                });
            }

            {
                var t = new Territory(19)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory19
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3150, Y = 950 }
                });
            }

            {
                var t = new Territory(20)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory20
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2700, Y = 1315 }
                });
            }

            {
                HoleInTheRock = new Territory(21)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory21
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HoleInTheRock,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2890, Y = 1375 }
                });
            }

            {
                SihayaRidge = new Territory(22)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory22
                };
                Locations.Add(new Location(id++)
                {
                    Territory = SihayaRidge,
                    Name = "",
                    Sector = 8,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 3258, Y = 1055 },
                    Center = new Point { X = 3150, Y = 1170 }
                });
            }

            {
                ShieldWall = new Territory(23)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory23
                };
                Locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Name = "South",
                    Sector = 7,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2820, Y = 1730 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = ShieldWall,
                    Name = "North",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2615, Y = 1725 }
                });
            }

            {
                GaraKulon = new Territory(24)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory24
                };
                Locations.Add(new Location(id++)
                {
                    Territory = GaraKulon,
                    Name = "",
                    Sector = 7,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 3415, Y = 1360 }
                });
            }

            {
                var t = new Territory(25)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory25
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 8,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2945, Y = 865 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "Middle",
                    Sector = 9,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 2465, Y = 633 },
                    Center = new Point { X = 2625, Y = 685 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2300, Y = 590 }
                });
            }

            {
                BrokenLand = new Territory(26)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory26
                };
                Locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2050, Y = 605 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = BrokenLand,
                    Name = "West",
                    Sector = 11,
                    SpiceBlowAmount = 8,
                    SpiceLocation = new Point { X = 1443, Y = 728 },
                    Center = new Point { X = 1615, Y = 715 }
                });
            }

            {
                Tsimpo = new Territory(27)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory27
                };
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2000, Y = 840 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "Middle",
                    Sector = 11,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1690, Y = 1010 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = Tsimpo,
                    Name = "West",
                    Sector = 12,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1410, Y = 1305 }
                });
            }

            {
                var t = new Territory(28)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory28
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 10,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 2025, Y = 1640 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 11,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1960, Y = 1970 }
                });
            }

            {
                RockOutcroppings = new Territory(29)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory29
                };
                Locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Name = "North",
                    Sector = 12,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 915, Y = 1150 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = RockOutcroppings,
                    Name = "South",
                    Sector = 13,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 695, Y = 1290 },
                    Center = new Point { X = 600, Y = 1320 }
                });
            }

            {
                PlasticBasin = new Territory(30)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory30
                };
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "North",
                    Sector = 11,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1460, Y = 970 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "Middle",
                    Sector = 12,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1185, Y = 1180 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = PlasticBasin,
                    Name = "South",
                    Sector = 13,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1270, Y = 1865 }
                });
            }

            {
                HaggaBasin = new Territory(31)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory31
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Name = "East",
                    Sector = 11,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1730, Y = 1385 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = HaggaBasin,
                    Name = "West",
                    Sector = 12,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 1592, Y = 1715 },
                    Center = new Point { X = 1465, Y = 1495 }
                });
            }

            {
                BightOfTheCliff = new Territory(32)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory32
                };
                Locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Name = "North",
                    Sector = 13,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 520, Y = 1575 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = BightOfTheCliff,
                    Name = "South",
                    Sector = 14,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 490, Y = 1795 }
                });
            }

            {
                var t = new Territory(33)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory33
                };
                FuneralPlain = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 14,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 435, Y = 1955 },
                    Center = new Point { X = 625, Y = 1920 }
                };
                Locations.Add(FuneralPlain);
            }

            {
                var t = new Territory(34)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory34
                };
                TheGreatFlat = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 14,
                    SpiceLocation = new Point { X = 405, Y = 2155 },
                    SpiceBlowAmount = 10,
                    Center = new Point { X = 1050, Y = 2170 }
                };
                Locations.Add(TheGreatFlat);
            }

            {
                WindPass = new Territory(35)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory35
                };
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "Far North",
                    Sector = 13,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1740, Y = 2110 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "North",
                    Sector = 14,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1670, Y = 2230 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "South",
                    Sector = 15,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1570, Y = 2406 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPass,
                    Name = "Far South",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1525, Y = 2626 }
                });
            }

            {
                var t = new Territory(36)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory36
                };
                TheGreaterFlat = new Location(id++)
                {
                    Territory = t,
                    Name = "",
                    Sector = 15,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1142, Y = 2401 }
                };

                Locations.Add(TheGreaterFlat);
            }

            {
                HabbanyaErg = new Territory(37)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory37
                };
                Locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Name = "West",
                    Sector = 15,
                    SpiceBlowAmount = 8,
                    SpiceLocation = new Point { X = 445, Y = 2700 },
                    Center = new Point { X = 620, Y = 2726 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = HabbanyaErg,
                    Name = "East",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1065, Y = 2756 }
                });
            }

            {
                FalseWallWest = new Territory(38)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = true,
                    IsProtectedFromWorm = true,
                    Shape = ShapeManager.territory38
                };
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "North",
                    Sector = 15,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1410, Y = 2456 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "Middle",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1340, Y = 2676 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = FalseWallWest,
                    Name = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1200, Y = 3300 }
                });
            }

            {
                WindPassNorth = new Territory(39)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory39
                };
                Locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Name = "North",
                    Sector = 16,
                    SpiceBlowAmount = 6,
                    SpiceLocation = new Point { X = 1715, Y = 2500 },
                    Center = new Point { X = 1775, Y = 2470 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = WindPassNorth,
                    Name = "South",
                    Sector = 17,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1820, Y = 2610 }
                });
            }

            {
                var t = new Territory(40)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory40
                };
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "West",
                    Sector = 16,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 580, Y = 3001 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = t,
                    Name = "East",
                    Sector = 17,
                    SpiceBlowAmount = 10,
                    SpiceLocation = new Point { X = 980, Y = 3552 },
                    Center = new Point { X = 1130, Y = 3552 }
                });
            }

            {
                CielagoWest = new Territory(41)
                {
                    IsStronghold = false,
                    IsProtectedFromStorm = false,
                    IsProtectedFromWorm = false,
                    Shape = ShapeManager.territory41
                };
                Locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Name = "North",
                    Sector = 17,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1510, Y = 2951 }
                });
                Locations.Add(new Location(id++)
                {
                    Territory = CielagoWest,
                    Name = "South",
                    Sector = 00,
                    SpiceBlowAmount = 0,
                    Center = new Point { X = 1480, Y = 3406 }
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

        public List<Location> FindNeighbours(Location start, int distance, bool ignoreStorm, Faction f, int sectorInStorm, Dictionary<Location, List<Battalion>> forceLocations, List<Territory> blockedTerritories)
        {
            List<Location> neighbours = new List<Location>();
            FindNeighbours(neighbours, start, null, 0, distance, ignoreStorm, f, sectorInStorm, forceLocations, blockedTerritories);
            neighbours.Remove(start);
            return neighbours;
        }

        public List<Location> FindNeighboursForHmsMovement(Location start, int distance, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new List<Location>();
            FindNeighboursForHmsMovement(neighbours, start, null, 0, distance, ignoreStorm, sectorInStorm);
            neighbours.Remove(start);
            return neighbours;
        }

        private void FindNeighbours(List<Location> found, Location current, Location previous, int currentDistance, int maxDistance, bool ignoreStorm, Faction f, int sectorInStorm, Dictionary<Location, List<Battalion>> forceLocations, List<Territory> blockedTerritories)
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
                        if (blockedTerritories == null || !blockedTerritories.Any(t => t == neighbour.Territory))
                        {
                            if (!neighbour.IsStronghold || forceLocations == null || !forceLocations.ContainsKey(neighbour) || forceLocations[neighbour].Any(b => b.Faction == f) || forceLocations[neighbour].Count(b => b.Faction != f && b.CanOccupy) < 2)
                            {
                                int distance = (current.Territory == neighbour.Territory) ? 0 : 1;

                                if (currentDistance + distance <= maxDistance)
                                {
                                    FindNeighbours(found, neighbour, current, currentDistance + distance, maxDistance, ignoreStorm, f, sectorInStorm, forceLocations, blockedTerritories);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FindNeighboursForHmsMovement(List<Location> found, Location current, Location previous, int currentDistance, int maxDistance, bool ignoreStorm, int sectorInStorm)
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

        public List<Location> FindNeighboursWithinTerritory(Location start, bool ignoreStorm, int sectorInStorm)
        {
            List<Location> neighbours = new List<Location>();
            FindNeighboursWithinTerritory(neighbours, start, null, ignoreStorm, sectorInStorm);
            return neighbours;
        }

        private void FindNeighboursWithinTerritory(List<Location> found, Location current, Location previous, bool ignoreStorm, int sectorInStorm)
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
                    if (CorrectForPreVersion46IssueWithHMS)
                    {
                        if (id == 0)
                        {
                            return _map.HiddenMobileStronghold;
                        }
                        else
                        {
                            return _map.Locations[id - 1];
                        }
                    }
                    else
                    {
                        return _map.Locations[id];
                    }
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
                    if (CorrectForPreVersion46IssueWithHMS)
                    {
                        if (obj == _map.HiddenMobileStronghold)
                        {
                            return 0;
                        }
                        else
                        {
                            return obj.Id + 1;
                        }
                    }
                    else
                    {
                        return obj.Id;
                    }
                }
            }
        }
    }
}