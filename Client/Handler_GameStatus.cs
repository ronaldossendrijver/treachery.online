/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public partial class Handler
    {
        public GameStatus Status { get; private set; }

        public void UpdateStatus()
        {
            Status = DetermineStatus();
            Status.FlashInfo = DetermineFlash(Game);
        }

        public GameStatus DetermineStatus()
        {
            switch (Game.CurrentPhase)
            {
                case Phase.StormReport:
                case Phase.BlowReport:
                case Phase.CharityReport:
                case Phase.BiddingReport:
                case Phase.ResurrectionReport:
                case Phase.BattleReport:
                case Phase.CollectionReport:
                case Phase.TurnConcluded:
                    return S(Skin.Current.Format("Factions may now review the {0} report...", Game.CurrentReport.About));

                case Phase.AssigningInitialSkills:
                case Phase.AssigningSkill:
                    return S(
                     Skin.Current.Format("You may now assign a skill to a leader."),
                     Skin.Current.Format("Waiting for factions to assign leader skills..."),
                     Game.Players.Where(p => p.SkillsToChooseFrom.Any()));

                case Phase.PerformingKarmaHandSwap:
                    return S(
                    Skin.Current.Format("Please decide which {0} cards to return to {1}.", Game.KarmaHandSwapNumberOfCards, Game.KarmaHandSwapTarget),
                    Skin.Current.Format("{0} are deciding which {1} cards to return to {2}.", Faction.Black, Game.KarmaHandSwapNumberOfCards, Game.KarmaHandSwapTarget),
                    Faction.Black);

                case Phase.Clairvoyance:
                    return S(
                    Skin.Current.Format("Please answer a question asked to you by {0} in {1}...", Game.LatestClairvoyance.Initiator, TreacheryCardType.Clairvoyance),
                    Skin.Current.Format("Waiting for {0} to answer a question asked in {1}...", Game.LatestClairvoyance.Target, TreacheryCardType.Clairvoyance),
                    Game.LatestClairvoyance.Target);

                case Phase.Thought:
                    return S(
                    Skin.Current.Format("{0} asked you a question! All factions are waiting for you answer...", Game.CurrentThought.Initiator),
                    Skin.Current.Format("Waiting for {0} to answer a question...", Game.CurrentBattle.OpponentOf(Game.CurrentThought.Initiator).Faction),
                    Game.CurrentBattle.OpponentOf(Game.CurrentThought.Initiator));

                case Phase.SearchingDiscarded:
                    return S(
                    Skin.Current.Format("Please select a card from the treachery discard pile to take..."),
                    Skin.Current.Format("Waiting for a card to be searched and taken from the treachery discard pile..."),
                    Game.OwnerOf(TreacheryCardType.SearchDiscarded));

                case Phase.StormLosses:
                    return S(
                    Skin.Current.Format("Please decide which forces were killed by the storm in {0}.", TakeLosses.LossesToTake(Game).Location),
                    Skin.Current.Format("{0} are deciding which forces were killed by the storm in {1}...", TakeLosses.LossesToTake(Game).Faction, TakeLosses.LossesToTake(Game).Location),
                    Game.StormLossesToTake[0].Faction);

                case Phase.TradingCards:
                    return S(
                    Skin.Current.Format("Please select a card to give to your ally in return"),
                    Skin.Current.Format("Waiting for a card to be returned..."),
                    Game.CurrentCardTradeOffer.Player.Ally);

                case Phase.Bureaucracy:
                    return S(
                    Skin.Current.Format("Please decide whether to apply Bureaucracy to the latest payment."),
                    Skin.Current.Format("Waiting for Bureaucracy to be applied to the latest payment..."),
                    Game.PlayerSkilledAs(LeaderSkill.Bureaucrat));

                case Phase.AwaitingPlayers:
                    return S(
                    Skin.Current.Format("Configure the game and wait for players to join. Start the game when ready."),
                    Skin.Current.Format("Waiting for the host to configure and start the game..."));

                case Phase.SelectingFactions: return S(Skin.Current.Format("Players may now choose a faction..."));

                case Phase.TradingFactions: return S(Skin.Current.Format("Players may now offer to trade factions with other players..."));

                case Phase.BluePredicting:
                    return S(
                    Skin.Current.Format("Please predict who will win the game and when."),
                    Skin.Current.Format("{0} are predicting who will win the game and when...", Faction.Blue),
                    Faction.Blue);

                case Phase.BlackMulligan:
                    return S(
                    Skin.Current.Format("You may draw a new set of traitors if you were dealt two or more of your own leaders."),
                    Skin.Current.Format("{0} may draw a new set of traitors if they were dealt two or more of their own leaders...", Faction.Black),
                    Faction.Black);

                case Phase.SelectingTraitors:
                    return S(
                    Skin.Current.Format("Please select one leader to keep as a traitor"),
                    Skin.Current.Format("Factions are selecting traitors..."),
                    PlayersThatHaventActedOrPassed);

                case Phase.CustomizingDecks:
                    return S("Please customize the cards in play.", "The host is customizing the cards in play.");

                case Phase.PerformCustomSetup:
                    return S(
                    Skin.Current.Format("Please set up {0} initial {1} and force positions.", Game.NextFactionToPerformCustomSetup, Concept.Resource),
                    Skin.Current.Format("The host is setting up {0} initial {1} and force positions.", Game.NextFactionToPerformCustomSetup, Concept.Resource));

                case Phase.YellowSettingUp:
                    return S(
                    Skin.Current.Format("Please choose your initial force positions."),
                    Skin.Current.Format("{0} are setting up their initial force positions...", Faction.Yellow),
                    Faction.Yellow);

                case Phase.BlueSettingUp:
                    return S(
                    Skin.Current.Format("Please select your initial force position."),
                    Skin.Current.Format("{0} are selecting their initial force position...", Faction.Blue),
                    Faction.Blue);

                case Phase.GreySelectingCard:
                    return S(
                    Skin.Current.Format("Please select your starting Treachery Card."),
                    Skin.Current.Format("{0} are selecting their starting Treachery Card...", Faction.Grey),
                    Faction.Grey);

                case Phase.HmsPlacement:
                    return S(
                    Skin.Current.Format("Please position the Hidden Mobile Stronghold."),
                    Skin.Current.Format("The Hidden Mobile Stronghold is being positioned..."),
                    Faction.Grey);

                case Phase.DiallingStorm:
                    return S(
                    Skin.Current.Format("Please dial a number to determine storm movement."),
                    Skin.Current.Format("Storm movement is being determined..."),
                    Game.FactionsInPlay.Where(f => Game.HasBattleWheel.Contains(f) && !Game.HasActedOrPassed.Contains(f)));

                case Phase.HmsMovement:
                    return S(
                    Skin.Current.Format("You may move the Hidden Mobile Stronghold to a sector in an adjacent territory, or pass."),
                    Skin.Current.Format("The Hidden Mobile Stronghold is being moved..."),
                    Faction.Grey);

                case Phase.MetheorAndStormSpell: return S(Skin.Current.Format("Factions may now use {0} or {1}...", TreacheryCardType.Metheor, TreacheryCardType.StormSpell));

                case Phase.Thumper:
                    if (Game.CurrentTurn > 1)
                    {
                        return S(Skin.Current.Format("Factions may now use a {0} to call {1}...", TreacheryCardType.Thumper, Concept.Monster));
                    }
                    else
                    {
                        return S(Skin.Current.Format("You may now proceed to the first Spice Blow"), Skin.Current.Format("Waiting for the host to proceed to the first Spice Blow"));
                    }

                case Phase.YellowSendingMonsterA:
                case Phase.YellowSendingMonsterB:
                    return S(
                    Skin.Current.Format("Please decide where to make {0} appear.", Concept.Monster),
                    Skin.Current.Format("{0} are deciding where to make {1} appear...", Faction.Yellow, Concept.Monster),
                    Faction.Yellow);

                case Phase.YellowRidingMonsterA:
                case Phase.YellowRidingMonsterB:
                    return S(
                    Skin.Current.Format("Please decide where to travel with {0}.", Concept.Monster),
                    Skin.Current.Format("{0} are deciding where to travel with {1}...", Faction.Yellow, Concept.Monster),
                    Faction.Yellow);

                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                    return S(
                    Skin.Current.Format("Please decide what to do in response to an intrusion of {0}; be fighters or advisors?", Game.LastShippedOrMovedTo.Territory.Name),
                    Skin.Current.Format("{0} are deciding what to do in response to an intrusion of {1}...", Faction.Blue, Game.LastShippedOrMovedTo.Territory.Name),
                    Faction.Blue);

                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    return S(
                    Skin.Current.Format("Factions may use a {0} to double the {1} blow in {2}...",
                    TreacheryCardType.Harvester, Concept.Resource, CurrentPhase == Phase.HarvesterA ? Game.LatestSpiceCardA : Game.LatestSpiceCardB));

                case Phase.AllianceA:
                case Phase.AllianceB:
                    return S(
                    Skin.Current.Format("Factions may now make and break alliances."));

                /* Charity */

                case Phase.BeginningOfCharity:
                    return S(
                        Skin.Current.Format("You may now continue with the {0} phase...", MainPhase.Charity),
                        Skin.Current.Format("Waiting for the host to continue the {0} phase...", MainPhase.Charity));


                case Phase.ClaimingCharity: return S(Skin.Current.Format("Factions may now claim charity if eligible."));

                /* Bidding */

                case Phase.BlackMarketAnnouncement:
                    return S(
                    Skin.Current.Format("You may now select a card to sell on the Black Markt, or pass."),
                    Skin.Current.Format("{0} are thinking about selling one of their cards on the Black Market...", Faction.White),
                    Faction.White);

                case Phase.BlackMarketBidding:

                    if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent)
                    {
                        return S(
                        Skin.Current.Format("Please bid or pass."),
                        Skin.Current.Format("{0} are thinking about their bid...", Game.BidSequence.CurrentFaction),
                        Game.BidSequence.CurrentPlayer);
                    }
                    else
                    {
                        return S(
                        Skin.Current.Format("Please bid."),
                        Skin.Current.Format("Factions are thinking about their bids...", Game.BidSequence.CurrentFaction),
                        Game.Players.Where(p => p.HasRoomForCards && !Game.Bids.ContainsKey(p.Faction)));
                    }

                case Phase.WhiteAnnouncingAuction:
                    return S(
                    Skin.Current.Format("Please decide if you will auction a card from your cache First or Last."),
                    Skin.Current.Format(Skin.Current.Format("{0} are deciding to auction a card from their cache First or Last...", Faction.White)),
                    Faction.White);

                case Phase.WhiteSpecifyingAuction:
                    return S(
                    Skin.Current.Format("Please select a card from your cache to sell and select the type of auction."),
                    Skin.Current.Format("Waiting for {0} to put a card from their cache on auction...", Faction.White),
                    Faction.White);

                case Phase.WhiteKeepingUnsoldCard:
                    return S(
                    Skin.Current.Format("Please decide if you wish to keep this unsold card."),
                    Skin.Current.Format("{0} are deciding about keeping the unsold card...", Faction.White),
                    Faction.White);

                case Phase.GreyRemovingCardFromBid:
                    return S(
                    Skin.Current.Format("Please put one card from the auction on top or at the bottom of the Treachery Card deck."),
                    Skin.Current.Format("{0} are putting one card from the auction on top or at the bottom of the Treachery Card deck...", Faction.Grey),
                    Faction.Grey);

                case Phase.GreySwappingCard:
                    return S(
                    Skin.Current.Format("You may swap the next card on auction with a card from your hand, or pass."),
                    Skin.Current.Format("{0} are thinking about swapping the next card on auction with a card from their hand...", Faction.Grey),
                    Faction.Grey);

                case Phase.Bidding:

                    if (Game.CurrentAuctionType != AuctionType.WhiteSilent)
                    {
                        return S(
                        Skin.Current.Format("Please bid or pass."),
                        Skin.Current.Format("{0} are thinking about their bid...", Game.BidSequence.CurrentFaction),
                        Game.BidSequence.CurrentPlayer, Game.LatestEvent());
                    }
                    else
                    {
                        return S(
                        Skin.Current.Format("Please bid."),
                        Skin.Current.Format("Factions are thinking about their bids..."),
                        Game.Players.Where(p => p.HasRoomForCards && !Game.Bids.ContainsKey(p.Faction)), Game.LatestEvent());
                    }

                case Phase.ReplacingCardJustWon:
                    return S(
                    Skin.Current.Format("You may now discard the card you just won and draw a new card instead."),
                    Skin.Current.Format("{0} are thinking about replacing the card they just won with a new card from the deck...", Game.GetPlayer(Faction.Grey).Ally),
                    Game.GetPlayer(Faction.Grey).Ally);

                case Phase.WaitingForNextBiddingRound: return S(Skin.Current.Format("Factions are waiting for the next card to be put on auction..."));

                /* Revival */

                case Phase.BeginningOfResurrection:
                    return S(
                        Skin.Current.Format("You may now continue with the {0} phase...", MainPhase.Resurrection),
                        Skin.Current.Format("Waiting for the host to continue the {0} phase...", MainPhase.Resurrection));

                case Phase.Resurrection: return S(Skin.Current.Format("Factions may now reclaim forces and leaders."));

                /* Ship & Move */

                case Phase.BeginningOfShipAndMove:
                    return S(
                        Skin.Current.Format("You may now start the ship & move sequence..."),
                        Skin.Current.Format("Waiting for the host to start the ship & move sequence..."),
                        Game.ShipmentAndMoveSequence.GetPlayersInSequence().Select(ps => ps.Player));

                case Phase.NonOrangeShip:
                    return S(
                    Skin.Current.Format("Please decide to {0} forces or pass.", Game.ShipmentAndMoveSequence.CurrentFaction == Faction.Yellow ? "rally" : "ship"),
                    Skin.Current.Format("{0} are thinking about {1} forces...", Game.ShipmentAndMoveSequence.CurrentFaction, Game.ShipmentAndMoveSequence.CurrentFaction == Faction.Yellow ? "rallying" : "shipping"),
                    Game.ShipmentAndMoveSequence.CurrentPlayer, Game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move)));

                case Phase.OrangeShip:
                    return S(
                    Skin.Current.Format(Game.OrangeMayDelay ? Skin.Current.Format("Please decide to ship now or delay your turn and let other factions go first.") : Skin.Current.Format("Please decide to ship forces or pass.")),
                    Skin.Current.Format(Game.OrangeMayDelay ? Skin.Current.Format("{0} are deciding about taking their turn now...", Faction.Orange) : Skin.Current.Format("{0} are thinking about shipping forces...", Faction.Orange)),
                    Faction.Orange, Game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move)));

                case Phase.BlueAccompaniesNonOrange:
                case Phase.BlueAccompaniesOrange:
                    return S(
                    Skin.Current.Format("Do you wish to accompany the latest shipment?"),
                    Skin.Current.Format(Skin.Current.Format("{0} are thinking about accompanying the latest shipment...", Faction.Blue)),
                    Faction.Blue);

                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.BlueIntrudedByCaravan:
                    return S(
                    Skin.Current.Format("Please decide what to do in response to an intrusion of {0}; be fighters or advisors?", Game.LastShippedOrMovedTo.Territory.Name),
                    Skin.Current.Format("{0} are deciding what to do in response to an intrusion...", Faction.Blue),
                    Faction.Blue);

                case Phase.NonOrangeMove:
                    return S(
                    Skin.Current.Format("Please decide to move forces or pass."),
                    Skin.Current.Format("{0} are thinking about about moving forces.", Game.ShipmentAndMoveSequence.CurrentFaction),
                    Game.ShipmentAndMoveSequence.CurrentPlayer, Game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move)));

                case Phase.OrangeMove:
                    return S(
                    Skin.Current.Format("Please decide to move forces or pass."),
                    Skin.Current.Format("{0} are thinking about about moving forces.", Faction.Orange),
                    Faction.Orange, Game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move)));

                case Phase.ShipmentAndMoveConcluded: return S(Skin.Current.Format("Waiting for factions to be ready to enter the Battle phase..."));

                /* Battle */

                case Phase.BeginningOfBattle:
                    return S(
                        Skin.Current.Format("You may proceed to the first battle when ready."),
                        Skin.Current.Format("Waiting for the host to proceed to the first battle..."),
                        Game.BattleSequence.GetPlayersInSequence().Select(ps => ps.Player));

                case Phase.BattlePhase:

                    if (Game.CurrentBattle == null)
                    {
                        return S(
                        Skin.Current.Format("Please choose whom and where to battle."),
                        Skin.Current.Format("{0} are deciding whom and where to battle...", Game.NextPlayerToBattle.Faction),
                        Game.NextPlayerToBattle);
                    }
                    else
                    {
                        var latestBattleEvent = Game.LatestEvent(typeof(BattleInitiated));
                        if (IAm(Game.CurrentBattle.Aggressor))
                        {
                            return S(
                            Skin.Current.Format("You are aggressor against {0} in {1}! Please confirm your Battle Plan.", Game.CurrentBattle.Defender, Game.CurrentBattle.Territory),
                            Skin.Current.Format("You are waiting for {0} to defend {1}...", Game.CurrentBattle.Defender, Game.CurrentBattle.Territory),
                            FactionsThatNeedToMakeABattlePlan, latestBattleEvent);
                        }
                        else if (IAm(Game.CurrentBattle.Defender))
                        {
                            return S(
                            Skin.Current.Format("You must defend against {0} in {1}! Please confirm your Battle Plan.", Game.CurrentBattle.Aggressor, Game.CurrentBattle.Territory),
                            Skin.Current.Format("You are waiting for {0} to attack {1}...", Game.CurrentBattle.Aggressor, Game.CurrentBattle.Territory),
                            FactionsThatNeedToMakeABattlePlan, latestBattleEvent);
                        }
                        else
                        {
                            return S("",
                            Skin.Current.Format("{0} are defending against {1} aggression in {2}...", Game.CurrentBattle.Defender, Game.CurrentBattle.Aggressor, Game.CurrentBattle.Territory),
                            FactionsThatNeedToMakeABattlePlan, latestBattleEvent);
                        }
                    }

                case Phase.MeltingRock:
                    return S(
                    Skin.Current.Format("Please decide how to use your {0}.", TreacheryCardType.Rockmelter),
                    Skin.Current.Format("Waiting for a decision on how the {0} will be used...", TreacheryCardType.Rockmelter),
                    Game.CurrentBattle.AggressorAction.HasRockMelter ? Game.CurrentBattle.Player : Game.CurrentBattle.DefendingPlayer);

                case Phase.CallTraitorOrPass:
                    return S(
                    Skin.Current.Format("You may now call TREACHERY if the enemy leader is a traitor under your command."),
                    Skin.Current.Format("Waiting for a faction to call TREACHERY..."),
                    FactionsThatNeedToCallTraitor);

                case Phase.AvoidingAudit:
                    return S(
                    Skin.Current.Format("Please decide if you wish to avoid being audited."),
                    Skin.Current.Format("{0} are thinking about avoiding a scheduled audit...", Game.Auditee.Faction),
                    Game.Auditee);

                case Phase.CaptureDecision:
                    return S(
                    Skin.Current.Format("Please decide if you wish to capture a leader."),
                    Skin.Current.Format("Waiting for {0} to decide about capturing a leader...", Faction.Black),
                    Faction.Black);

                case Phase.Auditing:
                    return S(
                    Skin.Current.Format("Please conclude the audit when done inspecting your opponents cards."),
                    Skin.Current.Format("Waiting for {0} to finish their audit...", Faction.Brown),
                    Faction.Brown);

                case Phase.BattleConclusion:
                    return S(
                    Skin.Current.Format("You won! Conclude the battle when done celebrating your victory."),
                    Skin.Current.Format("{0} are celebrating their victory in battle...", Game.BattleWinner),
                    Game.BattleWinner);

                case Phase.Facedancing:
                    return S(
                    Skin.Current.Format("You may reveal a leader to be one of your face dancers."),
                    Skin.Current.Format("Waiting for {0} to reveal a face dancer...", Faction.Purple),
                    Faction.Purple);

                /* Collection */

                case Phase.BeginningOfCollection:
                    return S(
                        Skin.Current.Format("You may now continue with the {0} phase...", MainPhase.Collection),
                        Skin.Current.Format("Waiting for the host to continue the {0} phase...", MainPhase.Collection));

                /* Mentat */

                case Phase.ReplacingFaceDancer:
                    return S(
                    Skin.Current.Format("You may replace an unrevealed Face Dancer with a new one from the Traitor Deck."),
                    Skin.Current.Format("Waiting for a Face Dancer to be replaced..."),
                    Faction.Purple);

                case Phase.Contemplate:
                    return S(
                    Skin.Current.Format("Check for victories?", Concept.Resource),
                    Skin.Current.Format("Waiting for the host to start determining victories..."));

                case Phase.GameEnded:
                    return S(Skin.Current.Format("The game has ended."));
            }

            return S(Skin.Current.Format("Unknown phase."));
        }

        public IEnumerable<Player> PlayersThatHaventActedOrPassed => Game.Players.Where(p => !Game.HasActedOrPassed.Contains(p.Faction));

        public IEnumerable<Faction> FactionsThatNeedToMakeABattlePlan
        {
            get
            {
                var result = new List<Faction>();
                if (Game.AggressorBattleAction == null) result.Add(Game.CurrentBattle.Aggressor);
                if (Game.DefenderBattleAction == null) result.Add(Game.CurrentBattle.Defender);
                return result;
            }
        }

        public IEnumerable<Faction> FactionsThatNeedToCallTraitor
        {
            get
            {
                var result = new List<Faction>();
                if (Game.AggressorTraitorAction == null) result.Add(Game.CurrentBattle.Aggressor);
                if (Game.DefenderTraitorAction == null) result.Add(Game.CurrentBattle.Defender);
                return result;
            }
        }

        public IEnumerable<Territory> HighlightedTerritories
        {
            get
            {
                switch (Game.CurrentPhase)
                {
                    case Phase.YellowSettingUp: return new Territory[] { Game.Map.SietchTabr.Territory, Game.Map.FalseWallSouth, Game.Map.FalseWallWest };
                    case Phase.BlueIntrudedByCaravan:
                    case Phase.BlueIntrudedByNonOrangeMove:
                    case Phase.BlueIntrudedByNonOrangeShip:
                    case Phase.BlueIntrudedByOrangeMove:
                    case Phase.BlueIntrudedByOrangeShip:
                    case Phase.BlueIntrudedByYellowRidingMonsterA:
                    case Phase.BlueIntrudedByYellowRidingMonsterB:
                        if (Game.LastShippedOrMovedTo != null)
                        {
                            return new Territory[] { Game.LastShippedOrMovedTo.Territory };
                        }
                        break;

                    case Phase.BlueAccompaniesOrange:
                    case Phase.BlueAccompaniesNonOrange:
                        if (Game.LastShippedOrMovedTo != null)
                        {
                            return new Territory[] { Game.LastShippedOrMovedTo.Territory };
                        }
                        break;

                    case Phase.BattlePhase:
                    case Phase.MeltingRock:
                    case Phase.CallTraitorOrPass:
                    case Phase.CaptureDecision:
                    case Phase.BattleConclusion:
                    case Phase.Facedancing:
                        if (Game.CurrentBattle != null)
                        {
                            return new Territory[] { Game.CurrentBattle.Territory };
                        }
                        break;
                }

                return Array.Empty<Territory>();
            }
        }

        public bool HighlightPlayer(Player p)
        {
            return Game.CurrentPhase == Phase.Bidding && Game.CurrentAuctionType != AuctionType.WhiteSilent && p == Game.BidSequence.CurrentPlayer ||
                   Game.CurrentPhase == Phase.BlackMarketBidding && Game.CurrentAuctionType != AuctionType.BlackMarketSilent && p == Game.BidSequence.CurrentPlayer ||
                   (Game.CurrentPhase == Phase.OrangeMove || Game.CurrentPhase == Phase.OrangeShip) && p.Faction == Faction.Orange ||
                   (Game.CurrentPhase == Phase.NonOrangeMove || Game.CurrentPhase == Phase.NonOrangeShip) && p == Game.ShipmentAndMoveSequence.CurrentPlayer ||
                   (Game.CurrentPhase == Phase.BlueAccompaniesOrange || Game.CurrentPhase == Phase.BlueAccompaniesNonOrange || Game.CurrentPhase == Phase.BlueIntrudedByOrangeMove || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeMove || Game.CurrentPhase == Phase.BlueIntrudedByOrangeShip || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeShip) && p.Faction == Faction.Blue ||
                   (Game.CurrentMainPhase == MainPhase.Battle) && p == Game.CurrentBattle?.AggressivePlayer;
        }

        private List<FlashInfo> DetermineFlash(Game g)
        {
            var latestEvent = g.History.LastOrDefault();
            var result = new List<FlashInfo>();

            if (latestEvent != null)
            {
                if (latestEvent is RaiseDeadPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.RaiseDead));
                else if (latestEvent is MetheorPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Metheor));
                else if (latestEvent is StormSpellPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.StormSpell));
                else if (latestEvent is ClairVoyancePlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Clairvoyance));
                else if (latestEvent is AmalPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Amal));
                else if (latestEvent is HarvesterPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Harvester));
                else if (latestEvent is ThumperPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Thumper));
                else if (latestEvent is ResidualPlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Residual));
                else if (latestEvent is FlightUsed) result.Add(CardInfo(latestEvent, TreacheryCardType.Flight));
                else if (latestEvent is DistransUsed) result.Add(CardInfo(latestEvent, TreacheryCardType.Distrans));
                else if (latestEvent is PortableAntidoteUsed) result.Add(CardInfo(latestEvent, TreacheryCardType.PortableAntidote));
                else if (latestEvent is RockWasMelted) result.Add(CardInfo(latestEvent, TreacheryCardType.Rockmelter));
                else if (latestEvent is DiscardedTaken) result.Add(CardInfo(latestEvent, TreacheryCardType.TakeDiscarded));
                else if (latestEvent is DiscardedSearched) result.Add(CardInfo(latestEvent, TreacheryCardType.SearchDiscarded));
                else if (latestEvent is JuicePlayed) result.Add(CardInfo(latestEvent, TreacheryCardType.Juice));
                else if (latestEvent is Retreat || latestEvent is Diplomacy) result.Add(CardInfo(latestEvent, LeaderSkill.Diplomat));
                else if (latestEvent is Bureaucracy) result.Add(CardInfo(latestEvent, LeaderSkill.Bureaucrat));
                else if (latestEvent is Planetology) result.Add(CardInfo(latestEvent, LeaderSkill.Planetologist));
                else if (latestEvent is BattleConcluded battleconcluded && g.TraitorsDeciphererCanLookAt.Count > 0) result.Add(CardInfo(latestEvent, LeaderSkill.Decipherer));
                else if (latestEvent is Thought) result.Add(CardInfo(latestEvent, LeaderSkill.Thinker));
                else if (latestEvent is EstablishPlayers && g.CurrentPhase != Phase.SelectingFactions && IsPlayer) result.Add(CardInfo(Player.Faction));
                else if (latestEvent is FactionTradeOffered fto && (fto.Initiator == Faction || fto.Target == Faction) && !g.CurrentTradeOffers.Any(t => t.Initiator == Faction)) result.Add(CardInfo(Player.Faction));

            }

            int nrOfSpiceBlows = g.RecentMilestones.Count(m => m == Milestone.Resource);
            bool resourceCardAlreadyAdded = false;

            foreach (var m in g.RecentMilestones)
            {
                switch (m)
                {
                    case Milestone.TreacheryCalled:
                    case Milestone.FaceDanced:
                        {
                            result.Add(EventInfo(latestEvent));
                            break;
                        }
                    case Milestone.Karma:
                        {
                            if (g.TreacheryDiscardPile.Top != null)
                            {
                                result.Add(CardInfo(latestEvent, g.TreacheryDiscardPile.Top));
                            }
                            else
                            {
                                result.Add(EventInfo(latestEvent));
                            }
                            break;
                        }
                    case Milestone.BabyMonster:
                        {
                            result.Add(CardInfo(Map.GetResourceCardsInAndOutsidePlay(g.Map).FirstOrDefault(c => c.IsSandTrout), Message.Express(Concept.BabyMonster, " detected!")));
                            break;
                        }
                    case Milestone.Monster:
                        {
                            result.Add(CardInfo(Map.GetResourceCardsInAndOutsidePlay(g.Map).FirstOrDefault(c => c.IsShaiHulud), Message.Express(Concept.Monster, " detected!")));
                            break;
                        }
                    case Milestone.Resource:
                        {
                            ResourceCard cardToShow = null;

                            if (nrOfSpiceBlows == 2)
                            {
                                if (!resourceCardAlreadyAdded)
                                {
                                    cardToShow = g.LatestSpiceCardA;
                                    resourceCardAlreadyAdded = true;
                                }
                                else
                                {
                                    cardToShow = g.LatestSpiceCardB;
                                }
                            }
                            else
                            {
                                if (g.Applicable(Rule.IncreasedResourceFlow) && (g.CurrentPhase == Phase.AllianceB || g.CurrentPhase == Phase.HarvesterB || g.CurrentPhase == Phase.BlowReport))
                                {
                                    cardToShow = g.LatestSpiceCardB;
                                }
                                else
                                {
                                    cardToShow = g.LatestSpiceCardA;
                                }
                            }

                            if (cardToShow != null)
                            {
                                result.Add(CardInfo(cardToShow, Message.Express(Concept.Resource, " in ", cardToShow.ToString())));
                            }

                            break;
                        }

                }
            }

            return result;
        }

        private static GameStatus S(string description, GameEvent timedEvent = null) => new(description, timedEvent);

        private static GameStatus S(string descriptionWhenAwaited, string descriptionWhenWaiting, GameEvent timedEvent = null) => new(descriptionWhenAwaited, descriptionWhenWaiting, timedEvent);

        private static GameStatus S(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null) =>
            new(descriptionWhenAwaited, descriptionWhenWaiting, waitingForPlayers, timedEvent);

        private GameStatus S(string descriptionWhenAwaited, string descriptionWhenWaiting, IEnumerable<Faction> waitingForFactions, GameEvent timedEvent = null) =>
            new(descriptionWhenAwaited, descriptionWhenWaiting, waitingForFactions.Select(f => Game.GetPlayer(f)), timedEvent);

        private static GameStatus S(string descriptionWhenAwaited, string descriptionWhenWaiting, Player waitingForPlayer, GameEvent timedEvent = null) =>
            new(descriptionWhenAwaited, descriptionWhenWaiting, waitingForPlayer, timedEvent);

        private GameStatus S(string descriptionWhenAwaited, string descriptionWhenWaiting, Faction waitingForFaction, GameEvent timedEvent = null) =>
            new(descriptionWhenAwaited, descriptionWhenWaiting, Game.GetPlayer(waitingForFaction), timedEvent);


        private FlashInfo EventInfo(GameEvent e)
        {
            FlashInfo result;
            if (e is TreacheryCalled t)
            {
                var victim = Game.CurrentBattle.OpponentOf(t.Initiator);
                var victimPlan = Game.CurrentBattle.PlanOf(victim);
                result.Message = Message.Express(victimPlan.Hero, " is a ", t.Initiator, " traitor!");
                result.Url = Skin.Current.GetImageURL(victimPlan.Hero);
            }
            else if (e is FaceDanced)
            {
                var dancer = Game.WinnerHero;
                result.Message = Message.Express(dancer, " is a ", e.Initiator, " facedancer!");
                result.Url = Skin.Current.GetImageURL(dancer);
            }
            else
            {
                result.Message = e?.GetMessage();
                result.Url = Skin.Current.Planet_URL;
            }

            return result;
        }

        private static FlashInfo CardInfo(GameEvent e, TreacheryCardType t)
        {
            FlashInfo result;
            result.Url = Skin.Current.GetImageURL(TreacheryCardManager.GetCardsInAndOutsidePlay().First(card => card.Type == t));
            result.Message = e?.GetMessage();
            return result;
        }

        private static FlashInfo CardInfo(GameEvent e, LeaderSkill s)
        {
            FlashInfo result;
            result.Url = Skin.Current.GetImageURL(s);
            result.Message = e?.GetMessage();
            return result;
        }

        private static FlashInfo CardInfo(Faction f)
        {
            FlashInfo result;
            result.Url = Skin.Current.GetImageURL(f);
            result.Message = Message.Express("You play ", f);
            return result;
        }

        private static FlashInfo CardInfo(GameEvent e, TreacheryCard c)
        {
            FlashInfo result;
            result.Url = Skin.Current.GetImageURL(c);

            if ((c.Type == TreacheryCardType.Karma || c.Type == TreacheryCardType.Useless) && e is Bid && (e as Bid).Passed)
            {
                result.Message = Message.Express("Card was won using ", TreacheryCardType.Karma);
            }
            else
            {
                result.Message = e?.GetMessage();
            }

            return result;
        }

        private static FlashInfo CardInfo(ResourceCard c, Message m)
        {
            FlashInfo result;
            result.Url = Skin.Current.GetImageURL(c);
            result.Message = m;
            return result;
        }
    }

    public struct FlashInfo
    {
        public string Url;
        public Message Message;
    }
}
