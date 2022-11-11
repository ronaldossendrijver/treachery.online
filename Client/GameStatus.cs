/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public class GameStatus
    {
        public Message DescriptionWhenAwaited { get; private set; }

        public Message DescriptionWhenWaiting { get; private set; }

        public IEnumerable<Territory> HighlightedTerritories { get; private set; }

        public IEnumerable<SequenceElement> WaitingInSequence { get; private set; } = Array.Empty<SequenceElement>();

        public IEnumerable<Player> WaitingForPlayers { get; private set; } = Array.Empty<Player>();

        public List<FlashInfo> FlashInfo { get; set; } = new List<FlashInfo>();

        public GameEvent TimedEvent { get; private set; } = null;

        public bool WaitingForHost { get; private set; } = false;

        private GameStatus(Message messageWhenAwaited, Message messageWhenWaiting, Player waitingForPlayer, GameEvent timedEvent = null) :
            this(messageWhenAwaited, messageWhenWaiting, new Player[] { waitingForPlayer }, timedEvent)
        {
        }

        private GameStatus(Message messageWhenAwaited, Message messageWhenWaiting, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null)
        {
            DescriptionWhenAwaited = messageWhenAwaited;
            DescriptionWhenWaiting = messageWhenWaiting;
            WaitingForPlayers = waitingForPlayers;
            TimedEvent = timedEvent;
        }

        private GameStatus(Message messageWhenAwaited, Message messageWhenWaiting, GameEvent timedEvent = null)
        {
            DescriptionWhenAwaited = messageWhenAwaited;
            DescriptionWhenWaiting = messageWhenWaiting;
            WaitingForHost = true;
            TimedEvent = timedEvent;
        }

        private GameStatus(Message message, GameEvent timedEvent = null) : this(message, message, timedEvent)
        {
        }

        public bool WaitingForMe(Player player, bool isHost) => WaitingForHost && isHost ||
                WaitingForPlayers.Contains(player) ||
                WaitingInSequence.Any(se => se.Player == player && se.HasTurn);

        public bool WaitingForOthers(Player player, bool isHost) => !WaitingForMe(player, isHost);

        public bool PlayerShouldBeHighlighted(Player p) => WaitingForPlayers.Contains(p);

        public Message GetMessage(Player player, bool isHost)
        {
            if (WaitingForMe(player, isHost))
            {
                return DescriptionWhenAwaited;
            }
            else
            {
                return DescriptionWhenWaiting;
            }
        }

        public static GameStatus DetermineStatus(Game game, Player me, bool isPlayer)
        {
            GameStatus result = (game.CurrentPhase) switch
            {
                /* Phase Beginnings */

                Phase.Thumper or
                Phase.BeginningOfCharity or
                Phase.BeginningOfResurrection or
                Phase.BeginningOfCollection => Status(
                    Express("You may now continue with the ", game.CurrentMainPhase, " phase..."),
                    Express("Waiting for the host to continue the ", game.CurrentMainPhase, " phase...")),

                /* Phase Endings */

                Phase.StormReport or
                Phase.BlowReport or
                Phase.CharityReport or
                Phase.BiddingReport or
                Phase.ResurrectionReport or
                Phase.ShipmentAndMoveConcluded or
                Phase.CollectionReport or
                Phase.TurnConcluded => Status(Express(game.CurrentMainPhase, " phase ended.")),

                /* Non-mainphase-related phases */

                Phase.Clairvoyance => Status(game,
                    Express("Please answer a question from ", game.LatestClairvoyance.Initiator, " by ", TreacheryCardType.Clairvoyance, "."),
                    Express("Waiting for an answer to a ", TreacheryCardType.Clairvoyance, " question..."),
                    game.LatestClairvoyance.Target),

                Phase.SearchingDiscarded => Status(
                    "Please take a card from the treachery discard pile.",
                    "Waiting for a card to be taken from the treachery discard pile...",
                    game.OwnerOf(TreacheryCardType.SearchDiscarded)),

                Phase.TradingCards => Status(game,
                    "Please select which card to give to your ally.",
                    "Waiting for a card to be returned...",
                    game.CurrentCardTradeOffer.Player.Ally),

                Phase.Bureaucracy => Status(
                    "Please decide whether to apply Bureaucracy to the latest payment.",
                    "Waiting for Bureaucracy to be applied to the latest payment...",
                    game.PlayerSkilledAs(LeaderSkill.Bureaucrat)),

                /* Setup */

                Phase.AwaitingPlayers => Status(
                    "Configure the game and wait for players to join. Start the game when ready.",
                    "Waiting for the host to configure and start the game..."),

                Phase.SelectingFactions => Status("Players may now choose a faction..."),

                Phase.TradingFactions => Status("Players may now trade factions..."),

                Phase.AssigningInitialSkills or Phase.AssigningSkill => Status(
                    "You may now assign a skill to a leader.",
                    "Waiting for factions to assign leader skills...",
                    game.Players.Where(p => p.SkillsToChooseFrom.Any())),

                Phase.BluePredicting => Status(game,
                    Express("Please predict who will win the game and when."),
                    Express(Faction.Blue, " are predicting who will win the game and when..."), Faction.Blue),

                Phase.BlackMulligan => Status(game,
                    Express("You may draw a new set of traitors if you were dealt two or more of your own leaders."),
                    Express(Faction.Black, " may draw a new set of traitors if they were dealt two or more of their own leaders..."),
                    Faction.Black),

                Phase.SelectingTraitors => Status(
                    "Please select one leader to keep as a traitor.",
                    "Factions are selecting traitors...",
                    PlayersThatHaventActedOrPassed(game)),

                Phase.CustomizingDecks => Status(
                    "Please select which cards will be in play.",
                    "The host is selecting which cards will be in play..."),

                Phase.PerformCustomSetup => Status(
                    Express("Please set up ", game.NextFactionToPerformCustomSetup, " starting ", Concept.Resource, " and force positions."),
                    Express("The host is setting up ", game.NextFactionToPerformCustomSetup, " starting ", Concept.Resource, " and force positions...")),

                Phase.YellowSettingUp => Status(game,
                    Express("Please choose your initial force positions."),
                    Express(Faction.Yellow, " are setting up their starting force positions..."),
                    Faction.Yellow),

                Phase.BlueSettingUp => Status(game,
                    Express("Please select your starting force position."),
                    Express(Faction.Blue, " are selecting their starting force position..."),
                    Faction.Blue),

                Phase.GreySelectingCard => Status(game,
                    Express("Please select your starting Treachery Card."),
                    Express(Faction.Grey, " are selecting their starting Treachery Card..."),
                    Faction.Grey),

                Phase.HmsPlacement => Status(game,
                    Express("Please position the Hidden Mobile Stronghold."),
                    Express(Faction.Grey, " are positioning the Hidden Mobile Stronghold..."),
                    Faction.Grey),

                /* Storm */

                Phase.HmsMovement => Status(game,
                    Express("You may move the Hidden Mobile Stronghold to a sector in an adjacent territory, or pass."),
                    Express(Faction.Grey, " are moving the Hidden Mobile Stronghold..."),
                    Faction.Grey),

                Phase.DiallingStorm => Status(game,
                    "Please dial a number to determine storm movement.",
                    "Storm movement is being determined...",
                    game.FactionsInPlay.Where(f => game.HasBattleWheel.Contains(f) && !game.HasActedOrPassed.Contains(f))),

                Phase.MetheorAndStormSpell => Status(Express("Factions may now use ", TreacheryCardType.Metheor, " or ", TreacheryCardType.StormSpell, "...")),

                Phase.StormLosses => Status(game,
                    Express("Please decide which forces were killed by the storm in ", TakeLosses.LossesToTake(game).Location, "."),
                    Express(TakeLosses.LossesToTake(game).Faction, " are deciding which forces were killed by the storm in ", TakeLosses.LossesToTake(game).Location, "..."),
                    game.StormLossesToTake[0].Faction),

                /* Spice Blow */

                Phase.YellowSendingMonsterA or Phase.YellowSendingMonsterB => Status(game,
                    Express("Please decide where to let ", Concept.Monster, " appear."),
                    Express(Faction.Yellow, " are deciding where to let ", Concept.Monster, " appear..."),
                    Faction.Yellow),

                Phase.YellowRidingMonsterA or Phase.YellowRidingMonsterB => Status(game,
                    Express("Please decide where to travel with ", Concept.Monster, "."),
                    Express(Faction.Yellow, " are deciding where to travel with ", Concept.Monster, "..."),
                    Faction.Yellow),

                Phase.BlueIntrudedByYellowRidingMonsterA or Phase.BlueIntrudedByYellowRidingMonsterB => Status(game,
                    Express("Please decide what to do in response to an intrusion of ", game.LastShipmentOrMovement.To.Territory, "; be fighters or advisors?"),
                    Express(Faction.Blue, " are deciding what to do in response to an intrusion of ", game.LastShipmentOrMovement.To.Territory, "..."),
                    Faction.Blue),

                Phase.BlowA or Phase.HarvesterA => Status(Express("Factions may use a ", TreacheryCardType.Harvester, " to double the ", Concept.Resource, " blow in ", game.LatestSpiceCardA.Location.Territory, "...")),
                Phase.BlowB or Phase.HarvesterB => Status(Express("Factions may use a ", TreacheryCardType.Harvester, " to double the ", Concept.Resource, " blow in ", game.LatestSpiceCardB.Location.Territory, "...")),

                Phase.AllianceA or Phase.AllianceB => Status("Factions may now make and break alliances."),

                Phase.NexusCards => Status(
                    "Do you wish to draw a Nexus card in case you have none or have your own faction?",
                    "Factions are thinking about drawing Nexus cards...",
                    game.Players.Where(p => NexusCardDrawn.Applicable(game, p))),

                /* Charity */

                Phase.ClaimingCharity => Status(Skin.Current.Format("Factions may now claim charity if eligible.")),

                /* Bidding */

                Phase.BlackMarketAnnouncement => Status(game,
                    Express("You may now select a card to sell on the Black Markt, or pass."),
                    Express(Faction.White, " are thinking about selling one of their cards on the Black Market..."),
                    Faction.White),

                Phase.BlackMarketBidding when game.CurrentAuctionType != AuctionType.BlackMarketSilent => Status(
                    Express("Please bid or pass."),
                    Express(game.BidSequence.CurrentFaction, " are thinking about their bid..."),
                    game.BidSequence.CurrentPlayer, game.LatestEvent()),

                Phase.BlackMarketBidding when game.CurrentAuctionType == AuctionType.BlackMarketSilent => Status(
                    "Please bid.",
                    "Factions are thinking about their bids...",
                    game.Players.Where(p => p.HasRoomForCards && !game.Bids.ContainsKey(p.Faction)), game.LatestEvent(typeof(WhiteAnnouncesBlackMarket))),

                Phase.WhiteAnnouncingAuction => Status(game,
                    Express("Please decide if you will auction a card from your cache First or Last."),
                    Express(Faction.White, " are deciding to auction a card from their cache First or Last..."),
                    Faction.White),

                Phase.WhiteSpecifyingAuction => Status(game,
                    Express("Please select a card from your cache to sell and select the type of auction."),
                    Express(Faction.White, " are putting a card from their cache on auction..."),
                    Faction.White),

                Phase.WhiteKeepingUnsoldCard => Status(game,
                    Express("Please decide if you wish to keep this unsold card."),
                    Express(Faction.White, " are deciding about keeping the unsold card..."),
                    Faction.White),

                Phase.GreyRemovingCardFromBid => Status(game,
                    Express("Please put one card from the auction on top or at the bottom of the Treachery Card deck."),
                    Express(Faction.Grey, " are putting one card from the auction on top or at the bottom of the Treachery Card deck..."),
                    Faction.Grey),

                Phase.GreySwappingCard => Status(game,
                    Express("You may swap the next card on auction with a card from your hand, or pass."),
                    Express(Faction.Grey, " are thinking about swapping the next card on auction with a card from their hand..."),
                    Faction.Grey),

                Phase.Bidding when game.CurrentAuctionType != AuctionType.WhiteSilent => Status(
                    Express("Please bid or pass."),
                    Express(game.BidSequence.CurrentFaction, " are thinking about their bid..."),
                    game.BidSequence.CurrentPlayer, game.LatestEvent()),

                Phase.Bidding when game.CurrentAuctionType == AuctionType.WhiteSilent => Status(
                    "Please bid.",
                    "Factions are thinking about their bids...",
                    game.Players.Where(p => p.HasRoomForCards && !game.Bids.ContainsKey(p.Faction)), game.LatestEvent(typeof(WhiteSpecifiesAuction))),

                Phase.ReplacingCardJustWon => Status(
                    Express("You may now discard the card you just won and draw a new card instead."),
                    Express(game.GetAlly(Faction.Grey), " are thinking about replacing the card they just won with a card from the deck..."),
                    game.GetAlliedPlayer(Faction.Grey)),

                Phase.WaitingForNextBiddingRound => Status("Factions are waiting for the next card to be put on auction..."),

                Phase.PerformingKarmaHandSwap => Status(game,
                    Express("Please decide which ", game.KarmaHandSwapNumberOfCards, " cards to return to ", game.KarmaHandSwapTarget, "."),
                    Express(Faction.Black, " are deciding which ", game.KarmaHandSwapNumberOfCards, " cards to return to ", game.KarmaHandSwapTarget, "..."),
                    Faction.Black),

                /* Revival */

                Phase.Resurrection => Status("Factions may now reclaim forces and leaders."),

                /* Ship & Move */

                Phase.BeginningOfShipAndMove => Status(
                        Express("You may now start the Ship & Move sequence..."),
                        Express("Waiting for the host to start the Ship & Move sequence...")),

                Phase.NonOrangeShip when game.ShipmentAndMoveSequence.CurrentFaction == Faction.Yellow => Status(game,
                    Express("Please decide to rally forces or pass."),
                    Express(Faction.Yellow, " are thinking about rallying forces..."),
                    Faction.Yellow, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                Phase.NonOrangeShip when game.ShipmentAndMoveSequence.CurrentFaction != Faction.Yellow => Status(
                    Express("Please decide to ship forces or pass."),
                    Express(game.ShipmentAndMoveSequence.CurrentFaction, " are thinking about shipping forces..."),
                    game.ShipmentAndMoveSequence.CurrentPlayer, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                Phase.OrangeShip when game.OrangeMayDelay => Status(game,
                    Express("Please decide to ship now or delay your turn and let other factions go first."),
                    Express(Faction.Orange, " are deciding about taking their turn now..."),
                    Faction.Orange, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                Phase.OrangeShip when !game.OrangeMayDelay => Status(game,
                    Express("Please decide to ship forces or pass."),
                    Express(Faction.Orange, " are thinking about shipping forces..."),
                    Faction.Orange, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                Phase.BlueAccompaniesNonOrange or Phase.BlueAccompaniesOrange => Status(game,
                    Express("Do you wish to accompany the latest shipment?"),
                    Express(Faction.Blue, " are thinking about accompanying the latest shipment..."),
                    Faction.Blue),

                Phase.BlueIntrudedByNonOrangeShip or
                Phase.BlueIntrudedByOrangeShip or
                Phase.BlueIntrudedByNonOrangeMove or
                Phase.BlueIntrudedByOrangeMove or
                Phase.BlueIntrudedByCaravan => Status(game,
                    Express("Please decide how to respond to the intrusion in ", game.LastShipmentOrMovement.To.Territory, "; be fighters or advisors?"),
                    Express(Faction.Blue, " are deciding how to respond to the intrusion in ", game.LastShipmentOrMovement.To.Territory, "..."),
                    Faction.Blue),

                Phase.NonOrangeMove => Status(
                    Express("Please decide to move forces or pass."),
                    Express(game.ShipmentAndMoveSequence.CurrentFaction, " are thinking about about moving forces."),
                    game.ShipmentAndMoveSequence.CurrentPlayer, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                Phase.OrangeMove => Status(game,
                    Express("Please decide to move forces or pass."),
                    Express(Faction.Orange, " are thinking about about moving forces."),
                    Faction.Orange, game.LatestEvent(typeof(EndPhase), typeof(OrangeDelay), typeof(Move))),

                /* Battle */

                Phase.BeginningOfBattle => Status(
                    "You may proceed to the first battle when ready.",
                    "Waiting for the host to proceed to the first battle..."),

                Phase.Thought => Status(
                    Express(game.CurrentThought.Initiator, " asked you a question and are waiting for your answer."),
                    Express("Waiting for ", game.CurrentBattle.OpponentOf(game.CurrentThought.Initiator).Faction, " to answer a question..."),
                    game.CurrentBattle.OpponentOf(game.CurrentThought.Initiator)),

                Phase.BattlePhase => DetermineBattleStatus(game, me),

                Phase.MeltingRock => Status(
                    Express("Please decide how to use your ", TreacheryCardType.Rockmelter, "."),
                    Express("Waiting for a decision on how they wish to use their ", TreacheryCardType.Rockmelter, "..."),
                    game.CurrentBattle.AggressorAction.HasRockMelter ? game.CurrentBattle.Player : game.CurrentBattle.DefendingPlayer),

                Phase.CallTraitorOrPass => Status(
                    "You may now call TREACHERY if the enemy leader is a traitor under your command.",
                    "Waiting for a faction to call TREACHERY...",
                    PlayersThatNeedToCallTraitor(game)),

                Phase.Retreating => Status(game,
                    Express("Please decide about retreating forces from ", game.CurrentBattle.Territory, "."),
                    Express(game.BattleLoser, " are thinking about retreating forces from ", game.CurrentBattle.Territory, "."),
                    game.BattleLoser),

                Phase.AvoidingAudit => Status(
                    Express("Please decide if you wish to avoid being audited."),
                    Express(game.Auditee.Faction, " are thinking about avoiding a scheduled audit..."),
                    game.Auditee),

                Phase.CaptureDecision => Status(game,
                    Express("Please decide if you wish to capture a leader."),
                    Express(Faction.Black, " are deciding about capturing a leader..."),
                    Faction.Black),

                Phase.Auditing => Status(game,
                    Express("Please conclude the audit when done inspecting your opponents cards."),
                    Express(Faction.Brown, " are performing their audit..."),
                    Faction.Brown),

                Phase.BattleConclusion => Status(game,
                    Express("You won! Conclude the battle when done celebrating your victory."),
                    Express(game.BattleWinner, " are celebrating their victory in battle..."),
                    game.BattleWinner),

                Phase.Facedancing => Status(game,
                    Express("You may reveal a leader to be one of your face dancers."),
                    Express("Waiting for ", Faction.Purple, " to reveal a face dancer..."),
                    Faction.Purple),

                Phase.BattleReport when game.NextPlayerToBattle != null => Status(Express("Factions may now review the battle report before the next battle begins...")),
                Phase.BattleReport when game.NextPlayerToBattle == null => Status(Express(game.CurrentMainPhase, " phase ended.")),

                /* Mentat */

                Phase.ReplacingFaceDancer => Status(game,
                    "You may replace an unrevealed Face Dancer with a new one from the Traitor Deck.",
                    "Waiting for a Face Dancer to be replaced...",
                    Faction.Purple),

                Phase.Contemplate => Status("Check for victories?", "Waiting for the host to start determining victories..."),

                Phase.GameEnded => Status("The game has ended."),

                Phase.None or _ => Status(Express("Unknown phase: " + game.CurrentPhase))
            };

            result.FlashInfo = DetermineFlash(game, me != null ? me.Faction : Faction.None, isPlayer);
            result.HighlightedTerritories = DetermineHighlights(game);

            return result;
        }

        private static IEnumerable<Territory> DetermineHighlights(Game game)
        {
            return (game.CurrentPhase) switch
            {
                Phase.YellowSettingUp => new Territory[] { game.Map.SietchTabr.Territory, game.Map.FalseWallSouth, game.Map.FalseWallWest },

                Phase.StormLosses => new Territory[] { TakeLosses.LossesToTake(game).Location.Territory },

                Phase.HarvesterA => new Territory[] { game.LatestSpiceCardA.Location.Territory },
                Phase.HarvesterB => new Territory[] { game.LatestSpiceCardB.Location.Territory },

                Phase.BlueIntrudedByCaravan or
                Phase.BlueIntrudedByNonOrangeMove or
                Phase.BlueIntrudedByNonOrangeShip or
                Phase.BlueIntrudedByOrangeMove or
                Phase.BlueIntrudedByOrangeShip or
                Phase.BlueIntrudedByYellowRidingMonsterA or
                Phase.BlueIntrudedByYellowRidingMonsterB or
                Phase.BlueAccompaniesOrange or
                Phase.BlueAccompaniesNonOrange when game.LastShipmentOrMovement != null => new Territory[] { game.LastShipmentOrMovement.To.Territory },

                Phase.BattlePhase or
                Phase.MeltingRock or
                Phase.CallTraitorOrPass or
                Phase.CaptureDecision or
                Phase.BattleConclusion or
                Phase.AvoidingAudit or
                Phase.Auditing or
                Phase.Facedancing when game.CurrentBattle != null => new Territory[] { game.CurrentBattle.Territory },

                _ => Array.Empty<Territory>()
            };
        }

        private static GameStatus DetermineBattleStatus(Game game, Player me)
        {
            if (game.CurrentBattle == null)
            {
                return Status(
                    Express("Please choose whom and where to battle."),
                    Express(game.NextPlayerToBattle.Faction, " are deciding whom and where to battle..."),
                    game.NextPlayerToBattle);
            }
            else
            {
                var latestBattleEvent = game.LatestEvent(typeof(BattleInitiated));
                var toMakePlan = PlayersThatNeedToMakeABattlePlan(game);

                if (game.CurrentBattle.Aggressor == me.Faction)
                {
                    return Status(
                        Express("You are aggressor against ", game.CurrentBattle.Defender, " in ", game.CurrentBattle.Territory, "! Please confirm your Battle Plan."),
                        Express("You are waiting for ", game.CurrentBattle.Defender, " to defend ", game.CurrentBattle.Territory, "..."), toMakePlan, latestBattleEvent);
                }
                else if (game.CurrentBattle.Defender == me.Faction)
                {
                    return Status(
                        Express("You must defend against ", game.CurrentBattle.Aggressor, " in ", game.CurrentBattle.Territory, "! Please confirm your Battle Plan."),
                        Express("You are waiting for ", game.CurrentBattle.Aggressor, " to attack ", game.CurrentBattle.Territory, "..."), toMakePlan, latestBattleEvent);
                }
                else
                {
                    return Status(
                        Express(game.CurrentBattle.Defender, " are defending against ", game.CurrentBattle.Aggressor, " aggression in ", game.CurrentBattle.Territory, "..."), toMakePlan, latestBattleEvent);
                }
            }
        }

        public static IEnumerable<Player> PlayersThatHaventActedOrPassed(Game Game) => Game.Players.Where(p => !Game.HasActedOrPassed.Contains(p.Faction));

        public static IEnumerable<Player> PlayersThatNeedToMakeABattlePlan(Game Game)
        {
            var result = new List<Player>();
            if (Game.AggressorBattleAction == null) result.Add(Game.CurrentBattle.AggressivePlayer);
            if (Game.DefenderBattleAction == null) result.Add(Game.CurrentBattle.DefendingPlayer);
            return result;
        }

        public static IEnumerable<Player> PlayersThatNeedToCallTraitor(Game Game)
        {
            var result = new List<Player>();
            if (Game.AggressorTraitorAction == null) result.Add(Game.CurrentBattle.AggressivePlayer);
            if (Game.DefenderTraitorAction == null) result.Add(Game.CurrentBattle.DefendingPlayer);
            return result;
        }

        private static GameStatus Status(string message, GameEvent timedEvent = null) => new(Message.Express(message), timedEvent);

        private static GameStatus Status(Message message, GameEvent timedEvent = null) => new(message, timedEvent);

        private static GameStatus Status(Message message, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null) => new(message, message, waitingForPlayers, timedEvent);

        private static GameStatus Status(string messageAwaited, string messageWhenWaiting, GameEvent timedEvent = null) => new(Message.Express(messageAwaited), Message.Express(messageWhenWaiting), timedEvent);

        private static GameStatus Status(Message messageAwaited, Message messageWhenWaiting, GameEvent timedEvent = null) => new(messageAwaited, messageWhenWaiting, timedEvent);

        private static GameStatus Status(string messageWhenAwaited, string messageWhenWaiting, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null) =>
            new(Message.Express(messageWhenAwaited), Message.Express(messageWhenWaiting), waitingForPlayers, timedEvent);

        private static GameStatus Status(Message messageWhenAwaited, Message messageWhenWaiting, IEnumerable<Player> waitingForPlayers, GameEvent timedEvent = null) =>
            new(messageWhenAwaited, messageWhenWaiting, waitingForPlayers, timedEvent);

        private static GameStatus Status(Game game, string messageWhenAwaited, string messageWhenWaiting, IEnumerable<Faction> waitingForFactions, GameEvent timedEvent = null) =>
            new(Message.Express(messageWhenAwaited), Message.Express(messageWhenWaiting), waitingForFactions.Select(f => game.GetPlayer(f)), timedEvent);

        private static GameStatus Status(string messageWhenAwaited, string messageWhenWaiting, Player waitingForPlayer, GameEvent timedEvent = null) =>
            new(Message.Express(messageWhenAwaited), Message.Express(messageWhenWaiting), waitingForPlayer, timedEvent);

        private static GameStatus Status(Message messageWhenAwaited, Message messageWhenWaiting, Player waitingForPlayer, GameEvent timedEvent = null) =>
            new(messageWhenAwaited, messageWhenWaiting, waitingForPlayer, timedEvent);

        private static GameStatus Status(Game game, string messageWhenAwaited, string messageWhenWaiting, Faction waitingForFaction, GameEvent timedEvent = null) =>
            new(Message.Express(messageWhenAwaited), Message.Express(messageWhenWaiting), game.GetPlayer(waitingForFaction), timedEvent);

        private static GameStatus Status(Game game, Message messageWhenAwaited, Message messageWhenWaiting, Faction waitingForFaction, GameEvent timedEvent = null) =>
            new(messageWhenAwaited, messageWhenWaiting, game.GetPlayer(waitingForFaction), timedEvent);

        private static List<FlashInfo> DetermineFlash(Game g, Faction myFaction, bool isPlayer)
        {
            var result = new List<FlashInfo>();

            if (g.CurrentPhase == Phase.GameEnded)
            {
                foreach (var p in g.Winners)
                {
                    Flash(result, Message.Express(p.Faction, " win!"), Skin.Current.GetImageURL(p.Faction));
                }

                return result;
            }
            else if (g.CurrentPhase == Phase.BattleConclusion && g.BattleWinner != Faction.None)
            {
                Flash(result, Message.Express(g.BattleWinner, " win!"), Skin.Current.GetImageURL(g.BattleWinner));
                return result;
            }

            var latestEvent = g.History.LastOrDefault();

            if (latestEvent != null)
            {
                switch (latestEvent)
                {
                    //Show treachery card played
                    case RaiseDeadPlayed: Flash(result, latestEvent, TreacheryCardType.RaiseDead); break;
                    case MetheorPlayed: Flash(result, latestEvent, TreacheryCardType.Metheor); break;
                    case StormSpellPlayed: Flash(result, latestEvent, TreacheryCardType.StormSpell); break;
                    case ClairVoyancePlayed: Flash(result, latestEvent, TreacheryCardType.Clairvoyance); break;
                    case AmalPlayed: Flash(result, latestEvent, TreacheryCardType.Amal); break;
                    case HarvesterPlayed: Flash(result, latestEvent, TreacheryCardType.Harvester); break;
                    case ThumperPlayed: Flash(result, latestEvent, TreacheryCardType.Thumper); break;
                    case ResidualPlayed: Flash(result, latestEvent, TreacheryCardType.Residual); break;
                    case FlightUsed: Flash(result, latestEvent, TreacheryCardType.Flight); break;
                    case DistransUsed: Flash(result, latestEvent, TreacheryCardType.Distrans); break;
                    case Karma or KarmaFreeRevival or KarmaHandSwapInitiated or KarmaHmsMovement or KarmaMonster or KarmaPrescience or KarmaRevivalPrevention or KarmaShipmentPrevention or KarmaWhiteBuy or KarmaBrownDiscard: Flash(result, latestEvent, TreacheryCardType.Karma); break;
                    case PortableAntidoteUsed: Flash(result, latestEvent, TreacheryCardType.PortableAntidote); break;
                    case RockWasMelted: Flash(result, latestEvent, TreacheryCardType.Rockmelter); break;
                    case DiscardedTaken: Flash(result, latestEvent, TreacheryCardType.TakeDiscarded); break;
                    case DiscardedSearched: Flash(result, latestEvent, TreacheryCardType.SearchDiscarded); break;
                    case JuicePlayed: Flash(result, latestEvent, TreacheryCardType.Juice); break;

                    //Show Leader skill used
                    case Retreat or Diplomacy: Flash(result, latestEvent, LeaderSkill.Diplomat); break;
                    case Bureaucracy bc when !bc.Passed: Flash(result, latestEvent, LeaderSkill.Bureaucrat); break;
                    case Planetology: Flash(result, latestEvent, LeaderSkill.Planetologist); break;
                    case BattleConcluded when g.TraitorsDeciphererCanLookAt.Count > 0: Flash(result, latestEvent, LeaderSkill.Decipherer); break;
                    case Thought: Flash(result, latestEvent, LeaderSkill.Thinker); break;

                    //Show Event description
                    case WhiteSpecifiesAuction or BlueBattleAnnouncement or Voice or Prescience or GreyRemovedCardFromAuction or BrownDiscarded or CardTraded or BrownEconomics or SwitchedSkilledLeader: Flash(result, latestEvent); break;
                    case WhiteAnnouncesBlackMarket wbm when !wbm.Passed: Flash(result, latestEvent); break;
                    case GreySwappedCardOnBid gsc when !gsc.Passed: Flash(result, latestEvent); break;
                    case ReplacedCardWon rcw when !rcw.Passed: Flash(result, latestEvent); break;
                    case FaceDancerReplaced fdr when !fdr.Passed: Flash(result, latestEvent); break;

                    //Show Faction
                    case EstablishPlayers when g.CurrentPhase != Phase.SelectingFactions && isPlayer: Flash(result, myFaction); break;
                    case FactionTradeOffered fto when (fto.Initiator == myFaction || fto.Target == myFaction) && !g.CurrentTradeOffers.Any(t => t.Initiator == myFaction): Flash(result, myFaction); break;
                }
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
                            Flash(result, latestEvent);
                            break;
                        }
                    case Milestone.BabyMonster:
                        {
                            Flash(result, Message.Express(Concept.BabyMonster, " detected!"), Skin.Current.GetImageURL(Map.GetResourceCardsInAndOutsidePlay(g.Map).FirstOrDefault(c => c.IsSandTrout)));
                            break;
                        }
                    case Milestone.Karma when latestEvent is Bid:
                        {
                            if (g.TreacheryDiscardPile.Top != null)
                            {
                                Flash(result, Message.Express("Card was won using ", TreacheryCardType.Karma), Skin.Current.GetImageURL(g.TreacheryDiscardPile.Top));
                            }
                            break;
                        }
                    case Milestone.Monster:
                        {
                            Flash(result, Message.Express(Concept.Monster, " detected!"), Skin.Current.GetImageURL(Map.GetResourceCardsInAndOutsidePlay(g.Map).FirstOrDefault(c => c.IsShaiHulud)));
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
                                Flash(result, Message.Express(Concept.Resource, " in ", Skin.Current.Describe(cardToShow)), Skin.Current.GetImageURL(cardToShow));
                            }

                            break;
                        }

                }
            }

            return result;
        }

        private static void Flash(IList<FlashInfo> flashes, Message message, string imageURL)
        {
            FlashInfo result;
            result.Message = message;
            result.Url = imageURL;
            flashes.Add(result);
        }

        private static void Flash(IList<FlashInfo> flashes, GameEvent e)
        {
            switch (e)
            {
                case TreacheryCalled t:
                    var victim = e.Game.CurrentBattle.OpponentOf(t.Initiator);
                    var victimPlan = e.Game.CurrentBattle.PlanOf(victim);
                    Flash(flashes, Message.Express(victimPlan.Hero, " is a ", t.Initiator, " traitor!"), Skin.Current.GetImageURL(victimPlan.Hero));
                    break;

                case FaceDanced:
                    var dancer = e.Game.WinnerHero;
                    Flash(flashes, Message.Express(dancer, " is a ", e.Initiator, " facedancer!"), Skin.Current.GetImageURL(dancer));
                    break;

                case SwitchedSkilledLeader:
                    var leader = SwitchedSkilledLeader.SwitchableLeader(e.Game, e.Player);
                    Flash(
                        flashes,
                        Message.Express(e.Initiator, " place ", e.Game.Skill(leader), " ", leader, e.Game.IsInFrontOfShield(leader) ? " in front of" : " behind", " their shield"),
                        Skin.Current.GetImageURL(leader));
                    break;

                default: Flash(flashes, e?.GetMessage(), Skin.Current.GetImageURL(e.Initiator)); break;
            }
        }

        private static void Flash(IList<FlashInfo> flashes, GameEvent e, TreacheryCardType t)
        {
            if (e.Game.TreacheryDiscardPile.Top?.Type == t || t == TreacheryCardType.Karma && e.Game.TreacheryDiscardPile.Top?.Type == TreacheryCardType.Useless)
            {
                Flash(flashes, e?.GetMessage(), Skin.Current.GetImageURL(e.Game.TreacheryDiscardPile.Top));
            }
            else
            {
                Flash(flashes, e?.GetMessage(), Skin.Current.GetImageURL(TreacheryCardManager.GetCardsInAndOutsidePlay().First(card => card.Type == t)));
            }
        }

        private static void Flash(IList<FlashInfo> flashes, GameEvent e, LeaderSkill s)
        {
            if (s == LeaderSkill.Decipherer)
            {
                Flash(flashes, Message.Express(e.Initiator, " use their ", s, " skill"), Skin.Current.GetImageURL(s));
            }
            else
            {
                Flash(flashes, e?.GetMessage(), Skin.Current.GetImageURL(s));
            }
        }

        private static void Flash(IList<FlashInfo> flashes, Faction f)
        {
            Flash(flashes, Message.Express("You play ", f), Skin.Current.GetImageURL(f));
        }

        private static Message Express(params object[] elements)
        {
            return Message.Express(elements);
        }

    }
    public struct FlashInfo
    {
        public string Url;
        public Message Message;
    }
}
