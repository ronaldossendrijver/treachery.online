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

    private static ClairVoyanceQandA? MyClairvoyanceAboutEnemyDefenseInCurrentBattle(Game game, Player player) =>
        game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } &&
        game.LatestClairvoyanceBattle == game.CurrentBattle &&
        (game.LatestClairvoyance.Initiator == player.Faction || game.LatestClairvoyance.Initiator == player.Ally) &&
        game.LatestClairvoyance.Question is ClairvoyanceQuestion.CardTypeAsDefenseInBattle or ClairvoyanceQuestion.CardTypeInBattle ? game.LatestClairvoyanceQandA : null;

    private static ClairVoyanceQandA? PlayerClairvoyanceAboutEnemyWeaponInCurrentBattle(Game game, Player player) =>
        game is { LatestClairvoyance: not null, LatestClairvoyanceQandA: not null } &&
        game.LatestClairvoyanceBattle == game.CurrentBattle &&
        (game.LatestClairvoyance.Initiator == player.Faction || game.LatestClairvoyance.Initiator == player.Ally) &&
        game.LatestClairvoyance.Question is ClairvoyanceQuestion.CardTypeAsWeaponInBattle or ClairvoyanceQuestion.CardTypeInBattle ? game.LatestClairvoyanceQandA : null;

    private Voice? MyVoice => Game.CurrentVoice != null && (Faction == Faction.Blue || Ally == Faction.Blue) ? Game.CurrentVoice : null;

    private bool MayUseUselessAsKarma => Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);

    #endregion

    #region CardKnowledge

    private static List<TreacheryCard> CardsUnknownToPlayer(Game game, Player player) 
        => TreacheryCardManager.GetCardsInPlay(game).Where(c => !game.KnownCards(player).Contains(c)).ToList();

    private static List<TreacheryCard> OpponentCardsUnknownToPlayer(Game game, Player player, Player opponent) 
        => opponent.TreacheryCards.Where(c => !game.KnownCards(player).Contains(c)).ToList();

    private static bool IsKnownToOpponent(Game game, Player opponent, TreacheryCard card) 
        => game.KnownCards(opponent).Contains(card);

    private static List<TreacheryCard> CardsPlayerHasOrMightHave(Game game, Player player)
    {
        var known = game.KnownCards(player).ToList();
        var result = new List<TreacheryCard>(player.TreacheryCards.Where(c => known.Contains(c)));

        var playerHasUnknownCards = player.TreacheryCards.Any(c => !known.Contains(c));
        if (playerHasUnknownCards) result.AddRange(CardsUnknownToPlayer(game, player));

        return result;
    }

    private static List<TreacheryCard> KnownCardsHeldByPlayer(Game game, Player player)
    {
        var known = game.KnownCards(player).ToList();
        return player.TreacheryCards.Where(c => known.Contains(c)).ToList();
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
    
    protected static Player? OccupyingOpponentIn(Territory t) =>
        Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t))
            .HighestOrDefault(p => MaxDial(p, t, Player));

    protected static List<Player> OccupyingOpponentsIn(Territory t) 
        => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t)).ToList();

    protected static List<Player> OccupyingOpponentsIn(Location l) 
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

    private bool IsAlmostWinningOpponent(Player opponent) =>
        opponent != Player && opponent != AlliedPlayer &&
        Game.NumberOfVictoryPoints(opponent, true) + 1 >= Game.ThresholdForWin(opponent) &&
        (CanShip(Game, opponent) || (opponent.HasAlly && CanShip(Game, opponent.AlliedPlayer)) || opponent.TechTokens.Count >= 2);

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

    protected static float MaxDial(Game game, Player player, Territory t, Player? opponent, bool ignoreSpiceDialing = false)
    {
        var countForcesForWhite = 0;
        if (player.Faction == Faction.White && player.SpecialForcesIn(t) > 0) countForcesForWhite = Faction == Faction.White || Ally == Faction.White ? Game.CurrentNoFieldValue : Game.LatestRevealedNoFieldValue == 5 ? 3 : 5;

        return MaxDial(
            game,
            ignoreSpiceDialing ? 99 : player.Resources,
            player.ForcesIn(t) + countForcesForWhite,
            player.Faction != Faction.White ? player.SpecialForcesIn(t) : 0,
            player,
            opponent?.Faction ?? Faction.Black);
    }

    protected static float MaxDial(Game game, int resources, Battalion battalion, Faction opponent)
    {
        return MaxDial(game, resources, battalion.AmountOfForces, battalion.AmountOfSpecialForces, Game.GetPlayer(battalion.Faction), opponent);
    }

    protected static float MaxDial(Game game, int resources, int forces, int specialForces, Player p, Faction opponentFaction)
    {
        var spice = Battle.MustPayForAnyForcesInBattle(game, p) ? resources : 99;

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

    private static bool CanShip(Game game, Player player) 
        => game.CurrentMainPhase < MainPhase.ShipmentAndMove || (game.CurrentMainPhase == MainPhase.ShipmentAndMove && !game.HasActedOrPassed.Contains(player.Faction));

    protected virtual int NrOfBattlesToFight => Battle.BattlesToBeFought(Game, Player).Count;

    private static float MaxReinforcedDialTo(Game game, Player player, Player opponent, Territory to)
    {
        //if (player == null || to == null) return 0;

        if (CanShip(game, opponent))
        {
            var specialForces = 0;
            var normalForces = 0;

            var opponentResources = opponent.Resources + opponent.AlliedResources;

            var opponentMayUseWorthlessAsKarma = opponent.Faction == Faction.Blue && game.Applicable(Rule.BlueWorthlessAsKarma);
            var hasKarma = KnownCardsHeldByPlayer(game, opponent).Any(c => c.Type == TreacheryCardType.Karma || (opponentMayUseWorthlessAsKarma && c.Type == TreacheryCardType.Karma));

            while (specialForces + 1 <= opponent.SpecialForcesInReserve && Shipment.DetermineCost(game, opponent, normalForces, specialForces + 1, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) specialForces++;

            while (normalForces + 1 <= opponent.ForcesInReserve && Shipment.DetermineCost(game, opponent, normalForces + 1, specialForces, to.MiddleLocation, hasKarma, false, false, false, false) <= opponentResources) normalForces++;

            return specialForces * Battle.DetermineSpecialForceStrength(game, opponent.Faction, player.Faction) + normalForces * Battle.DetermineNormalForceStrength(game, opponent.Faction);
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

    private static List<TreacheryCard> KnownOpponentWeapons(Game game, Player player, Player opponent) 
        => opponent.TreacheryCards.Where(c => c.IsWeapon && game.KnownCards(player).Contains(c)).ToList();

    private static List<TreacheryCard> KnownOpponentCards(Game game, Player player, Player opponent) 
        => opponent.TreacheryCards.Where(c => game.KnownCards(player).Contains(c)).ToList();

    private static List<TreacheryCard> KnownOpponentDefenses(Game game, Player player, Player opponent) 
        => opponent.TreacheryCards.Where(c => c.IsDefense && game.KnownCards(player).Contains(c)).ToList();

    private static List<TreacheryCard> Defenses(Game game, Player player, TreacheryCard? usingThisWeapon, Territory? territory) 
        => Battle.ValidDefenses(game, player, usingThisWeapon, territory).ToList();

    private static List<TreacheryCard> Weapons(Game game, Player player, TreacheryCard? usingThisDefense, IHero? usingThisHero, Territory? territory) 
        => Battle.ValidWeapons(game, player, usingThisDefense, usingThisHero, territory).ToList();

    private static TreacheryCard? UselessAsWeapon(Game game, Player player, TreacheryCard? usingThisDefense) 
        => Weapons(game, player, usingThisDefense, null, null).FirstOrDefault(c => c.Type == TreacheryCardType.Useless);

    private static TreacheryCard? UselessAsDefense(Game game, Player player, TreacheryCard? usingThisWeapon) 
        => Defenses(game, player, usingThisWeapon, null).LastOrDefault(c => c.Type == TreacheryCardType.Useless);

    private static bool MayPlayNoWeapon(Game game, Player player, TreacheryCard? usingThisDefense) 
        => Battle.ValidWeapons(game, player, usingThisDefense, null, null, true).Contains(null);

    private static bool MayPlayNoDefense(Game game, Player player, TreacheryCard? usingThisWeapon) 
        => Battle.ValidDefenses(game, player, usingThisWeapon, null, true).Contains(null);

    private static int CountDifferentWeaponTypes(List<TreacheryCard> cards)
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

    private static int CountDifferentDefenseTypes(List<TreacheryCard> cards)
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