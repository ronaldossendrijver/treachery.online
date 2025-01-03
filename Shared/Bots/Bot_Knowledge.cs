/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared.Model;

public partial class Player
{
    #region GeneralInformation

    protected int MaxExpectedStormMoves => Game.HasStormPrescience(this) ? Game.NextStormMoves : Param.Shipment_ExpectedStormMovesWhenUnknown;

    protected virtual bool MayFlipToAdvisors => Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors);


    protected IEnumerable<Player> Others => Game.Players.Where(p => p.Faction != Faction);

    protected IEnumerable<Player> Opponents => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally);

    protected IEnumerable<Player> MeAndMyAlly => Game.Players.Where(p => p.Faction == Faction || p.Faction == Ally);

    protected bool WinWasPredictedByMeThisTurn(Faction opponentFaction)
    {
        var ally = Game.GetPlayer(opponentFaction).Ally;
        return Faction == Faction.Blue && Game.CurrentTurn == PredictedTurn && (opponentFaction == PredictedFaction || ally == PredictedFaction);
    }

    protected virtual bool LastTurn => Game.CurrentTurn == Game.MaximumTurns;

    protected virtual bool AlmostLastTurn => Game.CurrentTurn >= Game.MaximumTurns - 1;

    protected virtual IEnumerable<Player> OpponentsToShipAndMove => Opponents.Where(p => !Game.HasActedOrPassed.Contains(p.Faction));

    protected virtual int NrOfNonWinningPlayersToShipAndMoveIncludingMe => Game.Players.Where(p => !Game.MeetsNormalVictoryCondition(p, true)).Count() - Game.HasActedOrPassed.Count;

    protected bool IAmWinning => Game.MeetsNormalVictoryCondition(this, true);

    protected bool OpponentsAreWinning => Opponents.Any(o => Game.MeetsNormalVictoryCondition(o, true));

    protected bool IsWinning(Player p)
    {
        return Game.MeetsNormalVictoryCondition(p, true);
    }

    protected bool IsWinning(Faction f)
    {
        return Game.MeetsNormalVictoryCondition(Game.GetPlayer(f), true);
    }

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

    protected List<TreacheryCard> OpponentCardsUnknownToMe(Player p)
    {
        return p.TreacheryCards.Where(c => !Game.KnownCards(this).Contains(c)).ToList();
    }

    protected bool IsKnownToOpponent(Player p, TreacheryCard card)
    {
        return Game.KnownCards(p).Contains(card);
    }

    protected IEnumerable<TreacheryCard> CardsPlayerHasOrMightHave(Player player)
    {
        var known = Game.KnownCards(this).ToList();
        var result = new List<TreacheryCard>(player.TreacheryCards.Where(c => known.Contains(c)));

        var playerHasUnknownCards = player.TreacheryCards.Any(c => !known.Contains(c));
        if (playerHasUnknownCards) result.AddRange(CardsUnknownToMe);

        return result;
    }

    protected IEnumerable<TreacheryCard> CardsPlayerHas(Player player)
    {
        var known = Game.KnownCards(this).ToList();
        return player.TreacheryCards.Where(c => known.Contains(c));
    }

    protected int CardQuality(TreacheryCard cardToRate, Player forWhom)
    {
        var cardsToTakeIntoAccount = new List<TreacheryCard>(TreacheryCards);
        if (forWhom != null && forWhom != this)
        {
            var myKnownCards = Game.KnownCards(this).ToList();
            cardsToTakeIntoAccount = forWhom.TreacheryCards.Where(c => myKnownCards.Contains(c)).ToList();
        }

        cardsToTakeIntoAccount.Remove(cardToRate);

        if (cardToRate.Type == TreacheryCardType.Useless) return 0;

        if (cardToRate.Type == TreacheryCardType.Thumper ||
            cardToRate.Type == TreacheryCardType.Harvester ||
            cardToRate.Type == TreacheryCardType.Flight ||
            cardToRate.Type == TreacheryCardType.Juice ||
            cardToRate.IsMirrorWeapon) return 1;

        if (cardToRate.Type == TreacheryCardType.ProjectileAndPoison) return 5;
        if (cardToRate.Type == TreacheryCardType.ShieldAndAntidote) return 5;
        if (cardToRate.Type == TreacheryCardType.Laser) return 5;
        if (cardToRate.Type == TreacheryCardType.Rockmelter) return 5;
        if (cardToRate.Type == TreacheryCardType.Karma && Faction == Faction.Black && !SpecialKarmaPowerUsed) return 5;

        var qualityWhenObtainingBothKinds = Faction == Faction.Green || Faction == Faction.Blue ? 5 : 4;
        if (cardToRate.IsProjectileDefense && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileDefense) && cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonDefense)) return qualityWhenObtainingBothKinds;
        if (cardToRate.IsPoisonDefense && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonDefense) && cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileDefense)) return qualityWhenObtainingBothKinds;
        if (cardToRate.IsProjectileWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileWeapon) && cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonWeapon)) return qualityWhenObtainingBothKinds;
        if (cardToRate.IsPoisonWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonWeapon) && cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileWeapon)) return qualityWhenObtainingBothKinds;

        if (Faction == Faction.Blue)
        {
            if (cardToRate.IsProjectileWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileWeapon)) return 5;
            if (cardToRate.IsPoisonWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonWeapon)) return 5;
        }

        if (cardToRate.Type == TreacheryCardType.Chemistry) return 4;
        if (cardToRate.Type == TreacheryCardType.WeirdingWay) return 4;
        if (cardToRate.Type == TreacheryCardType.ArtilleryStrike) return 4;
        if (cardToRate.Type == TreacheryCardType.PoisonTooth) return 4;

        if (cardToRate.Type == TreacheryCardType.SearchDiscarded) return 3;
        if (cardToRate.Type == TreacheryCardType.TakeDiscarded) return 3;

        if (cardToRate.IsPoisonWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonWeapon)) return 3;
        if (cardToRate.IsProjectileWeapon && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileWeapon)) return 3;
        if (cardToRate.IsPoisonDefense && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsPoisonDefense)) return 3;
        if (cardToRate.IsProjectileDefense && !cardsToTakeIntoAccount.Any(c => c != cardToRate && c.IsProjectileDefense)) return 3;

        return 2;
    }

    #endregion

    #region ResourceKnowledge

    protected virtual bool IAmDesparateForResources => ResourcesIncludingAllyContribution < 5;

    protected virtual int ResourcesIncludingAllyContribution => Resources + ResourcesFromAlly;

    protected virtual int ResourcesIncludingAllyAndRedContribution => Resources + ResourcesFromAlly + ResourcesFromRed;

    protected virtual int ResourcesFromAlly => Ally != Faction.None ? Game.GetPermittedUseOfAllyResources(Faction) : 0;

    protected virtual int ResourcesFromRed => Game.SpiceForBidsRedCanPay(Faction);

    protected virtual int AllyResources => Ally != Faction.None ? AlliedPlayer.Resources : 0;

    protected virtual int ResourcesIn(Location l)
    {
        if (Game.ResourcesOnPlanet.TryGetValue(l, out var value))
            return value;
        return 0;
    }

    protected virtual bool HasResources(Location l)
    {
        return Game.ResourcesOnPlanet.ContainsKey(l);
    }

    #endregion

    #region LocationInformation

    protected bool IsStronghold(Location l)
    {
        return
            l.IsStronghold ||
            Game.IsSpecialStronghold(l.Territory) ||
            (l is DiscoveredLocation dl && (dl.Discovery == DiscoveryToken.Cistern || dl.Discovery == DiscoveryToken.TestingStation || dl.Discovery == DiscoveryToken.ProcessingStation || (dl.Discovery == DiscoveryToken.Shrine && Has(TreacheryCardType.Clairvoyance))));
    }

    protected bool NotOccupiedByOthers(Location l)
    {
        return NotOccupiedByOthers(l.Territory);
    }

    protected bool NotOccupiedByOthers(Territory t)
    {
        return Game.NrOfOccupantsExcludingFaction(t, Faction) == 0 && AllyDoesntBlock(t);
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

        for (var i = 1; i <= MaxExpectedStormMoves; i++)
            if ((Game.SectorInStorm + i) % Map.NUMBER_OF_SECTORS == l.Sector) return true;

        return false;
    }

    protected virtual bool VacantAndSafeFromStorm(Location location)
    {
        if (Faction == Faction.Yellow && !Game.Prevented(FactionAdvantage.YellowProtectedFromStorm))
            return NotOccupied(location.Territory);
        return NotOccupied(location.Territory) && location.Sector != Game.SectorInStorm && !StormWillProbablyHit(location);
    }


    protected bool IDontHaveAdvisorsIn(Location l)
    {
        return Faction != Faction.Blue || SpecialForcesIn(l.Territory) == 0;
    }

    protected bool AllyDoesntBlock(Territory t)
    {
        return Ally is Faction.None or Faction.Pink ||
               Faction is Faction.Pink ||
               AlliedPlayer.AnyForcesIn(t) == 0 ||
               (Faction is Faction.Blue && Game.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                SpecialForcesIn(t) > 0) ||
               (Ally is Faction.Blue && Game.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                AlliedPlayer.SpecialForcesIn(t) > 0);
    }

    protected bool AllyDoesntBlock(Location l)
    {
        return AllyDoesntBlock(l.Territory);
    }

    protected virtual bool WithinRange(Location from, Location to, Battalion b)
    {
        var onlyAdvisors = b.Faction == Faction.Blue && b.AmountOfForces == 0;

        var willGetOrnithopters =
            !onlyAdvisors && !Game.Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) && (from == Game.Map.Arrakeen || from == Game.Map.Carthag) ? 3 : 0;

        var moveDistance = Math.Max(willGetOrnithopters, Game.DetermineMaximumMoveDistance(this, new[] { b }));

        //Game.ForcesOnPlanet used to be null
        var result = Game.Map.FindNeighbours(from, moveDistance, false, Faction, Game).Contains(to);

        return result;
    }

    protected virtual bool WithinDistance(Location from, Location to, int distance)
    {
        return Game.Map.FindNeighbours(from, distance, false, Faction, Game).Contains(to);
    }

    protected bool ProbablySafeFromShaiHulud(Territory t)
    {
        return Game.CurrentTurn == Game.MaximumTurns || Game.ProtectedFromMonster(this) || t != Game.LatestSpiceCardA.Location.Territory || Game.SandTroutOccured || !Game.HasResourceDeckPrescience(this) || (Game.ResourceCardDeck.Top != null && !Game.ResourceCardDeck.Top.IsShaiHulud);
    }

    #endregion

    #region PlanetaryForceInformation

    public bool HasNoFieldIn(Territory territory)
    {
        return Faction == Faction.White && SpecialForcesIn(territory) > 0;
    }

    protected virtual Player OccupyingOpponentIn(Territory t)
    {
        return Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t))
            .HighestOrDefault(p => MaxDial(p, t, this));
    }

    protected virtual IEnumerable<Player> OccupyingOpponentsIn(Territory t)
    {
        return Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t));
    }

    protected virtual IEnumerable<Player> OccupyingOpponentsIn(Location l)
    {
        return OccupyingOpponentsIn(l.Territory);
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
                //Ally has already acted => move biggest battalion
                return ForcesOnPlanet.Where(locationWithBattalion =>
                        !(locationWithBattalion.Key == Game.Map.PolarSink) &&
                        !InStorm(locationWithBattalion.Key) &&
                        !AllyDoesntBlock(locationWithBattalion.Key))
                    .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);
            //Ally has not acted yet => move smallest battalion
            return ForcesOnPlanet.Where(locationWithBattalion =>
                    !(locationWithBattalion.Key == Game.Map.PolarSink) &&
                    !InStorm(locationWithBattalion.Key) &&
                    !AllyDoesntBlock(locationWithBattalion.Key.Territory))
                .LowestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);
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
    ).HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationOnRock => ForcesOnPlanet.Where(locationWithBattalion =>
            !IsStronghold(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            Game.IsProtectedFromStorm(locationWithBattalion.Key) &&
            ResourcesIn(locationWithBattalion.Key) == 0 &&
            (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
            !IsStronghold(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            (!Game.IsProtectedFromStorm(locationWithBattalion.Key) || !Game.Map.Strongholds.Any(s => WithinRange(locationWithBattalion.Key, s, locationWithBattalion.Value))) &&
            ResourcesIn(locationWithBattalion.Key) == 0 &&
            (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice => ForcesOnPlanet.Where(locationWithBattalion =>
            !IsStronghold(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            ResourcesIn(locationWithBattalion.Key) == 0 &&
            (!Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa) &&
            VacantAndSafeNearbyStronghold(locationWithBattalion) == null &&
            BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) == null)
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            locationWithBattalion.Value.TotalAmountOfForces >= 8 &&
            VacantAndSafeNearbyStronghold(locationWithBattalion) != null)
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
            locationWithBattalion.Value.Faction == Faction.Blue &&
            locationWithBattalion.Value.AmountOfSpecialForces > 0 &&
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key) &&
            !InStorm(locationWithBattalion.Key) &&
            VacantAndSafeNearbyStronghold(locationWithBattalion) != null)
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    protected virtual KeyValuePair<Location, Battalion> BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice => ForcesOnPlanet.Where(locationWithBattalion =>
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key.Territory) &&
            !InStorm(locationWithBattalion.Key) &&
            locationWithBattalion.Value.TotalAmountOfForces >= (IAmDesparateForResources ? 5 : 7) &&
            BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) != null)
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);

    private Location BestSafeAndNearbyResources(Location location, Battalion b, bool mayFight = false)
    {
        return Game.ResourcesOnPlanet.Where(l => IsSafeAndNearby(location, l.Key, b, mayFight)).HighestOrDefault(r => r.Value).Key;
    }

    private Location BestSafeAndNearbyDiscovery(Location location, Battalion b, bool mayFight = false)
    {
        return Game.DiscoveriesOnPlanet.Keys.Where(l => IsSafeAndNearby(location, l, b, mayFight)).FirstOrDefault();
    }

    private IEnumerable<Battalion> NearbyBattalionsOutsideStrongholds(Location l)
    {
        return ForcesOnPlanet.Where(kvp => !kvp.Key.IsStronghold && WithinRange(kvp.Key, l, kvp.Value))
            .Select(kvp => kvp.Value);
    }

    protected virtual bool IsSafeAndNearby(Location source, Location destination, Battalion b, bool mayFight)
    {
        var opponent = GetOpponentThatOccupies(destination.Territory);

        return WithinRange(source, destination, b) &&
               AllyDoesntBlock(destination.Territory) &&
               ProbablySafeFromShaiHulud(destination.Territory) &&
               (opponent == null || (mayFight && GetDialNeeded(destination.Territory, opponent, false) < MaxDial(Resources, b, opponent.Faction))) &&
               !StormWillProbablyHit(destination);
    }

    #endregion

    #region DestinationsOfMovement

    protected IEnumerable<Location> ValidMovementLocations(Location from, Battalion battalion)
    {
        var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
        return PlacementEvent.ValidTargets(Game, this, from, battalion).Where(l => !forbidden.Contains(l.Territory));
    }

    protected virtual Location VacantAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
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
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            !StormWillProbablyHit(to) &&
            NotOccupiedByOthers(to)
        ).FirstOrDefault();
    }

    protected virtual Location WeakAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            AnyForcesIn(to) > 0 &&
            AllyDoesntBlock(to.Territory) &&
            !StormWillProbablyHit(to)
        ).FirstOrDefault();
    }

    protected virtual Location NearbyStrongholdOfWinningOpponent(Location from, Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            WinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
        ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }

    protected virtual Location NearbyStrongholdOfAlmostWinningOpponent(Location from, Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            AlmostWinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
        ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }

    private bool IsWinningOpponent(Player p)
    {
        return p != this && p.Faction != Ally && Game.MeetsNormalVictoryCondition(p, true);
    }

    private bool IsWinningOpponent(Faction f)
    {
        return IsWinningOpponent(Game.GetPlayer(f));
    }

    private bool IsAlmostWinningOpponent(Player p)
    {
        return p != this && p != AlliedPlayer &&
               Game.NumberOfVictoryPoints(p, true) + 1 >= Game.TresholdForWin(p) &&
               (CanShip(p) || (p.HasAlly && CanShip(p.AlliedPlayer)) || p.TechTokens.Count >= 2);
    }

    private IEnumerable<Player> WinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots)
    {
        return Game.Players.Where(p =>
            (includeBots || !p.IsBot || !p.AllyIsBot) && IsWinningOpponent(p) &&
            Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds &&
            !WinWasPredictedByMeThisTurn(p.Faction));
    }

    private IEnumerable<Player> AlmostWinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots)
    {
        return Game.Players.Where(p =>
            (includeBots || !p.IsBot || !p.AllyIsBot) && IsAlmostWinningOpponent(p) &&
            Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds &&
            !WinWasPredictedByMeThisTurn(p.Faction));
    }


    protected virtual Location WinnableNearbyStronghold(Location from, Battalion battalion)
    {
        var enemyWeakStrongholds = ValidMovementLocations(from, battalion).Where(to =>
                IsStronghold(to) &&
                OccupiedByOpponent(to) &&
                AllyDoesntBlock(to) &&
                !StormWillProbablyHit(to))
            .Select(l => new { Stronghold = l, Opponent = OccupyingOpponentIn(l.Territory) })
            .Where(s => s.Opponent != null).Select(s => new
            {
                s.Stronghold,
                Opponent = s.Opponent.Faction,
                DialNeeded = GetDialNeeded(s.Stronghold.Territory, GetOpponentThatOccupies(s.Stronghold.Territory), true)
            });

        var resourcesForBattle = Ally == Faction.Brown ? ResourcesIncludingAllyContribution : Resources;

        var winnableNearbyStronghold = enemyWeakStrongholds.Where(s =>
            WinWasPredictedByMeThisTurn(s.Opponent) ||
            DetermineDialShortageForBattle(s.DialNeeded, s.Opponent, s.Stronghold.Territory, battalion.AmountOfForces + ForcesIn(s.Stronghold), battalion.AmountOfSpecialForces + SpecialForcesIn(s.Stronghold), resourcesForBattle) <= 0
        ).OrderBy(s => s.DialNeeded).FirstOrDefault();

        if (winnableNearbyStronghold == null)
            return null;
        return winnableNearbyStronghold.Stronghold;
    }

    #endregion

    #region BattleInformation_Dial

    protected virtual bool IMustPayForForcesInBattle => Battle.MustPayForAnyForcesInBattle(Game, this);

    protected virtual float MaxDial(Player p, Territory t, Player opponent, bool ignoreSpiceDialing = false)
    {
        var countForcesForWhite = 0;
        if (p.Faction == Faction.White && p.SpecialForcesIn(t) > 0) countForcesForWhite = Faction == Faction.White || Ally == Faction.White ? Game.CurrentNoFieldValue : Game.LatestRevealedNoFieldValue == 5 ? 3 : 5;

        return MaxDial(
            ignoreSpiceDialing ? 99 : p.Resources,
            p.ForcesIn(t) + countForcesForWhite,
            p.Faction != Faction.White ? p.SpecialForcesIn(t) : 0,
            p,
            opponent != null ? opponent.Faction : Faction.Black);
    }

    protected virtual float MaxDial(int resources, Battalion battalion, Faction opponent)
    {
        return MaxDial(resources, battalion.AmountOfForces, battalion.AmountOfSpecialForces, Game.GetPlayer(battalion.Faction), opponent);
    }

    protected virtual float MaxDial(int resources, int forces, int specialForces, Player player, Faction opponentFaction)
    {
        var spice = Battle.MustPayForAnyForcesInBattle(Game, player) ? resources : 99;

        var specialForcesAtFullStrength = Math.Min(specialForces, spice);
        spice -= specialForcesAtFullStrength;
        var specialForcesAtHalfStrength = specialForces - specialForcesAtFullStrength;

        var forcesAtFullStrength = Math.Min(forces, spice);
        var forcesAtHalfStrength = forces - forcesAtFullStrength;

        var result =
            Battle.DetermineSpecialForceStrength(Game, player.Faction, opponentFaction) * (specialForcesAtFullStrength + 0.5f * specialForcesAtHalfStrength) +
            Battle.DetermineNormalForceStrength(Game, player.Faction) * (forcesAtFullStrength + 0.5f * forcesAtHalfStrength);

        return result;
    }

    protected virtual float TotalMaxDialOfOpponents(Territory t)
    {
        return Opponents.Sum(o => MaxDial(o, t, this));
    }

    protected bool IWillBeAggressorAgainst(Player opponent)
    {
        if (opponent == null) return false;

        var firstPlayerPosition = PlayerSequence.DetermineFirstPlayer(Game).Seat;

        for (var i = 0; i < Game.MaximumPlayers; i++)
        {
            var position = (firstPlayerPosition + i) % Game.MaximumPlayers;
            if (position == Seat)
                return true;
            if (position == opponent.Seat) return false;
        }

        return false;
    }

    public bool CanShip(Player p)
    {
        return Game.CurrentMainPhase < MainPhase.ShipmentAndMove || (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && !Game.HasActedOrPassed.Contains(p.Faction));
    }

    protected virtual int NrOfBattlesToFight => Battle.BattlesToBeFought(Game, this).Count();

    protected virtual float MaxReinforcedDialTo(Player player, Territory to)
    {
        if (player == null || to == null) return 0;

        if (CanShip(player))
        {
            var specialForces = 0;
            var normalForces = 0;

            var opponentResources = player.Resources + (player.Ally == Faction.None ? 0 : player.AlliedPlayer.Resources);

            var opponentMayUseWorthlessAsKarma = player.Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);
            var hasKarma = CardsPlayerHas(player).Any(c => c.Type == TreacheryCardType.Karma || (opponentMayUseWorthlessAsKarma && c.Type == TreacheryCardType.Karma));

            while (specialForces + 1 <= player.SpecialForcesInReserve && Shipment.DetermineCost(Game, player, normalForces, specialForces + 1, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) specialForces++;

            while (normalForces + 1 <= player.ForcesInReserve && Shipment.DetermineCost(Game, player, normalForces + 1, specialForces, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) normalForces++;

            return specialForces * Battle.DetermineSpecialForceStrength(Game, player.Faction, Faction) + normalForces * Battle.DetermineNormalForceStrength(Game, player.Faction);
        }

        return 0;
    }

    #endregion

    #region BattleInformation_Leaders

    protected virtual IEnumerable<IHero> SafeOrKnownTraitorLeaders
    {
        get
        {
            var ally = Ally != Faction.None ? AlliedPlayer : null;
            var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : Array.Empty<IHero>();
            var knownNonTraitors = Traitors.Union(KnownNonTraitors).Union(knownNonTraitorsByAlly);

            var myKnownTraitorsAndNonTraitors = Traitors.Union(KnownNonTraitors);
            var allyKnownTraitorsAndNonTraitors = HasAlly ? AlliedPlayer.Traitors.Union(AlliedPlayer.KnownNonTraitors) : Array.Empty<IHero>();
            var revealedOrToldTraitors = Game.Players.SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors));

            return myKnownTraitorsAndNonTraitors.Union(allyKnownTraitorsAndNonTraitors).Union(revealedOrToldTraitors);
        }
    }

    #endregion

    #region BattleInformation_WeaponsAndDefenses

    private IEnumerable<TreacheryCard> KnownOpponentWeapons(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => c.IsWeapon && Game.KnownCards(this).Contains(c));
    }

    private IEnumerable<TreacheryCard> KnownOpponentCards(Faction opponent)
    {
        return KnownOpponentCards(Game.GetPlayer(opponent));
    }

    private IEnumerable<TreacheryCard> KnownOpponentCards(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => Game.KnownCards(this).Contains(c));
    }

    private IEnumerable<TreacheryCard> KnownOpponentDefenses(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => c.IsDefense && Game.KnownCards(this).Contains(c));
    }

    protected virtual IEnumerable<TreacheryCard> Weapons(TreacheryCard usingThisDefense, IHero usingThisHero, Territory territory)
    {
        return Battle.ValidWeapons(Game, this, usingThisDefense, usingThisHero, territory);
    }

    protected virtual IEnumerable<TreacheryCard> Defenses(TreacheryCard usingThisWeapon, Territory territory)
    {
        return Battle.ValidDefenses(Game, this, usingThisWeapon, territory);
    }

    protected virtual TreacheryCard UselessAsWeapon(TreacheryCard usingThisDefense)
    {
        return Weapons(usingThisDefense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);
    }

    protected virtual TreacheryCard UselessAsDefense(TreacheryCard usingThisWeapon)
    {
        return Defenses(usingThisWeapon, null).LastOrDefault(c => c.Type == TreacheryCardType.Useless);
    }

    protected bool MayPlayNoWeapon(TreacheryCard usingThisDefense)
    {
        return Battle.ValidWeapons(Game, this, usingThisDefense, null, null, true).Contains(null);
    }

    protected bool MayPlayNoDefense(TreacheryCard usingThisWeapon)
    {
        return Battle.ValidDefenses(Game, this, usingThisWeapon, null, true).Contains(null);
    }

    private int CountDifferentWeaponTypes(IEnumerable<TreacheryCard> cards)
    {
        var result = 0;
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
        var result = 0;
        if (cards.Any(card => card.IsProjectileDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
        if (cards.Any(card => card.IsPoisonDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
        if (cards.Any(card => card.Type == TreacheryCardType.ShieldAndAntidote)) result++;
        return result;
    }

    #endregion
}