/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

public partial class ClassicBot
{
    #region Orange

    private SetShipmentPermission? DetermineSetShipmentPermission()
    {
        if (Game.CurrentPhase != Phase.BeginningOfShipAndMove) 
            return null;
        
        var toDeny = Game.ShipmentPermissions.Keys.Where(IsWinningOpponent).ToArray();
        if (toDeny.Length != 0) return new SetShipmentPermission(Game, Faction) { Factions = toDeny.ToArray(), Permission = ShipmentPermission.None };

        var discountPermission = Resources > 10 || WinningOpponentsIWishToAttack(99, true).Any() ? ShipmentPermission.OrangeRate : ShipmentPermission.None;
        var permission = ShipmentPermission.Cross | ShipmentPermission.ToHomeworld | discountPermission;
        var toAllow = SetShipmentPermission.ValidTargets(Game, Player)
            .Where(f => !IsWinningOpponent(f) && (!Game.ShipmentPermissions.TryGetValue(f, out var currentPermission) || currentPermission != permission))
            .ToArray();
        return toAllow.Length != 0 
            ? new SetShipmentPermission(Game, Faction) { Factions = toAllow.ToArray(), Permission = permission } 
            : null;
    }

    #endregion

    #region Red

    private RedDiscarded? DetermineRedDiscarded()
    {
        if (Player is not { Resources: >= 10, HasRoomForCards: false }) return null;
        
        var worstCard = Player.TreacheryCards.OrderBy(c => CardQuality(c, null)).First();

        return CardQuality(worstCard, null) <= 1 ? new RedDiscarded(Game, Faction) { Card = worstCard } : null;
    }

    #endregion

    #region Grey

    protected virtual ReplacedCardWon DetermineReplacedCardWon()
    {
        var replace = Game.CardJustWon != null && CardQuality(Game.CardJustWon, Player) <= 2;
        return new ReplacedCardWon(Game, Faction) { Passed = !replace };
    }

    private GreyRemovedCardFromAuction DetermineGreyRemovedCardFromAuction()
    {
        TreacheryCard? toBeRemoved = null;
        if (Player.TreacheryCards.Any(c => c.Type == TreacheryCardType.ProjectileAndPoison)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.Type == TreacheryCardType.ShieldAndAntidote);
        if (toBeRemoved == null && Player.TreacheryCards.All(c => c.Type != TreacheryCardType.ShieldAndAntidote)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.Type == TreacheryCardType.ProjectileAndPoison);
        if (toBeRemoved == null && Player.TreacheryCards.Any(c => c.IsProjectileWeapon)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsProjectileDefense);
        if (toBeRemoved == null && Player.TreacheryCards.Any(c => c.IsPoisonWeapon)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsPoisonDefense);
        if (toBeRemoved == null && !Player.TreacheryCards.Any(c => c.IsProjectileDefense)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsProjectileWeapon);
        if (toBeRemoved == null && !Player.TreacheryCards.Any(c => c.IsPoisonDefense)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsPoisonWeapon);

        if (toBeRemoved == null)
        {
            if (Player.TreacheryCards.Count >= 3 || ResourcesIncludingAllyAndRedContribution < 5)
                //Remove the best card from auction
                toBeRemoved = Game.CardsOnAuction.Items.HighestOrDefault(c => CardQuality(c, Player));
            else
                //Remove the worst card from auction
                toBeRemoved = Game.CardsOnAuction.Items.LowestOrDefault(c => CardQuality(c, Player));
        }

        toBeRemoved ??= Game.CardsOnAuction.Items.First();

        var putOnTop = (CardQuality(toBeRemoved, Player) <= 2 && Ally == Faction.None) || (CardQuality(toBeRemoved, Player) >= 4 && Ally != Faction.None);

        return new GreyRemovedCardFromAuction(Game, Faction) { Card = toBeRemoved, PutOnTop = putOnTop };
    }

    private GreySwappedCardOnBid DetermineGreySwappedCardOnBid()
    {
        var card = Player.TreacheryCards.LowestOrDefault(c => CardQuality(c, Player));

        if (card != null && CardQuality(card, Player) <= 2)
            return new GreySwappedCardOnBid(Game, Faction) { Passed = false, Card = card };
        return new GreySwappedCardOnBid(Game, Faction) { Passed = true };
    }

    private GreySelectedStartingCard DetermineGreySelectedStartingCard()
    {
        var cards = Game.StartingTreacheryCards.Items;

        var card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.ProjectileAndPoison);
        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.ShieldAndAntidote);
        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.Laser);

        if (card == null && cards.Any(c => c.IsPoisonWeapon) && !cards.Any(c => c.IsPoisonDefense)) card = cards.FirstOrDefault(c => c.IsPoisonWeapon);

        if (card == null && cards.Any(c => c.IsProjectileWeapon) && !cards.Any(c => c.IsProjectileDefense)) card = cards.FirstOrDefault(c => c.IsProjectileWeapon);

        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.WeirdingWay);
        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.Chemistry);
        card ??= cards.FirstOrDefault(c => c.IsProjectileWeapon);
        card ??= cards.FirstOrDefault(c => c.IsPoisonWeapon);
        card ??= cards.FirstOrDefault(c => c.IsProjectileDefense);
        card ??= cards.FirstOrDefault(c => c.IsPoisonDefense);
        card ??= cards.FirstOrDefault(c => c.Type != TreacheryCardType.Useless);
        card ??= cards.FirstOrDefault();

        return new GreySelectedStartingCard(Game, Faction) { Card = card };
    }

    private PerformHmsMovement DeterminePerformHmsMovement()
    {
        var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;

        var richestAdjacentSpiceLocation = PerformHmsMovement.ValidLocations(Game).Where(l => !Equals(l, currentLocation) && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
        if (richestAdjacentSpiceLocation != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = richestAdjacentSpiceLocation };

        var reachableFromCurrentLocation = Game.Map.FindNeighbours(currentLocation, Game.HmsMovesLeft, false, Faction, Game, false);

        var richestReachableSpiceLocation = reachableFromCurrentLocation.Where(l => !Equals(l, currentLocation) && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
        if (richestReachableSpiceLocation != null)
        {
            var nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => WithinDistance(l, richestReachableSpiceLocation, 1));

            if (nextStepTowardsSpice != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsSpice };

            nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => WithinDistance(l, richestReachableSpiceLocation, 2));
            return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsSpice };
        }

        //If there is nowhere to go, move toward Imperial Basin or Polar Sink
        Location? alternativeLocation;
        if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
            alternativeLocation = Game.Map.ImperialBasin.MiddleLocation;
        else
            alternativeLocation = Game.Map.PolarSink;

        if (!Equals(alternativeLocation, currentLocation))
        {
            var nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => Equals(l, alternativeLocation));
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };

            nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => WithinDistance(l, alternativeLocation, 1));
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };

            nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => WithinDistance(l, alternativeLocation, 2));
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };
        }

        return new PerformHmsMovement(Game, Faction) { Passed = true };
    }

    private PerformHmsPlacement DeterminePerformHmsPlacement()
    {
        if (Faction != Faction.Grey)
            return new PerformHmsPlacement(Game, Faction) { Target = Game.Map.ShieldWall.Locations.First() };
        
        if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
            return new PerformHmsPlacement(Game, Faction) { Target = PerformHmsPlacement.ValidLocations(Game, Player).First(l => l.Territory == Game.Map.ImperialBasin) };
        
        return new PerformHmsPlacement(Game, Faction) { Target = Game.Map.PolarSink };
    }

    #endregion Grey

    #region Yellow

    private TakeLosses DetermineTakeLosses()
    {
        var normalForces = Math.Min(TakeLosses.LossesToTake(Game).Amount, TakeLosses.ValidMaxForceAmount(Game, Player));
        var specialForces = TakeLosses.LossesToTake(Game).Amount - normalForces;
        var useUseless = TakeLosses.CanPreventLosses(Game, Player);
        return new TakeLosses(Game, Faction) { ForceAmount = normalForces, SpecialForceAmount = specialForces, UseUselessCard = useUseless };
    }

    private PerformYellowSetup DeterminePerformYellowSetup()
    {
        var forceLocations = new Dictionary<Location, Battalion>
        {
            { Game.Map.FalseWallSouth.MiddleLocation, new Battalion(Faction, 3 + D(1, 4), Player.SpecialForcesInReserve > 0 ? 1 : 0, Game.Map.FalseWallSouth.MiddleLocation) }
        };

        var forcesLeft = 10 - forceLocations.Sum(kvp => kvp.Value.TotalAmountOfForces);

        if (forcesLeft > 0)
        {
            var amountOfSpecialForces = Player.SpecialForcesInReserve > 1 ? 1 : 0;
            forceLocations.Add(Game.Map.FalseWallWest.MiddleLocation, new Battalion(Faction, forcesLeft - amountOfSpecialForces, amountOfSpecialForces, Game.Map.FalseWallWest.MiddleLocation));
        }

        return new PerformYellowSetup(Game, Faction) { ForceLocations = forceLocations };
    }

    private YellowSentMonster DetermineYellowSentMonster()
    {
        var target = YellowSentMonster.ValidTargets(Game).HighestOrDefault(t => TotalMaxDialOfOpponents(t) + 2 * Player.AnyForcesIn(t));
        return new YellowSentMonster(Game, Faction) { Territory = target };
    }

    private YellowRidesMonster DetermineYellowRidesMonster()
    {
        Location? target = null;
        var validLocations = YellowRidesMonster.ValidTargets(Game, Player).ToList();
        var battalionsToMove = YellowRidesMonster.ValidSources(Game).ToDictionary(l => l, l => Player.BattalionIn(l));
        var forcesFromReserves = Math.Min(YellowRidesMonster.MaxForcesFromReserves(Game, Player, false), 4);
        var specialForcesFromReserves = Math.Min(YellowRidesMonster.MaxForcesFromReserves(Game, Player, true), 2);

        if (validLocations.Contains(Game.Map.TueksSietch) && VacantAndSafeFromStorm(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
        if (target == null && validLocations.Contains(Game.Map.Carthag) && VacantAndSafeFromStorm(Game.Map.Carthag)) target = Game.Map.Carthag;
        if (target == null && validLocations.Contains(Game.Map.Arrakeen) && VacantAndSafeFromStorm(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
        if (target == null && validLocations.Contains(Game.Map.HabbanyaSietch) && VacantAndSafeFromStorm(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;

        target ??= Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key)).HighestOrDefault(r => r.Value).Key;

        if (target == null)
        {
            var strength = battalionsToMove.Sum(forcesAtLocation => forcesAtLocation.Value.AmountOfForces + forcesAtLocation.Value.AmountOfSpecialForces * 2) + forcesFromReserves + specialForcesFromReserves * 2;

            target ??= validLocations.Where(l => Game.ResourcesOnPlanet.ContainsKey(l) && TotalMaxDialOfOpponents(l.Territory) < strength).HighestOrDefault(l => Game.ResourcesOnPlanet[l]);
            target ??= validLocations.Where(l => !Equals(l, Game.Map.SietchTabr) && l.IsStronghold && TotalMaxDialOfOpponents(l.Territory) < strength).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
        }

        target ??= Game.Map.PolarSink;

        if (target != null && battalionsToMove.Values.Sum(b => b.TotalAmountOfForces) + forcesFromReserves + specialForcesFromReserves > 0)
            return new YellowRidesMonster(Game, Faction) { Passed = false, ForceLocations = new Dictionary<Location, Battalion>(battalionsToMove), To = target, ForcesFromReserves = forcesFromReserves, SpecialForcesFromReserves = specialForcesFromReserves };
        return new YellowRidesMonster(Game, Faction) { Passed = true };
    }

    #endregion Yellow

    #region Blue

    private PerformBluePlacement DetermineBluePlacement()
    {
        Location target;
        if (Player.AnyForcesIn(Game.Map.Arrakeen) == 0) target = Game.Map.Carthag;
        else if (Player.AnyForcesIn(Game.Map.Carthag) == 0) target = Game.Map.Arrakeen;
        else if (Player.AnyForcesIn(Game.Map.HabbanyaSietch) == 0) target = Game.Map.HabbanyaSietch;
        else target = Game.Map.Carthag;

        return new PerformBluePlacement(Game, Faction) { Target = target };
    }

    private BlueFlip DetermineBlueFlip()
    {
        var territory = BlueFlip.GetTerritory(Game);

        if (IWantToBeFightersIn(territory))
            return new BlueFlip(Game, Faction) { AsAdvisors = false };
        return new BlueFlip(Game, Faction) { AsAdvisors = true };
    }

    private bool IWantToBeFightersIn(Territory territory)
    {
        if (!territory.IsStronghold) return false;

        if (IAmWinning) return true;

        var opponent = GetOpponentThatOccupies(territory);

        if (opponent != null)
        {
            var potentialWinningOpponents = Game.Players.Where(p => p != Player 
                                                                    && p != AlliedPlayer && Game.MeetsNormalVictoryCondition(p, true) 
                                                                    && Game.CountChallengedVictoryPoints(p) < 2);
            var amountICanReinforce = MaxReinforcedDialTo(Player, territory);
            var maxDial = MaxDial(Player, territory, opponent);

            var dialNeeded = GetDialNeeded(territory, opponent, false);

            if ((!opponent.Is(Faction.Black) && WinWasPredictedByMeThisTurn(opponent.Faction)) ||
                dialNeeded <= maxDial ||
                (LastTurn && dialNeeded - 1 <= amountICanReinforce + maxDial) ||
                (potentialWinningOpponents.Contains(opponent) && dialNeeded <= amountICanReinforce + maxDial))
                return true;
        }

        return false;
    }

    private BlueBattleAnnouncement? DetermineBlueBattleAnnouncement()
    {
        if (Game.CurrentMainPhase > MainPhase.Bidding && NrOfBattlesToFight <= Battle.ValidBattleHeroes(Game, Player).Count())
        {
            var territory = BlueBattleAnnouncement.ValidTerritories(Game, Player).Where(IWantToAnnounceBattleIn).LowestOrDefault(t => GetDialNeeded(t, GetOpponentThatOccupies(t), false));

            if (territory != null) return new BlueBattleAnnouncement(Game, Faction) { Territory = territory };
        }

        return null;
    }

    private bool IWantToAnnounceBattleIn(Territory territory)
    {
        var opponent = GetOpponentThatOccupies(territory);

        if (opponent == null) return false;
        
        var dialNeeded = GetDialNeeded(territory, opponent, false);

        var lastTurnConfidenceBonus = LastTurn ? 3 : 0;

        return (territory.IsStronghold && WinWasPredictedByMeThisTurn(opponent.Faction)) ||
               dialNeeded + lastTurnConfidenceBonus <= MaxDial(Player, territory, opponent) + MaxReinforcedDialTo(Player, territory);
    }

    private BlueAccompanies DetermineBlueAccompanies()
    {
        if (Player.ForcesInReserve <= 0)
            return new BlueAccompanies(Game, Faction) { Location = null, Accompanies = false };
        
        var target = BlueAccompanies.ValidTargets(Game, Player).FirstOrDefault(l => l.IsStronghold);

        var shippingOpponentCanWin = false;
        if (target != null)
        {
            var opponent = GetOpponentThatOccupies(target);
            var potentialWinningOpponents = Game.Players.Where(p => p != Player && p != AlliedPlayer && Game.MeetsNormalVictoryCondition(p, true) && Game.CountChallengedVictoryPoints(p) < 2);
            shippingOpponentCanWin = potentialWinningOpponents.Contains(opponent);
        }

        if (target != null &&
            !shippingOpponentCanWin &&
            (ResourcesIncludingAllyContribution < 5 || Player.ForcesInReserve > 3) &&
            Player.AnyForcesIn(target) < 8 &&
            ((!LastTurn && Player.AnyForcesIn(target) > 0) || (!Player.HasAlly && Player.ForcesInLocations.Count(kvp => IsStronghold(kvp.Key)) <= 3)))
            return new BlueAccompanies(Game, Faction) { Location = target, Accompanies = true, ExtraAdvisor = false };

        if (BlueAccompanies.ValidTargets(Game, Player).Contains(Game.Map.PolarSink) &&
            (ResourcesIncludingAllyContribution < 5 || Player.ForcesInReserve > 3) &&
            !(LastTurn && Game.HasActedOrPassed.Contains(Faction)) &&
            Player.AnyForcesIn(Game.Map.PolarSink) < 8)
            return new BlueAccompanies(Game, Faction) { Location = Game.Map.PolarSink, Accompanies = true, ExtraAdvisor = BlueAccompanies.MaySendExtraAdvisor(Game, Player, Game.Map.PolarSink) };

        return new BlueAccompanies(Game, Faction) { Location = null, Accompanies = false };
    }

    private BluePrediction DetermineBluePrediction()
    {
        var predicted = D(1, 2) == 1 ? Others.Where(p => !Game.IsBot(p)).RandomOrDefault() : null;
        predicted ??= Others.Where(p => !Game.IsBot(p)).RandomOrDefault();
        predicted ??= Others.RandomOrDefault();

        int turn;
        if (predicted.Is(Faction.Black))
        {
            turn = D(1, D(1, 2) == 1 ? Math.Min(3, Game.MaximumTurns) : Game.MaximumTurns);
        }
        else if (predicted.Is(Faction.Green) || predicted.Is(Faction.Grey))
        {
            if (D(1, 2) == 1) turn = D(1, Game.MaximumTurns);
            else turn = 2 + D(1, Game.MaximumTurns - 2);
        }
        else if (predicted.Is(Faction.Purple))
        {
            turn = 3 + D(1, Game.MaximumTurns - 3);
        }
        else
        {
            if (D(1, 2) == 1) turn = 1 + D(1, Game.MaximumTurns - 1);
            else turn = 2 + D(1, Game.MaximumTurns - 2);
        }

        return new BluePrediction(Game, Faction) { ToWin = predicted.Faction, Turn = turn };
    }

    private VoicePlan? VoicePlan { get; set; }

    protected virtual Voice? DetermineVoice()
    {
        LogInfo("DetermineVoice()");

        VoicePlan = null;

        if (Game.CurrentBattle.IsAggressorOrDefender(Player))
        {
            VoicePlan = BestVoice(Game.CurrentBattle, Player, Game.CurrentBattle.OpponentOf(Faction));
            LogInfo(VoicePlan.Voice.GetMessage());
            return VoicePlan.Voice;
        }

        return null;
    }

    private VoicePlan BestVoice(BattleInitiated? battle, Player p, Player opponent)
    {
        var result = new VoicePlan
        {
            Battle = battle,
            PlayerHeroWillCertainlySurvive = false,
            OpponentHeroWillCertainlyBeZero = false
        };

        if (WinWasPredictedByMeThisTurn(opponent.Faction))
        {
            result.WeaponToUse = null;
            result.DefenseToUse = null;
            result.Voice = new Voice(Game, Faction) { Must = true, Type = TreacheryCardType.Laser };
            result.OpponentHeroWillCertainlyBeZero = true;
            result.PlayerHeroWillCertainlySurvive = true;
        }

        var knownOpponentDefenses = KnownOpponentDefenses(opponent);
        var knownOpponentWeapons = KnownOpponentWeapons(opponent);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var weapons = Weapons(null, null, null)
            .Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth).ToArray();
        result.WeaponToUse = weapons.FirstOrDefault(w => w.Type == TreacheryCardType.ProjectileAndPoison); //use poison blade if available
        result.WeaponToUse ??= weapons.FirstOrDefault(w => w.Type == TreacheryCardType.Laser); //use lasgun if available
        result.WeaponToUse ??= weapons.FirstOrDefault(w => Game.KnownCards(Player).Contains(w)); //use a known weapon if available
        result.WeaponToUse ??= weapons.FirstOrDefault(); //use any weapon

        var type = TreacheryCardType.None;
        var must = false;

        var opponentMightHaveDefenses = !(knownOpponentDefenses.Any() && nrOfUnknownOpponentCards == 0);

        var cardsPlayerHasOrMightHave = CardsPlayerHasOrMightHave(opponent);

        if (opponentMightHaveDefenses && result.WeaponToUse != null)
        {
            result.OpponentHeroWillCertainlyBeZero = true;

            //opponent might have defenses and player has a weapon. Use voice to disable the corresponding defense, if possible by forcing use of the wrong defense and otherwise by forcing not to use the correct defense.
            if (result.WeaponToUse.Type == TreacheryCardType.ProjectileAndPoison)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d.Type != TreacheryCardType.ShieldAndAntidote);
                if (uselessDefense != null)
                {
                    must = true;
                    type = uselessDefense.Type;
                }
                else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.ShieldAndAntidote))
                {
                    must = false;
                    type = TreacheryCardType.ShieldAndAntidote;
                }
            }
            else if (result.WeaponToUse.IsLaser)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d is { IsPoisonDefense: true, IsShield: false });
                if (uselessDefense != null)
                {
                    must = true;
                    type = uselessDefense.Type;
                }
                else if (cardsPlayerHasOrMightHave.Any(c => c.IsShield))
                {
                    must = false;
                    type = TreacheryCardType.Shield;
                }
            }
            else if (result.WeaponToUse.IsProjectileWeapon)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d is { IsPoisonDefense: true, IsProjectileDefense: false });
                if (uselessDefense != null)
                {
                    must = true;
                    type = uselessDefense.Type;
                }
                else
                {
                    must = false;

                    if (!knownOpponentDefenses.Any(d => d.IsProjectileDefense && d.Type != TreacheryCardType.WeirdingWay)
                        && knownOpponentDefenses.Any(d => d.Type == TreacheryCardType.WeirdingWay)
                        && knownOpponentWeapons.Any(d => d.IsWeapon)
                        && cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.WeirdingWay))
                        type = TreacheryCardType.WeirdingWay;
                    else if (!Game.Applicable(Rule.BlueVoiceMustNameSpecialCards) && cardsPlayerHasOrMightHave.Any(c => c.IsProjectileDefense))
                        type = TreacheryCardType.ProjectileDefense;
                    else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.ShieldAndAntidote))
                        type = TreacheryCardType.ShieldAndAntidote;
                    else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.Shield)) type = TreacheryCardType.Shield;
                }
            }
            else if (result.WeaponToUse.IsPoisonWeapon)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d.IsProjectileDefense && !d.IsProjectileDefense);
                if (uselessDefense != null)
                {
                    must = true;
                    type = uselessDefense.Type;
                }
                else if (!Game.Applicable(Rule.BlueVoiceMustNameSpecialCards) && cardsPlayerHasOrMightHave.Any(c => c.IsPoisonDefense))
                {
                    must = false;
                    type = TreacheryCardType.PoisonDefense;
                }
                else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.ShieldAndAntidote))
                {
                    must = false;
                    type = TreacheryCardType.ShieldAndAntidote;
                }
                else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.Chemistry))
                {
                    must = false;
                    type = TreacheryCardType.Chemistry;
                }
                else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.Antidote))
                {
                    must = false;
                    type = TreacheryCardType.Antidote;
                }
            }

            LogInfo("Using {0}, disable enemy defense: {1} {2}", result.WeaponToUse, must ? "must use" : "may not use", type);
        }

        if (type == TreacheryCardType.None)
        {
            var opponentMightHaveWeapons = !(knownOpponentWeapons.Any() && nrOfUnknownOpponentCards == 0);

            //I have no weapon or the opponent has no defense. Focus on disabling their weapon.
            DetermineBestDefense(opponent, null, out result.DefenseToUse);

            if (opponentMightHaveWeapons && result.DefenseToUse != null)
            {
                //opponent might have weapons and player has a defense. Use voice to disable the corresponding weapon, if possible by forcing use of the wrong weapon and otherwise by forcing not to use the correct weapon.
                if (result.DefenseToUse.Type == TreacheryCardType.ShieldAndAntidote)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => w.Type != TreacheryCardType.Laser && w.Type != TreacheryCardType.PoisonTooth && w.Type != TreacheryCardType.Rockmelter);
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.PlayerHeroWillCertainlySurvive = true;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.PoisonTooth))
                    {
                        must = false;
                        type = TreacheryCardType.PoisonTooth;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Laser))
                    {
                        must = false;
                        type = TreacheryCardType.Laser;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Rockmelter))
                    {
                        must = false;
                        type = TreacheryCardType.Rockmelter;
                    }

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.DefenseToUse, must ? "must use" : "may not use", type);
                }
                else if (result.DefenseToUse.IsProjectileDefense)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => w is { IsProjectileWeapon: true, IsPoisonWeapon: false });
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.PlayerHeroWillCertainlySurvive = true;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Laser))
                    {
                        must = false;
                        type = TreacheryCardType.Laser;
                        result.PlayerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.PoisonTooth))
                    {
                        must = false;
                        type = TreacheryCardType.PoisonTooth;
                        result.PlayerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else
                    {
                        if (!knownOpponentWeapons.Any(d => d.IsPoisonWeapon && d.Type != TreacheryCardType.Chemistry)
                            && knownOpponentWeapons.Any(d => d.Type == TreacheryCardType.Chemistry)
                            && knownOpponentDefenses.Any(d => d.IsDefense)
                            && cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.Chemistry))
                        {
                            must = false;
                            type = TreacheryCardType.Chemistry;
                        }
                        else if (cardsPlayerHasOrMightHave.Any(c => c.IsPoisonWeapon))
                        {
                            must = false;
                            type = TreacheryCardType.Poison;
                        }
                    }

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.DefenseToUse, must ? "must use" : "may not use", type);
                }
                else if (result.DefenseToUse.IsPoisonDefense)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => w is { IsProjectileWeapon: false, IsPoisonWeapon: true });
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.PlayerHeroWillCertainlySurvive = true;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Laser))
                    {
                        must = false;
                        type = TreacheryCardType.Laser;
                        result.PlayerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.PoisonTooth))
                    {
                        must = false;
                        type = TreacheryCardType.PoisonTooth;
                        result.PlayerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (cardsPlayerHasOrMightHave.Any(c => c.IsProjectileWeapon))
                    {
                        must = false;
                        type = TreacheryCardType.Projectile;
                    }

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.DefenseToUse, must ? "must use" : "may not use", type);
                }
            }
        }

        if (type == TreacheryCardType.None && opponent.TreacheryCards.Any(c => Game.KnownCards(Player).Contains(c) && c.Type == TreacheryCardType.Mercenary))
        {
            type = TreacheryCardType.Mercenary;
            must = true;
        }

        if (type == TreacheryCardType.None)
        {
            must = false;
            if (D(1, 2) > 1)
            {
                type = cardsPlayerHasOrMightHave.Any(c => c.IsProjectileWeapon) 
                    ? TreacheryCardType.Projectile 
                    : TreacheryCardType.Poison;
            }
            else
            {
                type = cardsPlayerHasOrMightHave.Any(c => c.IsPoisonWeapon) 
                    ? TreacheryCardType.Poison 
                    : TreacheryCardType.Projectile;
            }

            LogInfo("Remaining category: {0} {1}", must ? "must use" : "may not use", type);
        }

        var voice = new Voice(Game, p.Faction) { Must = must, Type = type };
        result.Voice = voice;

        return result;
    }

    #endregion Blue

    #region Green

    protected virtual Prescience? DeterminePrescience()
    {
        var opponent = Game.CurrentBattle.OpponentOf(Faction);

        if (Voice.MayUseVoice(Game, opponent))
            if (Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null)
                //Wait for voice or finalized battle plan
                return null;

        if (!Game.CurrentBattle.IsAggressorOrDefender(Player)) return null;
        
        var existingAspect = Game.CurrentPrescience?.Aspect ?? PrescienceAspect.None;
        return new Prescience(Game, Faction) { Aspect = BestPrescience(Player, opponent, MaxDial(Player, Game.CurrentBattle.Territory, opponent), existingAspect, Game.CurrentBattle.Territory) };
    }


    private PrescienceAspect BestPrescience(Player p, Player opponent, float maxForceStrengthInBattle, PrescienceAspect earlierPrescience, Territory territory)
    {
        var myDefenses = Battle.ValidDefenses(Game, p, null, territory).Where(c => Game.KnownCards(p).Contains(c)).ToArray();
        var myWeapons = Battle.ValidWeapons(Game, p, null, null, territory).Where(c => Game.KnownCards(p).Contains(c)).ToArray();

        var cardsOpponentHasOrMightHave = CardsPlayerHasOrMightHave(opponent);
        var weaponIsCertain = CountDifferentWeaponTypes(cardsOpponentHasOrMightHave) <= 1;
        var defenseIsCertain = CountDifferentDefenseTypes(cardsOpponentHasOrMightHave) <= 1;

        var iHaveShieldSnooper = myDefenses.Any(d => d.Type == TreacheryCardType.ShieldAndAntidote);
        var iHavePoisonBlade = myWeapons.Any(d => d.Type == TreacheryCardType.ProjectileAndPoison);

        PrescienceAspect aspect;
        if (earlierPrescience != PrescienceAspect.Weapon && !weaponIsCertain && myDefenses.Any(d => d.IsProjectileDefense) && myDefenses.Any(d => d.IsPoisonDefense) && !iHaveShieldSnooper)
            //I don't have shield snooper and I have choice between shield and snooper, therefore ask for the weapon used
            aspect = PrescienceAspect.Weapon;
        else if (earlierPrescience != PrescienceAspect.Defense && !defenseIsCertain && myWeapons.Any(d => d.IsProjectileWeapon) && myWeapons.Any(d => d.IsPoisonWeapon) && !iHavePoisonBlade)
            //I don't have poison blade and I have choice between poison weapon and projectile weapon, therefore ask for the defense used
            aspect = PrescienceAspect.Defense;
        else if (earlierPrescience != PrescienceAspect.Weapon && !weaponIsCertain && myDefenses.Any() && !iHaveShieldSnooper)
            aspect = PrescienceAspect.Weapon;
        else if (earlierPrescience != PrescienceAspect.Defense && !defenseIsCertain && myWeapons.Any() && !iHavePoisonBlade)
            aspect = PrescienceAspect.Defense;
        else if (earlierPrescience != PrescienceAspect.Dial && maxForceStrengthInBattle > 2 && Prescience.ValidAspects(Game, p).Contains(PrescienceAspect.Dial))
            aspect = PrescienceAspect.Dial;
        else if (earlierPrescience != PrescienceAspect.Leader)
            aspect = PrescienceAspect.Leader;
        else
            aspect = PrescienceAspect.None;

        return aspect;
    }

    #endregion Green

    #region Purple

    private SetIncreasedRevivalLimits? DetermineSetIncreasedRevivalLimits()
    {
        var targets = SetIncreasedRevivalLimits.ValidTargets(Game, Player).ToArray();
        if (Game.FactionsWithIncreasedRevivalLimits.Length != targets.Length)
            return new SetIncreasedRevivalLimits(Game, Faction) { Factions = targets };
        
        return null;
    }

    private FaceDancerRevealed DetermineFaceDancerRevealed()
    {
        if (FaceDanced.MayCallFaceDancer(Game, Player))
        {
            var faceDancer = Player.FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));
            var faceDancedHeroIsLivingLeader = faceDancer is Leader && Game.IsAlive(faceDancer);

            if ((FaceDanced.MaximumNumberOfForces(Game, Player) > 0 || faceDancedHeroIsLivingLeader) && Game.BattleWinner != Ally)
            {
                var result = new FaceDancerRevealed(Game, Faction) { Passed = false };
                LogInfo(result.GetMessage());
                return result;
            }
        }

        return new FaceDancerRevealed(Game, Faction) { Passed = true };
    }

    protected virtual FaceDanced DetermineFaceDanced()
    {
        var forcesFromPlanet = new Dictionary<Location, Battalion>();

        var toPlace = FaceDanced.MaximumNumberOfForces(Game, Player);

        var biggest = BiggestBattalionThreatenedByStormWithoutSpice;

        if (biggest == null) return new FaceDanced { Passed = true };
        
        var toTake = biggest.Battalion.Take(toPlace, false);
        forcesFromPlanet.Add(biggest.Location, toTake);
        toPlace -= toTake.TotalAmountOfForces;

        var fromReserves = Math.Min(Player.ForcesInReserve, toPlace);

        var targetLocation = FaceDanced.ValidTargetLocations(Game).FirstOrDefault(l => Game.ResourcesOnPlanet.ContainsKey(l));
        targetLocation ??= FaceDanced.ValidTargetLocations(Game).FirstOrDefault();

        if (targetLocation == null) return new FaceDanced { Passed = true };
        
        var targetLocations = new Dictionary<Location, Battalion>
        {
            { targetLocation, new Battalion(Faction, forcesFromPlanet.Sum(kvp => kvp.Value.AmountOfForces) + fromReserves, forcesFromPlanet.Sum(kvp => kvp.Value.AmountOfSpecialForces), targetLocation )}
        };

        var result = new FaceDanced(Game, Faction) { ForceLocations = forcesFromPlanet, ForcesFromReserve = fromReserves, TargetForceLocations = targetLocations };
        LogInfo(result.GetMessage());
        return result;
    }

    protected virtual FaceDancerReplaced DetermineFaceDancerReplaced()
    {
        var replaceable = Player.FaceDancers.Where(f => !Player.RevealedFaceDancers.Contains(f)).OrderBy(f => f.Value).ToArray();
        var toReplace = replaceable.FirstOrDefault(f => Player.Leaders.Contains(f) || (Ally != Faction.None && AlliedPlayer!.Leaders.Contains(f)));
        toReplace ??= replaceable.FirstOrDefault(f => f is Leader && !Game.IsAlive(f));

        if (toReplace != null)
            return new FaceDancerReplaced(Game, Faction) { Passed = false, SelectedDancer = toReplace };
        
        return new FaceDancerReplaced(Game, Faction) { Passed = true };
    }

    private Dictionary<IHero, int> PriceSetEarlier { get; } = new();
    private int TurnWhenRevivalWasRequested { get; set; } = -1;

    protected virtual RequestPurpleRevival? DetermineRequestPurpleRevival()
    {
        if (Game.CurrentRevivalRequests.Any(r => r.Initiator == Faction) || Game.EarlyRevivalsOffers.Keys.Any(h => h.Faction == Faction)) return null;

        if (TurnWhenRevivalWasRequested == Game.CurrentTurn) return null;

        var toRevive = RequestPurpleRevival.ValidTargets(Game, Player).Where(l => SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);

        if (toRevive == null)
        {
            var knownOpponentTraitors = Opponents.SelectMany(p => p.RevealedTraitors);
            toRevive = RequestPurpleRevival.ValidTargets(Game, Player).Where(l => !knownOpponentTraitors.Contains(l)).HighestOrDefault(l => l.Value);
        }

        if (toRevive != null && Battle.ValidBattleHeroes(Game, Player).Count() <= 3) toRevive = RequestPurpleRevival.ValidTargets(Game, Player).HighestOrDefault(l => l.Value);

        if (toRevive != null)
        {
            TurnWhenRevivalWasRequested = Game.CurrentTurn;
            return new RequestPurpleRevival(Game, Faction) { Hero = toRevive };
        }

        return null;
    }

    protected virtual AcceptOrCancelPurpleRevival? DetermineAcceptOrCancelPurpleRevival()
    {
        var requestToHandle = Game.CurrentRevivalRequests.FirstOrDefault();
        if (requestToHandle != null)
        {
            var hero = requestToHandle.Hero;

            int price;
            if (requestToHandle.Initiator == Ally)
            {
                price = 0;
            }
            else if (PriceSetEarlier.TryGetValue(hero, out var value))
            {
                price = value;
            }
            else
            {
                if ((Player.FaceDancers.Contains(hero) && !Player.RevealedFaceDancers.Contains(hero)) || Player.HasAlly && AlliedPlayer!.Traitors.Contains(hero) && !AlliedPlayer.RevealedTraitors.Contains(hero))
                    price = 1 + D(1, hero.Value);
                else
                    price = 2 + D(2, hero.Value);

                PriceSetEarlier.Add(hero, price);
            }

            return new AcceptOrCancelPurpleRevival(Game, Faction) { Hero = hero, Price = price, Cancel = false };
        }

        return null;
    }

    #endregion Purple

    #region Brown

    private ResourcesAudited? DetermineResourcesAudited()
    {
        if (Game.CurrentBattle == null || !Game.CurrentBattle.IsAggressorOrDefender(Player)) return null;
        
        var opponent = Game.CurrentBattle.OpponentOf(Player);

        if (opponent.TreacheryCards.Any(tc => !Player.KnownCards.Contains(tc)) && 
            ResourcesAudited.ValidFactions(Game, Player).Contains(opponent.Faction)) return new ResourcesAudited(Game, Faction) { Target = opponent.Faction };

        return null;
    }

    protected virtual BrownEconomics? DetermineBrownEconomics()
    {
        if (ResourcesIncludingAllyContribution >= 14 &&
            Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || (Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue)) >= 2)
            return new BrownEconomics(Game, Faction) { Status = BrownEconomicsStatus.Cancel };
        
        if (ResourcesIncludingAllyContribution <= 8 &&
            Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || (Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue)) <= 2)
            return new BrownEconomics(Game, Faction) { Status = BrownEconomicsStatus.Double };

        return null;
    }

    protected virtual BrownRemoveForce? DetermineBrownRemoveForce()
    {
        var opponents = WinningOpponentsIWishToAttack(20, true);

        var stronghold = Game.Map.Carthag;
        var opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);

        if (opponentWithOneBattalionInStronghold == null)
        {
            stronghold = Game.Map.Arrakeen;
            opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattalionInStronghold == null)
        {
            stronghold = Game.Map.TueksSietch;
            opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattalionInStronghold == null)
        {
            stronghold = Game.Map.SietchTabr;
            opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattalionInStronghold == null)
        {
            stronghold = Game.Map.HabbanyaSietch;
            opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattalionInStronghold == null)
        {
            stronghold = Game.Map.HiddenMobileStronghold;
            opponentWithOneBattalionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattalionInStronghold != null) return new BrownRemoveForce(Game, Faction) { Location = stronghold, Target = opponentWithOneBattalionInStronghold.Faction, SpecialForce = opponentWithOneBattalionInStronghold.SpecialForcesIn(stronghold) > 0 };

        return null;
    }

    #endregion

    #region Cyan

    protected virtual ExtortionPrevented? DetermineExtortionPrevented()
    {
        if (Player.Ally != Faction.Cyan && Resources > 12 && D(1,6) >= 3) 
            return new ExtortionPrevented(Game, Faction);
        
        return null;
    }

    private PerformCyanSetup DeterminePerformCyanSetup()
    {
        Location target;

        if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.Carthag))
            target = Game.Map.Carthag;
        else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.Arrakeen))
            target = Game.Map.Arrakeen;
        else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.TueksSietch))
            target = Game.Map.TueksSietch;
        else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.HabbanyaSietch))
            target = Game.Map.HabbanyaSietch;
        else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.ImperialBasin.MiddleLocation))
            target = Game.Map.ImperialBasin.MiddleLocation;
        else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.PolarSink))
            target = Game.Map.PolarSink;
        else
            target = PerformCyanSetup.ValidLocations(Game).First(l => l.IsProtectedFromStorm);

        return new PerformCyanSetup(Game, Faction) { Target = target };
    }

    protected virtual TerrorPlanted? DetermineTerrorPlanted()
    {
        if (Game.CurrentMainPhase is not MainPhase.Contemplate)
            return null;
        
        var availableToPlace = TerrorPlanted.ValidTerrorTypes(Game, false).Where(t => !Game.TerrorOnPlanet.ContainsKey(t)).ToArray();

        if (availableToPlace.Any())
        {
            var type = TerrorType.None;

            if (availableToPlace.Contains(TerrorType.Extortion) 
                && Opponents.Sum(p => p.Resources) > Opponents.Count * 10) type = TerrorType.Robbery;
            if (type == TerrorType.None 
                && availableToPlace.Contains(TerrorType.Atomics) 
                && TerrorPlanted.ValidStrongholds(Game, Player).Any(t => Opponents.Sum(o => o.AnyForcesIn(t)) > 12 && MeAndMyAlly.Sum(o => o.AnyForcesIn(t)) < 2)) type = TerrorType.Atomics;
            if (type == TerrorType.None 
                && availableToPlace.Contains(TerrorType.Extortion) 
                && Resources < 5) type = TerrorType.Extortion;
            if (type == TerrorType.None) type = availableToPlace.RandomOrDefault();

            var stronghold = type == TerrorType.Atomics 
                ? TerrorPlanted.ValidStrongholds(Game, Player).HighestOrDefault(t => Opponents.Sum(o => o.AnyForcesIn(t))) 
                : TerrorPlanted.ValidStrongholds(Game, Player).HighestOrDefault(t => Player.AnyForcesIn(t));
            
            if (stronghold == null && Player.HasAlly) stronghold = TerrorPlanted.ValidStrongholds(Game, Player).HighestOrDefault(t => AlliedPlayer!.AnyForcesIn(t) > 0);
            
            stronghold ??= TerrorPlanted.ValidStrongholds(Game, Player).RandomOrDefault();

            if (stronghold != null) return new TerrorPlanted(Game, Faction) { Type = type, Stronghold = stronghold };
        }

        return new TerrorPlanted(Game, Faction) { Passed = true }; 
    }

    protected virtual TerrorRevealed DetermineTerrorRevealed()
    {
        //This is for now just quite random
        var territory = TerrorRevealed.GetTerritory(Game);
        var mayUseAtomics = Opponents.Sum(o => o.AnyForcesIn(TerrorRevealed.GetTerritory(Game))) > 5 &&
                            AlliedPlayer?.AnyForcesIn(territory) == 0; 
            
        var validTokens = TerrorRevealed.GetTypes(Game).Where(t => 
            (t != TerrorType.SneakAttack || TerrorRevealed.ValidSneakAttackTargets(Game, Player).Any()) &&
            (t != TerrorType.Atomics || mayUseAtomics)
        );

        if (!validTokens.Any()) return new TerrorRevealed(Game, Faction) { Passed = true };

        var type = TerrorRevealed.GetTypes(Game).Where(t => 
            (t != TerrorType.SneakAttack || TerrorRevealed.ValidSneakAttackTargets(Game, Player).Any()) &&
            (t != TerrorType.Atomics || mayUseAtomics)).RandomOrDefault();
        var cardInSabotage = Player.TreacheryCards.FirstOrDefault(c => c.IsUseless);
        var victim = Game.GetPlayer(TerrorRevealed.GetVictim(Game));

        var offerAlliance = TerrorRevealed.MayOfferAlliance(Game) && PlayerStanding(victim) > 1.5f * PlayerStanding(Player);

        if (offerAlliance)
            return new TerrorRevealed(Game, Faction) { AllianceOffered = true };
        return new TerrorRevealed(Game, Faction) { Type = type, RobberyTakesCard = false, CardToGiveInSabotage = cardInSabotage, ForcesInSneakAttack = TerrorRevealed.MaxAmountOfForcesInSneakAttack(Player), SneakAttackTo = TerrorRevealed.ValidSneakAttackTargets(Game, Player).FirstOrDefault() };
    }

    #endregion

    #region Pink

    protected virtual AmbassadorPlaced? DetermineAmbassadorPlaced()
    {
        if ((Resources <= 1 || Game.AmbassadorsPlacedThisTurn != 0) &&
            Resources <= 3 + Game.AmbassadorsPlacedThisTurn) return null;
        
        var stronghold = AmbassadorPlaced.ValidStrongholds(Game, Player).Where(s => Player.AnyForcesIn(s) > 0).RandomOrDefault();
        if (stronghold == null && Player.HasAlly) 
            stronghold = AmbassadorPlaced.ValidStrongholds(Game, Player).Where(s => AlliedPlayer!.AnyForcesIn(s) > 0).RandomOrDefault();
        
        var avoidEntering = stronghold != null;

        stronghold ??= AmbassadorPlaced.ValidStrongholds(Game, Player).Where(Vacant).RandomOrDefault();
        stronghold ??= AmbassadorPlaced.ValidStrongholds(Game, Player).RandomOrDefault();

        var ambassador = Ambassador.None;
        var availableAmbassadors = AmbassadorPlaced.ValidAmbassadors(Player).ToList();
        if (avoidEntering)
        {
            if (availableAmbassadors.Contains(Ambassador.Black)) ambassador = Ambassador.Black;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Green)) ambassador = Ambassador.Green;
        }

        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Purple) 
                                          && Player.Leaders.Count(l => !Game.IsAlive(l)) >= 2 
                                          && Player.ForcesKilled >= 4) ambassador = Ambassador.Purple;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Orange) 
                                          && Player.TreacheryCards.Any(c => c.IsWeapon) 
                                          && Player.TreacheryCards.Any(c => c.IsDefense)) ambassador = Ambassador.Orange;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Brown) 
                                          && Player.TreacheryCards.Count(c => CardQuality(c, Player) <= 1) >= 2) ambassador = Ambassador.Brown;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Grey) 
                                          && Player.TreacheryCards.Any(c => CardQuality(c, Player) <= 1)) ambassador = Ambassador.Grey;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.White) 
                                          && Player is { HasRoomForCards: true, Resources: > 6 }) ambassador = Ambassador.White;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Red) 
                                          && Resources <= 4) ambassador = Ambassador.Red;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Yellow) 
                                          && DetermineMovedBattalion(false) != null) ambassador = Ambassador.Yellow;
        if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Pink) 
                                          && !Player.HasAlly) ambassador = Ambassador.Pink;
        if (Game.AmbassadorsPlacedThisTurn == 0 && ambassador == Ambassador.None) ambassador = AmbassadorPlaced.ValidAmbassadors(Player).RandomOrDefault();

        return ambassador != Ambassador.None 
            ? new AmbassadorPlaced(Game, Faction) { Ambassador = ambassador, Stronghold = stronghold } 
            : null;
    }

    protected virtual AmbassadorActivated? DetermineAmbassadorActivated()
    {
        var victimPlayer = AmbassadorActivated.GetVictimPlayer(Game);
        var ambassador = AmbassadorActivated.GetAmbassador(Game);
        var blueSelectedAmbassador = Ambassador.None;

        if (ambassador == Ambassador.Blue)
        {
            //This is for now just random
            blueSelectedAmbassador = AmbassadorActivated.GetValidBlueAmbassadors(Game).RandomOrDefault();
            ambassador = blueSelectedAmbassador;
        }

        switch (ambassador)
        {
            case Ambassador.Brown:
                var toDiscard = AmbassadorActivated.GetValidBrownCards(Player).Where(c => CardQuality(c, Player) < 2).ToArray();
                if (toDiscard.Any())
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, BrownCards = toDiscard };
                return PassAmbassadorActivated();

            case Ambassador.Pink:
                var offerAlliance = AmbassadorActivated.AllianceCanBeOffered(Game, Player) && PlayerStanding(victimPlayer) > 0.33 * PlayerStanding(Player);
                var takeVidal = !offerAlliance && AmbassadorActivated.VidalCanBeTaken(Game, Player);
                var offerVidal = offerAlliance && AmbassadorActivated.VidalCanBeOfferedToNewAlly(Game, Player) &&
                                 HeroesForBattle(Player, true).Count() >= 3 &&
                                 HeroesForBattle(victimPlayer, true).Count() < 3;

                if (offerAlliance || takeVidal)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PinkOfferAlliance = offerAlliance, PinkTakeVidal = takeVidal, PinkGiveVidalToAlly = offerVidal };
                
                return PassAmbassadorActivated();

            case Ambassador.Yellow:

                Location? target = null;
                var validLocations = AmbassadorActivated.ValidYellowTargets(Game, Player).ToList();
                if (validLocations.Contains(Game.Map.TueksSietch) && VacantAndSafeFromStorm(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
                if (target == null && validLocations.Contains(Game.Map.Carthag) && VacantAndSafeFromStorm(Game.Map.Carthag)) target = Game.Map.Carthag;
                if (target == null && validLocations.Contains(Game.Map.Arrakeen) && VacantAndSafeFromStorm(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
                if (target == null && validLocations.Contains(Game.Map.HabbanyaSietch) && VacantAndSafeFromStorm(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;
                target ??= Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key)).HighestOrDefault(r => r.Value).Key;

                if (target != null)
                {
                    var toMove = DetermineMovedBattalion(true);
                    if (toMove != null && AmbassadorActivated.ValidYellowSources(Game, Player).Contains(toMove.From.Territory))
                    {
                        var forcesToMove = new Dictionary<Location, Battalion>
                        {
                            { toMove.From, toMove.Battalion }
                        };

                        return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, YellowOrOrangeTo = target, YellowForceLocations = forcesToMove };
                    }
                }

                return PassAmbassadorActivated();

            case Ambassador.Grey:
                var toReplace = AmbassadorActivated.GetValidGreyCards(Player).FirstOrDefault(c => CardQuality(c, Player) < 2);
                if (toReplace != null)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, GreyCard = toReplace };
                return PassAmbassadorActivated();

            case Ambassador.White:
                if (Resources > 3 && Player.HasRoomForCards)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador };
                return PassAmbassadorActivated();

            case Ambassador.Orange:

                var potentialTargets = AmbassadorActivated.ValidOrangeTargets(Game, Player).Where(l => l.IsStronghold);

                var dangerousOpponents = Opponents.Where(IsAlmostWinningOpponent).ToArray();

                var possibleAttacks = potentialTargets
                    .Where(l => dangerousOpponents.Length == 0 || dangerousOpponents.Any(p => p.Occupies(l)))
                    .Where(l => l.Territory.IsStronghold && Player.AnyForcesIn(l) == 0 && AllyDoesntBlock(l.Territory) && !StormWillProbablyHit(l) && !InStorm(l) && IDontHaveAdvisorsIn(l))
                    .Select(l => ConstructAttack(l, 0, 0, 4))
                    .Where(s => s is { ForcesToShip: <= 4, Opponent: not null } && !WinWasPredictedByMeThisTurn(s.Opponent.Faction));

                var attack = possibleAttacks.Where(s => s is { HasForces: true, ShortageForShipment: 0 }).LowestOrDefault(s => s.DialNeeded + DeterminePenalty(s.Opponent));

                if (attack != null)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, YellowOrOrangeTo = attack.Location, OrangeForceAmount = AmbassadorActivated.ValidOrangeMaxForces(Player) };
                return PassAmbassadorActivated();

            case Ambassador.Purple:

                var heroToRevive = AmbassadorActivated.ValidPurpleHeroes(Game, Player).Where(l => SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);
                heroToRevive ??= AmbassadorActivated.ValidPurpleHeroes(Game, Player).HighestOrDefault(l => l.Value);

                if (heroToRevive != null && (Player.ForcesInReserve > 3 || Battle.ValidBattleHeroes(Game, Player).Count() <= 1))
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PurpleHero = heroToRevive, PurpleAssignSkill = Revival.MayAssignSkill(Game, Player, heroToRevive) };
                if (AmbassadorActivated.ValidPurpleMaxAmount(Player) >= 3)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PurpleAmountOfForces = AmbassadorActivated.ValidPurpleMaxAmount(Player) };
                return PassAmbassadorActivated();

            default:
                return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador };

        }
    }

    private AmbassadorActivated? PassAmbassadorActivated()
    {
        return AmbassadorActivated.MayPass(Player) ? new AmbassadorActivated(Game, Faction) { Passed = true } : null;
    }

    #endregion


    #region White

    protected virtual WhiteAnnouncesBlackMarket DetermineWhiteAnnouncesBlackMarket()
    {
        var card = Player.TreacheryCards.FirstOrDefault(c => CardQuality(c, Player) < 3);
        if (card != null)
            return new WhiteAnnouncesBlackMarket(Game, Faction) { Passed = false, Card = card, AuctionType = D(1, 2) > 1 ? AuctionType.BlackMarketSilent : AuctionType.BlackMarketOnceAround, Direction = D(1, 2) > 1 ? 1 : -1 };
        return new WhiteAnnouncesBlackMarket(Game, Faction) { Passed = true };
    }

    protected virtual WhiteRevealedNoField? DetermineWhiteRevealedNoField()
    {
        if (Game.CurrentPhase == Phase.ShipmentAndMoveConcluded)
        {
            var locationWithNoField = ForcesOnPlanet.FirstOrDefault(b => b.Value.AmountOfSpecialForces > 0).Key;
            if (locationWithNoField != null && Game.ResourcesOnPlanet.ContainsKey(locationWithNoField) && !OccupiedByOpponent(locationWithNoField.Territory)) return new WhiteRevealedNoField(Game, Faction);
        }

        return null;
    }

    protected virtual WhiteAnnouncesAuction DetermineWhiteAnnouncesAuction()
    {
        return new WhiteAnnouncesAuction(Game, Faction) { First = D(1, 2) > 1 };
    }

    protected virtual WhiteSpecifiesAuction DetermineWhiteSpecifiesAuction()
    {
        var toAuction = new Deck<TreacheryCard>(Game.WhiteCache, _random);
        toAuction.Shuffle();
        return new WhiteSpecifiesAuction(Game, Faction) { Card = !toAuction.IsEmpty ? toAuction.Draw() : null, AuctionType = D(1, 2) > 1 ? AuctionType.WhiteSilent : AuctionType.WhiteOnceAround, Direction = D(1, 2) > 1 ? 1 : -1 };
    }

    protected virtual WhiteKeepsUnsoldCard DetermineWhiteKeepsUnsoldCard()
    {
        return new WhiteKeepsUnsoldCard(Game, Faction) { Passed = false };
    }

    #endregion White

    #region Black

    private Captured DetermineCaptured()
    {
        return new Captured(Game, Faction) { Passed = false };
    }

    #endregion
}

public class VoicePlan
{
    public BattleInitiated? Battle;
    public Voice Voice = new();
    public TreacheryCard? WeaponToUse;
    public TreacheryCard? DefenseToUse;
    public bool PlayerHeroWillCertainlySurvive;
    public bool OpponentHeroWillCertainlyBeZero;
}