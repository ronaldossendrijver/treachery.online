/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        #region Grey

        protected virtual ReplacedCardWon DetermineReplacedCardWon()
        {
            var replace = Game.CardJustWon != null && CardQuality(Game.CardJustWon) <= 2;
            return new ReplacedCardWon(Game) { Initiator = Faction, Passed = !replace };
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
                {
                    //Remove the best card from auction
                    toBeRemoved = Game.CardsOnAuction.Items.HighestOrDefault(c => CardQuality(c));
                }
                else
                {
                    //Remove the worst card from auction
                    toBeRemoved = Game.CardsOnAuction.Items.LowestOrDefault(c => CardQuality(c));
                }
            }

            if (toBeRemoved == null) Game.CardsOnAuction.Items.FirstOrDefault();

            bool putOnTop = CardQuality(toBeRemoved) <= 2 && Ally == Faction.None || CardQuality(toBeRemoved) >= 4 && Ally != Faction.None;

            return new GreyRemovedCardFromAuction(Game) { Initiator = Faction, Card = toBeRemoved, PutOnTop = putOnTop };
        }

        protected GreySwappedCardOnBid DetermineGreySwappedCardOnBid()
        {
            var card = TreacheryCards.LowestOrDefault(c => CardQuality(c));

            if (card != null && CardQuality(card) <= 2)
            {
                return new GreySwappedCardOnBid(Game) { Initiator = Faction, Passed = false, Card = card };
            }
            else
            {
                return new GreySwappedCardOnBid(Game) { Initiator = Faction, Passed = true };
            }
        }

        protected GreySelectedStartingCard DetermineGreySelectedStartingCard()
        {
            var cards = Game.StartingTreacheryCards.Items;

            var card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.ProjectileAndPoison);
            if (card == null) card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.ShieldAndAntidote);
            if (card == null) card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.Laser);

            if (card == null && cards.Any(c => c.IsPoisonWeapon) && !cards.Any(c => c.IsPoisonDefense))
            {
                card = cards.FirstOrDefault(c => c.IsPoisonWeapon);
            }

            if (card == null && cards.Any(c => c.IsProjectileWeapon) && !cards.Any(c => c.IsProjectileDefense))
            {
                card = cards.FirstOrDefault(c => c.IsProjectileWeapon);
            }

            if (card == null) card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.WeirdingWay);
            if (card == null) card = cards.FirstOrDefault(c => c.Type == TreacheryCardType.Chemistry);
            if (card == null) card = cards.FirstOrDefault(c => c.IsProjectileWeapon);
            if (card == null) card = cards.FirstOrDefault(c => c.IsPoisonWeapon);
            if (card == null) card = cards.FirstOrDefault(c => c.IsProjectileDefense);
            if (card == null) card = cards.FirstOrDefault(c => c.IsPoisonDefense);

            if (card == null) card = cards.FirstOrDefault(c => c.Type != TreacheryCardType.Useless);

            if (card == null) card = cards.FirstOrDefault();

            return new GreySelectedStartingCard(Game) { Initiator = Faction, Card = card };
        }

        protected PerformHmsMovement DeterminePerformHmsMovement()
        {
            var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;

            var richestAdjacentSpiceLocation = PerformHmsMovement.ValidLocations(Game).Where(l => l != currentLocation && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
            if (richestAdjacentSpiceLocation != null)
            {
                return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = richestAdjacentSpiceLocation };
            }

            var reachableFromCurrentLocation = Game.Map.FindNeighbours(currentLocation, Game.HmsMovesLeft, false, Faction, Game, false);

            var richestReachableSpiceLocation = reachableFromCurrentLocation.Where(l => l != currentLocation && ResourcesIn(l) > 0).HighestOrDefault(l => ResourcesIn(l));
            if (richestReachableSpiceLocation != null)
            {
                var nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, richestReachableSpiceLocation, 1)).FirstOrDefault();

                if (nextStepTowardsSpice != null)
                {
                    return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = nextStepTowardsSpice };
                }
                else
                {
                    nextStepTowardsSpice = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, richestReachableSpiceLocation, 2)).FirstOrDefault();
                    return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = nextStepTowardsSpice };
                }
            }

            //If there is nowhere to go, move toward Imperial Basin or Polar Sink
            Location alternativeLocation = null;
            if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
            {
                alternativeLocation = Game.Map.ImperialBasin.MiddleLocation;
            }
            else
            {
                alternativeLocation = Game.Map.PolarSink;
            }

            if (alternativeLocation != currentLocation)
            {
                var nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).FirstOrDefault(l => l == alternativeLocation);
                if (nextStepTowardsAlternative != null)
                {
                    return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = nextStepTowardsAlternative };
                }

                nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, alternativeLocation, 1)).FirstOrDefault();
                if (nextStepTowardsAlternative != null)
                {
                    return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = nextStepTowardsAlternative };
                }

                nextStepTowardsAlternative = PerformHmsMovement.ValidLocations(Game).Where(l => WithinDistance(l, alternativeLocation, 2)).FirstOrDefault();
                if (nextStepTowardsAlternative != null)
                {
                    return new PerformHmsMovement(Game) { Initiator = Faction, Passed = false, Target = nextStepTowardsAlternative };
                }
            }

            return new PerformHmsMovement(Game) { Initiator = Faction, Passed = true };
        }

        protected PerformHmsPlacement DeterminePerformHmsPlacement()
        {
            if (Faction == Faction.Grey)
            {
                if (Game.BattalionsIn(Game.Map.Arrakeen).Sum(b => b.TotalAmountOfForces) == 0 || Game.BattalionsIn(Game.Map.Carthag).Sum(b => b.TotalAmountOfForces) == 0)
                {
                    return new PerformHmsPlacement(Game) { Initiator = Faction, Target = PerformHmsPlacement.ValidLocations(Game, this).First(l => l.Territory == Game.Map.ImperialBasin) };
                }
                else
                {
                    return new PerformHmsPlacement(Game) { Initiator = Faction, Target = Game.Map.PolarSink };
                }
            }
            else
            {
                return new PerformHmsPlacement(Game) { Initiator = Faction, Target = Game.Map.ShieldWall.Locations.First() };
            }
        }

        #endregion Grey

        #region Yellow

        protected TakeLosses DetermineTakeLosses()
        {
            int normalForces = Math.Min(TakeLosses.LossesToTake(Game).Amount, TakeLosses.ValidMaxForceAmount(Game, this));
            int specialForces = TakeLosses.LossesToTake(Game).Amount - normalForces;
            bool useUseless = TakeLosses.ValidUselessCardToPreventLosses(Game, this) != null;
            return new TakeLosses(Game) { Initiator = Faction, ForceAmount = normalForces, SpecialForceAmount = specialForces, UseUselessCard = useUseless };
        }

        protected PerformYellowSetup DeterminePerformYellowSetup()
        {
            var forceLocations = new Dictionary<Location, Battalion>
            {
                { Game.Map.FalseWallSouth.MiddleLocation, new Battalion() { Faction = Faction, AmountOfForces = 3 + D(1, 4), AmountOfSpecialForces = SpecialForcesInReserve > 0 ? 1 : 0 } }
            };

            int forcesLeft = 10 - forceLocations.Sum(kvp => kvp.Value.TotalAmountOfForces);

            if (forcesLeft > 0)
            {
                var amountOfSpecialForces = SpecialForcesInReserve > 1 ? 1 : 0;
                forceLocations.Add(Game.Map.FalseWallWest.MiddleLocation, new Battalion() { Faction = Faction, AmountOfForces = forcesLeft - amountOfSpecialForces, AmountOfSpecialForces = amountOfSpecialForces });
            }

            return new PerformYellowSetup(Game) { Initiator = Faction, ForceLocations = forceLocations };
        }

        protected YellowSentMonster DetermineYellowSentMonster()
        {
            var target = YellowSentMonster.ValidTargets(Game).HighestOrDefault(t => TotalMaxDialOfOpponents(t) + 2 * AnyForcesIn(t));
            return new YellowSentMonster(Game) { Initiator = Faction, Territory = target };
        }

        protected YellowRidesMonster DetermineYellowRidesMonster()
        {
            Location target = null;
            var validLocations = YellowRidesMonster.ValidTargets(Game, this).ToList();
            var battalionsToMove = ForcesOnPlanet.Where(forcesAtLocation => YellowRidesMonster.ValidSources(Game).Contains(forcesAtLocation.Key.Territory));
            var nrOfForces = battalionsToMove.Sum(forcesAtLocation => forcesAtLocation.Value.TotalAmountOfForces);

            if (validLocations.Contains(Game.Map.TueksSietch) && VacantAndSafeFromStorm(Game.Map.TueksSietch)) target = Game.Map.TueksSietch;
            if (target == null && validLocations.Contains(Game.Map.Carthag) && VacantAndSafeFromStorm(Game.Map.Carthag)) target = Game.Map.Carthag;
            if (target == null && validLocations.Contains(Game.Map.Arrakeen) && VacantAndSafeFromStorm(Game.Map.Arrakeen)) target = Game.Map.Arrakeen;
            if (target == null && validLocations.Contains(Game.Map.HabbanyaSietch) && VacantAndSafeFromStorm(Game.Map.HabbanyaSietch)) target = Game.Map.HabbanyaSietch;

            if (target == null && Game.LatestSpiceCardA != null && validLocations.Contains(Game.LatestSpiceCardA.Location) && Game.ResourcesOnPlanet.ContainsKey(Game.LatestSpiceCardA.Location) && VacantAndSafeFromStorm(Game.LatestSpiceCardA.Location)) target = Game.LatestSpiceCardA.Location;
            if (target == null && Game.LatestSpiceCardB != null && validLocations.Contains(Game.LatestSpiceCardB.Location) && Game.ResourcesOnPlanet.ContainsKey(Game.LatestSpiceCardB.Location) && VacantAndSafeFromStorm(Game.LatestSpiceCardB.Location)) target = Game.LatestSpiceCardB.Location;
            if (target == null) target = Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key)).HighestOrDefault(r => r.Value).Key;

            if (target == null && Game.LatestSpiceCardA != null && validLocations.Contains(Game.LatestSpiceCardA.Location) && Game.ResourcesOnPlanet.ContainsKey(Game.LatestSpiceCardA.Location) && TotalMaxDialOfOpponents(Game.LatestSpiceCardA.Location.Territory) + 3 < nrOfForces) target = Game.LatestSpiceCardA.Location;
            if (target == null && Game.LatestSpiceCardB != null && validLocations.Contains(Game.LatestSpiceCardB.Location) && Game.ResourcesOnPlanet.ContainsKey(Game.LatestSpiceCardB.Location) && TotalMaxDialOfOpponents(Game.LatestSpiceCardB.Location.Territory) + 3 < nrOfForces) target = Game.LatestSpiceCardB.Location;
            if (target == null) target = Game.ResourcesOnPlanet.Where(l => validLocations.Contains(l.Key) && VacantAndSafeFromStorm(l.Key) && TotalMaxDialOfOpponents(l.Key.Territory) + 3 < nrOfForces).HighestOrDefault(r => r.Value).Key;

            if (target == null) target = Game.Map.PolarSink;

            return new YellowRidesMonster(Game) { Initiator = Faction, Passed = false, ForceLocations = new Dictionary<Location, Battalion>(battalionsToMove), To = target };
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

            return new PerformBluePlacement(Game) { Initiator = Faction, Target = target };
        }

        private BlueFlip DetermineBlueFlip()
        {
            var territory = BlueFlip.GetTerritory(Game);

            if (IWantToBeFightersIn(territory))
            {
                return new BlueFlip(Game) { Initiator = Faction, AsAdvisors = false };
            }
            else
            {
                return new BlueFlip(Game) { Initiator = Faction, AsAdvisors = true };
            }
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

                if (!opponent.Is(Faction.Black) && WinWasPredictedByMeThisTurn(opponent.Faction) ||
                    dialNeeded <= maxDial ||
                    LastTurn && dialNeeded - 1 <= amountICanReinforce + maxDial ||
                    potentialWinningOpponents.Contains(opponent) && dialNeeded <= amountICanReinforce + maxDial)
                {
                    return true;
                }
            }

            return false;
        }

        private BlueBattleAnnouncement DetermineBlueBattleAnnouncement()
        {
            if (Game.CurrentMainPhase == MainPhase.Resurrection && NrOfBattlesToFight <= Battle.ValidBattleHeroes(Game, this).Count())
            {
                var territory = BlueBattleAnnouncement.ValidTerritories(Game, this).Where(t => IWantToAnnounceBattleIn(t)).LowestOrDefault(t => GetDialNeeded(t, GetOpponentThatOccupies(t), false));

                if (territory != null)
                {
                    return new BlueBattleAnnouncement(Game) { Initiator = Faction, Territory = territory };
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        private bool IWantToAnnounceBattleIn(Territory territory)
        {
            var opponent = GetOpponentThatOccupies(territory);

            if (opponent != null)
            {
                var dialNeeded = GetDialNeeded(territory, opponent, false);

                int lastTurnConfidenceBonus = LastTurn ? 3 : 0;

                if (territory.IsStronghold && WinWasPredictedByMeThisTurn(opponent.Faction) ||
                    dialNeeded + lastTurnConfidenceBonus <= MaxDial(this, territory, opponent) + MaxReinforcedDialTo(this, territory))
                {
                    return true;
                }
            }

            return false;
        }

        private BlueAccompanies DetermineBlueAccompanies()
        {
            if (ForcesInReserve > 0)
            {
                var target = BlueAccompanies.ValidTargets(Game, this).FirstOrDefault(l => l.IsStronghold);

                bool shippingOpponentCanWin = false;
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
                    (!LastTurn && AnyForcesIn(target) > 0 || !HasAlly && ForcesInLocations.Count(kvp => IsStronghold(kvp.Key)) <= 3))
                {
                    return new BlueAccompanies(Game) { Initiator = Faction, Location = target, Accompanies = true };
                }

                if (BlueAccompanies.ValidTargets(Game, this).Contains(Game.Map.PolarSink) &&
                    (ResourcesIncludingAllyContribution < 5 || ForcesInReserve > 3) &&
                    !(LastTurn && Game.HasActedOrPassed.Contains(Faction)) &&
                    AnyForcesIn(Game.Map.PolarSink) < 8)
                {
                    return new BlueAccompanies(Game) { Initiator = Faction, Location = Game.Map.PolarSink, Accompanies = true };
                }
            }

            return new BlueAccompanies(Game) { Initiator = Faction, Location = null, Accompanies = false };
        }

        private BluePrediction DetermineBluePrediction()
        {
            var factionId = D(1, BluePrediction.ValidTargets(Game, this).Count()) - 1;
            var faction = BluePrediction.ValidTargets(Game, this).ElementAt(factionId);

            int turn;
            if (faction == Faction.Black)
            {
                if (D(1, 2) == 1) turn = D(1, Math.Min(3, Game.MaximumNumberOfTurns));
                else turn = D(1, Game.MaximumNumberOfTurns);
            }
            else
            {
                if (D(1, 2) == 1) turn = D(1, Math.Min(5, Game.MaximumNumberOfTurns));
                else turn = 2 + D(1, Game.MaximumNumberOfTurns - 2);
            }

            return new BluePrediction(Game) { Initiator = Faction, ToWin = faction, Turn = turn };
        }

        private VoicePlan voicePlan = null;
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
                result.voice = new Voice(Game) { Initiator = Faction, Must = true, Type = TreacheryCardType.Laser };
                result.opponentHeroWillCertainlyBeZero = true;
                result.playerHeroWillCertainlySurvive = true;
            }

            var knownOpponentDefenses = KnownOpponentDefenses(opponent);
            var knownOpponentWeapons = KnownOpponentWeapons(opponent);
            int nrOfUnknownOpponentCards = NrOfUnknownOpponentCards(opponent);

            var weapons = Weapons(null, null).Where(w => w.Type != TreacheryCardType.Useless && w.Type != TreacheryCardType.ArtilleryStrike && w.Type != TreacheryCardType.PoisonTooth);
            result.weaponToUse = weapons.FirstOrDefault(w => w.Type == TreacheryCardType.ProjectileAndPoison); //use poisonblade if available
            if (result.weaponToUse == null) result.weaponToUse = weapons.FirstOrDefault(w => w.Type == TreacheryCardType.Laser); //use lasgun if available
            if (result.weaponToUse == null) result.weaponToUse = weapons.FirstOrDefault(w => Game.KnownCards(this).Contains(w)); //use a known weapon if available
            if (result.weaponToUse == null) result.weaponToUse = weapons.FirstOrDefault(); //use any weapon

            TreacheryCardType type = TreacheryCardType.None;
            bool must = false;

            bool opponentMightHaveDefenses = !(knownOpponentDefenses.Any() && nrOfUnknownOpponentCards == 0);

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
                        {
                            type = TreacheryCardType.WeirdingWay;
                        }
                        else if (!Game.Applicable(Rule.BlueVoiceMustNameSpecialCards) && cardsPlayerHasOrMightHave.Any(c => c.IsProjectileDefense))
                        {
                            type = TreacheryCardType.ProjectileDefense;
                        }
                        else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.ShieldAndAntidote))
                        {
                            type = TreacheryCardType.ShieldAndAntidote;
                        }
                        else if (cardsPlayerHasOrMightHave.Any(c => c.Type == TreacheryCardType.Shield))
                        {
                            type = TreacheryCardType.Shield;
                        }
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
                bool opponentMightHaveWeapons = !(knownOpponentWeapons.Any() && nrOfUnknownOpponentCards == 0);

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
                    {
                        type = TreacheryCardType.Projectile;
                    }
                    else
                    {
                        type = TreacheryCardType.Poison;
                    }
                }
                else
                {
                    if (cardsPlayerHasOrMightHave.Any(c => c.IsPoisonWeapon))
                    {
                        type = TreacheryCardType.Poison;
                    }
                    else
                    {
                        type = TreacheryCardType.Projectile;
                    }
                }

                LogInfo("Remaining category: {0} {1}", must ? "must use" : "may not use", type);
            }

            var voice = new Voice(Game) { Initiator = player.Faction, Must = must, Type = type };
            result.voice = voice;

            return result;
        }

        #endregion Blue

        #region Green

        protected virtual Prescience DeterminePrescience()
        {
            var opponent = Game.CurrentBattle.OpponentOf(Faction);

            if (Voice.MayUseVoice(Game, opponent))
            {
                LogInfo("Opponent may use voice: {0} {1}", Game.CurrentVoice == null, Game.CurrentBattle.PlanOf(opponent) == null);
                if (Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null)
                {
                    //Wait for voice or finalized battle plan
                    return null;
                }
            }

            if (Game.CurrentBattle.IsAggressorOrDefender(this))
            {
                return BestPrescience(opponent, MaxDial(this, Game.CurrentBattle.Territory, opponent));
            }

            return null;
        }

        protected Prescience BestPrescience(Player opponent, float maxForceStrengthInBattle)
        {
            var myDefenses = Battle.ValidDefenses(Game, this, null).Where(c => Game.KnownCards(this).Contains(c));
            var myWeapons = Battle.ValidWeapons(Game, this, null, null).Where(c => Game.KnownCards(this).Contains(c));

            var knownOpponentDefenses = Battle.ValidDefenses(Game, opponent, null).Where(c => Game.KnownCards(this).Contains(c));
            var knownOpponentWeapons = Battle.ValidWeapons(Game, opponent, null, null).Where(c => Game.KnownCards(this).Contains(c));
            //int nrOfUnknownOpponentCards = opponent.TreacheryCards.Count(c => !Game.KnownCards(this).Contains(c));

            var cardsOpponentHasOrMightHave = CardsPlayerHasOrMightHave(opponent);
            bool weaponIsCertain = CountDifferentWeaponTypes(cardsOpponentHasOrMightHave) <= 1;
            bool defenseIsCertain = CountDifferentDefenseTypes(cardsOpponentHasOrMightHave) <= 1;

            bool iHaveShieldSnooper = myDefenses.Any(d => d.Type == TreacheryCardType.ShieldAndAntidote);
            bool iHavePoisonBlade = myWeapons.Any(d => d.Type == TreacheryCardType.ProjectileAndPoison);

            PrescienceAspect aspect;
            if (!weaponIsCertain && myDefenses.Any(d => d.IsProjectileDefense) && myDefenses.Any(d => d.IsPoisonDefense) && !iHaveShieldSnooper)
            {
                //I dont have shield snooper and I have choice between shield and snooper, therefore ask for the weapon used
                aspect = PrescienceAspect.Weapon;
            }
            else if (!defenseIsCertain && myWeapons.Any(d => d.IsProjectileWeapon) && myWeapons.Any(d => d.IsPoisonWeapon) && !iHavePoisonBlade)
            {
                //I dont have poison blade and I have choice between poison weapon and projectile weapon, therefore ask for the defense used
                aspect = PrescienceAspect.Defense;
            }
            else if (!weaponIsCertain && myDefenses.Any() && !iHaveShieldSnooper)
            {
                aspect = PrescienceAspect.Weapon;
            }
            else if (!defenseIsCertain && myWeapons.Any() && !iHavePoisonBlade)
            {
                aspect = PrescienceAspect.Defense;
            }
            else if (maxForceStrengthInBattle > 2 && Prescience.ValidAspects(Game, this).Contains(PrescienceAspect.Dial))
            {
                aspect = PrescienceAspect.Dial;
            }
            else
            {
                aspect = PrescienceAspect.Leader;
            }

            return new Prescience(Game) { Initiator = Faction, Aspect = aspect };
        }

        #endregion Green

        #region Purple

        protected SetIncreasedRevivalLimits DetermineSetIncreasedRevivalLimits()
        {
            var targets = SetIncreasedRevivalLimits.ValidTargets(Game, this).ToArray();
            if (Game.FactionsWithIncreasedRevivalLimits.Length != targets.Length)
            {
                return new SetIncreasedRevivalLimits(Game) { Initiator = Faction, Factions = targets };
            }
            else
            {
                return null;
            }
        }

        protected virtual FaceDanced DetermineFaceDanced()
        {
            if (FaceDanced.MayCallFaceDancer(Game, this))
            {
                var facedancer = FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));
                var facedancedHeroIsLivingLeader = facedancer is Leader && Game.IsAlive(facedancer);

                if ((FaceDanced.MaximumNumberOfForces(Game, this) > 0 || facedancedHeroIsLivingLeader) && Game.BattleWinner != Ally)
                {
                    var forcesFromPlanet = new Dictionary<Location, Battalion>();

                    int toPlace = FaceDanced.MaximumNumberOfForces(Game, this);

                    var biggest = BiggestBattalionThreatenedByStormWithoutSpice;
                    if (biggest.Key != null)
                    {
                        var toTake = biggest.Value.Take(toPlace, false);
                        forcesFromPlanet.Add(biggest.Key, toTake);
                        toPlace -= toTake.TotalAmountOfForces;
                    }

                    int fromReserves = Math.Min(ForcesInReserve, toPlace);

                    var targetLocation = FaceDanced.ValidTargetLocations(Game).FirstOrDefault(l => Game.ResourcesOnPlanet.ContainsKey(l));
                    if (targetLocation == null) targetLocation = FaceDanced.ValidTargetLocations(Game).FirstOrDefault();

                    var targetLocations = new Dictionary<Location, Battalion>
                    {
                        { targetLocation, new Battalion() { AmountOfForces = forcesFromPlanet.Sum(kvp => kvp.Value.AmountOfForces) + fromReserves, AmountOfSpecialForces = forcesFromPlanet.Sum(kvp => kvp.Value.AmountOfSpecialForces) } }
                    };

                    var result = new FaceDanced(Game) { Initiator = Faction, FaceDancerCalled = true, ForceLocations = forcesFromPlanet, ForcesFromReserve = fromReserves, TargetForceLocations = targetLocations };
                    LogInfo(result.GetMessage());
                    return result;
                }
            }

            return new FaceDanced(Game) { Initiator = Faction, FaceDancerCalled = false };
        }

        protected virtual FaceDancerReplaced DetermineFaceDancerReplaced()
        {
            var replacable = FaceDancers.Where(f => !RevealedDancers.Contains(f)).OrderBy(f => f.Value);
            var toReplace = replacable.FirstOrDefault(f => Leaders.Contains(f) || (Ally != Faction.None && AlliedPlayer.Leaders.Contains(f)));
            if (toReplace == null) toReplace = replacable.FirstOrDefault(f => f is Leader && !Game.LeaderState[f].Alive);

            if (toReplace != null)
            {
                return new FaceDancerReplaced(Game) { Initiator = Faction, Passed = false, SelectedDancer = toReplace };
            }
            else
            {
                return new FaceDancerReplaced(Game) { Initiator = Faction, Passed = true };
            }
        }

        private readonly Dictionary<IHero, int> priceSetEarlier = new Dictionary<IHero, int>();
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

            if (toRevive != null && Battle.ValidBattleHeroes(Game, this).Count() <= 3)
            {
                toRevive = RequestPurpleRevival.ValidTargets(Game, this).HighestOrDefault(l => l.Value);
            }

            if (toRevive != null)
            {
                turnWhenRevivalWasRequested = Game.CurrentTurn;
                return new RequestPurpleRevival(Game) { Initiator = Faction, Hero = toRevive };
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
                    if (FaceDancers.Contains(hero) && !RevealedDancers.Contains(hero) || (Ally != Faction.None && AlliedPlayer.Traitors.Contains(hero) && !AlliedPlayer.RevealedTraitors.Contains(hero)))
                    {
                        price = 1 + D(1, hero.Value);
                    }
                    else
                    {
                        price = 2 + D(2, hero.Value);
                    }

                    priceSetEarlier.Add(hero, price);
                }

                return new AcceptOrCancelPurpleRevival(Game) { Initiator = Faction, Hero = hero, Price = price, Cancel = false };
            }

            return null;
        }

        #endregion Purple

        #region Brown

        protected virtual BrownEconomics DetermineBrownEconomics()
        {
            if (ResourcesIncludingAllyContribution >= 14 &&
                Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue) >= 2)
            {
                return new BrownEconomics(Game) { Initiator = Faction, Status = BrownEconomicsStatus.Cancel };
            }
            else if (ResourcesIncludingAllyContribution <= 8 &&
                Game.Players.Where(p => p.Faction != Ally).Count(p => p.Resources < 2 || Game.Applicable(Rule.BlueAutoCharity) && p.Faction == Faction.Blue) <= 2)
            {
                return new BrownEconomics(Game) { Initiator = Faction, Status = BrownEconomicsStatus.Double };
            }

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
            var forceLocations = new Dictionary<Location, Battalion>();

            if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.Carthag))
            {
                forceLocations.Add(Game.Map.Carthag, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.Arrakeen))
            {
                forceLocations.Add(Game.Map.Arrakeen, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.TueksSietch))
            {
                forceLocations.Add(Game.Map.TueksSietch, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.HabbanyaSietch))
            {
                forceLocations.Add(Game.Map.HabbanyaSietch, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.ImperialBasin.MiddleLocation))
            {
                forceLocations.Add(Game.Map.ImperialBasin.MiddleLocation, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else if (PerformCyanSetup.ValidLocations(Game).Contains(Game.Map.PolarSink))
            {
                forceLocations.Add(Game.Map.PolarSink, new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }
            else 
            {
                forceLocations.Add(PerformCyanSetup.ValidLocations(Game).FirstOrDefault(l => l.IsProtectedFromStorm), new Battalion() { Faction = Faction, AmountOfForces = 6 });
            }

            return new PerformCyanSetup(Game) { Initiator = Faction, ForceLocations = forceLocations };
        }

        protected virtual TerrorPlanted DetermineTerrorPlanted()
        {
            //This is for now just random
            var type = TerrorPlanted.ValidTerrorTypes(Game, false).RandomOrDefault();
            if (TerrorPlanted.ValidTerrorTypes(Game, false).Contains(TerrorType.Extortion)) type = TerrorType.Extortion;

            return new TerrorPlanted(Game) { Initiator = Faction, Type = type, Stronghold = TerrorPlanted.ValidStrongholds(Game, this).First() };
        }

        protected virtual TerrorRevealed DetermineTerrorRevealed()
        {
            //This is for now just random
            var type = TerrorRevealed.GetTypes(Game).RandomOrDefault();
            var cardInSabotage = TreacheryCards.FirstOrDefault(c => c.IsUseless);
            var victim = Game.GetPlayer(TerrorRevealed.GetVictim(Game));
            var offerAlliance = TerrorRevealed.MayOfferAlliance(Game) && PlayerStanding(victim) > 1.5f * PlayerStanding(this);

            if (offerAlliance)
            {
                return new TerrorRevealed(Game) { Initiator = Faction, AllianceOffered = offerAlliance };
            }
            else
            {
                return new TerrorRevealed(Game) { Initiator = Faction, Type = type, RobberyTakesCard = false, CardToGiveInSabotage = cardInSabotage, ForcesInSneakAttack = TerrorRevealed.MaxAmountOfForcesInSneakAttack(Game, this) };
            }
        }

        #endregion

        #region Pink

        protected virtual AmbassadorPlaced DetermineAmbassadorPlaced()
        {
            if (Game.AmbassadorsPlacedThisTurn == 0 || Resources > 6 + Game.AmbassadorsPlacedThisTurn) {
                //This is for now just random
                var faction = AmbassadorPlaced.ValidAmbassadors(this).RandomOrDefault();

                return new AmbassadorPlaced(Game) { Initiator = Faction, Faction = faction, Stronghold = TerrorPlanted.ValidStrongholds(Game, this).First() };
            }
            else {

                return null;
            }
        }

        protected virtual AmbassadorActivated DetermineAmbassadorActivated()
        {
            //This is for now just random
            switch (AmbassadorActivated.GetFaction(Game))
            {
                case Faction.Blue:
                    return new AmbassadorActivated(Game) { Initiator = Faction, BlueSelectedFaction = AmbassadorActivated.GetValidBlueFactions(Game).First() };

                case Faction.Brown:
                    var toDiscard = AmbassadorActivated.GetValidBrownCards(this).Where(c => CardQuality(c) < 2);
                    if (toDiscard.Any())
                    {
                        return new AmbassadorActivated(Game) { Initiator = Faction, BrownCards = toDiscard };
                    }
                    else
                    {
                        return new AmbassadorActivated(Game) { Initiator = Faction, Passed = true };
                    }

                case Faction.Pink:
                    var victim = AmbassadorActivated.GetVictim(Game);
                    var victimPlayer = Game.GetPlayer(victim);
                    bool offerAlliance = AmbassadorActivated.AllianceCanBeOffered(Game, this) && PlayerStanding(victimPlayer) > 0.33 * PlayerStanding(this);
                    bool takeVidal = AmbassadorActivated.VidalCanBeTaken(Game);
                    bool offerVidal = takeVidal && AmbassadorActivated.VidalCanBeGivenTo(Game, victimPlayer) && HeroesForBattle(this, true).Count() >= 3 && HeroesForBattle(victimPlayer, true).Count() < 3;

                    if (offerAlliance || takeVidal || offerVidal)
                    {
                        return new AmbassadorActivated(Game) { Initiator = Faction, PinkOfferAlliance = offerAlliance, PinkTakeVidal = takeVidal, PinkGiveVidalToAlly = offerVidal };
                    }
                    else
                    {
                        return new AmbassadorActivated(Game) { Initiator = Faction, Passed = true };
                    }

                case Faction.Yellow:
                    var toMove = DetermineMovedBatallion(true);
                    if (toMove != null && AmbassadorActivated.ValidYellowSources(Game, this).Contains(toMove.From.Territory))
                    {
                        var forcesToMove = new Dictionary<Location, Battalion>
                        {
                            { toMove.From, toMove.Batallion }
                        };

                        return new AmbassadorActivated(Game) { Initiator = Faction, YellowOrOrangeTo = toMove.To, YellowForceLocations = forcesToMove };
                    }
                    else
                    {
                        return null;
                    }

                default:
                    return new AmbassadorActivated(Game) { Initiator = Faction };

            }
        }

        #endregion


        #region White

        protected virtual WhiteAnnouncesBlackMarket DetermineWhiteAnnouncesBlackMarket()
        {
            var card = TreacheryCards.FirstOrDefault(c => CardQuality(c) < 3);
            if (card != null)
            {
                return new WhiteAnnouncesBlackMarket(Game) { Initiator = Faction, Passed = false, Card = card, AuctionType = D(1, 2) > 1 ? AuctionType.BlackMarketSilent : AuctionType.BlackMarketOnceAround, Direction = D(1, 2) > 1 ? 1 : -1 };
            }
            else
            {
                return new WhiteAnnouncesBlackMarket(Game) { Initiator = Faction, Passed = true };
            }
        }

        protected virtual WhiteRevealedNoField DetermineWhiteRevealedNoField()
        {
            if (Game.CurrentPhase == Phase.ShipmentAndMoveConcluded)
            {
                var locationWithNoField = ForcesOnPlanet.FirstOrDefault(b => b.Value.AmountOfSpecialForces > 0).Key;
                if (locationWithNoField != null && Game.ResourcesOnPlanet.ContainsKey(locationWithNoField) && !OccupiedByOpponent(locationWithNoField.Territory))
                {
                    return new WhiteRevealedNoField(Game) { Initiator = Faction };
                }
            }

            return null;
        }

        protected virtual WhiteAnnouncesAuction DetermineWhiteAnnouncesAuction()
        {
            return new WhiteAnnouncesAuction(Game) { Initiator = Faction, First = D(1, 2) > 1 };
        }

        protected virtual WhiteSpecifiesAuction DetermineWhiteSpecifiesAuction()
        {
            var toAuction = new Deck<TreacheryCard>(Game.WhiteCache, random);
            toAuction.Shuffle();
            return new WhiteSpecifiesAuction(Game) { Initiator = Faction, Card = toAuction.Draw(), AuctionType = D(1, 2) > 1 ? AuctionType.WhiteSilent : AuctionType.WhiteOnceAround, Direction = D(1, 2) > 1 ? 1 : -1 };
        }

        protected virtual WhiteKeepsUnsoldCard DetermineWhiteKeepsUnsoldCard()
        {
            return new WhiteKeepsUnsoldCard(Game) { Initiator = Faction, Passed = false };
        }

        #endregion White

        #region Black

        private Captured DetermineCaptured()
        {
            return new Captured(Game) { Initiator = Faction, Passed = false };
        }

        #endregion
    }

    public class VoicePlan
    {
        public BattleInitiated battle;
        public Voice voice = null;
        public TreacheryCard weaponToUse = null;
        public TreacheryCard defenseToUse = null;
        public bool playerHeroWillCertainlySurvive = false;
        public bool opponentHeroWillCertainlyBeZero = false;
    }
}
