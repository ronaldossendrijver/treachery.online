/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player : ICloneable
    {
        public string Name { get; set; }

        private Faction _faction = Faction.None;
        public Faction Faction
        {
            get
            {
                return _faction;
            }
            set
            {
                _faction = value;
                Param = BotParameters.GetDefaultParameters(value);
            }
        }

        public int PositionAtTable { get; set; } = -1;

        public int Resources { get; set; } = 0;

        public int Bribes { get; set; } = 0;

        public int ResourcesAfterBidding { get; set; } = 0;

        public int BankedResources { get; set; } = 0;

        public IList<TreacheryCard> TreacheryCards { get; set; } = new List<TreacheryCard>();

        public IList<TreacheryCard> KnownCards { get; set; } = new List<TreacheryCard>();

        public IList<IHero> Traitors { get; set; } = new List<IHero>();

        public IList<IHero> RevealedTraitors { get; set; } = new List<IHero>();

        public IList<IHero> ToldTraitors { get; set; } = new List<IHero>();

        public IList<IHero> ToldNonTraitors { get; set; } = new List<IHero>();

        public IList<IHero> KnownNonTraitors { get; set; } = new List<IHero>();

        public IList<IHero> DiscardedTraitors { get; set; } = new List<IHero>();

        public IList<IHero> FaceDancers { get; set; } = new List<IHero>();

        public IList<IHero> RevealedDancers { get; set; } = new List<IHero>();

        public IList<IHero> ToldFacedancers { get; set; } = new List<IHero>();

        public IList<IHero> ToldNonFacedancers { get; set; } = new List<IHero>();


        public IList<Leader> Leaders { get; set; } = new List<Leader>();

        public int ForcesInReserve { get; set; } = 0;

        public int SpecialForcesInReserve { get; set; } = 0;

        public IDictionary<Location, Battalion> ForcesOnPlanet { get; set; } = new Dictionary<Location, Battalion>();

        public Faction PredictedFaction { get; set; } = 0;

        public int PredictedTurn { get; set; } = 0;

        public int ForcesKilled { get; set; } = 0;

        public int SpecialForcesKilled { get; set; } = 0;

        public int TotalForcesKilledInBattle { get; set; } = 0;

        public Faction Ally { get; set; }

        public bool SpecialKarmaPowerUsed { get; set; }

        public IList<TechToken> TechTokens { get; private set; } = new List<TechToken>();

        public bool NoFieldIsActive => Faction == Faction.White && ForcesOnPlanet.Any(locationWithForces => locationWithForces.Value.AmountOfSpecialForces > 0);

        public Leader MostRecentlyRevivedLeader { get; set; }

        public IList<LeaderSkill> SkillsToChooseFrom { get; set; } = new List<LeaderSkill>();

        public IList<Homeworld> Homeworlds { get; set; } = new List<Homeworld>();

        protected Game Game { get; set; }

        public Player(Game game, string name)
        {
            Game = game;
            Name = name;
        }

        public Player(Game game, string name, Faction faction, bool isBot = false)
        {
            Game = game;
            Name = name;
            Faction = faction;
            IsBot = isBot;
        }

        public Player AlliedPlayer => Game.GetPlayer(Ally);

        public bool HasAlly => Ally != Faction.None;

        public bool Has(TreacheryCardType cardtype) => TreacheryCards.Any(c => c.Type == cardtype);

        public bool Is(Faction f) => Faction == f;

        private Battalion GetAndCreateIfNeeded(Location location)
        {
            if (!ForcesOnPlanet.ContainsKey(location))
            {
                ForcesOnPlanet[location] = new Battalion() { Faction = Faction, AmountOfForces = 0, AmountOfSpecialForces = 0 };
            }

            return ForcesOnPlanet[location];
        }

        public void ChangeForces(Location location, int nrOfForces)
        {
            var result = GetAndCreateIfNeeded(location);
            result.ChangeForces(nrOfForces);
            if (result.TotalAmountOfForces == 0) ForcesOnPlanet.Remove(location);
        }

        public void ChangeSpecialForces(Location location, int nrOfForces)
        {
            var result = GetAndCreateIfNeeded(location);
            result.ChangeSpecialForces(nrOfForces);
            if (result.TotalAmountOfForces == 0) ForcesOnPlanet.Remove(location);
        }

        public int ForcesIn(Location location)
        {
            if (ForcesOnPlanet.ContainsKey(location))
            {
                return ForcesOnPlanet[location].AmountOfForces;
            }
            else
            {
                return 0;
            }
        }

        public int SpecialForcesIn(Location location)
        {
            if (ForcesOnPlanet.ContainsKey(location))
            {
                return ForcesOnPlanet[location].AmountOfSpecialForces;
            }
            else
            {
                return 0;
            }
        }

        public int AnyForcesIn(Location location)
        {
            if (ForcesOnPlanet.ContainsKey(location))
            {
                return ForcesOnPlanet[location].TotalAmountOfForces;
            }
            else
            {
                return 0;
            }
        }

        public int ForcesIn(Territory t)
        {
            return t.Locations.Sum(l => ForcesIn(l));
        }

        public int SpecialForcesIn(Territory t)
        {
            return t.Locations.Sum(l => SpecialForcesIn(l));
        }

        public int AnyForcesIn(Territory t)
        {
            return t.Locations.Sum(l => AnyForcesIn(l));
        }

        public void ForcesToReserves(Location location)
        {
            var battaltion = ForcesOnPlanet[location];

            ForcesInReserve += battaltion.AmountOfForces;

            if (Faction == Faction.Blue)
            {
                ForcesInReserve += battaltion.AmountOfSpecialForces;
            }
            else if (Faction != Faction.White)
            {
                SpecialForcesInReserve += battaltion.AmountOfSpecialForces;
            }

            ForcesOnPlanet.Remove(location);
        }

        public void ForcesToReserves(Location location, int amount)
        {
            ForcesInReserve += amount;
            ChangeForces(location, -amount);
        }

        public void SpecialForcesToReserves(Location location, int amount)
        {
            if (Faction == Faction.Blue)
            {
                ForcesInReserve += amount;
            }
            else if (Faction != Faction.White)
            {
                SpecialForcesInReserve += amount;
            }

            ChangeSpecialForces(location, -amount);
        }

        public void ForcesToReserves(Territory t, int amount, bool special)
        {
            int toRemoveInTotal = amount;
            foreach (var l in Game.Map.Locations.Where(l => l.Territory == t).OrderBy(l => l.SpiceBlowAmount))
            {
                int forcesIn = special ? SpecialForcesIn(l) : ForcesIn(l);
                if (forcesIn > 0)
                {
                    int toRemoveInThisLocation = Math.Min(forcesIn, toRemoveInTotal);

                    if (special && Faction != Faction.Blue)
                    {
                        SpecialForcesInReserve += toRemoveInTotal;
                    }
                    else if (!special || Faction != Faction.White)
                    {
                        ForcesInReserve += toRemoveInTotal;
                    }

                    if (special)
                    {
                        ChangeSpecialForces(l, -toRemoveInTotal);
                    }
                    else
                    {
                        ChangeForces(l, -toRemoveInTotal);
                    }

                    toRemoveInTotal -= toRemoveInThisLocation;
                }

                if (toRemoveInTotal == 0) break;
            }

        }

        public void ForcesToReserves(Territory t)
        {
            foreach (var l in t.Locations.Where(l => AnyForcesIn(l) > 0))
            {
                ForcesToReserves(l);
            }
        }

        public int KillAllForces(Location location, bool inBattle)
        {
            if (ForcesOnPlanet.ContainsKey(location))
            {
                var battallion = ForcesOnPlanet[location];

                int killCount = battallion.AmountOfForces;
                ForcesKilled += killCount;

                int specialKillCount = battallion.AmountOfSpecialForces;
                if (Faction == Faction.Blue)
                {
                    ForcesKilled += specialKillCount;
                }
                else
                {
                    SpecialForcesKilled += specialKillCount;
                }

                ForcesOnPlanet.Remove(location);

                if (inBattle)
                {
                    TotalForcesKilledInBattle += killCount + specialKillCount;
                }

                return killCount + specialKillCount;
            }

            return 0;
        }

        public void KillAllForces(Territory t, bool inBattle)
        {
            foreach (var l in t.Locations.Where(l => AnyForcesIn(l) > 0))
            {
                KillAllForces(l, inBattle);
            }
        }

        public int KillForces(Location location, int amountOfForces, int amountOfSpecialForces, bool inBattle)
        {
            var battallion = ForcesOnPlanet[location];

            battallion.ChangeForces(-amountOfForces);
            battallion.ChangeSpecialForces(-amountOfSpecialForces);

            if (battallion.TotalAmountOfForces == 0) ForcesOnPlanet.Remove(location);

            ForcesKilled += amountOfForces;

            if (Faction == Faction.Blue)
            {
                ForcesKilled += amountOfSpecialForces;
            }
            else
            {
                SpecialForcesKilled += amountOfSpecialForces;
            }

            if (inBattle)
            {
                TotalForcesKilledInBattle += amountOfForces + amountOfSpecialForces;
            }

            return amountOfForces + amountOfSpecialForces;
        }

        public void KillForces(Territory t, int amount, bool special, bool inBattle)
        {
            int toKill = amount;
            foreach (var l in Game.Map.Locations.Where(l => l.Territory == t).OrderBy(l => l.SpiceBlowAmount))
            {
                int forcesIn = special ? SpecialForcesIn(l) : ForcesIn(l);
                if (forcesIn > 0)
                {
                    int toBeKilled = Math.Min(forcesIn, toKill);
                    if (special)
                    {
                        KillForces(l, 0, toBeKilled, inBattle);
                    }
                    else
                    {
                        KillForces(l, toBeKilled, 0, inBattle);
                    }
                    toKill -= toBeKilled;
                }

                if (toKill == 0) break;
            }
        }

        public void ReviveForces(int amount)
        {
            ForcesKilled -= amount;
            ForcesInReserve += amount;
        }

        public void ReviveSpecialForces(int amount)
        {
            SpecialForcesKilled -= amount;
            SpecialForcesInReserve += amount;
        }

        public void ShipForces(Location l, int amount)
        {
            ForcesInReserve -= amount;
            ChangeForces(l, amount);
        }

        public void ShipSpecialForces(Location l, int amount)
        {
            if (Faction != Faction.White)
            {
                SpecialForcesInReserve -= amount;
            }

            ChangeSpecialForces(l, amount);
        }

        public void ShipAdvisors(Location l, int amount)
        {
            ForcesInReserve -= amount;
            ChangeSpecialForces(l, amount);
        }

        public void MoveForces(Location from, Location to, int amount)
        {
            ChangeForces(from, -amount);
            ChangeForces(to, amount);
        }

        public void MoveSpecialForces(Location from, Location to, int amount)
        {
            ChangeSpecialForces(from, -amount);
            ChangeSpecialForces(to, amount);
        }

        public void FlipForces(Location l, bool asAdvisors)
        {
            if (Faction == Faction.Blue)
            {
                if (asAdvisors)
                {
                    int numberOfForces = ForcesIn(l);
                    ChangeForces(l, -numberOfForces);
                    ChangeSpecialForces(l, numberOfForces);
                }
                else
                {
                    int numberOfSpecialForces = SpecialForcesIn(l);
                    ChangeForces(l, numberOfSpecialForces);
                    ChangeSpecialForces(l, -numberOfSpecialForces);
                }
            }
        }

        public void FlipForces(Territory t, bool asAdvisors)
        {
            foreach (var l in t.Locations)
            {
                FlipForces(l, asAdvisors);
            }
        }

        public int OccupyingForces(Location l)
        {
            return ForcesIn(l) + (Faction == Faction.Blue ? 0 : SpecialForcesIn(l));
        }

        public bool Occupies(Location l)
        {
            return OccupyingForces(l) > 0;
        }

        public bool Occupies(Territory t)
        {
            return t.Locations.Any(l => Occupies(l));
        }

        public IEnumerable<Location> OccupiedLocations
        {
            get
            {
                return Game.Map.Locations.Where(l => Occupies(l));
            }
        }

        public IEnumerable<Territory> OccupiedTerritories
        {
            get
            {
                return Game.Map.Territories.Where(t => Occupies(t));
            }
        }

        public bool Controls(Game g, Location l, bool contestedStongholdsCountAsOccupied)
        {
            if (contestedStongholdsCountAsOccupied)
            {
                return Occupies(l);
            }
            else
            {
                return Occupies(l) && g.NrOfOccupantsExcludingPlayer(l, this) == 0;
            }
        }

        public bool Controls(Game g, Territory t, bool contestedStongholdsCountAsOccupied)
        {
            if (contestedStongholdsCountAsOccupied)
            {
                return Occupies(t);
            }
            else
            {
                return Occupies(t) && g.NrOfOccupantsExcludingPlayer(t, this) == 0;
            }
        }

        public IEnumerable<Territory> TerritoriesWithForces => Game.Map.Territories.Where(t => AnyForcesIn(t) > 0);

        public IEnumerable<Location> LocationsWithAnyForces => ForcesOnPlanet.Keys;

        public IEnumerable<Location> LocationsWithAnyForcesInTerritory(Territory t)
        {
            if (t == null)
            {
                return new Location[] { };
            }
            else
            {
                return t.Locations.Where(l => AnyForcesIn(l) > 0);
            }
        }

        public int MaximumNumberOfCards
        {
            get
            {
                return Faction switch
                {
                    Faction.Black => 8,
                    Faction.Brown => 5,
                    _ => 4
                };
            }
        }

        public bool HasRoomForCards => TreacheryCards.Count < MaximumNumberOfCards;

        public int NumberOfTraitors => Faction == Faction.Black ? 4 : 1;

        public int NumberOfFacedancers => Faction == Faction.Purple ? 3 : 0;

        public FactionForce Force
        {
            get
            {
                return Faction switch
                {
                    Faction.Green => FactionForce.Green,
                    Faction.Black => FactionForce.Black,
                    Faction.Yellow => FactionForce.Yellow,
                    Faction.Red => FactionForce.Red,
                    Faction.Orange => FactionForce.Orange,
                    Faction.Blue => FactionForce.Blue,
                    Faction.Grey => FactionForce.Grey,
                    Faction.Purple => FactionForce.Purple,
                    Faction.Brown => FactionForce.Brown,
                    Faction.White => FactionForce.White,
                    Faction.Pink => FactionForce.Pink,
                    Faction.Cyan => FactionForce.Cyan,
                    _ => FactionForce.None,
                };
            }
        }

        public FactionSpecialForce SpecialForce
        {
            get
            {
                return Faction switch
                {
                    Faction.Red => FactionSpecialForce.Red,
                    Faction.Yellow => FactionSpecialForce.Yellow,
                    Faction.Blue => FactionSpecialForce.Blue,
                    Faction.Grey => FactionSpecialForce.Grey,
                    Faction.White => FactionSpecialForce.White,
                    _ => FactionSpecialForce.None,
                };
            }
        }
                
        public void AssignLeaders(Game g)
        {
            Leaders = Faction switch
            {
                Faction.Brown => LeaderManager.GetLeaders(Faction.Brown).Where(l => g.Applicable(Rule.BrownAuditor) || l.HeroType != HeroType.Auditor).ToList(),
                Faction.Pink => LeaderManager.GetLeaders(Faction.Pink).Where(l => l.HeroType != HeroType.Vidal).ToList(),
                _ => LeaderManager.GetLeaders(Faction).ToList(),
            };
        }

        public TreacheryCard Card(TreacheryCardType type) => TreacheryCards.FirstOrDefault(c => c.Type == type);

        public bool HasUnrevealedFaceDancers => UnrevealedFaceDancers.Any();

        public IEnumerable<IHero> UnrevealedFaceDancers => FaceDancers.Where(f => !RevealedDancers.Contains(f));

        public bool MessiahAvailable => Game.Applicable(Rule.GreenMessiah) && Is(Faction.Green) && TotalForcesKilledInBattle >= 7 && Game.IsAlive(LeaderManager.Messiah);

        public bool HasSpecialForces
        {
            get
            {
                return (
                    Game.Applicable(Rule.YellowSpecialForces) && Is(Faction.Yellow) ||
                    Game.Applicable(Rule.RedSpecialForces) && Is(Faction.Red)) ||
                    Is(Faction.Grey);
            }
        }

        public bool HasKarma => Karma.ValidKarmaCards(Game, this).Any();

        public void AddHomeworld(World world, bool isHomeOfNormalForces, bool isHomeOfSpecialForces, int threshold, int locationId)
        {
            Homeworlds.Add(new Homeworld(world, Faction, isHomeOfNormalForces, isHomeOfSpecialForces, threshold, locationId));
        }

        public object Clone()
        {
            var result = (Player)MemberwiseClone();

            result.TreacheryCards = new List<TreacheryCard>(TreacheryCards);
            result.Traitors = new List<IHero>(Traitors);
            result.FaceDancers = new List<IHero>(FaceDancers);
            result.RevealedDancers = new List<IHero>(RevealedDancers);
            result.Leaders = new List<Leader>(Leaders);
            result.ForcesOnPlanet = Utilities.CloneDictionary(ForcesOnPlanet);
            result.TechTokens = new List<TechToken>(TechTokens);

            return result;
        }

    }
}