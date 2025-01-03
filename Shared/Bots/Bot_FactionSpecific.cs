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
    #region Orange

    private SetShipmentPermission DetermineSetShipmentPermission()
    {
        if (Game.CurrentPhase == Phase.BeginningOfShipAndMove)
        {
            var toDeny = Game.ShipmentPermissions.Keys.Where(f => IsWinningOpponent(f));
            if (toDeny.Any()) return new SetShipmentPermission(Game, Faction) { Factions = toDeny.ToArray(), Permission = ShipmentPermission.None };

            var discountPermission = Resources > 10 || WinningOpponentsIWishToAttack(99, true).Any() ? ShipmentPermission.OrangeRate : ShipmentPermission.None;
            var permission = ShipmentPermission.Cross | ShipmentPermission.ToHomeworld | discountPermission;
            var toAllow = SetShipmentPermission.ValidTargets(Game, this).Where(f => !IsWinningOpponent(f) && (!Game.ShipmentPermissions.TryGetValue(f, out var currentpermission) || currentpermission != permission));
            if (toAllow.Any()) return new SetShipmentPermission(Game, Faction) { Factions = toAllow.ToArray(), Permission = permission };
        }

        return null;
    }

    #endregion

    #region Red

    private RedDiscarded DetermineRedDiscarded()
    {
        if (Resources >= 10 && !HasRoomForCards)
        {
            var worstCard = TreacheryCards.OrderBy(c => CardQuality(c, null)).First();

            if (CardQuality(worstCard, null) <= 1) return new RedDiscarded(Game, Faction) { Card = worstCard };
        }

        return null;
    }

    #endregion

    #region Grey

    protected virtual ReplacedCardWon DetermineReplacedCardWon()
    {
        var replace = Game.CardJustWon != null && CardQuality(Game.CardJustWon, this) <= 2;
        return new ReplacedCardWon(Game, Faction) { Passed = !replace };
    }

    protected GreyRemovedCardFromAuction DetermineGreyRemovedCardFromAuction()
    {
        TreacheryCard toBeRemoved = null;
        if (TreacheryCards.Any(c => c.Type == TreacheryCardType.ProjectileAndPoison)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.Type == TreacheryCardType.ShieldAndAntidote);
        if (toBeRemoved == null && !TreacheryCards.Any(c => c.Type == TreacheryCardType.ShieldAndAntidote)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.Type == TreacheryCardType.ProjectileAndPoison);

        if (toBeRemoved == null && TreacheryCards.Any(c => c.IsProjectileWeapon)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsProjectileDefense);
        if (toBeRemoved == null && TreacheryCards.Any(c => c.IsPoisonWeapon)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsPoisonDefense);
        if (toBeRemoved == null && !TreacheryCards.Any(c => c.IsProjectileDefense)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsProjectileWeapon);
        if (toBeRemoved == null && !TreacheryCards.Any(c => c.IsPoisonDefense)) toBeRemoved = Game.CardsOnAuction.Items.FirstOrDefault(c => c.IsPoisonWeapon);

        if (toBeRemoved == null)
        {
            if (TreacheryCards.Count >= 3 || ResourcesIncludingAllyAndRedContribution < 5)
                //Remove the best card from auction
                toBeRemoved = Game.CardsOnAuction.Items.HighestOrDefault(c => CardQuality(c, this));
            else
                //Remove the worst card from auction
                toBeRemoved = Game.CardsOnAuction.Items.LowestOrDefault(c => CardQuality(c, this));
        }

        toBeRemoved ??= Game.CardsOnAuction.Items.FirstOrDefault();

        var putOnTop = (CardQuality(toBeRemoved, this) <= 2 && Ally == Faction.None) || (CardQuality(toBeRemoved, this) >= 4 && Ally != Faction.None);

        return new GreyRemovedCardFromAuction(Game, Faction) { Card = toBeRemoved, PutOnTop = putOnTop };
    }

    protected GreySwappedCardOnBid DetermineGreySwappedCardOnBid()
    {
        var card = TreacheryCards.LowestOrDefault(c => CardQuality(c, this));

        if (card != null && CardQuality(card, this) <= 2)
            return new GreySwappedCardOnBid(Game, Faction) { Passed = false, Card = card };
        return new GreySwappedCardOnBid(Game, Faction) { Passed = true };
    }

    protected GreySelectedStartingCard DetermineGreySelectedStartingCard()
    {
        var cards = Game.StartingTreacheryCards.Items;

        var card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.ProjectileAndPoison);
        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.ShieldAndAntidote);
        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.Laser);

        if (card == null && cards.Any(c => c.IsPoisonWeapon) && !cards.Any(c => c.IsPoisonDefense)) card = cards.FirstOrDefault(c => c.IsPoisonWeapon);

        if (card == null && cards.Any(c => c.IsProjectileWeapon) && !cards.Any(c => c.IsProjectileDefense)) card = cards.FirstOrDefault(c => c.IsProjectileWeapon);

        card ??= cards.FirstOrDefault(c => c.Type == TreacheryCardType.WeirdingWay);
        card ??= card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.Chemistry);
        card ??= card = cards.FirstOrDefault(c => c.IsProjectileWeapon);
        card ??= card = cards.FirstOrDefault(c => c.IsPoisonWeapon);
        card ??= card = cards.FirstOrDefault(c => c.IsProjectileDefense);
        card ??= card = cards.FirstOrDefault(c => c.IsPoisonDefense);
        card ??= card = cards.FirstOrDefault(c => c.Type != TreacheryCardType.Useless);
        card ??= card = cards.FirstOrDefault();

        return new GreySelectedStartingCard(Game, Faction) { Card = card };
    }

    protected PerformHmsMovement DeterminePerformHmsMovement()
    {
        var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;

        var richestAdjacentSpiceLocation = PerformHmsMovement.ValidLocations(Game).Where(l => l != currentLocation && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
        if (richestAdjacentSpiceLocation != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = richestAdjacentSpiceLocation };

        var reachableFromCurrentLocation = Game.Map.FindNeighbours(currentLocation, Game.HmsMovesLeft, false, Faction, Game, false);

        var richestReachableSpiceLocation = reachableFromCurrentLocation.Where(l => l != currentLocation && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
        if (richestReachableSpiceLocation != null)
        {
            var nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, richestReachableSpiceLocation, 1)).FirstOrDefault();

            if (nextStepTowardsSpice != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsSpice };

            nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, richestReachableSpiceLocation, 2)).FirstOrDefault();
            return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsSpice };
        }

        //If there is nowhere to go, move toward Imperial Basin or Polar Sink
        Location alternativeLocation = null;
        if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
            alternativeLocation = Game.Map.ImperialBasin.MiddleLocation;
        else
            alternativeLocation = Game.Map.PolarSink;

        if (alternativeLocation != currentLocation)
        {
            var nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => l == alternativeLocation);
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };

            nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, alternativeLocation, 1)).FirstOrDefault();
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };

            nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, alternativeLocation, 2)).FirstOrDefault();
            if (nextStepTowardsAlternative != null) return new PerformHmsMovement(Game, Faction) { Passed = false, Target = nextStepTowardsAlternative };
        }

        return new PerformHmsMovement(Game, Faction) { Passed = true };
    }

    protected PerformHmsPlacement DeterminePerformHmsPlacement()
    {
        if (Faction == Faction.Grey)
        {
            if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
                return new PerformHmsPlacement(Game, Faction) { Target = PerformHmsPlacement.ValidLocations(Game, this).First(l => l.Territory == Game.Map.ImperialBasin) };
            return new PerformHmsPlacement(Game, Faction) { Target = Game.Map.PolarSink };
        }

        return new PerformHmsPlacement(Game, Faction) { Target = Game.Map.ShieldWall.Locations.First() };
    }

    #endregion Grey

    #region Yellow

    protected TakeLosses DetermineTakeLosses()
    {
        var normalForces = Math.Min(TakeLosses.LossesToTake(Game).Amount, TakeLosses.ValidMaxForceAmount(Game, this));
        var specialForces = TakeLosses.LossesToTake(Game).Amount - normalForces;
        var useUseless = TakeLosses.CanPreventLosses(Game, this);
        return new TakeLosses(Game, Faction) { ForceAmount = normalForces, SpecialForceAmount = specialForces, UseUselessCard = useUseless };
    }

    protected PerformYellowSetup DeterminePerformYellowSetup()
    {
        var forceLocations = new Dictionary<Location, Battalion>
        {
            { Game.Map.FalseWallSouth.MiddleLocation, new Battalion(Faction, 3 + D(1, 4), SpecialForcesInReserve > 0 ? 1 : 0, Game.Map.FalseWallSouth.MiddleLocation) }
        };

        var forcesLeft = 10 - forceLocations.Sum(kvp => kvp.Value.TotalAmountOfForces);

        if (forcesLeft > 0)
        {
            var amountOfSpecialForces = SpecialForcesInReserve > 1 ? 1 : 0;
            forceLocations.Add(Game.Map.FalseWallWest.MiddleLocation, new Battalion(Faction, forcesLeft - amountOfSpecialForces, amountOfSpecialForces, Game.Map.FalseWallWest.MiddleLocation));
        }

        return new PerformYellowSetup(Game, Faction) { ForceLocations = forceLocations };
    }

    protected YellowSentMonster DetermineYellowSentMonster()
    {
        var target = YellowSentMonster.ValidTargets(Game).HighestOrDefault(t => TotalMaxDialOfOpponents(t) + 2 * AnyForcesIn(t));
        return new YellowSentMonster(Game, Faction) { Territory = target };
    }

    protected YellowRidesMonster DetermineYellowRidesMonster()
    {
        Location target = null;
        var validLocations = YellowRidesMonster.ValidTargets(Game, this).ToList();
        var battalionsToMove = YellowRidesMonster.ValidSources(Game).ToDictionary(l => l, l => BattalionIn(l));
        var forcesFromReserves = Math.Min(YellowRidesMonster.MaxForcesFromReserves(Game, this, false), 4);
        var specialForcesFromReserves = Math.Min(YellowRidesMonster.MaxForcesFromReserves(Game, this, true), 2);

        if (validLocations.Contains(Game.Map.TueksSietch) && VacantAndSafeFromStorm(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
        if (target == null && validLocations.Contains(Game.Map.Carthag) && VacantAndSafeFromStorm(Game.Map.Carthag)) target = Game.Map.Carthag;
        if (target == null && validLocations.Contains(Game.Map.Arrakeen) && VacantAndSafeFromStorm(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
        if (target == null && validLocations.Contains(Game.Map.HabbanyaSietch) && VacantAndSafeFromStorm(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;

        target ??= Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key)).HighestOrDefault(r => r.Value).Key;

        if (target == null)
        {
            var strength = battalionsToMove.Sum(forcesAtLocation => forcesAtLocation.Value.AmountOfForces + forcesAtLocation.Value.AmountOfSpecialForces * 2) + forcesFromReserves + specialForcesFromReserves * 2;

            target ??= validLocations.Where(l => Game.ResourcesOnPlanet.ContainsKey(l) && TotalMaxDialOfOpponents(l.Territory) < strength).HighestOrDefault(l => Game.ResourcesOnPlanet[l]);
            target ??= validLocations.Where(l => l != Game.Map.SietchTabr && l.IsStronghold && TotalMaxDialOfOpponents(l.Territory) < strength).LowestOrDefault(l => TotalMaxDialOfOpponents(l.Territory));
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
        if (AnyForcesIn(Game.Map.Arrakeen) == 0) target = Game.Map.Carthag;
        else if (AnyForcesIn(Game.Map.Carthag) == 0) target = Game.Map.Arrakeen;
        else if (AnyForcesIn(Game.Map.HabbanyaSietch) == 0) target = Game.Map.HabbanyaSietch;
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
            var potentialWinningOpponents = Game.Players.Where(p => p != this && p != AlliedPlayer && Game.MeetsNormalVictoryCondition(p, true) && Game.CountChallengedVictoryPoints(p) < 2);
            var amountICanReinforce = MaxReinforcedDialTo(this, territory);
            var maxDial = MaxDial(this, territory, opponent);

            var dialNeeded = GetDialNeeded(territory, opponent, false);

            if ((!opponent.Is(Faction.Black) && WinWasPredictedByMeThisTurn(opponent.Faction)) ||
                dialNeeded <= maxDial ||
                (LastTurn && dialNeeded - 1 <= amountICanReinforce + maxDial) ||
                (potentialWinningOpponents.Contains(opponent) && dialNeeded <= amountICanReinforce + maxDial))
                return true;
        }

        return false;
    }

    private BlueBattleAnnouncement DetermineBlueBattleAnnouncement()
    {
        if (Game.CurrentMainPhase > MainPhase.Bidding && NrOfBattlesToFight <= Battle.ValidBattleHeroes(Game, this).Count())
        {
            var territory = BlueBattleAnnouncement.ValidTerritories(Game, this).Where(t => IWantToAnnounceBattleIn(t)).LowestOrDefault(t => GetDialNeeded(t, GetOpponentThatOccupies(t), false));

            if (territory != null) return new BlueBattleAnnouncement(Game, Faction) { Territory = territory };
        }

        return null;
    }

    private bool IWantToAnnounceBattleIn(Territory territory)
    {
        var opponent = GetOpponentThatOccupies(territory);

        if (opponent != null)
        {
            var dialNeeded = GetDialNeeded(territory, opponent, false);

            var lastTurnConfidenceBonus = LastTurn ? 3 : 0;

            if ((territory.IsStronghold && WinWasPredictedByMeThisTurn(opponent.Faction)) ||
                dialNeeded + lastTurnConfidenceBonus <= MaxDial(this, territory, opponent) + MaxReinforcedDialTo(this, territory))
                return true;
        }

        return false;
    }

    private BlueAccompanies DetermineBlueAccompanies()
    {
        if (ForcesInReserve > 0)
        {
            var target = BlueAccompanies.ValidTargets(Game, this).FirstOrDefault(l => l.IsStronghold);

            var shippingOpponentCanWin = false;
            if (target != null)
            {
                var opponent = GetOpponentThatOccupies(target);
                var potentialWinningOpponents = Game.Players.Where(p => p != this && p != AlliedPlayer && Game.MeetsNormalVictoryCondition(p, true) && Game.CountChallengedVictoryPoints(p) < 2);
                shippingOpponentCanWin = potentialWinningOpponents.Contains(opponent);
            }

            if (target != null &&
                !shippingOpponentCanWin &&
                (ResourcesIncludingAllyContribution < 5 || ForcesInReserve > 3) &&
                AnyForcesIn(target) < 8 &&
                ((!LastTurn && AnyForcesIn(target) > 0) || (!HasAlly && ForcesInLocations.Count(kvp => IsStronghold(kvp.Key)) <= 3)))
                return new BlueAccompanies(Game, Faction) { Location = target, Accompanies = true, ExtraAdvisor = false };

            if (BlueAccompanies.ValidTargets(Game, this).Contains(Game.Map.PolarSink) &&
                (ResourcesIncludingAllyContribution < 5 || ForcesInReserve > 3) &&
                !(LastTurn && Game.HasActedOrPassed.Contains(Faction)) &&
                AnyForcesIn(Game.Map.PolarSink) < 8)
                return new BlueAccompanies(Game, Faction) { Location = Game.Map.PolarSink, Accompanies = true, ExtraAdvisor = BlueAccompanies.MaySendExtraAdvisor(Game, this, Game.Map.PolarSink) };
        }

        return new BlueAccompanies(Game, Faction) { Location = null, Accompanies = false };
    }

    private BluePrediction DetermineBluePrediction()
    {
        Player predicted;
        if (D(1, 2) == 1) predicted = Others.Where(p => !Game.IsBot(p)).RandomOrDefault();
        predicted = Others.Where(p => !Game.IsBot(p)).RandomOrDefault();
        predicted ??= Others.RandomOrDefault();

        int turn;
        if (predicted.Is(Faction.Black))
        {
            if (D(1, 2) == 1) turn = D(1, Math.Min(3, Game.MaximumTurns));
            else turn = D(1, Game.MaximumTurns);
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

    private VoicePlan voicePlan;
    protected virtual Voice DetermineVoice()
    {
        LogInfo("DetermineVoice()");

        voicePlan = null;

        if (Game.CurrentBattle.IsAggressorOrDefender(this))
        {
            voicePlan = BestVoice(Game.CurrentBattle, this, Game.CurrentBattle.OpponentOf(Faction));
            LogInfo(voicePlan.voice.GetMessage());
            return voicePlan.voice;
        }

        return null;
    }

    private VoicePlan BestVoice(BattleInitiated battle, Player player, Player opponent)
    {
        var result = new VoicePlan
        {
            battle = battle,
            playerHeroWillCertainlySurvive = false,
            opponentHeroWillCertainlyBeZero = false
        };

        if (WinWasPredictedByMeThisTurn(opponent.Faction))
        {
            result.weaponToUse = null;
            result.defenseToUse = null;
            result.voice = new Voice(Game, Faction) { Must = true, Type = TreacheryCardType.Laser };
            result.opponentHeroWillCertainlyBeZero = true;
            result.playerHeroWillCertainlySurvive = true;
        }

        var knownOpponentDefenses = KnownOpponentDefenses(opponent);
        var knownOpponentWeapons = KnownOpponentWeapons(opponent);
        var nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

        var weapons = Weapons(null, null, null).Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth);
        result.weaponToUse = weapons.FirstOrDefault(w => w.Type == TreacheryCardType.ProjectileAndPoison); //use poisonblade if available
        result.weaponToUse ??= weapons.FirstOrDefault(w => w.Type == TreacheryCardType.Laser); //use lasgun if available
        result.weaponToUse ??= weapons.FirstOrDefault(w => Game.KnownCards(this).Contains(w)); //use a known weapon if available
        result.weaponToUse ??= weapons.FirstOrDefault(); //use any weapon

        var type = TreacheryCardType.None;
        var must = false;

        var opponentMightHaveDefenses = !(knownOpponentDefenses.Any() && nrOfUnknownOpponentCards == 0);

        var cardsPlayerHasOrMightHave = CardsPlayerHasOrMightHave(opponent);

        if (opponentMightHaveDefenses && result.weaponToUse != null)
        {
            result.opponentHeroWillCertainlyBeZero = true;

            //opponent might have defenses and player has a weapon. Use voice to disable the corresponding defense, if possible by forcing use of the wrong defense and otherwise by forcing not to use the correct defense.
            if (result.weaponToUse.Type == TreacheryCardType.ProjectileAndPoison)
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
            else if (result.weaponToUse.IsLaser)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d.IsPoisonDefense && !d.IsShield);
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
            else if (result.weaponToUse.IsProjectileWeapon)
            {
                var uselessDefense = knownOpponentDefenses.FirstOrDefault(d => d.IsPoisonDefense && !d.IsProjectileDefense);
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
            else if (result.weaponToUse.IsPoisonWeapon)
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

            LogInfo("Using {0}, disable enemy defense: {1} {2}", result.weaponToUse, must ? "must use" : "may not use", type);
        }

        if (type == TreacheryCardType.None)
        {
            var opponentMightHaveWeapons = !(knownOpponentWeapons.Any() && nrOfUnknownOpponentCards == 0);

            //I have no weapon or the opponent has no defense. Focus on disabling their weapon.
            DetermineBestDefense(opponent, null, out result.defenseToUse);

            if (opponentMightHaveWeapons && result.defenseToUse != null)
            {
                //opponent might have weapons and player has a defense. Use voice to disable the corresponding weapon, if possible by forcing use of the wrong weapon and otherwise by forcing not to use the correct weapon.
                if (result.defenseToUse.Type == TreacheryCardType.ShieldAndAntidote)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => w.Type != TreacheryCardType.Laser && w.Type != TreacheryCardType.PoisonTooth && w.Type != TreacheryCardType.Rockmelter);
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.playerHeroWillCertainlySurvive = true;
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

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.defenseToUse, must ? "must use" : "may not use", type);
                }
                else if (result.defenseToUse.IsProjectileDefense)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => w.IsProjectileWeapon && !w.IsPoisonWeapon);
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.playerHeroWillCertainlySurvive = true;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Laser))
                    {
                        must = false;
                        type = TreacheryCardType.Laser;
                        result.playerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.PoisonTooth))
                    {
                        must = false;
                        type = TreacheryCardType.PoisonTooth;
                        result.playerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
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

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.defenseToUse, must ? "must use" : "may not use", type);
                }
                else if (result.defenseToUse.IsPoisonDefense)
                {
                    var uselessWeapon = knownOpponentWeapons.FirstOrDefault(w => !w.IsProjectileWeapon && w.IsPoisonWeapon);
                    if (uselessWeapon != null)
                    {
                        must = true;
                        type = uselessWeapon.Type;
                        result.playerHeroWillCertainlySurvive = true;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.Laser))
                    {
                        must = false;
                        type = TreacheryCardType.Laser;
                        result.playerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (knownOpponentWeapons.Any(w => w.Type == TreacheryCardType.PoisonTooth))
                    {
                        must = false;
                        type = TreacheryCardType.PoisonTooth;
                        result.playerHeroWillCertainlySurvive = nrOfUnknownOpponentCards == 0;
                    }
                    else if (cardsPlayerHasOrMightHave.Any(c => c.IsProjectileWeapon))
                    {
                        must = false;
                        type = TreacheryCardType.Projectile;
                    }

                    LogInfo("Using {0}, disable enemy weapon: {1} {2}", result.defenseToUse, must ? "must use" : "may not use", type);
                }
            }
        }

        if (type == TreacheryCardType.None && opponent.TreacheryCards.Any(c => Game.KnownCards(this).Contains(c) && c.Type == TreacheryCardType.Mercenary))
        {
            type = TreacheryCardType.Mercenary;
            must = true;
        }

        if (type == TreacheryCardType.None)
        {
            must = false;
            if (D(1, 2) > 1)
            {
                if (cardsPlayerHasOrMightHave.Any(c => c.IsProjectileWeapon))
                    type = TreacheryCardType.Projectile;
                else
                    type = TreacheryCardType.Poison;
            }
            else
            {
                if (cardsPlayerHasOrMightHave.Any(c => c.IsPoisonWeapon))
                    type = TreacheryCardType.Poison;
                else
                    type = TreacheryCardType.Projectile;
            }

            LogInfo("Remaining category: {0} {1}", must ? "must use" : "may not use", type);
        }

        var voice = new Voice(Game, player.Faction) { Must = must, Type = type };
        result.voice = voice;

        return result;
    }

    #endregion Blue

    #region Green

    protected virtual Prescience DeterminePrescience()
    {
        var opponent = Game.CurrentBattle.OpponentOf(Faction);

        if (Voice.MayUseVoice(Game, opponent))
            if (Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null)
                //Wait for voice or finalized battle plan
                return null;

        if (Game.CurrentBattle.IsAggressorOrDefender(this))
        {
            var existingAspect = Game.CurrentPrescience != null ? Game.CurrentPrescience.Aspect : PrescienceAspect.None;
            return new Prescience(Game, Faction) { Aspect = BestPrescience(opponent, MaxDial(this, Game.CurrentBattle.Territory, opponent), existingAspect, Game.CurrentBattle.Territory) };
        }

        return null;
    }


    protected PrescienceAspect BestPrescience(Player opponent, float maxForceStrengthInBattle, PrescienceAspect earlierPrescience, Territory territory)
    {
        var myDefenses = Battle.ValidDefenses(Game, this, null, territory).Where(c => Game.KnownCards(this).Contains(c));
        var myWeapons = Battle.ValidWeapons(Game, this, null, null, territory).Where(c => Game.KnownCards(this).Contains(c));

        var knownOpponentDefenses = Battle.ValidDefenses(Game, opponent, null, territory).Where(c => Game.KnownCards(this).Contains(c));
        var knownOpponentWeapons = Battle.ValidWeapons(Game, opponent, null, null, territory).Where(c => Game.KnownCards(this).Contains(c));
        //int nrOfUnknownOpponentCards = opponent.TreacheryCards.Count(c => !Game.KnownCards(this).Contains(c));

        var cardsOpponentHasOrMightHave = CardsPlayerHasOrMightHave(opponent);
        var weaponIsCertain = CountDifferentWeaponTypes(cardsOpponentHasOrMightHave) <= 1;
        var defenseIsCertain = CountDifferentDefenseTypes(cardsOpponentHasOrMightHave) <= 1;

        var iHaveShieldSnooper = myDefenses.Any(d => d.Type == TreacheryCardType.ShieldAndAntidote);
        var iHavePoisonBlade = myWeapons.Any(d => d.Type == TreacheryCardType.ProjectileAndPoison);

        PrescienceAspect aspect;
        if (earlierPrescience != PrescienceAspect.Weapon && !weaponIsCertain && myDefenses.Any(d => d.IsProjectileDefense) && myDefenses.Any(d => d.IsPoisonDefense) && !iHaveShieldSnooper)
            //I dont have shield snooper and I have choice between shield and snooper, therefore ask for the weapon used
            aspect = PrescienceAspect.Weapon;
        else if (earlierPrescience != PrescienceAspect.Defense && !defenseIsCertain && myWeapons.Any(d => d.IsProjectileWeapon) && myWeapons.Any(d => d.IsPoisonWeapon) && !iHavePoisonBlade)
            //I dont have poison blade and I have choice between poison weapon and projectile weapon, therefore ask for the defense used
            aspect = PrescienceAspect.Defense;
        else if (earlierPrescience != PrescienceAspect.Weapon && !weaponIsCertain && myDefenses.Any() && !iHaveShieldSnooper)
            aspect = PrescienceAspect.Weapon;
        else if (earlierPrescience != PrescienceAspect.Defense && !defenseIsCertain && myWeapons.Any() && !iHavePoisonBlade)
            aspect = PrescienceAspect.Defense;
        else if (earlierPrescience != PrescienceAspect.Dial && maxForceStrengthInBattle > 2 && Prescience.ValidAspects(Game, this).Contains(PrescienceAspect.Dial))
            aspect = PrescienceAspect.Dial;
        else if (earlierPrescience != PrescienceAspect.Leader)
            aspect = PrescienceAspect.Leader;
        else
            aspect = PrescienceAspect.None;

        return aspect;
    }

    #endregion Green

    #region Purple

    protected SetIncreasedRevivalLimits DetermineSetIncreasedRevivalLimits()
    {
        var targets = SetIncreasedRevivalLimits.ValidTargets(Game, this).ToArray();
        if (Game.FactionsWithIncreasedRevivalLimits.Length != targets.Length)
            return new SetIncreasedRevivalLimits(Game, Faction) { Factions = targets };
        return null;
    }

    protected virtual FaceDancerRevealed DetermineFaceDancerRevealed()
    {
        if (FaceDanced.MayCallFaceDancer(Game, this))
        {
            var facedancer = FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));
            var facedancedHeroIsLivingLeader = facedancer is Leader && Game.IsAlive(facedancer);

            if ((FaceDanced.MaximumNumberOfForces(Game, this) > 0 || facedancedHeroIsLivingLeader) && Game.BattleWinner != Ally)
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

        var toPlace = FaceDanced.MaximumNumberOfForces(Game, this);

        var biggest = BiggestBattalionThreatenedByStormWithoutSpice;
        if (biggest.Key != null)
        {
            var toTake = biggest.Value.Take(toPlace, false);
            forcesFromPlanet.Add(biggest.Key, toTake);
            toPlace -= toTake.TotalAmountOfForces;
        }

        var fromReserves = Math.Min(ForcesInReserve, toPlace);

        var targetLocation = FaceDanced.ValidTargetLocations(Game).FirstOrDefault(l => Game.ResourcesOnPlanet.ContainsKey(l));
        targetLocation ??= FaceDanced.ValidTargetLocations(Game).FirstOrDefault();

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
        var replacable = FaceDancers.Where(f => !RevealedDancers.Contains(f)).OrderBy(f => f.Value);
        var toReplace = replacable.FirstOrDefault(f => Leaders.Contains(f) || (Ally != Faction.None && AlliedPlayer.Leaders.Contains(f)));
        toReplace ??= replacable.FirstOrDefault(f => f is Leader && !Game.IsAlive(f));

        if (toReplace != null)
            return new FaceDancerReplaced(Game, Faction) { Passed = false, SelectedDancer = toReplace };
        return new FaceDancerReplaced(Game, Faction) { Passed = true };
    }

    private readonly Dictionary<IHero, int> priceSetEarlier = new();
    private int turnWhenRevivalWasRequested = -1;

    protected virtual RequestPurpleRevival DetermineRequestPurpleRevival()
    {
        if (Game.CurrentRevivalRequests.Any(r => r.Initiator == Faction) || Game.EarlyRevivalsOffers.Keys.Any(h => h.Faction == Faction)) return null;

        if (turnWhenRevivalWasRequested == Game.CurrentTurn) return null;

        var toRevive = RequestPurpleRevival.ValidTargets(Game, this).Where(l => SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);

        if (toRevive == null)
        {
            var knownOpponentTraitors = Opponents.SelectMany(p => p.RevealedTraitors);
            toRevive = RequestPurpleRevival.ValidTargets(Game, this).Where(l => !knownOpponentTraitors.Contains(l)).HighestOrDefault(l => l.Value);
        }

        if (toRevive != null && Battle.ValidBattleHeroes(Game, this).Count() <= 3) toRevive = RequestPurpleRevival.ValidTargets(Game, this).HighestOrDefault(l => l.Value);

        if (toRevive != null)
        {
            turnWhenRevivalWasRequested = Game.CurrentTurn;
            return new RequestPurpleRevival(Game, Faction) { Hero = toRevive };
        }

        return null;
    }

    protected virtual AcceptOrCancelPurpleRevival DetermineAcceptOrCancelPurpleRevival()
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
            else if (priceSetEarlier.ContainsKey(hero))
            {
                price = priceSetEarlier[hero];
            }
            else
            {
                if ((FaceDancers.Contains(hero) && !RevealedDancers.Contains(hero)) || (Ally != Faction.None && AlliedPlayer.Traitors.Contains(hero) && !AlliedPlayer.RevealedTraitors.Contains(hero)))
                    price = 1 + D(1, hero.Value);
                else
                    price = 2 + D(2, hero.Value);

                priceSetEarlier.Add(hero, price);
            }

            return new AcceptOrCancelPurpleRevival(Game, Faction) { Hero = hero, Price = price, Cancel = false };
        }

        return null;
    }

    #endregion Purple

    #region Brown

    protected ResourcesAudited DetermineResourcesAudited()
    {
        if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(this))
        {
            var opponent = Game.CurrentBattle.OpponentOf(this);

            if (opponent.TreacheryCards.Any(tc => !KnownCards.Contains(tc)) && ResourcesAudited.ValidFactions(Game, this).Contains(opponent.Faction)) return new ResourcesAudited(Game, Faction) { Target = opponent.Faction };
        }

        return null;
    }

    protected virtual BrownEconomics DetermineBrownEconomics()
    {
        if (ResourcesIncludingAllyContribution >= 14 &&
            Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || (Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue)) >= 2)
            return new BrownEconomics(Game, Faction) { Status = BrownEconomicsStatus.Cancel };
        if (ResourcesIncludingAllyContribution <= 8 &&
            Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || (Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue)) <= 2)
            return new BrownEconomics(Game, Faction) { Status = BrownEconomicsStatus.Double };

        return null;
    }

    protected virtual BrownRemoveForce DetermineBrownRemoveForce()
    {
        var opponents = WinningOpponentsIWishToAttack(20, true);

        var stronghold = Game.Map.Carthag;
        var opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);

        if (opponentWithOneBattaltionInStronghold == null)
        {
            stronghold = Game.Map.Arrakeen;
            opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattaltionInStronghold == null)
        {
            stronghold = Game.Map.TueksSietch;
            opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattaltionInStronghold == null)
        {
            stronghold = Game.Map.SietchTabr;
            opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattaltionInStronghold == null)
        {
            stronghold = Game.Map.HabbanyaSietch;
            opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattaltionInStronghold == null)
        {
            stronghold = Game.Map.HiddenMobileStronghold;
            opponentWithOneBattaltionInStronghold = opponents.FirstOrDefault(opp => opp.BattalionIn(stronghold).TotalAmountOfForces == 1);
        }

        if (opponentWithOneBattaltionInStronghold != null) return new BrownRemoveForce(Game, Faction) { Location = stronghold, Target = opponentWithOneBattaltionInStronghold.Faction, SpecialForce = opponentWithOneBattaltionInStronghold.SpecialForcesIn(stronghold) > 0 };

        return null;
    }

    #endregion

    #region Cyan

    protected virtual ExtortionPrevented DetermineExtortionPrevented()
    {
        //This is for now just random
        return null;
    }

    protected PerformCyanSetup DeterminePerformCyanSetup()
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
            target = PerformCyanSetup.ValidLocations(Game).FirstOrDefault(l => l.IsProtectedFromStorm);

        return new PerformCyanSetup(Game, Faction) { Target = target };
    }

    protected virtual TerrorPlanted DetermineTerrorPlanted()
    {
        if (Game.CurrentMainPhase is not MainPhase.Contemplate)
            return null;
        
        var availableToPlace = TerrorPlanted.ValidTerrorTypes(Game, false).Where(t => !Game.TerrorOnPlanet.ContainsKey(t));

        if (availableToPlace.Any())
        {
            var type = TerrorType.None;

            if (type == TerrorType.None && availableToPlace.Contains(TerrorType.Extortion) && Opponents.Sum(p => p.Resources) > Opponents.Count() * 10) type = TerrorType.Robbery;
            if (type == TerrorType.None && availableToPlace.Contains(TerrorType.Atomics) && TerrorPlanted.ValidStrongholds(Game, this).Any(t => Opponents.Sum(o => o.AnyForcesIn(t)) > 12 && MeAndMyAlly.Sum(o => o.AnyForcesIn(t)) < 2)) type = TerrorType.Atomics;
            if (type == TerrorType.None && availableToPlace.Contains(TerrorType.Extortion) && Resources < 5) type = TerrorType.Extortion;
            if (type == TerrorType.None) type = availableToPlace.RandomOrDefault();

            var stronghold = type == TerrorType.Atomics ? TerrorPlanted.ValidStrongholds(Game, this).HighestOrDefault(t => Opponents.Sum(o => o.AnyForcesIn(t))) : null;
            if (stronghold == null) stronghold = TerrorPlanted.ValidStrongholds(Game, this).HighestOrDefault(t => AnyForcesIn(t));
            if (stronghold == null && HasAlly) stronghold = TerrorPlanted.ValidStrongholds(Game, this).HighestOrDefault(t => AlliedPlayer.AnyForcesIn(t) > 0);
            stronghold ??= TerrorPlanted.ValidStrongholds(Game, this).RandomOrDefault();

            if (stronghold != null) return new TerrorPlanted(Game, Faction) { Type = type, Stronghold = stronghold };
        }

        return new TerrorPlanted(Game, Faction) { Passed = true }; ;
    }

    protected virtual TerrorRevealed DetermineTerrorRevealed()
    {
        //This is for now just quite random
        var territory = TerrorRevealed.GetTerritory(Game);
        var mayUseAtomics = Opponents.Sum(o => o.AnyForcesIn(TerrorRevealed.GetTerritory(Game))) > 5 &&
                            AlliedPlayer?.AnyForcesIn(territory) == 0; 
            
        var validTokens = TerrorRevealed.GetTypes(Game).Where(t => 
            (t != TerrorType.SneakAttack || TerrorRevealed.ValidSneakAttackTargets(Game, this).Any()) &&
            (t != TerrorType.Atomics || mayUseAtomics)
        );

        if (!validTokens.Any()) return new TerrorRevealed(Game, Faction) { Passed = true };

        var type = TerrorRevealed.GetTypes(Game).Where(t => 
            (t != TerrorType.SneakAttack || TerrorRevealed.ValidSneakAttackTargets(Game, this).Any()) &&
            (t != TerrorType.Atomics || mayUseAtomics)).RandomOrDefault();
        var cardInSabotage = TreacheryCards.FirstOrDefault(c => c.IsUseless);
        var victim = Game.GetPlayer(TerrorRevealed.GetVictim(Game));

        var offerAlliance = TerrorRevealed.MayOfferAlliance(Game) && PlayerStanding(victim) > 1.5f * PlayerStanding(this);

        if (offerAlliance)
            return new TerrorRevealed(Game, Faction) { AllianceOffered = true };
        return new TerrorRevealed(Game, Faction) { Type = type, RobberyTakesCard = false, CardToGiveInSabotage = cardInSabotage, ForcesInSneakAttack = TerrorRevealed.MaxAmountOfForcesInSneakAttack(this), SneakAttackTo = TerrorRevealed.ValidSneakAttackTargets(Game, this).FirstOrDefault() };
    }

    #endregion

    #region Pink

    protected virtual AmbassadorPlaced DetermineAmbassadorPlaced()
    {
        if ((Resources > 1 && Game.AmbassadorsPlacedThisTurn == 0) || Resources > 3 + Game.AmbassadorsPlacedThisTurn)
        {
            var stronghold = AmbassadorPlaced.ValidStrongholds(Game, this).Where(s => AnyForcesIn(s) > 0).RandomOrDefault();
            if (stronghold == null && HasAlly) stronghold = AmbassadorPlaced.ValidStrongholds(Game, this).Where(s => AlliedPlayer.AnyForcesIn(s) > 0).RandomOrDefault();
            var avoidEntering = stronghold != null;

            stronghold ??= AmbassadorPlaced.ValidStrongholds(Game, this).Where(s => Vacant(s)).RandomOrDefault();
            stronghold ??= AmbassadorPlaced.ValidStrongholds(Game, this).RandomOrDefault();

            var ambassador = Ambassador.None;
            var availableAmbassadors = AmbassadorPlaced.ValidAmbassadors(this).ToList();
            if (avoidEntering)
            {
                if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Black)) ambassador = Ambassador.Black;
                if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Green)) ambassador = Ambassador.Green;
            }

            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Purple) && Leaders.Count(l => !Game.IsAlive(l)) >= 2 && ForcesKilled >= 4) ambassador = Ambassador.Purple;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Orange) && TreacheryCards.Any(c => c.IsWeapon) && TreacheryCards.Any(c => c.IsDefense)) ambassador = Ambassador.Orange;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Brown) && TreacheryCards.Count(c => CardQuality(c, this) <= 1) >= 2) ambassador = Ambassador.Brown;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Grey) && TreacheryCards.Any(c => CardQuality(c, this) <= 1)) ambassador = Ambassador.Grey;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.White) && HasRoomForCards && Resources > 6) ambassador = Ambassador.White;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Red) && Resources <= 4) ambassador = Ambassador.Red;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Yellow) && DetermineMovedBatallion(false) != null) ambassador = Ambassador.Yellow;
            if (ambassador == Ambassador.None && availableAmbassadors.Contains(Ambassador.Pink) && !HasAlly) ambassador = Ambassador.Pink;
            if (Game.AmbassadorsPlacedThisTurn == 0 && ambassador == Ambassador.None) ambassador = AmbassadorPlaced.ValidAmbassadors(this).RandomOrDefault();

            if (ambassador != Ambassador.None) return new AmbassadorPlaced(Game, Faction) { Ambassador = ambassador, Stronghold = stronghold };
        }

        return null;

    }

    protected virtual AmbassadorActivated DetermineAmbassadorActivated()
    {
        var victim = AmbassadorActivated.GetVictim(Game);
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
                var toDiscard = AmbassadorActivated.GetValidBrownCards(this).Where(c => CardQuality(c, this) < 2);
                if (toDiscard.Any())
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, BrownCards = toDiscard };
                return PassAmbassadorActivated();

            case Ambassador.Pink:
                var offerAlliance = AmbassadorActivated.AllianceCanBeOffered(Game, this) && PlayerStanding(victimPlayer) > 0.33 * PlayerStanding(this);
                var takeVidal = AmbassadorActivated.VidalCanBeTaken(Game, this);
                var offerVidal = offerAlliance && AmbassadorActivated.VidalCanBeOfferedToNewAlly(Game, this) && takeVidal && HeroesForBattle(this, true).Count() >= 3 && HeroesForBattle(victimPlayer, true).Count() < 3;

                if (offerAlliance || takeVidal || offerVidal)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PinkOfferAlliance = offerAlliance, PinkTakeVidal = takeVidal, PinkGiveVidalToAlly = offerVidal };
                return PassAmbassadorActivated();

            case Ambassador.Yellow:

                Location target = null;
                var validLocations = AmbassadorActivated.ValidYellowTargets(Game, this).ToList();
                if (validLocations.Contains(Game.Map.TueksSietch) && VacantAndSafeFromStorm(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
                if (target == null && validLocations.Contains(Game.Map.Carthag) && VacantAndSafeFromStorm(Game.Map.Carthag)) target = Game.Map.Carthag;
                if (target == null && validLocations.Contains(Game.Map.Arrakeen) && VacantAndSafeFromStorm(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
                if (target == null && validLocations.Contains(Game.Map.HabbanyaSietch) && VacantAndSafeFromStorm(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;
                target ??= Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key)).HighestOrDefault(r => r.Value).Key;

                if (target != null)
                {
                    var toMove = DetermineMovedBatallion(true);
                    if (toMove != null && AmbassadorActivated.ValidYellowSources(Game, this).Contains(toMove.From.Territory))
                    {
                        var forcesToMove = new Dictionary<Location, Battalion>
                        {
                            { toMove.From, toMove.Batallion }
                        };

                        return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, YellowOrOrangeTo = target, YellowForceLocations = forcesToMove };
                    }
                }

                return PassAmbassadorActivated();

            case Ambassador.Grey:
                var toReplace = AmbassadorActivated.GetValidGreyCards(this).FirstOrDefault(c => CardQuality(c, this) < 2);
                if (toReplace != null)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, GreyCard = toReplace };
                return PassAmbassadorActivated();

            case Ambassador.White:
                if (Resources > 3 && HasRoomForCards)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador };
                return PassAmbassadorActivated();

            case Ambassador.Orange:

                var potentialTargets = AmbassadorActivated.ValidOrangeTargets(Game, this).Where(l => l.IsStronghold);

                var dangerousOpponents = Opponents.Where(p => IsAlmostWinningOpponent(p));

                var possibleAttacks = potentialTargets
                    .Where(l => !dangerousOpponents.Any() || dangerousOpponents.Any(p => p.Occupies(l)))
                    .Where(l => l.Territory.IsStronghold && AnyForcesIn(l) == 0 && AllyDoesntBlock(l.Territory) && !StormWillProbablyHit(l) && !InStorm(l) && IDontHaveAdvisorsIn(l))
                    .Select(l => ConstructAttack(l, 0, 0, 4))
                    .Where(s => s.ForcesToShip <= 4 && s.HasOpponent && !WinWasPredictedByMeThisTurn(s.Opponent.Faction));

                var attack = possibleAttacks.Where(s => s.HasForces && s.ShortageForShipment == 0).LowestOrDefault(s => s.DialNeeded + DeterminePenalty(s.Opponent));

                if (attack != null)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, YellowOrOrangeTo = attack.Location, OrangeForceAmount = AmbassadorActivated.ValidOrangeMaxForces(this) };
                return PassAmbassadorActivated();

            case Ambassador.Purple:

                var heroToRevive = AmbassadorActivated.ValidPurpleHeroes(Game, this).Where(l => SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);
                heroToRevive ??= AmbassadorActivated.ValidPurpleHeroes(Game, this).HighestOrDefault(l => l.Value);

                if (heroToRevive != null && (ForcesInReserve > 3 || Battle.ValidBattleHeroes(Game, this).Count() <= 1))
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PurpleHero = heroToRevive, PurpleAssignSkill = Revival.MayAssignSkill(Game, this, heroToRevive) };
                if (AmbassadorActivated.ValidPurpleMaxAmount(this) >= 3)
                    return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador, PurpleAmountOfForces = AmbassadorActivated.ValidPurpleMaxAmount(this) };
                return PassAmbassadorActivated();

            default:
                return new AmbassadorActivated(Game, Faction) { BlueSelectedAmbassador = blueSelectedAmbassador };

        }
    }

    private AmbassadorActivated PassAmbassadorActivated()
    {
        return AmbassadorActivated.MayPass(this) ? new AmbassadorActivated(Game, Faction) { Passed = true } : null;
    }

    #endregion


    #region White

    protected virtual WhiteAnnouncesBlackMarket DetermineWhiteAnnouncesBlackMarket()
    {
        var card = TreacheryCards.FirstOrDefault(c => CardQuality(c, this) < 3);
        if (card != null)
            return new WhiteAnnouncesBlackMarket(Game, Faction) { Passed = false, Card = card, AuctionType = D(1, 2) > 1 ? AuctionType.BlackMarketSilent : AuctionType.BlackMarketOnceAround, Direction = D(1, 2) > 1 ? 1 : -1 };
        return new WhiteAnnouncesBlackMarket(Game, Faction) { Passed = true };
    }

    protected virtual WhiteRevealedNoField DetermineWhiteRevealedNoField()
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
        var toAuction = new Deck<TreacheryCard>(Game.WhiteCache, random);
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
    public BattleInitiated battle;
    public Voice voice;
    public TreacheryCard weaponToUse;
    public TreacheryCard defenseToUse;
    public bool playerHeroWillCertainlySurvive;
    public bool opponentHeroWillCertainlyBeZero;
}