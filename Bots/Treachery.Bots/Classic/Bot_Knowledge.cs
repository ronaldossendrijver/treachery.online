/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

using Shared.Model;

public partial class ClassicBot
{
    #region GeneralInformation

    private int MaxExpectedStormMoves => Game.HasStormPrescience(Player) ? Game.NextStormMoves : Param.Shipment_ExpectedStormMovesWhenUnknown;

    protected virtual bool MayFlipToAdvisors => Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors);

    private Dictionary<Location, Battalion> ForcesOnPlanet => Player.ForcesOnPlanet;

    private List<Player> Others => Game.Players.Where(p => p.Faction != Faction).ToList();

    private List<Player> Opponents => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally).ToList();

    private List<Player> MeAndMyAlly => Game.Players.Where(p => p.Faction == Faction || p.Faction == Ally).ToList();

    private bool WinWasPredictedByMeThisTurn(Faction opponentFaction)
    {
        var ally = Game.GetPlayer(opponentFaction).Ally;
        return Faction == Faction.Blue && Game.CurrentTurn == Player.PredictedTurn 
                                       && (opponentFaction == Player.PredictedFaction || ally == Player.PredictedFaction);
    }

    protected virtual bool LastTurn => Game.CurrentTurn == Game.MaximumTurns;

    protected virtual bool AlmostLastTurn => Game.CurrentTurn >= Game.MaximumTurns - 1;

    protected virtual List<Player> OpponentsToShipAndMove => Opponents.Where(p => !Game.HasActedOrPassed.Contains(p.Faction)).ToList();

    protected virtual int NrOfNonWinningPlayersToShipAndMoveIncludingMe => Game.Players.Count(p => !Game.MeetsNormalVictoryCondition(p, true)) - Game.HasActedOrPassed.Count;

    private bool IAmWinning => Game.MeetsNormalVictoryCondition(Player, true);

    private bool OpponentsAreWinning => Opponents.Any(o => Game.MeetsNormalVictoryCondition(o, true));

    protected bool IsWinning(Player p)
    {
        return Game.MeetsNormalVictoryCondition(p, true);
    }

    private bool IsWinning(Faction f)
    {
        return Game.MeetsNormalVictoryCondition(Game.GetPlayer(f), true);
    }

    private Prescience? MyPrescience => Game.CurrentPrescience != null && (Game.CurrentPrescience.Initiator == Faction || Game.CurrentPrescience.Initiator == Ally) ? Game.CurrentPrescience : null;

    private ClairVoyanceQandA? MyClairvoyanceAboutEnemyDefenseInCurrentBattle =>
        Game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } &&
        Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyance.Initiator == Faction || Game.LatestClairvoyance.Initiator == Ally) &&
        (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    private ClairVoyanceQandA? MyClairvoyanceAboutEnemyWeaponInCurrentBattle =>
        Game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } &&
        Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyance.Initiator == Faction || Game.LatestClairvoyance.Initiator == Ally) &&
        (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    private Voice? MyVoice => Game.CurrentVoice != null && (Faction == Faction.Blue || Ally == Faction.Blue) ? Game.CurrentVoice : null;

    private bool MayUseUselessAsKarma => Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);

    #endregion

    #region CardKnowledge

    private List<TreacheryCard> CardsUnknownToMe => TreacheryCardManager.GetCardsInPlay(Game).Where(c => !Game.KnownCards(Player).Contains(c)).ToList();

    private List<TreacheryCard> OpponentCardsUnknownToMe(Player p)
    {
        return p.TreacheryCards.Where(c => !Game.KnownCards(Player).Contains(c)).ToList();
    }

    private bool IsKnownToOpponent(Player p, TreacheryCard card)
    {
        return Game.KnownCards(p).Contains(card);
    }

    private List<TreacheryCard> CardsPlayerHasOrMightHave(Player p)
    {
        var known = Game.KnownCards(Player).ToList();
        var result = new List<TreacheryCard>(p.TreacheryCards.Where(c => known.Contains(c)));

        var playerHasUnknownCards = p.TreacheryCards.Any(c => !known.Contains(c));
        if (playerHasUnknownCards) result.AddRange(CardsUnknownToMe);

        return result;
    }

    private List<TreacheryCard> CardsPlayerHas(Player p)
    {
        var known = Game.KnownCards(Player).ToList();
        return p.TreacheryCards.Where(c => known.Contains(c)).ToList();
    }

    private int CardQuality(TreacheryCard cardToRate, Player? forWhom)
    {
        var cardsToTakeIntoAccount = new List<TreacheryCard>(Player.TreacheryCards);
        if (forWhom != null && forWhom != Player)
        {
            var myKnownCards = Game.KnownCards(Player).ToList();
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
        if (cardToRate.Type == TreacheryCardType.Karma && Faction == Faction.Black && !Player.SpecialKarmaPowerUsed) return 5;

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

    protected virtual bool IAmDesperateForResources => ResourcesIncludingAllyContribution < 5;

    protected virtual int ResourcesIncludingAllyContribution => Resources + ResourcesFromAlly;

    protected virtual int ResourcesIncludingAllyAndRedContribution => Resources + ResourcesFromAlly + ResourcesFromRed;

    protected virtual int ResourcesFromAlly => Ally != Faction.None ? Game.GetPermittedUseOfAllyResources(Faction) : 0;

    protected virtual int ResourcesFromRed => Game.SpiceForBidsRedCanPay(Faction);

    protected virtual int AllyResources => AlliedPlayer?.Resources ?? 0;

    protected virtual int ResourcesIn(Location l) 
        => Game.ResourcesOnPlanet.GetValueOrDefault(l, 0);

    protected virtual bool HasResources(Location l) 
        => Game.ResourcesOnPlanet.ContainsKey(l);

    #endregion

    #region LocationInformation

    private bool IsStronghold(Location l)
    {
        return
            l.IsStronghold ||
            Game.IsSpecialStronghold(l.Territory) ||
            (l is DiscoveredLocation dl && (dl.Discovery == DiscoveryToken.Cistern 
                                            || dl.Discovery == DiscoveryToken.TestingStation 
                                            || dl.Discovery == DiscoveryToken.ProcessingStation 
                                            || (dl.Discovery == DiscoveryToken.Shrine && Player.Has(TreacheryCardType.Clairvoyance))));
    }

    private bool NotOccupiedByOthers(Location l)
    {
        return NotOccupiedByOthers(l.Territory);
    }

    private bool NotOccupiedByOthers(Territory t)
    {
        return Game.NrOfOccupantsExcludingFaction(t, Faction) == 0 && AllyDoesntBlock(t);
    }

    private bool NotOccupied(Territory t)
    {
        return !Game.IsOccupied(t);
    }

    protected bool NotOccupied(Location l)
    {
        return !Game.IsOccupied(l.Territory);
    }

    private bool Vacant(Location l)
    {
        return Vacant(l.Territory);
    }

    private bool Vacant(Territory t)
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

    protected virtual Player? GetOpponentThatOccupies(Location l)
    {
        return GetOpponentThatOccupies(l.Territory);
    }

    protected virtual Player? GetOpponentThatOccupies(Territory t)
    {
        return Opponents.FirstOrDefault(o => o.Occupies(t));
    }

    protected virtual bool StormWillProbablyHit(Location l)
    {
        if (Game.IsProtectedFromStorm(l) || LastTurn) return false;

        for (var i = 1; i <= MaxExpectedStormMoves; i++)
            if ((Game.SectorInStorm + i) % Map.NumberOfSectors == l.Sector) return true;

        return false;
    }

    protected virtual bool VacantAndSafeFromStorm(Location location)
    {
        if (Faction == Faction.Yellow && !Game.Prevented(FactionAdvantage.YellowProtectedFromStorm))
            return NotOccupied(location.Territory);
        return NotOccupied(location.Territory) && location.Sector != Game.SectorInStorm && !StormWillProbablyHit(location);
    }


    private bool IDontHaveAdvisorsIn(Location l)
    {
        return Faction != Faction.Blue || Player.SpecialForcesIn(l.Territory) == 0;
    }

    private bool AllyDoesntBlock(Territory t)
    {
        return Ally is Faction.None or Faction.Pink ||
               Faction is Faction.Pink ||
               AlliedPlayer!.AnyForcesIn(t) == 0 ||
               (Faction is Faction.Blue && Game.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                Player.SpecialForcesIn(t) > 0) ||
               (Ally is Faction.Blue && Game.Applicable(Rule.AdvisorsDontConflictWithAlly) &&
                AlliedPlayer.SpecialForcesIn(t) > 0);
    }

    private bool AllyDoesntBlock(Location l)
    {
        return AllyDoesntBlock(l.Territory);
    }

    protected virtual bool WithinRange(Location from, Location to, Battalion b)
    {
        var onlyAdvisors = b is { Faction: Faction.Blue, AmountOfForces: 0 };

        var willGetOrnithopters =
            !onlyAdvisors && !Game.Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) && (Equals(from, Game.Map.Arrakeen) || Equals(from, Game.Map.Carthag)) ? 3 : 0;

        var moveDistance = Math.Max(willGetOrnithopters, Game.DetermineMaximumMoveDistance(Player, [b]));

        //Game.ForcesOnPlanet used to be null
        var result = Game.Map.FindNeighbours(from, moveDistance, false, Faction, Game).Contains(to);

        return result;
    }

    protected virtual bool WithinDistance(Location from, Location to, int distance) 
        => Game.Map.FindNeighbours(from, distance, false, Faction, Game).Contains(to);

    private bool ProbablySafeFromMonster(Territory t) 
        => Game.CurrentTurn == Game.MaximumTurns 
           || Game.ProtectedFromMonster(Player) 
           || t != Game.LatestSpiceCardA.Location.Territory 
           || Game.SandTroutOccured 
           || !Game.HasResourceDeckPrescience(Player) 
           || Game.ResourceCardDeck.Top is { IsShaiHulud: false };

    #endregion

    #region PlanetaryForceInformation
    
    protected virtual Player? OccupyingOpponentIn(Territory t) =>
        Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t))
            .HighestOrDefault(p => MaxDial(p, t, Player));

    protected virtual List<Player> OccupyingOpponentsIn(Territory t) 
        => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t)).ToList();

    protected virtual List<Player> OccupyingOpponentsIn(Location l) 
        => OccupyingOpponentsIn(l.Territory);

    protected virtual bool InStorm(Location l) 
        => l.Sector == Game.SectorInStorm;

    protected virtual BattalionInLocation? BattalionThatShouldBeMovedDueToAllyPresence
    {
        get
        {
            if (Ally == Faction.None) return null;

            if (Game.HasActedOrPassed.Contains(Ally))
                //Ally has already acted => move the biggest battalion
                return ForcesOnPlanet.Where(locationWithBattalion =>
                        !Equals(locationWithBattalion.Key, Game.Map.PolarSink) &&
                        !InStorm(locationWithBattalion.Key) &&
                        !AllyDoesntBlock(locationWithBattalion.Key))
                    .Select(x => new BattalionInLocation(x.Key,x.Value))
                    .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);
            
            //Ally has not acted yet => move the smallest battalion
            return ForcesOnPlanet.Where(locationWithBattalion =>
                    !Equals(locationWithBattalion.Key, Game.Map.PolarSink) &&
                    !InStorm(locationWithBattalion.Key) &&
                    !AllyDoesntBlock(locationWithBattalion.Key.Territory))
                .Select(x => new BattalionInLocation(x.Key,x.Value))
                .LowestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);
        }
    }

    private bool MayFleeOutOf(Location l)
    {
        return !IsStronghold(l) || !(IAmWinning || OpponentsAreWinning);
    }

    protected virtual BattalionInLocation? BiggestBattalionThreatenedByStormWithoutSpice => ForcesOnPlanet
        .Where(locationWithBattalion =>
            StormWillProbablyHit(locationWithBattalion.Key) &&
            !InStorm(locationWithBattalion.Key) &&
            MayFleeOutOf(locationWithBattalion.Key) &&
            !HasResources(locationWithBattalion.Key))
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestBattalionInSpicelessNonStrongholdLocationOnRock => ForcesOnPlanet
        .Where(locationWithBattalion =>
            !IsStronghold(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            Game.IsProtectedFromStorm(locationWithBattalion.Key) &&
            ResourcesIn(locationWithBattalion.Key) == 0 &&
            (!Player.Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold =>
        ForcesOnPlanet.Where(locationWithBattalion =>
                !IsStronghold(locationWithBattalion.Key) &&
                locationWithBattalion.Key.Sector != Game.SectorInStorm &&
                (!Game.IsProtectedFromStorm(locationWithBattalion.Key) || !Game.Map.Strongholds.Any(s =>
                    WithinRange(locationWithBattalion.Key, s, locationWithBattalion.Value))) &&
                ResourcesIn(locationWithBattalion.Key) == 0 &&
                (!Player.Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa))
            .Select(x => new BattalionInLocation(x.Key, x.Value))
            .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice 
        => ForcesOnPlanet.Where(locationWithBattalion =>
            !IsStronghold(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            ResourcesIn(locationWithBattalion.Key) == 0 &&
            (!Player.Has(TreacheryCardType.Metheor) || locationWithBattalion.Key.Territory != Game.Map.PastyMesa) &&
            VacantAndSafeNearbyStronghold(locationWithBattalion.Key, locationWithBattalion.Value) == null &&
            BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) == null)
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key) &&
            locationWithBattalion.Key.Sector != Game.SectorInStorm &&
            locationWithBattalion.Value.TotalAmountOfForces >= 8 &&
            VacantAndSafeNearbyStronghold(locationWithBattalion.Key, locationWithBattalion.Value) != null)
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold => ForcesOnPlanet.Where(locationWithBattalion =>
            locationWithBattalion.Value is { Faction: Faction.Blue, AmountOfSpecialForces: > 0 } &&
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key) &&
            !InStorm(locationWithBattalion.Key) &&
            VacantAndSafeNearbyStronghold(locationWithBattalion.Key, locationWithBattalion.Value) != null)
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    protected virtual BattalionInLocation? BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice => ForcesOnPlanet.Where(locationWithBattalion =>
            IsStronghold(locationWithBattalion.Key) &&
            NotOccupiedByOthers(locationWithBattalion.Key.Territory) &&
            !InStorm(locationWithBattalion.Key) &&
            locationWithBattalion.Value.TotalAmountOfForces >= (IAmDesperateForResources ? 5 : 7) &&
            BestSafeAndNearbyResources(locationWithBattalion.Key, locationWithBattalion.Value) != null)
        .Select(x => new BattalionInLocation(x.Key, x.Value))
        .HighestOrDefault(locationWithBattalion => locationWithBattalion.Battalion.TotalAmountOfForces);

    private Location? BestSafeAndNearbyResources(Location location, Battalion b, bool mayFight = false)
    {
        return Game.ResourcesOnPlanet.Where(l => IsSafeAndNearby(location, l.Key, b, mayFight)).HighestOrDefault(r => r.Value).Key;
    }

    private Location? BestSafeAndNearbyDiscovery(Location location, Battalion b, bool mayFight = false)
    {
        return Game.DiscoveriesOnPlanet.Keys.FirstOrDefault(l => IsSafeAndNearby(location, l, b, mayFight));
    }

    private List<Battalion> NearbyBattalionsOutsideStrongholds(Location l)
    {
        return ForcesOnPlanet.Where(kvp => !kvp.Key.IsStronghold && WithinRange(kvp.Key, l, kvp.Value))
            .Select(kvp => kvp.Value).ToList();
    }

    protected virtual bool IsSafeAndNearby(Location source, Location destination, Battalion b, bool mayFight)
    {
        var opponent = GetOpponentThatOccupies(destination.Territory);

        return WithinRange(source, destination, b) &&
               AllyDoesntBlock(destination.Territory) &&
               ProbablySafeFromMonster(destination.Territory) &&
               (opponent == null || (mayFight && GetDialNeeded(destination.Territory, opponent, false) < MaxDial(Resources, b, opponent.Faction))) &&
               !StormWillProbablyHit(destination);
    }

    #endregion

    #region DestinationsOfMovement

    private List<Location> ValidMovementLocations(Location from, Battalion battalion)
    {
        var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
        return PlacementEvent.ValidTargets(Game, Player, from, battalion).Where(l => !forbidden.Contains(l.Territory)).ToList();
    }

    protected virtual Location? VacantAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(from, battalion)
            .FirstOrDefault(to => IsStronghold(to) && !StormWillProbablyHit(to) && Vacant(to));
    }

    protected virtual Location? UnthreatenedAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(from, battalion).FirstOrDefault(to =>
            IsStronghold(to) &&
            !StormWillProbablyHit(to) &&
            NotOccupiedByOthers(to));
    }

    protected virtual Location? WeakAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(from, battalion).FirstOrDefault(to =>
            IsStronghold(to) &&
            Player.AnyForcesIn(to) > 0 &&
            AllyDoesntBlock(to.Territory) &&
            !StormWillProbablyHit(to));
    }

    protected virtual Location? NearbyStrongholdOfWinningOpponent(Location from, Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            WinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
        ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }

    protected virtual Location? NearbyStrongholdOfAlmostWinningOpponent(Location from, Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(from, battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            AlmostWinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
        ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }

    private bool IsWinningOpponent(Player p) 
        => p != Player && p.Faction != Ally && Game.MeetsNormalVictoryCondition(p, true);

    private bool IsWinningOpponent(Faction f) 
        => IsWinningOpponent(Game.GetPlayer(f));

    private bool IsAlmostWinningOpponent(Player p)
    {
        return p != Player && p != AlliedPlayer &&
               Game.NumberOfVictoryPoints(p, true) + 1 >= Game.ThresholdForWin(p) &&
               (CanShip(p) || (p.HasAlly && CanShip(p.AlliedPlayer)) || p.TechTokens.Count >= 2);
    }

    private List<Player> WinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots)
    {
        return Game.Players.Where(p =>
            (includeBots || !p.IsBot || !p.AllyIsBot) && IsWinningOpponent(p) &&
            Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds &&
            !WinWasPredictedByMeThisTurn(p.Faction)).ToList();
    }

    private List<Player> AlmostWinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots)
    {
        return Game.Players.Where(p =>
            (includeBots || !p.IsBot || !p.AllyIsBot) && IsAlmostWinningOpponent(p) &&
            Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds &&
            !WinWasPredictedByMeThisTurn(p.Faction)).ToList();
    }


    protected virtual Location? WinnableNearbyStronghold(Location from, Battalion battalion)
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
                Opponent = s.Opponent!.Faction,
                DialNeeded = GetDialNeeded(s.Stronghold.Territory, GetOpponentThatOccupies(s.Stronghold.Territory), true)
            });

        var resourcesForBattle = Ally == Faction.Brown ? ResourcesIncludingAllyContribution : Resources;

        var winnableNearbyStronghold = enemyWeakStrongholds.Where(s =>
            WinWasPredictedByMeThisTurn(s.Opponent) ||
            DetermineDialShortageForBattle(s.DialNeeded, s.Opponent, s.Stronghold.Territory, battalion.AmountOfForces + Player.ForcesIn(s.Stronghold), battalion.AmountOfSpecialForces + Player.SpecialForcesIn(s.Stronghold), resourcesForBattle) <= 0
        ).OrderBy(s => s.DialNeeded).FirstOrDefault();

        if (winnableNearbyStronghold == null)
            return null;
        return winnableNearbyStronghold.Stronghold;
    }

    #endregion

    #region BattleInformation_Dial

    protected virtual bool IMustPayForForcesInBattle => Battle.MustPayForAnyForcesInBattle(Game, Player);

    protected virtual float MaxDial(Player p, Territory t, Player? opponent, bool ignoreSpiceDialing = false)
    {
        var countForcesForWhite = 0;
        if (p.Faction == Faction.White && p.SpecialForcesIn(t) > 0) countForcesForWhite = Faction == Faction.White || Ally == Faction.White ? Game.CurrentNoFieldValue : Game.LatestRevealedNoFieldValue == 5 ? 3 : 5;

        return MaxDial(
            ignoreSpiceDialing ? 99 : p.Resources,
            p.ForcesIn(t) + countForcesForWhite,
            p.Faction != Faction.White ? p.SpecialForcesIn(t) : 0,
            p,
            opponent?.Faction ?? Faction.Black);
    }

    protected virtual float MaxDial(int resources, Battalion battalion, Faction opponent)
    {
        return MaxDial(resources, battalion.AmountOfForces, battalion.AmountOfSpecialForces, Game.GetPlayer(battalion.Faction), opponent);
    }

    protected virtual float MaxDial(int resources, int forces, int specialForces, Player p, Faction opponentFaction)
    {
        var spice = Battle.MustPayForAnyForcesInBattle(Game, p) ? resources : 99;

        var specialForcesAtFullStrength = Math.Min(specialForces, spice);
        spice -= specialForcesAtFullStrength;
        var specialForcesAtHalfStrength = specialForces - specialForcesAtFullStrength;

        var forcesAtFullStrength = Math.Min(forces, spice);
        var forcesAtHalfStrength = forces - forcesAtFullStrength;

        var result =
            Battle.DetermineSpecialForceStrength(Game, p.Faction, opponentFaction) * (specialForcesAtFullStrength + 0.5f * specialForcesAtHalfStrength) +
            Battle.DetermineNormalForceStrength(Game, p.Faction) * (forcesAtFullStrength + 0.5f * forcesAtHalfStrength);

        return result;
    }

    protected virtual float TotalMaxDialOfOpponents(Territory t)
    {
        return Opponents.Sum(o => MaxDial(o, t, Player));
    }

    private bool IWillBeAggressorAgainst(Player? opponent)
    {
        if (opponent == null) return false;

        var firstPlayerPosition = PlayerSequence.DetermineFirstPlayer(Game).Seat;

        for (var i = 0; i < Game.MaximumPlayers; i++)
        {
            var position = (firstPlayerPosition + i) % Game.MaximumPlayers;
            if (position == Player.Seat)
                return true;
            if (position == opponent.Seat) return false;
        }

        return false;
    }

    private bool CanShip(Player p) 
        => Game.CurrentMainPhase < MainPhase.ShipmentAndMove || (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && !Game.HasActedOrPassed.Contains(p.Faction));

    protected virtual int NrOfBattlesToFight => Battle.BattlesToBeFought(Game, Player).Count;

    protected virtual float MaxReinforcedDialTo(Player p, Territory to)
    {
        //if (player == null || to == null) return 0;

        if (CanShip(p))
        {
            var specialForces = 0;
            var normalForces = 0;

            var opponentResources = p.Resources + p.AlliedResources;

            var opponentMayUseWorthlessAsKarma = p.Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);
            var hasKarma = CardsPlayerHas(p).Any(c => c.Type == TreacheryCardType.Karma || (opponentMayUseWorthlessAsKarma && c.Type == TreacheryCardType.Karma));

            while (specialForces + 1 <= p.SpecialForcesInReserve && Shipment.DetermineCost(Game, p, normalForces, specialForces + 1, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) specialForces++;

            while (normalForces + 1 <= p.ForcesInReserve && Shipment.DetermineCost(Game, p, normalForces + 1, specialForces, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) normalForces++;

            return specialForces * Battle.DetermineSpecialForceStrength(Game, p.Faction, Faction) + normalForces * Battle.DetermineNormalForceStrength(Game, p.Faction);
        }

        return 0;
    }

    #endregion

    #region BattleInformation_Leaders

    protected virtual List<IHero> SafeOrKnownTraitorLeaders
    {
        get
        {
            var ally = Ally != Faction.None ? AlliedPlayer : null;
            var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : [];
            var knownNonTraitors = Player.Traitors.Union(Player.KnownNonTraitors).Union(knownNonTraitorsByAlly);

            var myKnownTraitorsAndNonTraitors = Player.Traitors.Union(knownNonTraitors);
            var allyKnownTraitorsAndNonTraitors = Player.HasAlly ? AlliedPlayer!.Traitors.Union(AlliedPlayer.KnownNonTraitors) : [];
            var revealedOrToldTraitors = Game.Players.SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors));

            return myKnownTraitorsAndNonTraitors.Union(allyKnownTraitorsAndNonTraitors).Union(revealedOrToldTraitors).ToList();
        }
    }

    #endregion

    #region BattleInformation_WeaponsAndDefenses

    private List<TreacheryCard> KnownOpponentWeapons(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => c.IsWeapon && Game.KnownCards(Player).Contains(c)).ToList();
    }

    private List<TreacheryCard> KnownOpponentCards(Faction opponent)
    {
        return KnownOpponentCards(Game.GetPlayer(opponent));
    }

    private List<TreacheryCard> KnownOpponentCards(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => Game.KnownCards(Player).Contains(c)).ToList();
    }

    private List<TreacheryCard> KnownOpponentDefenses(Player opponent)
    {
        return opponent.TreacheryCards.Where(c => c.IsDefense && Game.KnownCards(Player).Contains(c)).ToList();
    }
    
    private List<TreacheryCard> Weapons(TreacheryCard? usingThisDefense, IHero? usingThisHero, Territory? territory)
    {
        return Battle.ValidWeapons(Game, Player, usingThisDefense, usingThisHero, territory).ToList();
    }

    private List<TreacheryCard> Defenses(TreacheryCard? usingThisWeapon, Territory? territory)
    {
        return Battle.ValidDefenses(Game, Player, usingThisWeapon, territory).ToList();
    }

    private static List<TreacheryCard> Weapons(Game game, Player player, TreacheryCard? usingThisDefense, IHero? usingThisHero, Territory? territory)
    {
        return Battle.ValidWeapons(game, player, usingThisDefense, usingThisHero, territory).ToList();
    }

    protected virtual TreacheryCard? UselessAsWeapon(TreacheryCard? usingThisDefense)
    {
        return Weapons(usingThisDefense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);
    }

    protected virtual TreacheryCard? UselessAsDefense(TreacheryCard? usingThisWeapon)
    {
        return Defenses(usingThisWeapon, null).LastOrDefault(c => c.Type == TreacheryCardType.Useless);
    }

    private bool MayPlayNoWeapon(Player p, TreacheryCard? usingThisDefense)
    {
        return Battle.ValidWeapons(Game, p, usingThisDefense, null, null, true).Contains(null);
    }

    private bool MayPlayNoDefense(Player p, TreacheryCard? usingThisWeapon)
    {
        return Battle.ValidDefenses(Game, p, usingThisWeapon, null, true).Contains(null);
    }

    private int CountDifferentWeaponTypes(List<TreacheryCard> cards)
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

    private int CountDifferentDefenseTypes(List<TreacheryCard> cards)
    {
        var result = 0;
        if (cards.Any(card => card.IsProjectileDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
        if (cards.Any(card => card.IsPoisonDefense && card.Type != TreacheryCardType.ShieldAndAntidote)) result++;
        if (cards.Any(card => card.Type == TreacheryCardType.ShieldAndAntidote)) result++;
        return result;
    }

    #endregion
}

public class BattalionInLocation(Location location, Battalion battalion)
{
    public Location Location { get; } = location;
    
    public Battalion Battalion { get; } = battalion;
}