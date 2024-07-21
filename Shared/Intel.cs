using System;

namespace Treachery.Shared;

internal class Intel
{
    public static float BattleGain(Territory battleTerritory, Game game,
        Battle plan, Battle opponentPlan,
        bool traitorCalled, bool opponentTraitorCalled)
    {
        var opponentIsAggressor = PlayerSequence.IsAfter(game, plan.Player, opponentPlan.Player);

        var aggPlan = opponentIsAggressor ? opponentPlan : plan;
        var defPlan = opponentIsAggressor ? plan : opponentPlan;

        var lasgunShield = !traitorCalled && !opponentTraitorCalled && (plan.HasLaser || opponentPlan.HasLaser) && (plan.HasShield || opponentPlan.HasShield);

        if ((traitorCalled && opponentTraitorCalled) || lasgunShield) return 0;

        if (traitorCalled) return 0; //todo

        if (opponentTraitorCalled) return 0; //todo

        var outcome = Battle.DetermineBattleOutcome(aggPlan, defPlan, battleTerritory, game);

        var loserPlan = outcome.LoserBattlePlan;
        var winnerPlan = outcome.WinnerBattlePlan;


        if (outcome.Winner == plan.Player)
        {
            var result =
                (outcome.LoserHeroKilled ? loserPlan.Hero.ValueInCombatAgainst(winnerPlan.Hero) : 0) +
                (outcome.WinnerHeroKilled ? winnerPlan.Hero.ValueInCombatAgainst(loserPlan.Hero) : 0) +
                ValueOf(loserPlan.Weapon) +
                ValueOf(loserPlan.Defense) +
                (MustBeDiscardedAfterBattle(winnerPlan.Weapon) ? ValueOf(loserPlan.Weapon) : 0) +
                (MustBeDiscardedAfterBattle(winnerPlan.Defense) ? ValueOf(loserPlan.Defense) : 0) +
                Revival.GetPriceOfForceRevival(game, outcome.Winner, winnerPlan.Forces + winnerPlan.ForcesAtHalfStrength, winnerPlan.SpecialForces + winnerPlan.SpecialForcesAtHalfStrength, false, out _, out _) +
                Revival.GetPriceOfForceRevival(game, outcome.Loser, loserPlan.Player.ForcesIn(battleTerritory), loserPlan.Player.SpecialForcesIn(battleTerritory), false, out _, out _) +
                (loserPlan.Player.TechTokens.Any() ? 5 : 0) +
                (battleTerritory.IsStronghold ? 10 : 0) +
                (battleTerritory == game.Map.Carthag.Territory || battleTerritory == game.Map.Arrakeen.Territory ? 3 : 0) +
                battleTerritory.Locations.Sum(l => Math.Min(game.ResourcesIn(l), winnerPlan.Player.BattalionIn(l).TotalAmountOfForces * game.ResourceCollectionRate(winnerPlan.Player)));
        }

        return 0;//todo

    }

    private static int ValueOf(TreacheryCard c)
    {
        return c.IsUseless ? 0 : 5;
    }

    private static bool MustBeDiscardedAfterBattle(TreacheryCard c)
    {
        if (c == null) return false;

        return
            !(c.IsWeapon || c.IsDefense || c.IsUseless) ||
            c.IsArtillery || c.IsMirrorWeapon || c.IsRockMelter || c.IsPoisonTooth || c.IsPortableAntidote;
    }



    /*
    public Game Game { get; set; }

    public Player Player { get; set; }

    public Faction Faction => Player.Faction;

    #region Redundant

    protected virtual bool MayFlipToAdvisors => Player.Faction == Faction.Blue && Game.Applicable(Rule.BlueAdvisors);

    protected bool MayUseUselessAsKarma => Player.Is(Faction.Blue) && Game.Applicable(Rule.BlueWorthlessAsKarma);

    protected bool IDontHaveAdvisorsIn(Location l) => Faction != Faction.Blue || Player.SpecialForcesIn(l.Territory) == 0;

    #endregion Redundant



    #region StormAndWorm

    protected virtual bool InStorm(Location l) => l.Sector == Game.SectorInStorm;

    protected int MaxExpectedStormMoves => Game.HasStormPrescience(Player) ? Game.NextStormMoves : Player.Param.Shipment_ExpectedStormMovesWhenUnknown;

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

    protected bool ProbablySafeFromShaiHulud(Territory t) => Game.CurrentTurn == Game.MaximumNumberOfTurns || Game.ProtectedFromMonster(Player) || t != Game.LatestSpiceCardA.Location.Territory || Game.SandTroutOccured || !Game.HasResourceDeckPrescience(Player) || Game.ResourceCardDeck.Top != null && !Game.ResourceCardDeck.Top.IsShaiHulud;

    #endregion StormAndWorm



    #region FactionSpecific

    protected bool WinWasPredictedByMeThisTurn(Faction opponentFaction) => Player.Faction == Faction.Blue && Game.CurrentTurn == Player.PredictedTurn && (opponentFaction == Player.PredictedFaction || Game.GetAlly(opponentFaction) == Player.PredictedFaction);

    protected Prescience MyPrescience => Game.CurrentPrescience != null && (Game.CurrentPrescience.By(Player) || Game.CurrentPrescience.By(Ally)) ? Game.CurrentPrescience : null;

    protected Voice MyVoice => Game.CurrentVoice != null && (Game.CurrentVoice.By(Player) || Game.CurrentVoice.By(Ally)) ? Game.CurrentVoice : null;

    #endregion FactionSpecific



    #region GeneralInformation

    protected virtual bool LastTurn => Game.CurrentTurn == Game.MaximumNumberOfTurns;

    protected virtual bool AlmostLastTurn => Game.CurrentTurn >= Game.MaximumNumberOfTurns - 1;

    #endregion GeneralInformation



    #region PlayersAndOpponents

    protected Player AlliedPlayer => Player.AlliedPlayer;

    protected Faction Ally => Player.Ally;

    protected IEnumerable<Player> Others => Game.Players.Where(p => p.Faction != Player.Faction);

    protected IEnumerable<Player> Opponents => Game.Players.Where(p => p.Faction != Player.Faction && p.Faction != Ally);

    private bool IsWinningOpponent(Player p) => p != Player && p.Faction != Ally && Game.MeetsNormalVictoryCondition(p, true);

    private bool IsWinningOpponent(Faction f) => IsWinningOpponent(Game.GetPlayer(f));

    private bool IsAlmostWinningOpponent(Player p) => p != Player && p != AlliedPlayer &&
        Game.NumberOfVictoryPoints(p, true) + 1 >= Game.TresholdForWin(p) && (CanShip(p) || p.HasAlly && CanShip(p.AlliedPlayer) || p.TechTokens.Count >= 2);

    private IEnumerable<Player> WinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots) =>
        Game.Players.Where(p => (includeBots || !p.IsBot) && Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds && !WinWasPredictedByMeThisTurn(p.Faction));

    private IEnumerable<Player> AlmostWinningOpponentsIWishToAttack(int maximumChallengedStrongholds, bool includeBots) =>
        Game.Players.Where(p => (includeBots || !p.IsBot) && Game.CountChallengedVictoryPoints(p) <= maximumChallengedStrongholds && !WinWasPredictedByMeThisTurn(p.Faction));


    protected virtual IEnumerable<Player> OpponentsToShipAndMove => Opponents.Where(p => !Game.HasActedOrPassed.Contains(p.Faction));

    #endregion PlayersAndOpponents

    #region Winning

    protected virtual int NrOfNonWinningPlayersToShipAndMoveIncludingMe => Game.Players.Where(p => !Game.MeetsNormalVictoryCondition(p, true)).Count() - Game.HasActedOrPassed.Count;

    protected bool IAmWinning => Game.MeetsNormalVictoryCondition(Player, true);

    protected bool OpponentsAreWinning => Opponents.Any(o => Game.MeetsNormalVictoryCondition(o, true));

    protected bool IsWinning(Player p) => Game.MeetsNormalVictoryCondition(p, true);

    protected bool IsWinning(Faction f) => Game.MeetsNormalVictoryCondition(Game.GetPlayer(f), true);

    #endregion Winning

    #region BattleInformation

    protected ClairVoyanceQandA MyClairVoyanceAboutEnemyDefenseInCurrentBattle =>
        Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null &&
        Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyance.By(Player) || Game.LatestClairvoyance.By(Player.Ally)) &&
        (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    protected ClairVoyanceQandA MyClairVoyanceAboutEnemyWeaponInCurrentBattle =>
        Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null &&
        Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyance.By(Player) || Game.LatestClairvoyance.By(Player.Ally)) &&
        (Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyance.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;





    #endregion

    #region CardKnowledge

    protected List<TreacheryCard> CardsUnknownToMe() => TreacheryCardManager.GetCardsInPlay(Game).Where(c => !Game.KnownCards(Player).Contains(c)).ToList();

    protected List<TreacheryCard> CardsUnknownToMe(Player p) => p.TreacheryCards.Where(c => !Game.KnownCards(Player).Contains(c)).ToList();

    protected bool IsKnownToOpponent(Player p, TreacheryCard card) => Game.KnownCards(p).Contains(card);

    protected IEnumerable<TreacheryCard> CardsPlayerHasOrMightHave(Player player)
    {
        var known = Game.KnownCards(Player).ToList();
        var result = player.TreacheryCards.Where(c => known.Contains(c)).ToList();

        var playerHasUnknownCards = player.TreacheryCards.Any(c => !known.Contains(c));
        if (playerHasUnknownCards)
        {
            result.AddRange(CardsUnknownToMe());
        }

        return result;
    }

    protected IEnumerable<TreacheryCard> CardsPlayerHas(Player player)
    {
        var known = Game.KnownCards(Player).ToList();
        return player.TreacheryCards.Where(c => known.Contains(c));
    }

    protected int CardQuality(TreacheryCard cardToRate, Player forWhom)
    {
        var cardsToTakeIntoAccount = Player.TreacheryCards.ToList();
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
        if (cardToRate.Type == TreacheryCardType.Karma && Faction == Faction.Black && !forWhom.SpecialKarmaPowerUsed) return 5;

        int qualityWhenObtainingBothKinds = (Faction == Faction.Green || Faction == Faction.Blue) ? 5 : 4;
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

    protected virtual int ResourcesIncludingAllyContribution => Player.Resources + ResourcesFromAlly;

    protected virtual int ResourcesIncludingAllyAndRedContribution => Player.Resources + ResourcesFromAlly + ResourcesFromRed;

    protected virtual int ResourcesFromAlly => Player.HasAlly ? Game.GetPermittedUseOfAllyResources(Faction) : 0;

    protected virtual int ResourcesFromRed => Game.SpiceForBidsRedCanPay(Faction);

    protected virtual int AllyResources => Player.HasAlly ? AlliedPlayer.Resources : 0;

    protected virtual int ResourcesIn(Location l)
    {
        if (Game.ResourcesOnPlanet.TryGetValue(l, out int value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }

    protected virtual bool HasResources(Location l) => Game.ResourcesOnPlanet.ContainsKey(l);

    private Location BestSafeAndNearbyResources(Battalion b, bool mayFight = false) => Game.ResourcesOnPlanet.Where(l => IsSafeAndNearby(b.Location, l.Key, b, mayFight)).HighestOrDefault(r => r.Value).Key;

    private Location BestSafeAndNearbyDiscovery(Battalion b, bool mayFight = false) => Game.DiscoveriesOnPlanet.Keys.Where(l => IsSafeAndNearby(b.Location, l, b, mayFight)).FirstOrDefault();

    private IEnumerable<Battalion> NearbyBattalionsOutsideStrongholds(Location l) => Player.BattalionsOnPlanet.Where(b => !IsStronghold(b.Location) && WithinRange(b.Location, l, b));

    protected virtual bool IsSafeAndNearby(Location source, Location destination, Battalion b, bool mayFight)
    {
        var opponent = GetOpponentThatOccupies(destination.Territory);

        return WithinRange(source, destination, b) &&
            AllyDoesntBlock(destination.Territory) &&
            ProbablySafeFromShaiHulud(destination.Territory) &&
            (opponent == null || mayFight && GetDialNeeded(destination.Territory, opponent, false) < MaxDial(Player.Resources, b, opponent.Faction)) &&
            !StormWillProbablyHit(destination);
    }

    #endregion

    #region LocationInformation

    protected bool IsStronghold(Location l) => l.IsStronghold || Game.IsSpecialStronghold(l.Territory);

    protected bool NotOccupiedByOthers(Territory t) => Game.NrOfOccupantsExcludingPlayer(t, Player) == 0 && AllyDoesntBlock(t);

    protected bool NotOccupied(Territory t) => !Game.IsOccupied(t);

    protected bool Vacant(Territory t) => !Game.AnyForcesIn(t);

    protected virtual bool OccupiedByOpponent(Territory t) => Opponents.Any(o => o.Occupies(t));

    protected virtual bool OccupiedByOpponent(Location l) => OccupiedByOpponent(l.Territory);

    protected virtual Player GetOpponentThatOccupies(Territory t) => Opponents.FirstOrDefault(o => o.Occupies(t));

    protected virtual bool VacantAndSafeFromStorm(Location location)
    {
        if (Faction == Faction.Yellow && !Game.Prevented(FactionAdvantage.YellowProtectedFromStorm))
        {
            return Vacant(location.Territory);
        }
        else
        {
            return Vacant(location.Territory) && location.Sector != Game.SectorInStorm && !StormWillProbablyHit(location);
        }
    }




    protected bool AllyDoesntBlock(Territory t) => Ally == Faction.None || Faction == Faction.Pink || Ally == Faction.Pink || AlliedPlayer.AnyForcesIn(t) == 0;

    protected virtual bool WithinRange(Location from, Location to, Battalion b)
    {
        bool onlyAdvisors = b.Faction == Faction.Blue && b.AmountOfForces == 0;

        int willGetOrnithopters =
            !onlyAdvisors && !Game.Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) && (from == Game.Map.Arrakeen || from == Game.Map.Carthag) ? 3 : 0;

        int moveDistance = Math.Max(willGetOrnithopters, Game.DetermineMaximumMoveDistance(Player, new Battalion[] { b }));

        //Game.ForcesOnPlanet used to be null
        var result = Game.Map.FindNeighbours(from, moveDistance, false, Faction, Game, true).Contains(to);

        return result;
    }

    protected virtual bool WithinRange(Location from, Location to, int travelDistance) => Game.Map.FindNeighbours(from, travelDistance, false, Faction, Game).Contains(to);



    #endregion

    #region PlanetaryForceInformation

    public bool HasNoFieldIn(Territory territory) => Faction == Faction.White && Player.SpecialForcesIn(territory) > 0;

    protected virtual Player OccupyingOpponentIn(Territory t) => Game.Players.Where(p => p.Faction != Faction && p.Faction != Ally && p.Occupies(t)).HighestOrDefault(p => MaxDial(p, t, Player));

    protected virtual IEnumerable<Player> OccupyingOpponentsIn(Territory t) => Opponents.Where(p.Occupies(t));

    protected virtual KeyValuePair<Location, Battalion> BattalionThatShouldBeMovedDueToAllyPresence
    {
        get
        {
            if (Ally == Faction.None) return default;

            if (Game.HasActedOrPassed.Contains(Ally))
            {
                //Ally has already acted => move biggest battalion
                return Player.ForcesOnPlanet.Where(locationWithBattalion =>
                !(locationWithBattalion.Key == Game.Map.PolarSink) &&
                !InStorm(locationWithBattalion.Key) &&
                !AllyDoesntBlock(locationWithBattalion.Key.Territory))
                .HighestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);
            }
            else
            {
                //Ally has not acted yet => move smallest battalion
                return Player.ForcesOnPlanet.Where(locationWithBattalion =>
                !(locationWithBattalion.Key == Game.Map.PolarSink) &&
                !InStorm(locationWithBattalion.Key) &&
                !AllyDoesntBlock(locationWithBattalion.Key.Territory))
                .LowestOrDefault(locationWithBattalion => locationWithBattalion.Value.TotalAmountOfForces);
            }
        }
    }

    protected virtual Battalion BiggestBattalionThreatenedByStormWithoutSpice => Player.BattalionsOnPlanet.Where(b =>
            StormWillProbablyHit(b.Location) &&
            !InStorm(b.Location) &&
            (!IsStronghold(b.Location) || !IAmWinning) &&
            ResourcesIn(b.Location) == 0
            ).HighestOrDefault(locationWithBattalion => locationWithBattalion.TotalAmountOfForces);

    protected virtual Battalion BiggestBattalionInSpicelessNonStrongholdLocationOnRock => Player.BattalionsOnPlanet.Where(b =>
            !IsStronghold(b.Location) &&
            b.Location.Sector != Game.SectorInStorm &&
            Game.IsProtectedFromStorm(b.Location) &&
            ResourcesIn(b.Location) == 0 &&
            (!Player.Has(TreacheryCardType.Metheor) || b.Location.Territory != Game.Map.PastyMesa))
            .HighestOrDefault(b => b.TotalAmountOfForces);

    protected virtual Battalion BiggestBattalionInSpicelessNonStrongholdLocationInSandOrNotNearStronghold => Player.BattalionsOnPlanet.Where(b =>
            !IsStronghold(b.Location) &&
            b.Location.Sector != Game.SectorInStorm &&
            (!Game.IsProtectedFromStorm(b.Location) || !Game.Map.Strongholds.Any(s => WithinRange(b.Location, s, b))) &&
            ResourcesIn(b.Location) == 0 &&
            (!Player.Has(TreacheryCardType.Metheor) || b.Location.Territory != Game.Map.PastyMesa))
            .HighestOrDefault(b => b.TotalAmountOfForces);

    protected virtual Battalion BiggestBattalionInSpicelessNonStrongholdLocationNotNearStrongholdAndSpice => Player.BattalionsOnPlanet.Where(b =>
            !IsStronghold(b.Location) &&
            b.Location.Sector != Game.SectorInStorm &&
            ResourcesIn(b.Location) == 0 &&
            (!Player.Has(TreacheryCardType.Metheor) || b.Location.Territory != Game.Map.PastyMesa) &&
            VacantAndSafeNearbyStronghold(b) == null &&
            BestSafeAndNearbyResources(b) == null)
            .HighestOrDefault(b => b.TotalAmountOfForces);

    protected virtual Battalion BiggestLargeUnthreatenedMovableBattalionInStrongholdNearVacantStronghold => Player.BattalionsOnPlanet.Where(b =>
            IsStronghold(b.Location) &&
            NotOccupiedByOthers(b.Location.Territory) &&
            b.Location.Sector != Game.SectorInStorm &&
            b.TotalAmountOfForces >= 8 &&
            VacantAndSafeNearbyStronghold(b) != null)
            .HighestOrDefault(b => b.TotalAmountOfForces);

    protected virtual Battalion BiggestMovableStackOfAdvisorsInStrongholdNearVacantStronghold => Player.BattalionsOnPlanet.Where(b =>
            b.Faction == Faction.Blue &&
            b.AmountOfSpecialForces > 0 &&
            IsStronghold(b.Location) &&
            NotOccupiedByOthers(b.Location.Territory) &&
            !InStorm(b.Location) &&
            VacantAndSafeNearbyStronghold(b) != null)
            .HighestOrDefault(b => b.TotalAmountOfForces);

    protected virtual Battalion BiggestLargeUnthreatenedMovableBattalionInStrongholdNearSpice => Player.BattalionsOnPlanet.Where(b =>
            IsStronghold(b.Location) &&
            NotOccupiedByOthers(b.Location.Territory) &&
            !InStorm(b.Location) &&
            b.TotalAmountOfForces >= (IAmDesparateForResources ? 5 : 7) &&
            BestSafeAndNearbyResources(b) != null)
            .HighestOrDefault(b => b.TotalAmountOfForces);


    #endregion

    #region DestinationsOfMovement

    protected IEnumerable<Location> ValidMovementLocations(Battalion battalion)
    {
        var forbidden = Game.Deals.Where(deal => deal.BoundFaction == Faction && deal.Type == DealType.DontShipOrMoveTo).Select(deal => deal.GetParameter1<Territory>(Game));
        return PlacementEvent.ValidTargets(Game, Player, battalion.Location, battalion).Where(l => !forbidden.Contains(l.Territory));
    }

    protected virtual Location VacantAndSafeNearbyStronghold(Battalion battalion)
    {
        return ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            !StormWillProbablyHit(to) &&
            Vacant(to.Territory)
            ).FirstOrDefault();
    }

    protected virtual Location UnthreatenedAndSafeNearbyStronghold(Location from, Battalion battalion)
    {
        return ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            !StormWillProbablyHit(to) &&
            NotOccupiedByOthers(to.Territory)
            ).FirstOrDefault();
    }

    protected virtual Location SafeNearbyStrongholdAlreadyOccupiedByMe(Location from, Battalion battalion)
    {
        return ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            Player.AnyForcesIn(to) > 0 &&
            AllyDoesntBlock(to.Territory) &&
            !StormWillProbablyHit(to)
            ).FirstOrDefault();
    }

    protected virtual Location NearbyStrongholdOfWinningOpponent(Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            WinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
            ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }

    protected virtual Location NearbyStrongholdOfAlmostWinningOpponent(Location from, Battalion battalion, bool includeBots)
    {
        return ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            AllyDoesntBlock(to.Territory) &&
            AlmostWinningOpponentsIWishToAttack(20, includeBots).Any(opponent => opponent.Occupies(to))
            ).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
    }




    protected virtual Location WinnableNearbyStronghold(Battalion battalion)
    {
        var enemyWeakStrongholds = ValidMovementLocations(battalion).Where(to =>
            IsStronghold(to) &&
            OccupiedByOpponent(to.Territory) &&
            AllyDoesntBlock(to.Territory) &&
            !StormWillProbablyHit(to))
            .Select(l => new { Stronghold = l, Opponent = OccupyingOpponentIn(l.Territory) })
            .Where(s => s.Opponent != null).Select(s => new
            {
                s.Stronghold,
                Opponent = s.Opponent.Faction,
                DialNeeded = GetDialNeeded(s.Stronghold.Territory, GetOpponentThatOccupies(s.Stronghold.Territory), true)
            });

        int resourcesForBattle = Ally == Faction.Brown ? ResourcesIncludingAllyContribution : Resources;

        var winnableNearbyStronghold = enemyWeakStrongholds.Where(s =>
            WinWasPredictedByMeThisTurn(s.Opponent) ||
            DetermineDialShortageForBattle(s.DialNeeded, s.Opponent, s.Stronghold.Territory, battalion.AmountOfForces + ForcesIn(s.Stronghold), battalion.AmountOfSpecialForces + SpecialForcesIn(s.Stronghold), resourcesForBattle) <= 0
            ).OrderBy(s => s.DialNeeded).FirstOrDefault();

        if (winnableNearbyStronghold == null)
        {
            return null;
        }
        else
        {
            return winnableNearbyStronghold.Stronghold;
        }
    }

    #endregion

    #region BattleInformation_Dial

    protected virtual bool IMustPayForForcesInBattle => Battle.MustPayForForcesInBattle(Game, Player);

    protected virtual float MaxDial(Player p, Territory t, Player opponent, bool ignoreSpiceDialing = false)
    {
        int countForcesForWhite = 0;
        if (p.Faction == Faction.White && p.SpecialForcesIn(t) > 0)
        {
            countForcesForWhite = Faction == Faction.White || Ally == Faction.White ? Game.CurrentNoFieldValue : (Game.LatestRevealedNoFieldValue == 5 ? 3 : 5);
        }

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
        int spice = Battle.MustPayForForcesInBattle(Game, player) ? resources : 99;

        int specialForcesAtFullStrength = Math.Min(specialForces, spice);
        spice -= specialForcesAtFullStrength;
        int specialForcesAtHalfStrength = specialForces - specialForcesAtFullStrength;

        int forcesAtFullStrength = Math.Min(forces, spice);
        int forcesAtHalfStrength = forces - forcesAtFullStrength;

        var result =
            Battle.DetermineSpecialForceStrength(Game, player.Faction, opponentFaction) * (specialForcesAtFullStrength + 0.5f * specialForcesAtHalfStrength) +
            Battle.DetermineNormalForceStrength(Game, player.Faction) * (forcesAtFullStrength + 0.5f * forcesAtHalfStrength);

        return result;
    }

    protected virtual float TotalMaxDialOfOpponents(Territory t) => Opponents.Sum(o => MaxDial(o, t, Player));

    protected bool IWillBeAggressorAgainst(Player opponent)
    {
        if (opponent == null) return false;

        var firstPlayerPosition = PlayerSequence.DetermineFirstPlayer(Game).PositionAtTable;

        for (int i = 0; i < Game.MaximumNumberOfPlayers; i++)
        {
            var position = (firstPlayerPosition + i) % Game.MaximumNumberOfPlayers;
            if (position == Player.PositionAtTable)
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

    protected virtual int NrOfBattlesToFight => Battle.BattlesToBeFought(Game, Player).Count();

    protected virtual float MaxReinforcedDialTo(Player player, Territory to)
    {
        if (player == null || to == null) return 0;

        if (CanShip(player))
        {
            int specialForces = 0;
            int normalForces = 0;

            int opponentResources = player.Resources + (player.Ally == Faction.None ? 0 : player.AlliedPlayer.Resources);

            bool opponentMayUseWorthlessAsKarma = player.Faction == Faction.Blue && Game.Applicable(Rule.BlueWorthlessAsKarma);
            bool hasKarma = CardsPlayerHas(player).Any(c => c.Type == TreacheryCardType.Karma || (opponentMayUseWorthlessAsKarma && c.Type == TreacheryCardType.Karma));

            while (specialForces + 1 <= player.SpecialForcesInReserve && Shipment.DetermineCost(Game, player, normalForces + specialForces + 1, to.MiddleLocation, hasKarma, false, false, false) <= opponentResources)
            {
                specialForces++;
            }

            while (normalForces + 1 <= player.ForcesInReserve && Shipment.DetermineCost(Game, player, normalForces + 1 + specialForces, to.MiddleLocation, hasKarma, false, false, false) <= opponentResources)
            {
                normalForces++;
            }

            return specialForces * Battle.DetermineSpecialForceStrength(Game, player.Faction, Faction) + normalForces * Battle.DetermineNormalForceStrength(Game, player.Faction);
        }

        return 0;
    }

    #endregion

    #region BattleInformation_Leaders

    public static bool IsNoTraitorOf(Game game, Player p, IHero hero) => LeadersOfWhichTreacheryIsKnown(game, p).Contains(hero) && !p.Traitors.Contains(hero) && (p.Ally != Faction.Black || p.AlliedPlayer.Traitors.Contains(hero));

    protected static IEnumerable<IHero> LeadersOfWhichTreacheryIsKnown(Game game, Player p)
    {
        var knownBecauseCaptured = game.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && p.Faction == Faction.Black ? p.Leaders.Where(l => l.Faction != Faction.Black) : Array.Empty<Leader>();
        var knownByPlayer = p.Traitors.Union(p.KnownNonTraitors);
        var knownByAlly = p.HasAlly ? p.AlliedPlayer.Traitors.Union(p.AlliedPlayer.KnownNonTraitors) : Array.Empty<IHero>();
        var revealedOrToldTraitors = game.Players.SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors));

        return knownByPlayer.Union(knownByAlly).Union(revealedOrToldTraitors).Union(knownBecauseCaptured);
    }

    #endregion

    #region BattleInformation_WeaponsAndDefenses

    /// <summary>
    /// Todo: check if all possibilities of registring cards in Game are used.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>

    private IEnumerable<TreacheryCard> KnownCards(Player player) =>
        player == Player || player == AlliedPlayer ?
        player.TreacheryCards :
        player.TreacheryCards.Where(c => Game.KnownCards(Player).Contains(c) || Player.HasAlly && AlliedPlayer.IsBot && Game.KnownCards(AlliedPlayer).Contains(c));

    private IEnumerable<TreacheryCard> KnownCards(Faction player) => KnownCards(Game.GetPlayer(player));

    private IEnumerable<TreacheryCard> KnownWeapons(Player player) => KnownCards(Player).Where(c => c.IsWeapon);

    private IEnumerable<TreacheryCard> KnownDefenses(Player player) => KnownCards(Player).Where(c => c.IsDefense);

    private IEnumerable<TreacheryCard> PotentiaWeaponsInBattle(Player player) => Battle.ValidWeapons(Game, player, null, null, Game.CurrentBattle.Territory, false).Where(c => KnownCards(Player).Contains(c));

    private IEnumerable<TreacheryCard> PotentiaDefensesInBattle(Player player) => Battle.ValidDefenses(Game, player, null, Game.CurrentBattle.Territory, false).Where(c => KnownCards(Player).Contains(c));



    #endregion


    #region Battle

    protected float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, Territory territory, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable)
    {
        return DetermineDialShortageForBattle(dialNeeded, opponent, forcesAvailable, specialForcesAvailable, resourcesAvailable, territory, out _, out _, out _, out _);
    }

    protected float DetermineDialShortageForBattle(float dialNeeded, Faction opponent, int forcesAvailable, int specialForcesAvailable, int resourcesAvailable, Territory territory,
        out int forcesAtFullStrength, out int forcesAtHalfStrength, out int specialForcesAtFullStrength, out int specialForcesAtHalfStrength)
    {
        var normalStrength = Battle.DetermineNormalForceStrength(Game, Faction);
        var specialStrength = Battle.DetermineSpecialForceStrength(Game, Faction, opponent);
        int strongholdBonusToSupportingForces = Game.HasStrongholdAdvantage(Faction, StrongholdAdvantage.FreeResourcesForBattles, territory) ? 2 : 0;
        int resourcesLeftToSupportForces = resourcesAvailable + strongholdBonusToSupportingForces;
        int costPerForce = Battle.NormalForceCost(Game, Player);
        int costPerSpecialForce = Battle.SpecialForceCost(Game, Player);
        int numberOfForcesWithCunningBonus = Game.CurrentRedCunning != null && Game.CurrentRedCunning.Initiator == Faction ? 5 : 0;

        if (Battle.MustPayForForcesInBattle(Game, Player))
        {
            specialForcesAtFullStrength = 0;
            while (dialNeeded > normalStrength && specialForcesAvailable >= 1 && resourcesLeftToSupportForces >= costPerSpecialForce)
            {
                dialNeeded -= specialStrength;
                specialForcesAtFullStrength++;
                specialForcesAvailable--;
                resourcesLeftToSupportForces -= costPerSpecialForce;
            }

            forcesAtFullStrength = 0;
            while (dialNeeded >= normalStrength && forcesAvailable >= 1 && resourcesLeftToSupportForces >= costPerForce)
            {
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= specialStrength;
                }
                else
                {
                    dialNeeded -= normalStrength;
                }

                forcesAtFullStrength++;
                forcesAvailable--;
                resourcesLeftToSupportForces -= costPerForce;
            }

            specialForcesAtHalfStrength = 0;
            while (dialNeeded > 0 && specialForcesAvailable >= 1)
            {
                dialNeeded -= 0.5f * specialStrength;
                specialForcesAtHalfStrength++;
                specialForcesAvailable--;
            }

            forcesAtHalfStrength = 0;
            while (dialNeeded > 0 && forcesAvailable >= 1)
            {
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= 0.5f * specialStrength;
                }
                else
                {
                    dialNeeded -= 0.5f * normalStrength;
                }

                forcesAtHalfStrength++;
                forcesAvailable--;
            }
        }
        else
        {
            specialForcesAtFullStrength = 0;
            while (dialNeeded >= specialStrength && specialForcesAvailable >= 1 && resourcesLeftToSupportForces >= costPerSpecialForce)
            {
                dialNeeded -= specialStrength;
                specialForcesAtFullStrength++;
                specialForcesAvailable--;
                resourcesLeftToSupportForces -= costPerSpecialForce;
            }

            forcesAtFullStrength = 0;
            while (dialNeeded > 0 && forcesAvailable >= 1 && resourcesLeftToSupportForces >= costPerForce)
            {
                if (numberOfForcesWithCunningBonus > 0)
                {
                    numberOfForcesWithCunningBonus--;
                    dialNeeded -= specialStrength;
                }
                else
                {
                    dialNeeded -= normalStrength;
                }

                forcesAtFullStrength++;
                forcesAvailable--;
                resourcesLeftToSupportForces -= costPerForce;
            }

            while (dialNeeded > 0 && specialForcesAvailable >= 1 && resourcesLeftToSupportForces >= costPerSpecialForce)
            {
                dialNeeded -= specialStrength;
                specialForcesAtFullStrength++;
                specialForcesAvailable--;
                resourcesLeftToSupportForces -= costPerSpecialForce;
            }

            specialForcesAtHalfStrength = 0;
            forcesAtHalfStrength = 0;
        }

        return dialNeeded;
    }


    protected float ChanceOfMyLeaderSurviving(Player opponent, VoicePlan voicePlan, PrescienceAspect prescience, out TreacheryCard mostEffectiveDefense, TreacheryCard chosenWeapon)
    {
        if (!HeroesForBattle(opponent, true).Any())
        {
            mostEffectiveDefense = null;
            return 1;
        }

        if (voicePlan != null && voicePlan.defenseToUse != null)
        {
            mostEffectiveDefense = voicePlan.defenseToUse;
            return voicePlan.playerHeroWillCertainlySurvive ? 1 : 0.5f;
        }

        var availableDefenses = Defenses(chosenWeapon, Game.CurrentBattle?.Territory).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
            ).ToArray();

        if (chosenWeapon != null && chosenWeapon.IsArtillery)
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.IsShield);
            return 0;
        }

        if (chosenWeapon != null && chosenWeapon.IsRockmelter)
        {
            mostEffectiveDefense = null;
            return 0;
        }

        if (chosenWeapon != null && chosenWeapon.IsPoisonTooth)
        {
            mostEffectiveDefense = availableDefenses.FirstOrDefault(def => def.Type == TreacheryCardType.Chemistry);
            return 0;
        }

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
        if (prescience == PrescienceAspect.Weapon && opponentPlan != null)
        {
            if (opponentPlan.Weapon == null || opponentPlan.Weapon.Type == TreacheryCardType.Useless)
            {
                if (MayPlayNoDefense(chosenWeapon))
                {
                    mostEffectiveDefense = null;
                }
                else
                {
                    mostEffectiveDefense = Defenses(chosenWeapon, null).FirstOrDefault();
                }

                return 1;
            }
            else
            {
                mostEffectiveDefense = availableDefenses.FirstOrDefault(d => opponentPlan.Weapon.CounteredBy(d, chosenWeapon));
                return mostEffectiveDefense != null ? 1 : 0;
            }
        }

        var myClairvoyance = MyClairVoyanceAboutEnemyWeaponInCurrentBattle;
        if (myClairvoyance != null)
        {
            LogInfo("Clairvoyance detected!");

            if (myClairvoyance.Question.IsAbout(TreacheryCardType.Projectile))
            {
                if (myClairvoyance.Answer.IsYes)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsProjectileDefense);
                    return (mostEffectiveDefense != null) ? 1 : 0;
                }
                else if (myClairvoyance.Answer.IsNo)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                    if (mostEffectiveDefense != null) return 0.5f;
                }
            }

            if (myClairvoyance.Question.IsAbout(TreacheryCardType.Poison))
            {
                if (myClairvoyance.Answer.IsYes)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsPoisonDefense);
                    return (mostEffectiveDefense != null) ? 1 : 0;
                }
                else if (myClairvoyance.Answer.IsNo)
                {
                    mostEffectiveDefense = availableDefenses.FirstOrDefault(d => d.IsProjectileDefense);
                    if (mostEffectiveDefense != null) return 0.5f;
                }
            }
        }

        return DetermineBestDefense(opponent, chosenWeapon, out mostEffectiveDefense);
    }

    private int NrOfUnknownOpponentCards(Player opponent)
    {
        return opponent.TreacheryCards.Count(c => !Game.KnownCards(this).Contains(c));
    }

    private float DetermineBestDefense(Player opponent, TreacheryCard chosenWeapon, out TreacheryCard mostEffectiveDefense)
    {
        var knownEnemyWeapons = KnownOpponentWeapons(opponent).ToArray();
        var availableDefenses = Defenses(chosenWeapon, null).Where(def =>
            def != chosenWeapon &&
            (chosenWeapon == null || !(chosenWeapon.IsLaser && def.IsShield)) &&
            (def.Type != TreacheryCardType.WeirdingWay || chosenWeapon != null) &&
            def.Type != TreacheryCardType.Useless
            ).ToArray();

        var defenseQuality = new ObjectCounter<TreacheryCard>();

        var unknownCards = CardsUnknownToMe;

        var bestDefenseAgainstUnknownCards = availableDefenses
                .LowestOrDefault(def => NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, def, chosenWeapon));

        foreach (var def in availableDefenses)
        {
            defenseQuality.Count(def);

            if (def == bestDefenseAgainstUnknownCards)
            {
                defenseQuality.Count(def);
            }

            foreach (var knownWeapon in knownEnemyWeapons)
            {
                if (knownWeapon.CounteredBy(def, chosenWeapon))
                {
                    LogInfo("potentialWeapon " + knownWeapon + " is countered by " + def);
                    defenseQuality.Count2(def);
                }
            }
        }

        mostEffectiveDefense = defenseQuality.Highest;

        var defenseToCheck = mostEffectiveDefense;

        if (mostEffectiveDefense == null && knownEnemyWeapons.Any() || knownEnemyWeapons.Any(w => !w.CounteredBy(defenseToCheck, chosenWeapon))) return 0;

        return 1 - ChanceOfAnUnknownOpponentCardKillingMyLeader(unknownCards, mostEffectiveDefense, opponent, chosenWeapon);
    }

    private float ChanceOfAnUnknownOpponentCardKillingMyLeader(List<TreacheryCard> unknownCards, TreacheryCard usedDefense, Player opponent, TreacheryCard chosenWeapon)
    {
        if (unknownCards.Count == 0) return 0;
        float numberOfUnknownWeaponsThatCouldKillMeWithThisDefense = NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(unknownCards, usedDefense, chosenWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownWeaponsThatCouldKillMeWithThisDefense, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private float ChanceOfAnUnknownOpponentCardSavingHisLeader(List<TreacheryCard> unknownCards, TreacheryCard usedWeapon, Player opponent)
    {
        if (unknownCards.Count == 0) return 0;
        float numberOfUnknownDefensesThatCouldCounterThisWeapon = NumberOfUnknownDefensesThatCouldCounterThisWeapon(unknownCards, usedWeapon);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var result = 1 - (float)CumulativeChance(unknownCards.Count - numberOfUnknownDefensesThatCouldCounterThisWeapon, unknownCards.Count, nrOfUnknownOpponentCards);

        return result;
    }

    private double CumulativeChance(double a, double b, int length)
    {
        double result = 1;
        for (int i = 0; i < length; i++)
        {
            result *= (a - i) / (b - i);
        }
        return result;
    }

    private float NumberOfUnknownWeaponsThatCouldKillMeWithThisDefense(IEnumerable<TreacheryCard> unknownCards, TreacheryCard defense, TreacheryCard chosenWeapon)
    {
        return unknownCards.Count(c => c.IsWeapon && (defense == null || !c.CounteredBy(defense, chosenWeapon)));
    }

    private float NumberOfUnknownDefensesThatCouldCounterThisWeapon(IEnumerable<TreacheryCard> unknownCards, TreacheryCard weapon)
    {
        return unknownCards.Count(c => c.IsDefense && (weapon == null || weapon.CounteredBy(c, null)));
    }

    private ClairVoyanceQandA RulingWeaponClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsWeaponInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    private ClairVoyanceQandA RulingDefenseClairvoyanceForThisBattle => Game.LatestClairvoyance != null && Game.LatestClairvoyanceQandA != null && Game.LatestClairvoyanceQandA.Answer.Initiator == Faction && Game.LatestClairvoyanceBattle != null && Game.LatestClairvoyanceBattle == Game.CurrentBattle &&
        (Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeAsDefenseInBattle || Game.LatestClairvoyanceQandA.Question.Question == ClairvoyanceQuestion.CardTypeInBattle) ? Game.LatestClairvoyanceQandA : null;

    public Homeworld PrimaryHomeworld => Homeworlds.First();



    protected float ChanceOfEnemyLeaderDying(Player opponent, VoicePlan voicePlan, PrescienceAspect prescience, out TreacheryCard mostEffectiveWeapon, out bool enemyCanDefendPoisonTooth)
    {
        enemyCanDefendPoisonTooth = false;

        if (!HeroesForBattle(opponent, true).Any())
        {
            LogInfo("Opponent has no leaders");
            mostEffectiveWeapon = null;
            return 0f;
        }

        if (voicePlan != null && voicePlan.weaponToUse != null)
        {
            mostEffectiveWeapon = voicePlan.weaponToUse;
            return voicePlan.opponentHeroWillCertainlyBeZero ? 1 : 0.5f;
        }

        var availableWeapons = Weapons(null, null, null).Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth && w.Type != TreacheryCardType.Rockmelter)
            .OrderBy(w => NumberOfUnknownDefensesThatCouldCounterThisWeapon(CardsUnknownToMe, w)).ToArray();

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);

        var opponentMayBeUsingAWeapon = true;

        //Prescience available?
        if (prescience != PrescienceAspect.None && opponentPlan != null)
        {
            if (prescience == PrescienceAspect.Defense)
            {
                enemyCanDefendPoisonTooth = opponentPlan.Defense != null && opponentPlan.Defense.IsNonAntidotePoisonDefense;
                mostEffectiveWeapon = availableWeapons.FirstOrDefault(w => opponentPlan.Defense == null || !w.CounteredBy(opponentPlan.Defense, null));

                return mostEffectiveWeapon != null ? 1f : 0f;
            }

            if (prescience == PrescienceAspect.Weapon && (opponentPlan.Weapon == null || opponentPlan.Weapon.IsUseless))
            {
                opponentMayBeUsingAWeapon = false;
            }
        }

        var usefulWeapons = opponentMayBeUsingAWeapon ? availableWeapons : availableWeapons.Where(w => w.Type != TreacheryCardType.MirrorWeapon).ToArray();

        var knownEnemyDefenses = KnownOpponentDefenses(opponent);

        //Clairvoyance available?
        var myClairvoyance = MyClairVoyanceAboutEnemyDefenseInCurrentBattle;
        if (myClairvoyance != null)
        {
            if (myClairvoyance.Question.IsAbout(TreacheryCardType.ProjectileDefense))
            {
                if (Game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (Game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
            else if (myClairvoyance.Question.IsAbout(TreacheryCardType.PoisonDefense))
            {
                if (Game.LatestClairvoyanceQandA.Answer.IsNo)
                {
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsPoisonWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
                else if (Game.LatestClairvoyanceQandA.Answer.IsYes)
                {
                    enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);
                    mostEffectiveWeapon = usefulWeapons.FirstOrDefault(d => d.IsProjectileWeapon);
                    if (mostEffectiveWeapon != null) return 1f;
                }
            }
        }

        var unknownOpponentCards = OpponentCardsUnknownToMe(opponent);

        mostEffectiveWeapon = usefulWeapons.Where(w => !knownEnemyDefenses.Any(defense => w.CounteredBy(defense, null))).RandomOrDefault();

        if (mostEffectiveWeapon != null)
        {
            if (mostEffectiveWeapon.IsMirrorWeapon)
            {
                if (prescience == PrescienceAspect.Weapon && opponentPlan != null)
                {
                    return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(unknownOpponentCards, opponentPlan.Weapon, opponent);
                }
            }
            else if (!unknownOpponentCards.Any())
            {
                return 1f;
            }
            else
            {
                return 1f - ChanceOfAnUnknownOpponentCardSavingHisLeader(unknownOpponentCards, mostEffectiveWeapon, opponent);
            }
        }

        mostEffectiveWeapon = usefulWeapons.Where(w => !IsKnownToOpponent(opponent, w)).RandomOrDefault();

        if (mostEffectiveWeapon != null)
        {
            return 0.5f;
        }

        enemyCanDefendPoisonTooth = knownEnemyDefenses.Any(c => c.IsNonAntidotePoisonDefense);

        return 0f;
    }

    private bool IsAllowedWithClairvoyance(ClairVoyanceQandA clairvoyance, TreacheryCard toUse, bool asWeapon)
    {
        bool inScope = toUse != null && ClairVoyancePlayed.IsInScopeOf(asWeapon, toUse.Type, (TreacheryCardType)clairvoyance.Question.Parameter1);

        var answer = clairvoyance == null ||
                clairvoyance.Answer.Answer == ClairVoyanceAnswer.Unknown ||
                clairvoyance.Answer.Answer == ClairVoyanceAnswer.Yes && toUse != null && inScope ||
                clairvoyance.Answer.Answer == ClairVoyanceAnswer.No && (toUse == null || !inScope);

        //LogInfo("IsAllowedWithClairvoyance(): in scope: {0}, answer: {1}.", inScope, answer);

        return answer;
    }

    protected IEnumerable<IHero> HeroesForBattle(Player player, bool includeInFrontOfShield) => Battle.ValidBattleHeroes(Game, player).Where(l => includeInFrontOfShield || !Game.IsInFrontOfShield(l));

    protected int SelectHeroForBattle(Player opponent, bool highest, bool forfeit, bool messiahUsed, TreacheryCard weapon, TreacheryCard defense, out IHero hero, out bool isTraitor, bool includeInFrontOfShield = false)
    {
        isTraitor = false;

        var ally = Ally != Faction.None ? AlliedPlayer : null;
        var purple = Game.GetPlayer(Faction.Purple);

        var knownNonTraitorsByAlly = ally != null ? ally.Traitors.Union(ally.KnownNonTraitors) : Array.Empty<IHero>();
        var revealedOrToldTraitorsByNonOpponents = Game.Players.Where(p => p != opponent && (p.Ally != Faction.Black || p.Faction != opponent.Ally)).SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors));
        var toldNonTraitorsByOpponent = opponent.Ally != Faction.Black ? opponent.ToldNonTraitors.AsEnumerable() : Array.Empty<IHero>();
        var knownNonTraitors = Traitors.Union(KnownNonTraitors).Union(knownNonTraitorsByAlly).Union(revealedOrToldTraitorsByNonOpponents).Union(toldNonTraitorsByOpponent);

        var knownTraitorsForOpponentsInBattle = Game.Players.
            Where(p => p == opponent || (p.Faction == Faction.Black && p.Faction == opponent.Ally)).SelectMany(p => p.RevealedTraitors.Union(p.ToldTraitors))
            .Union(Leaders.Where(l => Game.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && l.Faction == opponent.Faction));

        var hasUnknownTraitorsThatMightBeMine = Game.Players.
            Where(p => p != opponent && (p.Ally != Faction.Black || p.Faction != opponent.Ally)).
            SelectMany(p => p.Traitors.Where(l => !knownNonTraitors.Contains(l))).Any();

        var highestOpponentLeader = HeroesForBattle(opponent, true).OrderByDescending(l => l.Value).FirstOrDefault();
        var safeLeaders = HeroesForBattle(this, includeInFrontOfShield).Where(l => messiahUsed || !hasUnknownTraitorsThatMightBeMine || knownNonTraitors.Contains(l));

        IHero safeHero = null;
        IHero unsafeHero = null;

        if (forfeit)
        {
            safeHero = null;
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).Where(l => !safeLeaders.Contains(l)).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
            if (unsafeHero == null) unsafeHero = HeroesForBattle(this, includeInFrontOfShield).LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else if (highest)
        {
            safeHero = safeLeaders.HighestOrDefault(l => l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).HighestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
        }
        else
        {
            safeHero = safeLeaders.LowestOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader));
            unsafeHero = HeroesForBattle(this, includeInFrontOfShield).OneOfLowestNOrDefault(l => l.HeroType == HeroType.Auditor ? 10 : l.ValueInCombatAgainst(highestOpponentLeader), 2);
        }

        if (safeHero == null ||
            opponent.Faction != Faction.Black && !knownTraitorsForOpponentsInBattle.Contains(unsafeHero) && safeHero.ValueInCombatAgainst(highestOpponentLeader) < unsafeHero.ValueInCombatAgainst(highestOpponentLeader) - 2)
        {
            hero = unsafeHero;
        }
        else
        {
            hero = safeHero;
        }

        isTraitor = !messiahUsed && knownTraitorsForOpponentsInBattle.Contains(hero);

        var usedSkill = LeaderSkill.None;
        return hero != null ? hero.ValueInCombatAgainst(highestOpponentLeader) + Battle.DetermineSkillBonus(Game, this, hero, weapon, defense, Resources > 3 ? 3 : 0, ref usedSkill) : 0;
    }

    protected float GetDialNeeded(Player inBattle, Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
    {
        //var opponent = GetOpponentThatOccupies(territory);
        var voicePlan = Voice.MayUseVoice(Game, inBattle) ? inBattle.BestVoice(null, inBattle, opponent) : null;
        var strength = inBattle.MaxDial(opponent, territory, inBattle);
        var prescience = Prescience.MayUsePrescience(Game, inBattle) ? inBattle.BestPrescience(opponent, strength, PrescienceAspect.None, territory) : PrescienceAspect.None;

        //More could be done with the information obtained in the below call
        return GetDialNeededForBattle(inBattle, inBattle.IWillBeAggressorAgainst(opponent), opponent, territory, voicePlan, prescience, takeReinforcementsIntoAccount, true, out _, out _, out _, out _, out _, out _, out _, out _, out _, out _);
    }

    protected float GetDialNeeded(Territory territory, Player opponent, bool takeReinforcementsIntoAccount)
    {
        //More could be done with the information obtained in the below call
        return GetDialNeeded(this, territory, opponent, takeReinforcementsIntoAccount);
    }

    protected float GetDialNeededForBattle(
        Player inBattle, bool iAmAggressor, Player opponent, Territory territory, VoicePlan voicePlan, PrescienceAspect prescience, bool takeReinforcementsIntoAccount, bool includeInFrontOfShield,
        out TreacheryCard bestDefense, out TreacheryCard bestWeapon, out IHero hero, out bool messiah, out bool isTraitor, out bool lasgunShieldDetected, out bool stoneBurnerDetected, out int bankerBoost,
        out float chanceOfMyHeroSurviving, out float chanceOfEnemyHeroSurviving)
    {

        chanceOfEnemyHeroSurviving = 1 - inBattle.ChanceOfEnemyLeaderDying(opponent, voicePlan, prescience, out bestWeapon, out bool enemyCanDefendPoisonTooth);

        LogInfo("Chance of enemy hero surviving: {0} with {1}", chanceOfEnemyHeroSurviving, bestWeapon);

        chanceOfMyHeroSurviving = inBattle.ChanceOfMyLeaderSurviving(opponent, voicePlan, prescience, out bestDefense, bestWeapon);

        inBattle.UseDestructiveWeaponIfApplicable(enemyCanDefendPoisonTooth, ref chanceOfMyHeroSurviving, ref chanceOfEnemyHeroSurviving, ref bestDefense, ref bestWeapon);

        bool iAssumeMyLeaderWillDie = chanceOfMyHeroSurviving < inBattle.Param.Battle_MimimumChanceToAssumeMyLeaderSurvives;
        bool iAssumeEnemyLeaderWillDie = chanceOfEnemyHeroSurviving < inBattle.Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives;

        var opponentPlan = Game.CurrentBattle?.PlanOf(opponent);
        lasgunShieldDetected = HasLasgunShield(
            bestWeapon,
            bestDefense,
            (prescience == PrescienceAspect.Weapon && opponentPlan != null) ? opponentPlan.Weapon : null,
            (prescience == PrescienceAspect.Defense && opponentPlan != null) ? opponentPlan.Defense : null);

        stoneBurnerDetected = bestWeapon != null && bestWeapon.IsRockmelter || prescience == PrescienceAspect.Weapon && opponentPlan != null && opponentPlan.Weapon != null && opponentPlan.Weapon.IsRockmelter;

        int myMessiahBonus = 0;
        if (Battle.MessiahAvailableForBattle(Game, inBattle) && !lasgunShieldDetected)
        {
            messiah = true;
            myMessiahBonus = 2;
        }
        else
        {
            messiah = false;
        }

        isTraitor = false;
        int myHeroValue = inBattle.SelectHeroForBattle(opponent, !lasgunShieldDetected && !iAssumeMyLeaderWillDie, false, messiah, bestWeapon, bestDefense, out hero, out isTraitor, includeInFrontOfShield);

        var usedSkill = LeaderSkill.None;
        var opponentPenalty = Battle.DetermineSkillPenalty(Game, hero, opponent, ref usedSkill);

        bankerBoost = 0;
        if (hero == null)
        {
            messiah = false;
            myMessiahBonus = 0;
        }

        if (inBattle.CanOnlyUseTraitorsOrFacedancers(opponent))
        {
            LogInfo("Opponent leader only has traitors or facedancers to use!");
            bestWeapon = null;
            bestDefense = null;
            chanceOfMyHeroSurviving = 1;
            iAssumeMyLeaderWillDie = false;
            chanceOfEnemyHeroSurviving = 0;
            iAssumeEnemyLeaderWillDie = true;
            return 0.5f;
        }
        else if (isTraitor)
        {
            LogInfo("My leader is a traitor: chanceOfMyHeroSurviving = 0");
            bestWeapon = null;
            bestDefense = null;
            chanceOfMyHeroSurviving = 0;
            iAssumeMyLeaderWillDie = true;
            chanceOfEnemyHeroSurviving = 1;
            iAssumeEnemyLeaderWillDie = false;
            return 99;
        }
        else if (lasgunShieldDetected)
        {
            LogInfo("Lasgun/Shield detected!");

            if (bestWeapon != null && !bestWeapon.IsLaser && inBattle.MayPlayNoWeapon(bestDefense))
            {
                bestWeapon = null;
            }

            chanceOfMyHeroSurviving = 0;
            iAssumeMyLeaderWillDie = true;
            chanceOfEnemyHeroSurviving = 0;
            iAssumeEnemyLeaderWillDie = true;
            return 0.5f;
        }

        if (Game.SkilledAs(hero, LeaderSkill.Banker) && !iAssumeMyLeaderWillDie)
        {
            bankerBoost = Math.Min(inBattle.Resources, 3);
        }

        if (hero is TreacheryCard && bestDefense != null && !bestDefense.IsUseless && inBattle.MayPlayNoDefense(bestWeapon))
        {
            bestDefense = null;
        }

        LogInfo("Chance of my hero surviving: {0} with {1} (my weapon: {2})", chanceOfMyHeroSurviving, bestDefense, bestWeapon);

        var myHeroToFightAgainst = hero;
        var opponentLeader = (prescience == PrescienceAspect.Leader && opponentPlan != null) ? opponentPlan.Hero : inBattle.HeroesForBattle(opponent, true).OrderByDescending(l => l.ValueInCombatAgainst(myHeroToFightAgainst)).FirstOrDefault(l => !inBattle.Traitors.Contains(l));
        int opponentLeaderValue = opponentLeader == null ? 0 : opponentLeader.ValueInCombatAgainst(hero);
        int opponentMessiahBonus = Battle.MessiahAvailableForBattle(Game, opponent) ? 2 : 0;
        int maxReinforcements = takeReinforcementsIntoAccount ? (int)Math.Ceiling(inBattle.MaxReinforcedDialTo(opponent, territory)) : 0;
        var opponentDial = (prescience == PrescienceAspect.Dial && opponentPlan != null) ? opponentPlan.Dial(Game, inBattle.Faction) : inBattle.MaxDial(opponent, territory, inBattle);
        int myHomeworldBonus = GetHomeworldBattleContributionAndLasgunShieldLimit(territory);
        int opponentHomeworldBonus = opponent.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);

        var result =
            opponentDial +
            opponentHomeworldBonus +
            maxReinforcements +
            (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
            (iAmAggressor ? 0 : 0.5f) -
            (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus) -
            myHomeworldBonus;


        if (inBattle.MaxDial(inBattle, territory, opponent) - result >= 5)
        {
            //I think I only need a small fraction of available forces. Am I really sure amout this?

            if (!iAssumeMyLeaderWillDie && chanceOfMyHeroSurviving < 0.8)
            {
                iAssumeMyLeaderWillDie = true;
            }

            if (iAssumeEnemyLeaderWillDie && chanceOfEnemyHeroSurviving > 0.1)
            {
                iAssumeEnemyLeaderWillDie = false;
            }

            result =
                opponentDial +
                maxReinforcements +
                (iAssumeEnemyLeaderWillDie ? 0 : 1) * (opponentLeaderValue + opponentMessiahBonus) +
                (iAmAggressor ? 0 : 0.5f) -
                (iAssumeMyLeaderWillDie ? 0 : 1) * (myHeroValue + opponentPenalty + myMessiahBonus);
        }

        LogInfo("{13}/{14}: opponentDial ({0}) + maxReinforcements ({8}) + (chanceOfEnemyHeroSurviving ({7}) < Battle_MimimumChanceToAssumeEnemyHeroSurvives ({10}) ? 0 : 1) * (highestleader ({1}) + messiahbonus ({2})) + defenderpenalty ({3}) - (chanceOfMyHeroSurviving ({4}) < Battle_MimimumChanceToAssumeMyLeaderSurvives ({11}) ? 0 : 1) * (myHeroValue ({5}) + messiahbonus ({9}) + bankerBoost ({12}) => *{6}*)",
            opponentDial,
            opponentLeaderValue,
            opponentMessiahBonus,
            (iAmAggressor ? 0 : 0.5f),
            chanceOfMyHeroSurviving,
            myHeroValue,
            result,
            chanceOfEnemyHeroSurviving,
            maxReinforcements,
            myMessiahBonus,
            inBattle.Param.Battle_MimimumChanceToAssumeEnemyHeroSurvives,
            inBattle.Param.Battle_MimimumChanceToAssumeMyLeaderSurvives,
            bankerBoost,
            territory,
            opponent.Faction);

        return result;
    }

    private bool CanOnlyUseTraitorsOrFacedancers(Player player)
    {
        return player.Leaders.All(l =>
            !player.MessiahAvailable && Traitors.Contains(l) ||
            FaceDancers.Contains(l) ||
            !player.MessiahAvailable && Ally == Faction.Black && AlliedPlayer.Traitors.Contains(l) ||
            Ally == Faction.Purple && AlliedPlayer.FaceDancers.Contains(l));
    }


    private bool HasLasgunShield(TreacheryCard myWeapon, TreacheryCard myDefense, TreacheryCard enemyWeapon, TreacheryCard enemyDefense)
    {
        return
            (myWeapon != null && myWeapon.IsLaser && enemyDefense != null && enemyDefense.IsShield) ||
            (enemyWeapon != null && enemyWeapon.IsLaser && myDefense != null && myDefense.IsShield);
    }

    #endregion Battle

    */

    /*
protected bool MayPlayNoWeapon(Player player, TreacheryCard usingThisDefense) => Battle.ValidWeapons(Game, player, usingThisDefense, null, null, true).Contains(null);

protected bool MayPlayNoDefense(Player player, TreacheryCard usingThisWeapon) => Battle.ValidDefenses(Game, player, usingThisWeapon, null, true).Contains(null);
*/

}