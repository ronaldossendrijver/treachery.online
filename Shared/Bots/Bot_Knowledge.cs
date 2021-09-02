/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        #region GeneralInformation

        protected int MaxExpectedStormMoves => Game.HasStormPrescience(this) ? Game.NextStormMoves : Param.Shipment_ExpectedStormMovesWhenUnknown;

        protected virtual bool MayFlipToAdvisors => Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors);


        protected IEnumerable<Player> Others => Game.Players.Where(p => p.Faction != Faction);

        protected IEnumerable<Player> Opponents => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally);

        protected bool WinWasPredictedByMeThisTurn(Faction opponentFaction)
        {
            var ally = Game.GetPlayer(opponentFaction).Ally;
            return Faction == Faction.Blue && Game.CurrentTurn == PredictedTurn && (opponentFaction == PredictedFaction || ally == PredictedFaction);
        }

        protected virtual bool LastTurn => Game.CurrentTurn == Game.MaximumNumberOfTurns;

        protected virtual bool AlmostLastTurn => Game.CurrentTurn >= Game.MaximumNumberOfTurns - 1;

        protected virtual int NrOfOpponentsToShipAndMove => Opponents.Where(p => !Game.HasActedOrPassed.Contains(p.Faction)).Count();

        protected virtual int NrOfNonWinningPlayersToShipAndMoveIncludingMe => Game.Players.Where(p => !Game.MeetsNormalVictoryCondition(p, true)).Count() - Game.HasActedOrPassed.Count;

        protected bool IAmWinning => Game.MeetsNormalVictoryCondition(this, true);

        protected bool OpponentsAreWinning => Opponents.Any(o => Game.MeetsNormalVictoryCondition(o, true));

        protected Prescience MyPrescience => Game.CurrentPrescience != null && (Game.CurrentPrescience.Initiator == Faction || Game.CurrentPrescience.Initiator == Ally) ? Game.CurrentPrescience : null;

        protected ClairVoyanceQandA MyClairVoyanceAboutEnemyDefenseInCurrentBattle =>
            Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null &&
            Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
            (Game.LatestClairvoyance.Initiator == Faction || Game.LatestClairvoyance.Initiator == Ally) &&
            (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

        protected ClairVoyanceQandA MyClairVoyanceAboutEnemyWeaponInCurrentBattle =>
            Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null &&
            Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
            (Game.LatestClairvoyance.Initiator == Faction || Game.LatestClairvoyance.Initiator == Ally) &&
            (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

        protected Voice MyVoice => Game.CurrentVoice != null && (Faction == Faction.Blue || Ally == Faction.Blue) ? Game.CurrentVoice : null;

        protected bool MayUseUselessAsKarma => Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);

        #endregion

        #region CardKnowledge

        protected List<TreacheryCard> CardsUnknownToMe => TreacheryCardManager.GetCardsInPlay(Game).Where(c => !Game.KnownCards(this).Contains(c)).ToList();

        protected List<TreacheryCard> OpponentCardsUnknownToMe(Player p) => p.TreacheryCards.Where(c => !Game.KnownCards(this).Contains(c)).ToList();

        protected bool IsKnownToOpponent(Player p, TreacheryCard card) => Game.KnownCards(p).Contains(card);

        protected IEnumerable<TreacheryCard> CardsPlayerHasOrMightHave(Player player)
        {
            var known = Game.KnownCards(this).ToList();
            var result = new List<TreacheryCard>(player.TreacheryCards.Where(c => known.Contains(c)));

            var playerHasUnknownCards = player.TreacheryCards.Any(c => !known.Contains(c));
            if (playerHasUnknownCards)
            {
                result.AddRange(CardsUnknownToMe);
            }

            return result;
        }

        protected IEnumerable<TreacheryCard> CardsPlayerHas(Player player)
        {
            var known = Game.KnownCards(this).ToList();
            return player.TreacheryCards.Where(c => known.Contains(c));
        }

        protected int CardQuality(TreacheryCard c)
        {
            if (c.Type == TreacheryCardType.Useless) return 0;

            if (c.Type == TreacheryCardType.Thumper ||
                c.Type == TreacheryCardType.Harvester ||
                c.Type == TreacheryCardType.Flight ||
                c.Type == TreacheryCardType.Juice) return 1;

            if (c.Type == TreacheryCardType.ProjectileAndPoison) return 5;
            if (c.Type == TreacheryCardType.ShieldAndAntidote) return 5;
            if (c.Type == TreacheryCardType.Laser) return 5;
            if (c.Type == TreacheryCardType.Karma && Faction == Faction.Black && !SpecialKarmaPowerUsed) return 5;

            if (c.Type == TreacheryCardType.Chemistry) return 4;
            if (c.Type == TreacheryCardType.WeirdingWay) return 4;

            if (c.IsRockmelter) return 3;
            if (c.IsMirrorWeapon) return 3;

            if (c.IsPoisonWeapon && !TreacheryCards.Any(c => c.IsPoisonWeapon)) return 3;
            if (c.IsProjectileWeapon && !TreacheryCards.Any(c => c.IsProjectileWeapon)) return 3;
            if (c.IsPoisonDefense && !TreacheryCards.Any(c => c.IsPoisonDefense)) return 3;
            if (c.IsProjectileDefense && !TreacheryCards.Any(c => c.IsProjectileDefense)) return 3;

            return 2;
        }

        #endregion

        #region ResourceKnowledge

        protected virtual bool IAmDesparateForResources => ResourcesIncludingAllyContribution < 5;

        protected virtual int ResourcesIncludingAllyContribution
        {
            get
            {
                return Resources + ResourcesFromAlly;
            }
        }

        protected virtual int ResourcesIncludingAllyAndRedContribution
        {
            get
            {
                return Resources + ResourcesFromAlly + ResourcesFromRed;
            }
        }

        protected virtual int ResourcesFromAlly
        {
            get
            {
                return Ally != Faction.None ? Game.GetPermittedUseOfAllySpice(Faction) : 0;
            }
        }

        protected virtual int ResourcesFromRed
        {
            get
            {
                return Game.SpiceForBidsRedCanPay(Faction);
            }
        }

        protected virtual int ResourcesIn(Location l)
        {
            if (Game.ResourcesOnPlanet.ContainsKey(l))
            {
                return Game.ResourcesOnPlanet[l];
            }
            else
            {
                return 0;
            }
        }

        protected virtual bool HasResources(Location l)
        {
            return Game.ResourcesOnPlanet.ContainsKey(l);
        }

        #endregion

        #region LocationInformation

        protected bool IsStronghold(Location l)
        {
            return l.IsStronghold || Game.IsSpecialStronghold(l.Territory);
        }

        protected bool NotOccupiedByOthers(Location l)
        {
            return NotOccupiedByOthers(l.Territory);
        }

        protected bool NotOccupiedByOthers(Territory t)
        {
            return Game.NrOfOccupantsExcludingPlayer(t, this) == 0 && AllyNotIn(t);
        }

        protected bool NotOccupied(Territory t)
        {
            return !Game.IsOccupied(t);
        }

        protected bool NotOccupied(Location l)
        {
            return !Game.IsOccupied(l.Territory);
        }

        protected bool Vacant(Location l)
        {
            return Vacant(l.Territory);
        }

        protected bool Vacant(Territory t)
        {
            return !Game.AnyForcesIn(t);
        }

        protected virtual bool OccupiedByOpponent(Territory t)
        {
            return Opponents.Any(o => o.Occupies(t));
        }

        protected virtual bool OccupiedByOpponent(Location l)
        {
            return OccupiedByOpponent(l.Territory);
        }

        protected virtual Player GetOpponentThatOccupies(Location l)
        {
            return GetOpponentThatOccupies(l.Territory);
        }

        protected virtual Player GetOpponentThatOccupies(Territory t)
        {
            return Opponents.FirstOrDefault(o => o.Occupies(t));
        }

        protected virtual bool StormWillProbablyHit(Location l)
        {
            if (Game.IsProtectedFromStorm(l) || LastTurn) return false;

            for (int i = 1; i <= MaxExpectedStormMoves; i++)
            {
                if ((Game.SectorInStorm + i) % Map.NUMBER_OF_SECTORS == l.Sector)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool VacantAndSafeFromStorm(Location location)
        {
            if (Faction == Faction.Yellow && !Game.Prevented(FactionAdvantage.YellowProtectedFromStorm))
            {
                return NotOccupied(location.Territory);
            }
            else
            {
                return NotOccupied(location.Territory) && location.Sector != Game.SectorInStorm && !StormWillProbablyHit(location);
            }
        }


        protected bool IDontHaveAdvisorsIn(Location l)
        {
            return Faction != Faction.Blue || SpecialForcesIn(l) == 0;
        }

        protected bool AllyNotIn(Territory t)
        {
            return Ally == Faction.None || AlliedPlayer.AnyForcesIn(t) == 0;
        }

        protected bool AllyNotIn(Location l)
        {
            return AllyNotIn(l.Territory);
        }

        protected virtual bool WithinRange(Location from, Location to, Battalion b)
        {
            bool onlyAdvisors = b.Faction == Faction.Blue && b.AmountOfForces == 0;

            int willGetOrnithopters =
                !onlyAdvisors && !Game.Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) && (from == Game.Map.Arrakeen || from == Game.Map.Carthag) ? 3 : 0;

            int moveDistance = Math.Max(willGetOrnithopters, Game.DetermineMaximumMoveDistance(this, new Battalion[] { b }));

            var result = Map.FindNeighbours(from, moveDistance, false, Faction, Game.SectorInStorm, null, Game.CurrentBlockedTerritories).Contains(to);

            return result;
        }

        protected virtual bool WithinDistance(Location from, Location to, int distance)
        {
            return Map.FindNeighbours(from, distance, false, Faction, Game.SectorInStorm, Game.ForcesOnPlanet, Game.CurrentBlockedTerritories).Contains(to);
        }

        protected bool ProbablySafeFromShaiHulud(Territory t)
        {
            return Game.CurrentTurn == Game.MaximumNumberOfTurns || Game.ProtectedFromMonster(this) || t != Game.LatestSpiceCardA.Location.Territory || Game.SandTroutOccured || !Game.HasResourceDeckPrescience(this) || !Game.ResourceCardDeck.Top.IsShaiHulud;
        }

        #endregion

        #region PlanetaryForceInformation

        protected virtual Player OccupyingOpponentIn(Territory t)
        {
            return Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t)).FirstOrDefault();
        }

        protected virtual bool InStorm(Location l)
        {
            return l.Sector == Game.SectorInStorm;
        }

        protected virtual KeyValuePair<Location, Battalion> BattalionThatShouldBeMovedDueToAllyPresence
        {
            get
            {
                if (Ally == Faction.None) return default;

                if (Game.HasActedOrPassed.Contains(Ally))
                {
                    //Ally has already acted => move biggest battalion
                    return ForcesOnPlanet.Where(locationWithBattalion =>
                    !InStorm(locationWithBattalion.Key) &&
                    !AllyNotIn(locationWithBattalion.Key))
                    .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();
                }
                else
                {
                    //Ally has not acted yet => move smallest battalion
                    return ForcesOnPlanet.Where(locationWithBattalion =>
                    !InStorm(locationWithBattalion.Key) &&
                    !AllyNotIn(locationWithBattalion.Key.Territory))
                    .OrderBy(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();
                }
            }
        }

        protected bool MayFleeOutOf(Location l)
        {
            return !IsStronghold(l) || !(IAmWinning || OpponentsAreWinning);
        }

        protected virtual KeyValuePair<Location, Battalion> BiggestBattalionThreatenedByStormWithoutSpice => ForcesOnPlanet.Where(locationWithBattalion =>
                StormWillProbablyHit(locationWithBattalion.Key) &&
                !InStorm(locationWithBattalion.Key) &&
                MayFleeOutOf(locationWithBattalion.Key) &&
                !HasResources(locationWithBattalion.Key)
                ).OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationOnRock => ForcesOnPlanet.Where(locationWithBattalion =>
                !IsStronghold(locationWithBattalion.Key) &&
                locationWithBattalion.Key.Sector != Game.SectorInStorm &&
                Game.IsProtectedFromStorm(locationWithBattalion.Key) &&
                ResourcesIn(locationWithBattalion.Key) == 0 &&
                (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
                !IsStronghold(locationWithBattalion.Key) &&
                locationWithBattalion.Key.Sector != Game.SectorInStorm &&
                (!Game.IsProtectedFromStorm(locationWithBattalion.Key) || !Game.Map.Strongholds.Any(s => WithinRange(locationWithBattalion.Key, s, locationWithBattalion.Value))) &&
                ResourcesIn(locationWithBattalion.Key) == 0 &&
                (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice => ForcesOnPlanet.Where(locationWithBattalion =>
                !IsStronghold(locationWithBattalion.Key) &&
                locationWithBattalion.Key.Sector != Game.SectorInStorm &&
                ResourcesIn(locationWithBattalion.Key) == 0 &&
                (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa) &&
                VacantAndSafeNearbyStronghold(locationWithBattalion) == null &&
                BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) == null)
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
                IsStronghold(locationWithBattalion.Key) &&
                NotOccupiedByOthers(locationWithBattalion.Key) &&
                locationWithBattalion.Key.Sector != Game.SectorInStorm &&
                locationWithBattalion.Value.TotalAmountOfForces >= 8 &&
                VacantAndSafeNearbyStronghold(locationWithBattalion) != null)
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
                locationWithBattalion.Value.Faction == Faction.Blue &&
                locationWithBattalion.Value.AmountOfSpecialForces > 0 &&
                IsStronghold(locationWithBattalion.Key) &&
                NotOccupiedByOthers(locationWithBattalion.Key) &&
                !InStorm(locationWithBattalion.Key) &&
                VacantAndSafeNearbyStronghold(locationWithBattalion) != null)
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        protected virtual KeyValuePair<Location, Battalion> BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice => ForcesOnPlanet.Where(locationWithBattalion =>
                IsStronghold(locationWithBattalion.Key) &&
                NotOccupiedByOthers(locationWithBattalion.Key.Territory) &&
                !InStorm(locationWithBattalion.Key) &&
                locationWithBattalion.Value.TotalAmountOfForces >= (IAmDesparateForResources ? 5 : 7) &&
                BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) != null)
                .OrderByDescending(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces).FirstOrDefault();

        #endregion

        #region DestinationsOfMovement

        protected virtual Location VacantAndSafeNearbyStronghold(Location from, Battalion battalion)
        {
            return PlacementEvent.ValidTargets(Game, this, from, battalion).Where(to =>
                IsStronghold(to) &&
                !StormWillProbablyHit(to) &&
                Vacant(to)
                ).FirstOrDefault();
        }

        protected virtual Location VacantAndSafeNearbyStronghold(KeyValuePair<Location, Battalion> battalionAtLocation)
        {
            return VacantAndSafeNearbyStronghold(battalionAtLocation.Key, battalionAtLocation.Value);
        }

        protected virtual Location UnthreatenedAndSafeNearbyStronghold(Location from, Battalion battalion)
        {
            return PlacementEvent.ValidTargets(Game, this, from, battalion).Where(to =>
                IsStronghold(to) &&
                !StormWillProbablyHit(to) &&
                NotOccupiedByOthers(to)
                ).FirstOrDefault();
        }

        protected virtual Location WeakAndSafeNearbyStronghold(Location from, Battalion battalion)
        {
            return PlacementEvent.ValidTargets(Game, this, from, battalion).Where(to =>
                IsStronghold(to) &&
                AnyForcesIn(to) > 0 &&
                AllyNotIn(to.Territory) &&
                !StormWillProbablyHit(to)
                ).FirstOrDefault();
        }

        protected virtual Location WinnableNearbyStronghold(Location from, Battalion battalion)
        {
            var enemyWeakStrongholds = PlacementEvent.ValidTargets(Game, this, from, battalion).Where(to =>
                IsStronghold(to) &&
                OccupiedByOpponent(to) &&
                AllyNotIn(to) &&
                !StormWillProbablyHit(to))
                .Select(l => new { Stronghold = l, Opponent = OccupyingOpponentIn(l.Territory) })
                .Where(s => s.Opponent != null).Select(s => new
                {
                    s.Stronghold,
                    Opponent = s.Opponent.Faction,
                    DialNeeded = GetDialNeeded(s.Stronghold.Territory, GetOpponentThatOccupies(s.Stronghold.Territory), true)
                });

            return enemyWeakStrongholds.Where(s =>
                WinWasPredictedByMeThisTurn(s.Opponent) ||
                DetermineRemainingDialInBattle(s.DialNeeded, s.Opponent, battalion.AmountOfForces + ForcesIn(s.Stronghold), battalion.AmountOfSpecialForces + SpecialForcesIn(s.Stronghold), Resources) <= 0
                ).OrderBy(s => s.DialNeeded).Select(s => s.Stronghold).FirstOrDefault();
        }

        #endregion

        #region BattleInformation_Dial

        protected virtual bool IMustPayForForcesInBattle => Battle.MustPayForForcesInBattle(Game, this);

        protected virtual float MaxDial(Player p, Territory t, Player opponent)
        {
            int countForcesForWhite = 0;
            if (p.Faction == Faction.White && p.SpecialForcesIn(t) > 0)
            {
                countForcesForWhite = Game.LatestRevealedNoFieldValue == 5 ? 3 : 5;
            }

            return MaxDial(p.Resources, p.ForcesIn(t) + countForcesForWhite, p.Faction != Faction.White ? p.SpecialForcesIn(t) : 0, p.Faction, opponent.Faction);
        }

        protected virtual float MaxDial(int resources, Battalion battalion, Faction opponent)
        {
            return MaxDial(resources, battalion.AmountOfForces, battalion.AmountOfSpecialForces, battalion.Faction, opponent);
        }

        protected virtual float MaxDial(int resources, int forces, int specialForces, Faction playerFaction, Faction opponentFaction)
        {
            int spice = Battle.MustPayForForcesInBattle(Game, this) ? resources : 99;

            int specialForcesAtFullStrength = Math.Min(specialForces, spice);
            spice -= specialForcesAtFullStrength;
            int specialForcesAtHalfStrength = specialForces - specialForcesAtFullStrength;

            int forcesAtFullStrength = Math.Min(forces, spice);
            int forcesAtHalfStrength = forces - forcesAtFullStrength;

            var result =
                Battle.DetermineSpecialForceStrength(Game, playerFaction, opponentFaction) * (specialForcesAtFullStrength + 0.5f * specialForcesAtHalfStrength) +
                Battle.DetermineNormalForceStrength(playerFaction) * (forcesAtFullStrength + 0.5f * forcesAtHalfStrength);

            return result;
        }

        protected virtual float TotalMaxDialOfOpponents(Territory t)
        {
            return Opponents.Sum(o => MaxDial(o, t, this));
        }

        protected bool IWillBeAggressorAgainst(Player opponent)
        {
            for (int i = 0; i < Game.MaximumNumberOfPlayers; i++)
            {
                var position = (Game.FirstPlayerPosition + i) % Game.MaximumNumberOfPlayers;
                if (position == PositionAtTable)
                {
                    return true;
                }
                else if (position == opponent.PositionAtTable)
                {
                    return false;
                }
            }

            return false;
        }

        public bool CanShip(Player p)
        {
            return Game.CurrentMainPhase < MainPhase.ShipmentAndMove || Game.CurrentMainPhase == MainPhase.ShipmentAndMove && !Game.HasActedOrPassed.Contains(p.Faction);
        }

        protected virtual int NrOfBattlesToFight => Battle.BattlesToBeFought(Game, this).Count();

        protected virtual float MaxReinforcedDialTo(Player player, Territory to)
        {
            if (CanShip(player))
            {
                int specialForces = 0;
                int normalForces = 0;

                int opponentResources = player.Resources + (player.Ally == Faction.None ? 0 : player.AlliedPlayer.Resources);

                bool opponentMayUseWorthlessAsKarma = player.Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);
                bool hasKarma = CardsPlayerHas(player).Any(c => c.Type == TreacheryCardType.Karma || (opponentMayUseWorthlessAsKarma && c.Type == TreacheryCardType.Karma));

                while (specialForces + 1 <= player.SpecialForcesInReserve && Shipment.DetermineCost(Game, player, normalForces + specialForces + 1, to.MiddleLocation, hasKarma, false, false) <= opponentResources)
                {
                    specialForces++;
                }

                while (normalForces + 1 <= player.ForcesInReserve && Shipment.DetermineCost(Game, player, normalForces + 1 + specialForces, to.MiddleLocation, hasKarma, false, false) <= opponentResources)
                {
                    normalForces++;
                }

                return specialForces * Battle.DetermineSpecialForceStrength(Game, player.Faction, Faction) + normalForces * Battle.DetermineNormalForceStrength(player.Faction);
            }

            return 0;
        }

        #endregion

        #region BattleInformation_Leaders

        protected virtual IHero LowestAvailableNonMercenaryLeader => Battle.ValidBattleHeroes(Game, this).OrderBy(l => l.Value).FirstOrDefault(l => l is Leader);

        protected virtual IEnumerable<IHero> SafeLeaders
        {
            get
            {
                var ally = Ally != Faction.None ? AlliedPlayer : null;
                var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : Array.Empty<IHero>();
                var knownNonTraitors = Traitors.Union(KnownNonTraitors).Union(knownNonTraitorsByAlly);
                return Battle.ValidBattleHeroes(Game, this).Where(l => knownNonTraitors.Contains(l));
            }
        }

        #endregion

        #region BattleInformation_WeaponsAndDefenses

        private IEnumerable<TreacheryCard> KnownOpponentWeapons(Player opponent)
        {
            return opponent.TreacheryCards.Where(c => c.IsWeapon && Game.KnownCards(this).Contains(c));
        }

        private IEnumerable<TreacheryCard> KnownOpponentDefenses(Player opponent)
        {
            return opponent.TreacheryCards.Where(c => c.IsDefense && Game.KnownCards(this).Contains(c));
        }

        protected virtual IEnumerable<TreacheryCard> Weapons(TreacheryCard usingThisDefense, IHero usingThisHero) => Battle.ValidWeapons(Game, this, usingThisDefense, usingThisHero);

        protected virtual IEnumerable<TreacheryCard> Defenses(TreacheryCard usingThisWeapon) => Battle.ValidDefenses(Game, this, usingThisWeapon);

        protected virtual TreacheryCard UselessAsWeapon(TreacheryCard usingThisDefense) => Weapons(usingThisDefense, null).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);

        protected virtual TreacheryCard UselessAsDefense(TreacheryCard usingThisWeapon) => Defenses(usingThisWeapon).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);

        protected bool MayPlayNoWeapon(TreacheryCard usingThisDefense) => Battle.ValidWeapons(Game, this, usingThisDefense, null, true).Contains(null);

        protected bool MayPlayNoDefense(TreacheryCard usingThisWeapon) => Battle.ValidDefenses(Game, this, usingThisWeapon, true).Contains(null);

        private int CountDifferentWeaponTypes(IEnumerable<TreacheryCard> cards)
        {
            int result = 0;
            if (cards.Any(card => card.IsProjectileWeapon && card.Type != TreacheryCardType.ProjectileAndPoison)) result++;
            if (cards.Any(card => card.IsPoisonWeapon && card.Type != TreacheryCardType.ProjectileAndPoison)) result++;
            if (cards.Any(card => card.Type == TreacheryCardType.ProjectileAndPoison)) result++;
            if (cards.Any(card => card.IsLaser)) result++;
            if (cards.Any(card => card.IsArtillery)) result++;
            if (cards.Any(card => card.IsPoisonTooth)) result++;
            return result;
        }

        private int CountDifferentDefenseTypes(IEnumerable<TreacheryCard> cards)
        {
            int result = 0;
            if (cards.Any(card => card.IsProjectileDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
            if (cards.Any(card => card.IsPoisonDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
            if (cards.Any(card => card.Type == TreacheryCardType.ShieldAndAntidote)) result++;
            return result;
        }

        #endregion
    }
}
