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
    private ResourcesTransferred? DetermineResourcesTransferred()
    {
        var amount = ResourcesTransferred.MaxAmount(Player);
        if (Resources - amount > 15 && Player.AlliedResources < Resources) 
            return new ResourcesTransferred(Game, Faction) { Resources = amount };

        return null;
    }

    private DiscoveryEntered DetermineDiscoveryEntered()
    {
        var to = DiscoveryEntered.ValidTargets(Game, Player).FirstOrDefault();
        var battalionsToMove = DiscoveryEntered.ValidSources(Player, to).ToDictionary(l => l, l => Player.BattalionIn(l));
        return new DiscoveryEntered(Game, Faction) { ForceLocations = new Dictionary<Location, Battalion>(battalionsToMove), To = to };
    }

    private DiscoveryRevealed DetermineDiscoveryRevealed()
    {
        return new DiscoveryRevealed(Game, Faction) { Location = DiscoveryRevealed.GetLocations(Game, Player).First() };
    }

    private TestingStationUsed? DetermineTestingStationUsed()
    {
        var currentStormEnd = (Game.SectorInStorm + Game.NextStormMoves) % Map.NumberOfSectors;
        var locationsInSector = Game.Map.Locations(false).Where(l => l.Sector == currentStormEnd && !l.IsProtectedFromStorm).ToList();
        var locationsInSectorMinus1 = Game.Map.Locations(false).Where(l => !l.IsProtectedFromStorm && l.Sector == (currentStormEnd == 0 ? Map.NumberOfSectors : currentStormEnd - 1)).ToList();
        var locationsInSectorPlus1 = Game.Map.Locations(false).Where(l => !l.IsProtectedFromStorm && l.Sector == (currentStormEnd == Map.NumberOfSectors ? 0 : currentStormEnd + 1)).ToList();

        var myForcesKilledInSector = locationsInSector.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction == Faction && b.Faction == Ally).Sum(b => b.TotalAmountOfForces));
        var myForcesKilledInSectorMinus1 = locationsInSectorMinus1.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction == Faction && b.Faction == Ally).Sum(b => b.TotalAmountOfForces));
        var myForcesKilledInSectorPlus1 = locationsInSectorPlus1.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction == Faction && b.Faction == Ally).Sum(b => b.TotalAmountOfForces));

        var myStrongholdsInSector = locationsInSector.Count(l => l.IsStronghold && ((Player.Occupies(l) && Player.AnyForcesIn(l) < 6) || (Player.HasAlly && AlliedPlayer!.Occupies(l) && AlliedPlayer.AnyForcesIn(l) < 6)));
        var myStrongholdsInSectorMinus1 = locationsInSectorMinus1.Count(l => l.IsStronghold && ((Player.Occupies(l) && Player.AnyForcesIn(l) < 6) || (Player.HasAlly && AlliedPlayer!.Occupies(l) && AlliedPlayer.AnyForcesIn(l) < 6)));
        var myStrongholdsInSectorPlus1 = locationsInSectorPlus1.Count(l => l.IsStronghold && ((Player.Occupies(l) && Player.AnyForcesIn(l) < 6) || (Player.HasAlly && AlliedPlayer!.Occupies(l) && AlliedPlayer.AnyForcesIn(l) < 6)));

        var enemyForcesKilledInSector = locationsInSector.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces));
        var enemyForcesKilledInSectorMinus1 = locationsInSectorMinus1.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces));
        var enemyForcesKilledInSectorPlus1 = locationsInSectorPlus1.Sum(l => Game.BattalionsIn(l).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces));

        var enemyStrongholdsInSector = locationsInSector.Count(l => l.IsStronghold && OccupiedByOpponent(l));
        var enemyStrongholdsInSectorMinus1 = locationsInSectorMinus1.Count(l => l.IsStronghold && OccupiedByOpponent(l));
        var enemyStrongholdsInSectorPlus1 = locationsInSectorPlus1.Count(l => l.IsStronghold && OccupiedByOpponent(l));

        var scoreIfMinus1 = enemyForcesKilledInSectorMinus1 + myStrongholdsInSectorMinus1 * 3 - myForcesKilledInSectorMinus1 - enemyStrongholdsInSectorMinus1 * 3;
        var scoreIfPassed = scoreIfMinus1 + enemyForcesKilledInSector + myStrongholdsInSector * 3 - myForcesKilledInSector - enemyStrongholdsInSector * 3;
        var scoreIfPlus1 = scoreIfPassed + enemyForcesKilledInSectorPlus1 + myStrongholdsInSectorPlus1 * 3 - myForcesKilledInSectorPlus1 - enemyStrongholdsInSectorPlus1 * 3;

        if (scoreIfMinus1 > scoreIfPassed && scoreIfMinus1 > scoreIfPlus1)
            return new TestingStationUsed(Game, Faction) { ValueAdded = -1 };
        if (scoreIfPlus1 > scoreIfPassed && scoreIfPlus1 > scoreIfMinus1)
            return new TestingStationUsed(Game, Faction) { ValueAdded = 1 };
        
        return null;
    }

    private DivideResources DetermineDivideResources()
    {
        var spiceIWant = Math.Max(0, DivideResources.GetResourcesToBeDivided(Game).Amount - Resources);
        return new DivideResources(Game, Faction) { PortionToFirstPlayer = spiceIWant };
    }

    private CardGiven DetermineCardGiven()
    {
        return new CardGiven(Game, Faction)
        {
            Passed = !(CardQuality(Game.CardThatMustBeKeptOrGivenToAlly, Player) <= 2 && (Ally == Faction.Blue || Ally == Faction.Brown || CardQuality(Game.CardThatMustBeKeptOrGivenToAlly, AlliedPlayer) > 0))
        };
    }

    private DivideResourcesAccepted DetermineDivideResourcesAccepted()
    {
        var tbd = DivideResources.GetResourcesToBeDivided(Game);

        var iGetWhenAgreed = DivideResources.GainedByOtherFaction(tbd, true, Game.CurrentDivisionProposal.PortionToFirstPlayer);
        var iGetWhenDenied = DivideResources.GainedByOtherFaction(tbd, false, Game.CurrentDivisionProposal.PortionToFirstPlayer);

        var accepted = iGetWhenAgreed >= iGetWhenDenied || Resources >= Player.AlliedResources;

        return new DivideResourcesAccepted(Game, Faction) { Passed = !accepted };
    }

    private Discarded DetermineDiscarded()
    {
        var worstCard = Player.TreacheryCards.OrderBy(c => CardQuality(c, null)).First();
        return new Discarded(Game, Faction) { Card = worstCard };
    }

    private TraitorDiscarded DetermineTraitorDiscarded()
    {
        var worstTraitor = DetermineWorstTraitor();
        worstTraitor ??= Player.Traitors.LowestOrDefault(t => t.Value);
        return new TraitorDiscarded(Game, Faction) { Traitor = worstTraitor };
    }

    private IHero? DetermineWorstTraitor()
    {
        var result = Player.Traitors.Where(t => Player.RevealedTraitors.Contains(t)).HighestOrDefault(t => t.Value);

        if (result == null && (Ally == Faction.Purple || !Game.IsPlaying(Faction.Purple))) result = Player.Traitors.Where(t => t.Faction == Faction).LowestOrDefault(t => t.Value);

        result ??= Player.Traitors.Where(t => t is Leader && !Game.IsAlive(t)).LowestOrDefault(t => t.Value);
        result ??= Player.Traitors.Where(t => t is Leader).LowestOrDefault(t => t.Value);

        return result;
    }

    private NexusPlayed? DetermineNexusPlayed()
    {
        var result = new NexusPlayed(Game, Faction) { Faction = Player.Nexus };

        if (NexusPlayed.CanUseCunning(Player))
            return DetermineNexusPlayed_Cunning(result);
        
        return NexusPlayed.CanUseSecretAlly(Game, Player) 
            ? DetermineNexusPlayed_SecretAlly(result) 
            : DetermineNexusPlayed_Betrayal(result);
    }

    private bool IWishToAttack(int maxNumberOfContestedStrongholds, Faction f)
    {
        return AlmostWinningOpponentsIWishToAttack(maxNumberOfContestedStrongholds, true).Select(p => p.Faction)
            .Contains(f);
    }

    private NexusPlayed? DetermineNexusPlayed_Betrayal(NexusPlayed result)
    {
        if (Ally != result.Faction)
            switch (Player.Nexus)
            {
                case Faction.Green:
                    if (IsWinningOrIsOpponentInBattle(Faction.Green) && !Game.Prevented(FactionAdvantage.GreenBattlePlanPrescience)) return result;
                    break;

                case Faction.Black:
                    if (Game.CurrentBattle.IsInvolved(Player)) return result;
                    break;

                case Faction.Yellow:
                    if ((Game.CurrentMainPhase == MainPhase.Blow && YellowRidesMonster.IsApplicable(Game)) ||
                        (Game.CurrentMainPhase == MainPhase.ShipmentAndMove && IWishToAttack(0, Faction.Yellow))) return result;
                    break;

                case Faction.Red:
                    if ((Game is { CurrentMainPhase: MainPhase.Bidding, CurrentBid: not null } && Game.CurrentBid.Player.Ally == Faction.Red && Game.CurrentBid.TotalAmount > 5 && !Game.Prevented(FactionAdvantage.RedReceiveBid)) ||
                        (Game.CurrentMainPhase == MainPhase.Battle && IsWinningOrIsOpponentInBattle(Faction.Red) && Game.GetPlayer(Faction.Red).SpecialForcesIn(Game.CurrentBattle.Territory) >= 3)) return result;
                    break;

                case Faction.Orange:
                    if (Game.RecentlyPaidTotalAmount > 5) return result;
                    break;

                case Faction.Blue:
                    if (IsWinningOrIsOpponentInBattle(Faction.Blue) && !Game.Prevented(FactionAdvantage.BlueUsingVoice)) return result;
                    break;

                case Faction.Grey: return result;

                case Faction.Purple:
                    if (Game.CurrentBattle.IsInvolved(Player)) return result;
                    break;

                case Faction.Brown:
                    if (IsWinningOrIsOpponentInBattle(Faction.Brown) && KnownOpponentCards(Faction.Brown).Any(c => c.IsWeapon || c.IsDefense)) return result;
                    break;

                case Faction.White:
                    var white = Game.GetPlayer(Faction.White);
                    if (white.Has(Game.CardJustWon))
                    {
                        if (CardQuality(Game.CardJustWon, white) > 2) return result;
                    }
                    else if (Game.RecentlyPaidTotalAmount > 4)
                    {
                        return result;
                    }

                    break;

                case Faction.Pink:
                    if (Game.CurrentPhase == Phase.ResurrectionReport)
                    {
                        var territoryToRemovePinkFrom = DetermineTerritoryToRemovePinkFrom();
                        if (territoryToRemovePinkFrom != null)
                        {
                            result.PinkTerritory = territoryToRemovePinkFrom;
                            return result;
                        }
                    }
                    break;

                case Faction.Cyan:
                    if ((Faction == Faction.Orange && Game is { CurrentPhase: Phase.OrangeShip, OrangeMayDelay: false }) ||
                        (Faction != Faction.Orange && Game.CurrentPhase == Phase.NonOrangeShip && Game.ShipmentAndMoveSequence.CurrentPlayer == Player))
                    {
                        var shipment = DetermineShipment();
                        if (Game.TerrorIn(shipment.To.Territory).Any())
                        {
                            result.CyanTerritory = shipment.To.Territory;
                            return result;
                        }
                    }

                    break;
            }

        return null;
    }

    private Territory? DetermineTerritoryToRemovePinkFrom()
    {
        Territory? result = null;
        var territoriesWithPinkAndPinkAlly = NexusPlayed.ValidPinkTerritories(Game).Where(t => t.IsStronghold || Game.IsSpecialStronghold(t)).ToList();
        if (territoriesWithPinkAndPinkAlly.Count != 0)
        {
            var pink = Game.GetPlayer(Faction.Pink);

            if (territoriesWithPinkAndPinkAlly.Count >= 2) result = territoriesWithPinkAndPinkAlly.HighestOrDefault(t => pink.AnyForcesIn(t));

            result ??= territoriesWithPinkAndPinkAlly.Where(t => pink.AnyForcesIn(t) > 7)
                .HighestOrDefault(t => pink.AnyForcesIn(t));
        }
        return result;
    }

    private bool IsWinningOrIsOpponentInBattle(Faction faction)
    {
        return faction != Ally && Game.CurrentBattle != null &&
               ((Game.CurrentBattle.IsInvolved(faction) && IsWinning(faction)) ||
                Game.CurrentBattle.OpponentOf(faction) == Player);
    }

    private NexusPlayed? DetermineNexusPlayed_Cunning(NexusPlayed result)
    {
        switch (Player.Nexus)
        {
            case Faction.Green:

                if (Game.CurrentPrescience != null)
                {
                    var opponent = Game.CurrentBattle.OpponentOf(Faction);

                    if (!(Voice.MayUseVoice(Game, opponent) && Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null))
                        if (Game.CurrentBattle.IsAggressorOrDefender(Player))
                        {
                            result.GreenPrescienceAspect = BestPrescience(Player, opponent, MaxDial(Player, Game.CurrentBattle.Territory, opponent), 
                                Game.CurrentPrescience.Aspect, Game.CurrentBattle.Territory);

                            if (result.GreenPrescienceAspect != PrescienceAspect.None) return result;
                        }
                }
                break;

            case Faction.Black:
                if (Game.CurrentPhase == Phase.ShipmentAndMoveConcluded && DetermineWorstTraitor() != null) return result;
                break;

            case Faction.Yellow:
                if (Game.Monsters.Any() && DetermineMovedBattalion(true) != null) return result;
                break;

            case Faction.Red:
                if (Player.ForcesIn(Game.CurrentBattle.Territory) >= 4) return result;
                break;

            case Faction.Orange:
                var shipment = DetermineShipment();
                if (!shipment.Passed && DecidedShipmentAction is ShipmentDecision.PreventNormalWin or ShipmentDecision.PreventYellowWin or ShipmentDecision.AttackWeakStronghold) return result;
                break;

            case Faction.Blue:
                var flip = DetermineBlueBattleAnnouncement();
                if (flip != null) return result;
                break;

            case Faction.Grey:
                if (Player.ForcesIn(Game.CurrentBattle.Territory) >= 3) return result;
                break;

            case Faction.Purple:
                if (Game.CurrentMainPhase == MainPhase.Battle && Player.RevealedFaceDancers.Any()) return result;
                break;

            case Faction.Pink:
                if (Game is { CurrentPhase: Phase.BattlePhase, CurrentBattle: not null } && Game.CurrentBattle.IsAggressorOrDefender(Player) && !Player.Leaders.Contains(Game.Vidal)) return result;
                break;

        }

        return null;
    }

    private NexusPlayed? DetermineNexusPlayed_SecretAlly(NexusPlayed result)
    {
        switch (Player.Nexus)
        {
            case Faction.Green:

                var opponent = Game.CurrentBattle.OpponentOf(Faction);
                if (!(Voice.MayUseVoice(Game, opponent) && Game.CurrentVoice == null && Game.CurrentBattle.PlanOf(opponent) == null))
                    if (Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        result.GreenPrescienceAspect = BestPrescience(Player, opponent, MaxDial(Player, Game.CurrentBattle.Territory, opponent), 
                            PrescienceAspect.None, Game.CurrentBattle.Territory);
                        if (result.GreenPrescienceAspect != PrescienceAspect.None) return result;
                    }

                break;

            case Faction.Black:
                if (DetermineWorstTraitor() != null) return result;
                break;

            case Faction.Yellow:
                if (Game.CurrentMainPhase == MainPhase.Resurrection && Player.ForcesKilled >= 3) return result;
                break;

            case Faction.Orange:
                var shipment = DetermineShipment();
                if (!shipment.Passed && Shipment.DetermineCost(Game, Player, shipment) > 5) return result;
                break;

            case Faction.Blue:
                if (Game.CurrentBattle != null && Game.CurrentBattle.IsInvolved(Player)) return result;
                break;

            case Faction.Grey:
                if (CardQuality(Game.CardJustWon, Player) < 2) return result;
                break;

            case Faction.Purple:
                if (Game.HasActedOrPassed.Contains(Faction))
                {
                    var amountOfSpecialForces = 0;
                    var amountOfForces = 0;

                    while (amountOfSpecialForces + 1 < NexusPlayed.ValidPurpleMaxAmount(Game, Player, true) &&
                           NexusPlayed.DeterminePurpleCost(0, amountOfSpecialForces + 1) < Resources &&
                           amountOfSpecialForces + 1 + amountOfForces <= 5)
                        amountOfSpecialForces++;

                    while (amountOfForces + 1 < NexusPlayed.ValidPurpleMaxAmount(Game, Player, false) &&
                           NexusPlayed.DeterminePurpleCost(0, amountOfForces + 1) < Resources &&
                           amountOfSpecialForces + amountOfForces + 1 <= 5)
                        amountOfForces++;

                    var hero = NexusPlayed.ValidPurpleHeroes(Game, Player).HighestOrDefault(l => l.Value);
                    if ((hero != null && amountOfForces + amountOfSpecialForces > 2) || amountOfForces + amountOfSpecialForces >= 5)
                    {
                        result.PurpleForces = amountOfForces;
                        result.PurpleSpecialForces = amountOfSpecialForces;
                        result.PurpleHero = hero;
                        result.PurpleAssignSkill = Revival.MayAssignSkill(Game, Player, hero);

                        result.PurpleNumberOfSpecialForcesInLocation = Revival.NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, amountOfSpecialForces);
                        if (result.PurpleNumberOfSpecialForcesInLocation > 0)
                        {
                            result.PurpleLocation = BestRevivalLocation(Revival.ValidRevivedForceLocations(Game, Player).ToList());
                            if (result.PurpleLocation == null) result.PurpleNumberOfSpecialForcesInLocation = 0;
                        }

                        return result;
                    }
                }
                break;

            case Faction.Brown:
                if (Game.CurrentMainPhase == MainPhase.Collection)
                {
                    if (Faction != Faction.Blue)
                    {
                        result.BrownCard = NexusPlayed.ValidBrownCards(Player).FirstOrDefault();
                        return result;
                    }
                }
                else if (Game.CurrentMainPhase == MainPhase.Battle)
                {
                    var auditee = Game.CurrentBattle.OpponentOf(Player);
                    var recentBattlePlan = Game.CurrentBattle.PlanOf(auditee);
                    var auditableCards = auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);

                    if (auditableCards.Any(c => !Player.KnownCards.Contains(c))) return result;
                }
                break;

            case Faction.Pink:
                if (Game is { CurrentPhase: Phase.BattlePhase, CurrentBattle: not null } && Game.CurrentBattle.IsAggressorOrDefender(Player))
                {
                    var opp = Game.CurrentBattle.OpponentOf(Player);
                    if (opp.Faction != Faction.Purple && opp.UnrevealedTraitors.Any())
                    {
                        result.PinkFaction = opp.Faction;
                        return result;
                    }
                }
                break;
        }

        return null;
    }

    private CharityClaimed? DetermineCharityClaimed()
    {
        if (!(Game.EconomicsStatus == BrownEconomicsStatus.Cancel || Game.EconomicsStatus == BrownEconomicsStatus.CancelFlipped))
            return new CharityClaimed(Game, Faction);
        
        return null;
    }

    private KarmaFreeRevival? DetermineKarmaFreeRevival()
    {
        var specialForcesThatCanBeRevived = Math.Min(3, Revival.ValidMaxRevivals(Game, Player, true, false));

        if (LastTurn || Player.ForcesKilled + specialForcesThatCanBeRevived >= 6)
        {
            var forces = Math.Max(0, Math.Min(3, Player.ForcesKilled) - specialForcesThatCanBeRevived);
            return new KarmaFreeRevival(Game, Faction) { Hero = null, AmountOfForces = forces, AmountOfSpecialForces = specialForcesThatCanBeRevived };
        }

        return null;
    }

    private KarmaHandSwap DetermineKarmaHandSwap()
    {
        var toReturn = new List<TreacheryCard>();
        foreach (var c in Player.TreacheryCards.OrderBy(c => CardQuality(c, Player)))
        {
            if (toReturn.Count == Game.KarmaHandSwapNumberOfCards) break;

            toReturn.Add(c);
        }

        return new KarmaHandSwap(Game, Faction) { ReturnedCards = toReturn };
    }

    private KarmaShipmentPrevention? DetermineKarmaShipmentPrevention()
    {
        if (Game.CurrentPhase != Phase.NonOrangeShip) return null;
        
        var validTargets = KarmaShipmentPrevention.GetValidTargets(Game, Player).ToList();

        var winningOpponentThatCanShipMost = OpponentsToShipAndMove
            .Where(p => validTargets.Contains(p.Faction))
            .Where(p => IsWinningOpponent(p) && p.ForcesInReserve + p.SpecialForcesInReserve > 2 && p.Resources + p.AlliedPlayer?.Resources > 2)
            .OrderByDescending(p => Math.Min(p.ForcesInReserve + p.SpecialForcesInReserve, p.Resources + (p.AlliedPlayer != null ? p.AlliedResources : 0)))
            .FirstOrDefault();

        if (winningOpponentThatCanShipMost != null && Game.ShipmentAndMoveSequence.CurrentPlayer == winningOpponentThatCanShipMost) return new KarmaShipmentPrevention(Game, Faction) { Target = winningOpponentThatCanShipMost.Faction };

        return null;
    }

    private KarmaHandSwapInitiated? DetermineKarmaHandSwapInitiated()
    {
        if (Game.CurrentPhase != Phase.BiddingReport) return null;
        
        if (Player.TreacheryCards.Count(c => CardQuality(c, Player) <= 2) >= 2)
        {
            var bestOpponentToSwapWith = Opponents.HighestOrDefault(o => CardsPlayerHas(o).Count(c => CardQuality(c, Player) >= 3));
            LogInfo("opponent with most known good cards: " + bestOpponentToSwapWith);

            if (bestOpponentToSwapWith != null && CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c, Player) >= 3) >= 2)
            {
                //Swap with an opponent that 2 or more good cards that I know of
                LogInfo("swapping, because number of good cards = " + CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c, Player) >= 3));
                return new KarmaHandSwapInitiated(Game, Faction) { Target = bestOpponentToSwapWith.Faction };
            }

            bestOpponentToSwapWith = Opponents.FirstOrDefault(o => o.TreacheryCards.Count == 4);
            LogInfo("opponent with 4 cards: " + bestOpponentToSwapWith);

            if (bestOpponentToSwapWith != null && CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c, Player) < 3) <= 2)
            {
                LogInfo("swapping, because number of known bad cards = " + CardsPlayerHas(bestOpponentToSwapWith).Count(c => CardQuality(c, Player) < 3));
                //Swap with an opponent that has 4 cards and 2 or less useless cards that I know of
                return new KarmaHandSwapInitiated(Game, Faction) { Target = bestOpponentToSwapWith.Faction };
            }
        }

        return null;
    }

    protected virtual HarvesterPlayed? DetermineHarvesterPlayed()
    {
        if (Game.CurrentTurn > 1 &&
            (
                (Game.CurrentPhase == Phase.HarvesterA && ResourcesIn(Game.LatestSpiceCardA.Location) > 6) ||
                (Game.CurrentPhase == Phase.HarvesterB && ResourcesIn(Game.LatestSpiceCardB.Location) > 6)
            ))
            return new HarvesterPlayed(Game, Faction);
        
        return null;
    }

    protected virtual MulliganPerformed DetermineMulliganPerformed()
    {
        return new MulliganPerformed(Game, Faction) { Passed = !MulliganPerformed.MayMulligan(Player) };
    }

    protected virtual OrangeDelay? DetermineDelay()
    {
        if (!Game.Prevented(FactionAdvantage.OrangeDetermineMoveMoment))
            return new OrangeDelay(Game, Faction);
        
        return null;
    }

    protected virtual ClairVoyancePlayed? DetermineClairvoyance()
    {
        var imInBattle = Game is { CurrentPhase: Phase.BattlePhase, CurrentBattle: not null } && Game.CurrentBattle.IsAggressorOrDefender(Player);
        var waitForPrescienceOrVoice = false;
        if (imInBattle)
        {
            var opponent = Game.CurrentBattle!.OpponentOf(Player);
            waitForPrescienceOrVoice = Game.CurrentBattle.PlanOf(opponent) == null &&
                                       (Prescience.MayUsePrescience(Game, opponent) ||
                                        Voice.MayUseVoice(Game, opponent));
        }  

        if (imInBattle && !waitForPrescienceOrVoice && Game.LatestClairvoyanceBattle != Game.CurrentBattle)
        {
            var opponent = Game.CurrentBattle!.OpponentOf(Player);

            if (NrOfUnknownOpponentCards(opponent) > 0)
            {
                LogInfo("Start using Clairvoyance against " + opponent);

                var myWeapons = Battle.ValidWeapons(Game, Player, null, null, Game.CurrentBattle.Territory)
                    .ToArray();
                var enemyDefenses = Battle.ValidDefenses(Game, opponent, null, Game.CurrentBattle.Territory)
                    .Where(w => Game.KnownCards(Player).Contains(w)).ToArray();

                if (
                    MyPrescience is not { Aspect: PrescienceAspect.Defense } && myWeapons.All(w => w.Type != TreacheryCardType.ProjectileAndPoison))
                {
                    if (myWeapons.Any(w => w.IsPoisonWeapon) && !OpponentMayNotUse(TreacheryCardType.PoisonDefense, false) && !enemyDefenses.Any(w => w.IsPoisonDefense)) return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsDefenseInBattle, TreacheryCardType.PoisonDefense);
                    if (myWeapons.Any(w => w.IsProjectileWeapon) && !OpponentMayNotUse(TreacheryCardType.ProjectileDefense, false) && !enemyDefenses.Any(w => w.IsProjectileDefense)) return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsDefenseInBattle, TreacheryCardType.ProjectileDefense);
                }

                var myDefenses = Battle.ValidDefenses(Game, Player, null, Game.CurrentBattle.Territory).ToArray();

                if (
                    MyPrescience is not { Aspect: PrescienceAspect.Weapon } && 
                    myDefenses.All(w => w.Type != TreacheryCardType.ShieldAndAntidote) &&
                    myDefenses.Any(w => w.IsPoisonDefense) &&
                    myDefenses.Any(w => w.IsProjectileDefense) &&
                    !OpponentMayNotUse(TreacheryCardType.Poison, true) &&
                    !OpponentMayNotUse(TreacheryCardType.Projectile, true)
                )
                    return UseClairvoyanceInBattle(opponent.Faction, ClairvoyanceQuestion.CardTypeAsWeaponInBattle, TreacheryCardType.Poison);
            }
        }

        if (Game.CurrentPhase == Phase.Bidding && !Player.HasRoomForCards)
        {
            var bestLeaderToAskAbout = Player.Leaders.Where(l => Game.IsAlive(l) && !SafeOrKnownTraitorLeaders.Contains(l)).HighestOrDefault(l => l.Value);
            var bestPlayerToAsk = Opponents.Where(o => !o.ToldNonTraitors.Contains(bestLeaderToAskAbout)).HighestOrDefault(p => p.Traitors.Count - p.RevealedTraitors.Count);

            if (bestLeaderToAskAbout != null && bestPlayerToAsk != null) return new ClairVoyancePlayed(Game, Faction) { Target = bestPlayerToAsk.Faction, Question = ClairvoyanceQuestion.LeaderAsTraitor, Parameter1 = bestLeaderToAskAbout.Id };
        }

        return null;
    }

    private bool OpponentMayNotUse(TreacheryCardType type, bool asWeapon)
    {
        var voice = MyVoice;
        return voice is { MayNot: true } && Voice.IsVoicedBy(Game, asWeapon, false, type, voice.Type);
    }

    private ClairVoyancePlayed UseClairvoyanceInBattle(Faction opponent, ClairvoyanceQuestion question, TreacheryCardType cardType)
    {
        return new ClairVoyancePlayed(Game, Faction) { Target = opponent, Question = question, QuestionParameter1 = cardType.ToString() };
    }

    protected virtual ClairVoyanceAnswered DetermineClairVoyanceAnswered()
    {
        LogInfo("DetermineClairVoyanceAnswered() {0}", Game.LatestClairvoyance.Question);

        var answer = ClairVoyanceAnswer.Unknown;

        try
        {
            switch (Game.LatestClairvoyance.Question)
            {
                case ClairvoyanceQuestion.CardTypeAsDefenseInBattle:
                    if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        var plan = Game.CurrentBattle.PlanOf(Player) ?? DetermineBattlePlan(false, true);
                        LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                        answer = Answer(plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(false, plan.Defense.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                    }
                    break;

                case ClairvoyanceQuestion.CardTypeAsWeaponInBattle:
                    if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        var plan = Game.CurrentBattle.PlanOf(Player) ?? DetermineBattlePlan(false, true);
                        LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                        answer = Answer(plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(true, plan.Weapon.Type, (TreacheryCardType)Game.LatestClairvoyance.Parameter1));
                    }
                    break;

                case ClairvoyanceQuestion.CardTypeInBattle:
                    if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        var plan = Game.CurrentBattle.PlanOf(Player);
                        plan ??= DetermineBattlePlan(false, true);
                        LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                        answer = Answer(
                            (plan.Defense != null && ClairVoyanceAnswered.IsQuestionedBy(false, plan.Defense.Type,
                                (TreacheryCardType)Game.LatestClairvoyance.Parameter1)) ||
                            (plan.Weapon != null && ClairVoyanceAnswered.IsQuestionedBy(true, plan.Weapon.Type,
                                (TreacheryCardType)Game.LatestClairvoyance.Parameter1)) ||
                            (plan.Hero is TreacheryCard &&
                             (TreacheryCardType)Game.LatestClairvoyance.Parameter1 == TreacheryCardType.Mercenary));
                    }
                    break;

                case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                    if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        var plan = Game.CurrentBattle.PlanOf(Player) ?? DetermineBattlePlan(false, true);
                        LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                        answer = Answer(plan.Dial(Game, Game.CurrentBattle.OpponentOf(Player).Faction) > (float)Game.LatestClairvoyance.Parameter1);
                    }
                    break;

                case ClairvoyanceQuestion.LeaderInBattle:
                    if (Game.CurrentBattle != null && Game.CurrentBattle.IsAggressorOrDefender(Player))
                    {
                        var plan = Game.CurrentBattle.PlanOf(Player) ?? DetermineBattlePlan(false, true);
                        LogInfo("My plan will be: " + plan.GetBattlePlanMessage());
                        answer = Answer(plan.Hero == (IHero)Game.LatestClairvoyance.Parameter1);
                    }
                    break;

                case ClairvoyanceQuestion.HasCardTypeInHand:
                    answer = Answer(Player.TreacheryCards.Any(c => Covers(c.Type, Game.LatestClairvoyance.Parameter1)));
                    break;

                case ClairvoyanceQuestion.LeaderAsFacedancer:
                    answer = Answer(Player.FaceDancers.Any(f => f.IsFaceDancer((IHero)Game.LatestClairvoyance.Parameter1)));
                    break;

                case ClairvoyanceQuestion.LeaderAsTraitor:
                    answer = Answer(Player.Traitors.Any(f => f.IsTraitor((IHero)Game.LatestClairvoyance.Parameter1)));
                    break;

                case ClairvoyanceQuestion.Prediction:
                    answer = Answer(Player.PredictedFaction == (Faction)Game.LatestClairvoyance.Parameter1 && Player.PredictedTurn == (int)Game.LatestClairvoyance.Parameter2);
                    break;

                case ClairvoyanceQuestion.WillAttackX:
                    answer = ClairVoyanceAnswer.No;
                    break;
            }
        }
        catch (Exception e)
        {
            LogInfo(e.ToString());
        }

        return new ClairVoyanceAnswered(Game, Faction) { Answer = answer };
    }

    private bool Covers(TreacheryCardType typeToCheck, object coveredByType)
    {
        return
            ClairVoyanceAnswered.IsQuestionedBy(true, typeToCheck, (TreacheryCardType)coveredByType) ||
            ClairVoyanceAnswered.IsQuestionedBy(false, typeToCheck, (TreacheryCardType)coveredByType);
    }

    private ClairVoyanceAnswer Answer(bool value)
    {
        return value ? ClairVoyanceAnswer.Yes : ClairVoyanceAnswer.No;
    }

    protected virtual RaiseDeadPlayed? DetermineRaiseDeadPlayed()
    {
        if ((Game.CurrentMainPhase == MainPhase.Bidding && !Player.HasRoomForCards) || Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
        {
            var nrOfLivingLeaders = Player.Leaders.Count(l => Game.IsAlive(l));
            var specialForcesThatCanBeRevived = Math.Min(5, Revival.ValidMaxRevivals(Game, Player, true, false));

            if ((Player.ForcesKilled + specialForcesThatCanBeRevived >= 7 && nrOfLivingLeaders > 1) || (Player.ForcesKilled + specialForcesThatCanBeRevived >= 17 && nrOfLivingLeaders > 0))
            {
                var forces = Math.Max(0, Math.Min(5, Player.ForcesKilled) - specialForcesThatCanBeRevived);

                Location? targetOfForces = null;
                var specialForcesToPlanet = Revival.NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, specialForcesThatCanBeRevived);
                if (specialForcesToPlanet > 0)
                {
                    targetOfForces = BestRevivalLocation(Revival.ValidRevivedForceLocations(Game, Player).ToList());
                    if (targetOfForces == null) specialForcesToPlanet = 0;
                }

                return new RaiseDeadPlayed(Game, Faction) { Hero = null, AmountOfForces = forces, AmountOfSpecialForces = specialForcesThatCanBeRevived, AssignSkill = false, Location = targetOfForces, NumberOfSpecialForcesInLocation = specialForcesToPlanet };
            }

            var minimumValue = Faction == Faction.Purple && nrOfLivingLeaders > 2 ? 4 : 0;

            var leaderToRevive = RaiseDeadPlayed.ValidHeroes(Game, Player).Where(l =>
                SafeOrKnownTraitorLeaders.Contains(l) &&
                l.Faction != Ally &&
                l.Value >= minimumValue
            ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l)) ?? RaiseDeadPlayed.ValidHeroes(Game, Player).Where(l =>
                l.Faction != Ally &&
                l.Value >= minimumValue
            ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));

            if (leaderToRevive == null) return null;
            
            var assignSkill = Revival.MayAssignSkill(Game, Player, leaderToRevive);
            return new RaiseDeadPlayed(Game, Faction) { Hero = leaderToRevive, AmountOfForces = 0, AmountOfSpecialForces = 0, AssignSkill = assignSkill };
        }

        return null;
    }

    protected virtual AmalPlayed? DetermineAmalPlayed()
    {
        if (Faction == Faction.Orange)
        {
            var allyResources = Ally != Faction.None ? Player.AlliedResources : 0;

            if (Game.CurrentPhase == Phase.Resurrection && Opponents.Sum(p => p.Resources) > 2 * (Resources + allyResources))
                return new AmalPlayed(Game, Faction);
        }
        else
        {
            var allyResources = Ally != Faction.None ? Player.AlliedResources : 0;

            if (Game is { CurrentTurn: > 1, CurrentMainPhase: MainPhase.Bidding } && Opponents.Sum(p => p.Resources) > 10 && Opponents.Sum(p => p.Resources) > (Opponents.Count() + 1) * (Resources + allyResources))
                return new AmalPlayed(Game, Faction);
        }

        return null;
    }

    protected virtual RecruitsPlayed? DetermineRecruitsPlayed()
    {
        if ((Player.ForcesKilled + Player.SpecialForcesKilled >= 6 && Resources >= 14) ||
            (Player.ForcesKilled + Player.SpecialForcesKilled >= 4 && Game.FreeRevivals(Player, false) >= 2))
            return new RecruitsPlayed(Game, Faction);

        return null;
    }

    protected virtual MetheorPlayed? DetermineMetheorPlayed()
    {
        var otherForcesInArrakeen = Game.BattalionsIn(Game.Map.Arrakeen).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
        var mineAndAlliedForcesInArrakeen = Game.BattalionsIn(Game.Map.Arrakeen).Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

        var otherForcesInCarthag = Game.BattalionsIn(Game.Map.Carthag).Where(b => b.Faction != Faction && b.Faction != Ally).Sum(b => b.TotalAmountOfForces);
        var mineAndAlliedForcesInCarthag = Game.BattalionsIn(Game.Map.Carthag).Where(b => b.Faction == Faction || b.Faction == Ally).Sum(b => b.TotalAmountOfForces);

        if (otherForcesInArrakeen + otherForcesInCarthag > 2 * (mineAndAlliedForcesInArrakeen + mineAndAlliedForcesInCarthag))
            return new MetheorPlayed(Game, Faction);
        return null;
    }

    protected virtual StormDialled DetermineStormDialled()
    {
        var min = StormDialled.ValidMinAmount(Game);
        var max = StormDialled.ValidMaxAmount(Game);
        return new StormDialled(Game, Faction) { Amount = min + D(1, 1 + max - min) - 1 };
    }

    protected virtual TraitorsSelected DetermineTraitorsSelected()
    {
        var traitor = Player.Traitors.Where(l => l.Faction != Faction).HighestOrDefault(l => l.Value) 
                      ?? Player.Traitors.HighestOrDefault(l => l.Value - (l.Faction == Faction.Green && Game.Applicable(Rule.GreenMessiah) ? 2 : 0));
        return new TraitorsSelected(Game, Faction) { SelectedTraitor = traitor };
    }

    protected virtual FactionTradeOffered? DetermineFactionTradeOffered()
    {
        var match = Game.CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Target == Faction);
        if (match != null) return new FactionTradeOffered(Game, Faction) { Target = match.Initiator };

        return null;
    }

    protected virtual ThumperPlayed DetermineThumperPlayed()
    {
        return new ThumperPlayed(Game, Faction);
    }

    protected virtual StormSpellPlayed? DetermineStormSpellPlayed()
    {
        var myKills = new Dictionary<int, int>
        {
            { 0, 0 }
        };

        var enemyKills = new Dictionary<int, int>
        {
            { 0, 0 }
        };

        for (var moves = 1; moves <= 10; moves++)
        {
            var movesToCheck = moves;
            var affectedLocations = Game.Map.Locations(false)
                .Where(l => l.Sector == (Game.SectorInStorm + movesToCheck) % Map.NumberOfSectors && !Game.IsProtectedFromStorm(l))
                .ToArray();
            var myAndAllyForces = affectedLocations.Sum(l => Game.BattalionsIn(l)
                .Where(bat => bat.Faction == Faction || bat.Faction == Ally)
                .Sum(bat => bat.TotalAmountOfForces));
            var enemyForces = affectedLocations.Sum(l => Game.BattalionsIn(l)
                .Where(bat => bat.Faction != Faction && bat.Faction != Ally)
                .Sum(bat => bat.TotalAmountOfForces));
            myKills.Add(moves, myKills[moves - 1] + myAndAllyForces);
            enemyKills.Add(moves, enemyKills[moves - 1] + enemyForces);
        }

        var mostEffectiveMove = myKills.HighestOrDefault(x => enemyKills[x.Key] - x.Value);
        LogInfo("StormSpellPlayed() - Most effective number of moves: {0} sectors with {1} allied and {2} enemy kills.", mostEffectiveMove.Key, myKills[mostEffectiveMove.Key], enemyKills[mostEffectiveMove.Key]);

        if (enemyKills[mostEffectiveMove.Key] - myKills[mostEffectiveMove.Key] >= 10 - (Game.CurrentTurn + (Player.HasRoomForCards ? 0 : 4)))
        {
            var stormSpell = new StormSpellPlayed(Game, Faction) { MoveAmount = mostEffectiveMove.Key };
            return stormSpell;
        }

        return null;
    }

    private DistransUsed? DetermineDistransUsed()
    {
        if (Game.CurrentPhase != Phase.WaitingForNextBiddingRound) return null;
        
        var worstCard = DistransUsed.ValidCards(Player).LowestOrDefault(c => CardQuality(c, Player));
        
        if (worstCard == null || CardQuality(worstCard, Player) > 1) return null;
        
        var target = DistransUsed.ValidTargets(Game, Player)
            .Where(f => f != Ally && (!Game.Applicable(Rule.BlueWorthlessAsKarma) || f != Faction.Blue))
            .Select(f => Game.GetPlayer(f))
            .HighestOrDefault(p => Game.NumberOfVictoryPoints(p, true));

        return target != null ? new DistransUsed(Game, Faction) { Card = worstCard, Target = target.Faction } : null;
    }

    private int HeroRevivalPenalty(IHero h)
    {
        if (h.HeroType == HeroType.Messiah || h.HeroType == HeroType.Auditor)
            return 20;
        if (Player.KnownNonTraitors.Contains(h))
            return 10;
        return 0;
    }

    protected virtual Revival? DetermineRevival()
    {
        if (Game.CurrentRevivalRequests.Any(r => r.By(Faction))) return null;

        var nrOfLivingLeaders = Player.Leaders.Count(l => Game.IsAlive(l));
        var minimumValue = Faction == Faction.Purple && nrOfLivingLeaders > 2 ? 4 : 0;
        var maxToSpendOnHeroRevival = Math.Min(Resources, 8);

        var leaderToRevive = Revival.ValidRevivalHeroes(Game, Player).Where(l =>
            SafeOrKnownTraitorLeaders.Contains(l) &&
            Revival.GetPriceOfHeroRevival(Game, Player, l) <= maxToSpendOnHeroRevival &&
            l.Faction != Ally &&
            l.Value >= minimumValue
        ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l)) ?? Revival.ValidRevivalHeroes(Game, Player).Where(l =>
            Revival.GetPriceOfHeroRevival(Game, Player, l) <= maxToSpendOnHeroRevival &&
            l.Faction != Ally &&
            l.Value >= minimumValue
        ).HighestOrDefault(l => l.Value + HeroRevivalPenalty(l));

        var useSecretAlly = Revival.MayUseRedSecretAlly(Game, Player) && Player.ForcesKilled - Game.FreeRevivals(Player, false) >= 3;

        DetermineOptimalUseOfRedRevivals(Game, Player, out var forcesRevivedByRed, out var specialForcesRevivedByRed, useSecretAlly);

        var specialForcesToRevive = 0;
        while (
            //check limit of special forces
            specialForcesRevivedByRed + specialForcesToRevive + 1 <= Revival.ValidMaxRevivals(Game, Player, true, useSecretAlly) &&

            //check if there are enough forces killed
            specialForcesRevivedByRed + specialForcesToRevive + 1 <= Player.SpecialForcesKilled &&

            //check if I have enough spice
            Revival.DetermineCost(Game, Player, leaderToRevive, 0, specialForcesToRevive + 1, forcesRevivedByRed, specialForcesRevivedByRed, useSecretAlly).TotalCostForPlayer <= Resources)
            specialForcesToRevive++;

        var normalForcesToRevive = 0;

        while (
            //check limit of total amount of forces
            specialForcesToRevive + normalForcesToRevive + 1 <= Revival.ValidMaxRevivals(Game, Player, false, useSecretAlly) &&

            //check if there are enough forces killed
            forcesRevivedByRed + normalForcesToRevive + 1 <= Player.ForcesKilled &&

            //check if I have enough spice
            Revival.DetermineCost(Game, Player, leaderToRevive, normalForcesToRevive + 1, specialForcesToRevive, forcesRevivedByRed, specialForcesRevivedByRed, useSecretAlly).TotalCostForPlayer <= Resources)
            normalForcesToRevive++;

        var assignSkill = leaderToRevive != null && Revival.MayAssignSkill(Game, Player, leaderToRevive);

        Location? targetOfForces = null;
        var forcesToPlanet = Revival.NumberOfForcesThatMayBePlacedOnPlanet(Game, Player, useSecretAlly, normalForcesToRevive + forcesRevivedByRed);
        var specialForcesToPlanet = Revival.NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, specialForcesToRevive + specialForcesRevivedByRed);

        if (forcesToPlanet > 0)
        {
            targetOfForces = BestRevivalLocation(Revival.ValidRevivedForceLocations(Game, Player).ToList());
            if (targetOfForces == null) forcesToPlanet = 0;
        }
        else if (specialForcesToPlanet > 0)
        {
            targetOfForces = BestRevivalLocation(Revival.ValidRevivedForceLocations(Game, Player).ToList());
            if (targetOfForces == null) specialForcesToPlanet = 0;
        }

        if (leaderToRevive != null || specialForcesToRevive + normalForcesToRevive > 0)
            return new Revival(Game, Faction)
            {
                Hero = leaderToRevive,
                AmountOfForces = normalForcesToRevive,
                ExtraForcesPaidByRed = forcesRevivedByRed,
                AmountOfSpecialForces = specialForcesToRevive,
                ExtraSpecialForcesPaidByRed = specialForcesRevivedByRed,
                AssignSkill = assignSkill,
                UsesRedSecretAlly = useSecretAlly,
                Location = targetOfForces,
                NumberOfForcesInLocation = forcesToPlanet,
                NumberOfSpecialForcesInLocation = specialForcesToPlanet
            };
        return null;
    }

    private Location? BestRevivalLocation(List<Location> validLocations)
    {
        var result = validLocations.FirstOrDefault(l => Equals(l, Game.Map.Carthag) && (Vacant(l) || (Player.AnyForcesIn(l) > 0 && Player.AnyForcesIn(l) < 4))) 
                     ?? validLocations.FirstOrDefault(l => Equals(l, Game.Map.Arrakeen) && (Vacant(l) || (Player.AnyForcesIn(l) > 0 && Player.AnyForcesIn(l) < 4))) 
                     ?? validLocations.FirstOrDefault(l => ResourcesIn(l) >= 4) 
                     ?? validLocations.FirstOrDefault(l => l.IsStronghold && (Vacant(l) || (Player.AnyForcesIn(l) > 0 && Player.AnyForcesIn(l) < 4)));
        
        if (result == null && Player.ForcesInReserve + Player.SpecialForcesInReserve > 0) result = validLocations.FirstOrDefault(l => Equals(l, Game.Map.PolarSink));

        return result;
    }

    private static void DetermineOptimalUseOfRedRevivals(Game g, Player p, out int forces, out int specialForces, bool useCunning)
    {
        forces = 0;
        specialForces = 0;

        if (p.Ally != Faction.Red) return;

        var red = g.GetPlayer(Faction.Red);

        var potentialMaximumByRed = p.Ally == Faction.Red && (g.Version < 113 || !g.Prevented(FactionAdvantage.RedLetAllyReviveExtraForces)) ? g.RedWillPayForExtraRevival : 0;

        var maxSpecialForces = Revival.ValidMaxRevivals(g, p, true, useCunning);

        var freeRevivals = g.FreeRevivals(p, useCunning);

        if (maxSpecialForces > 0 && freeRevivals > 0)
        {
            maxSpecialForces--;
            freeRevivals--;
        }

        while (
            specialForces + 1 <= p.SpecialForcesKilled &&
            specialForces + 1 <= maxSpecialForces &&
            specialForces + 1 <= potentialMaximumByRed &&
            Revival.DetermineCostOfForcesForRed(g, red, p.Faction, 0, specialForces + 1) <= red.Resources)
            specialForces++;

        var maxForces = Revival.ValidMaxRevivals(g, p, false, useCunning);

        maxForces = Math.Max(maxForces - freeRevivals, 0);

        while (
            forces + 1 <= p.ForcesKilled &&
            forces + 1 <= maxForces &&
            specialForces + forces + 1 <= potentialMaximumByRed &&
            Revival.DetermineCostOfForcesForRed(g, red, p.Faction, forces + 1, specialForces) <= red.Resources)
            forces++;
    }

    private DiscardedSearchedAnnounced? DetermineDiscardedSearchedAnnounced()
    {
        if (Game.CurrentMainPhase == MainPhase.Contemplate)
        {
            var cardToSearch = DiscardedSearched.ValidCards(Game).HighestOrDefault(c => CardQuality(c, Player));
            if (cardToSearch != null && CardQuality(cardToSearch, Player) >= 4) return new DiscardedSearchedAnnounced(Game, Faction);
        }

        return null;
    }

    private DiscardedSearched DetermineDiscardedSearched()
    {
        var cardToSearch = DiscardedSearched.ValidCards(Game).HighestOrDefault(c => CardQuality(c, Player));
        return new DiscardedSearched(Game, Faction) { Card = cardToSearch };
    }

    private DiscardedTaken? DetermineDiscardedTaken()
    {
        var cardToTake = DiscardedTaken.ValidCards(Game, Player).HighestOrDefault(c => CardQuality(c, Player));
        if (cardToTake != null && CardQuality(cardToTake, Player) >= 4) return new DiscardedTaken(Game, Faction) { Card = cardToTake };

        return null;
    }

    private JuicePlayed? DetermineJuicePlayed()
    {
        if (Game is { CurrentMainPhase: MainPhase.ShipmentAndMove, CurrentMoment: MainPhaseMoment.Start } 
            && Faction != Faction.Orange 
            && Game.ShipmentAndMoveSequence.GetPlayersInSequence().LastOrDefault()?.Player != Player)
            return new JuicePlayed(Game, Faction) { Type = JuiceType.GoLast };
        
        if (Game is { CurrentMainPhase: MainPhase.Battle, CurrentMoment: MainPhaseMoment.Start } 
            && Battle.BattlesToBeFought(Game, Player).Any()) 
            return new JuicePlayed(Game, Faction) { Type = JuiceType.GoLast };

        return null;
    }

    private Bureaucracy DetermineBureaucracy()
    {
        return new Bureaucracy(Game, Faction) { Passed = Game.TargetOfBureaucracy == Ally };
    }

    private Diplomacy? DetermineDiplomacy()
    {
        var opponentPlan = Game.CurrentBattle.PlanOfOpponent(Player);
        if (opponentPlan.Weapon != null && opponentPlan.Weapon.CounteredBy(opponentPlan.Defense, null)) return new Diplomacy(Game, Faction) { Card = Diplomacy.ValidCards(Game, Player).First() };

        return null;
    }

    private SkillAssigned DetermineSkillAssigned()
    {
        var skill = LeaderSkill.None;
        var skills = SkillAssigned.ValidSkills(Player).ToArray();

        if (Player.TreacheryCards.Any(c => c.IsProjectileWeapon)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Swordmaster);
        if (skill == LeaderSkill.None && Player.TreacheryCards.Any(c => c.IsPoisonWeapon)) skill = skills.FirstOrDefault(s => s == LeaderSkill.MasterOfAssassins);
        if (skill == LeaderSkill.None && Player.TreacheryCards.Any(c => c.IsProjectileDefense)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Adept);
        if (skill == LeaderSkill.None && Player.TreacheryCards.Any(c => c.IsPoisonDefense)) skill = skills.FirstOrDefault(s => s == LeaderSkill.KillerMedic);
        if (skill == LeaderSkill.None && Player.TreacheryCards.Any(c => c.IsUseless)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Warmaster);

        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Graduate);
        if (skill == LeaderSkill.None && !Player.Is(Faction.Green)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Thinker);

        if (skill == LeaderSkill.None && Player.Is(Faction.Yellow)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Sandmaster);
        if (skill == LeaderSkill.None && !Player.Is(Faction.Yellow)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Smuggler);

        var isEconomicFaction = Player.Is(Faction.Red) || Player.Is(Faction.Orange) || Player.Is(Faction.Brown) || Player.Is(Faction.Purple);
        if (skill == LeaderSkill.None && !isEconomicFaction) skill = skills.FirstOrDefault(s => s == LeaderSkill.Bureaucrat);
        if (skill == LeaderSkill.None && !isEconomicFaction) skill = skills.FirstOrDefault(s => s == LeaderSkill.Banker);

        if (skill == LeaderSkill.None && !Player.Is(Faction.Purple)) skill = skills.FirstOrDefault(s => s == LeaderSkill.Decipherer);

        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Swordmaster);
        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.MasterOfAssassins);
        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Adept);
        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.KillerMedic);
        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s == LeaderSkill.Warmaster);

        if (skill == LeaderSkill.None) skill = skills.FirstOrDefault(s => s != LeaderSkill.Planetologist);

        if (skill == LeaderSkill.None) skill = skills.First();

        return new SkillAssigned(Game, Faction) { Passed = false, Leader = RandomItemFrom(SkillAssigned.ValidLeaders(Game, Player)), Skill = skill };
    }

    private T RandomItemFrom<T>(IEnumerable<T> items)
    {
        var itemsAsArray = items.ToArray();
        return itemsAsArray[D(1, itemsAsArray.Length) - 1];
    }

    protected virtual Planetology DeterminePlanetology()
    {
        return new Planetology(Game, Faction) { AddOneToMovement = true };
    }
}