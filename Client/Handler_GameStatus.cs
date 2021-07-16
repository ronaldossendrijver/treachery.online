/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public partial class Handler
    {
        public string NextPhase()
        {
            switch (CurrentPhase)
            {
                case Phase.AwaitingPlayers:
                    return "Start the game with the currently joined players?";

                case Phase.SelectingFactions:
                    return "Continue with random assignment of factions to players who haven't selected a faction?";

                case Phase.TradingFactions:
                    return "Continue the game with currently assigned factions?";

                case Phase.BlackMulligan:
                    return "Proceed to selecting traitors?";

                case Phase.MetheorAndStormSpell:
                    return Skin.Current.Format("Continue with the {0} phase?", MainPhase.Storm);

                case Phase.StormReport:
                    return Skin.Current.Format("End {0} and start {1}?", MainPhase.Storm, MainPhase.Blow);

                case Phase.Thumper:
                    return Skin.Current.Format("Continue with the {0} phase?", MainPhase.Blow);

                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    return Skin.Current.Format("Continue with the {0} phase?", MainPhase.Blow);

                case Phase.AllianceA:
                case Phase.AllianceB:
                    return "End Nexus?";

                case Phase.BlowReport:
                    return Skin.Current.Format("End {0} and start {1}?", MainPhase.Blow, MainPhase.Charity);

                case Phase.BiddingReport:
                    return Skin.Current.Format("End the {0} phase and start {1}?", MainPhase.Bidding, MainPhase.Resurrection);

                case Phase.ClaimingCharity:
                    return Skin.Current.Format("End {0} and start {1}?", MainPhase.Charity, MainPhase.Bidding);

                case Phase.WaitingForNextBiddingRound:
                    return Skin.Current.Format("Put next card on auction?");

                case Phase.Resurrection:
                    return Skin.Current.Format("End {0} and start {1}", MainPhase.Resurrection, MainPhase.ShipmentAndMove);

                case Phase.ShipmentAndMoveConcluded:
                    return Skin.Current.Format("End {0}?", MainPhase.ShipmentAndMove);

                case Phase.Auditing:
                    return "Finish audit?";

                case Phase.BattleReport:
                    if (Game.Aggressor == null)
                    {
                        return Skin.Current.Format("End the {0} phase and continue with {1}?", MainPhase.Battle, MainPhase.Collection);
                    }
                    else
                    {
                        return "Continue with the next Battle?";
                    }

                case Phase.CollectionReport:
                    return Skin.Current.Format("End {0} and move to {1}", MainPhase.Collection, MainPhase.Contemplate);

                case Phase.Contemplate:
                    return Skin.Current.Format("Start checking for victories?");

                case Phase.TurnConcluded:
                    return Skin.Current.Format("End this turn and start the next {0}?", MainPhase.Storm);

                default:
                    return "Move to the next step?";
            }
        }

        public GameStatus Status
        {
            get
            {
                if (Game.CurrentPhase == Phase.PerformingKarmaHandSwap)
                {
                    if (IAm(Faction.Black))
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("Please decide which {0} cards to return to {1}.", Game.KarmaHandSwapNumberOfCards, Game.KarmaHandSwapTarget),
                            WaitingForOthers = false
                        };
                    }
                    else
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("{0} are deciding which {1} cards to return to {2}.", Faction.Black, Game.KarmaHandSwapNumberOfCards, Game.KarmaHandSwapTarget),
                            WaitingForOthers = true
                        };
                    }
                }
                else if (Game.CurrentPhase == Phase.Clairvoyance)
                {
                    if (IAm(Game.LatestClairvoyance.Target))
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("{0} asked you a question in {1}! All factions are waiting for you answer...", Game.LatestClairvoyance.Initiator, TreacheryCardType.Clairvoyance),
                            WaitingForOthers = false
                        };
                    }
                    else
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("Waiting for {0} to answer a question asked in {1}...", Game.LatestClairvoyance.Target, TreacheryCardType.Clairvoyance),
                            WaitingForOthers = true
                        };
                    }
                }
                else if (Game.CurrentPhase == Phase.StormLosses)
                {
                    if (IAm(Faction.Yellow))
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("Please decide which forces were killed by the storm in {0}.", TakeLosses.LossesToTake(Game).Location),
                            WaitingForOthers = false
                        };
                    }
                    else
                    {
                        return new GameStatus()
                        {
                            Description = Skin.Current.Format("{0} are deciding which forces were killed by the storm in {1}...", TakeLosses.LossesToTake(Game).Faction, TakeLosses.LossesToTake(Game).Location),
                            WaitingForOthers = true
                        };
                    }
                }
                else if (Game.CurrentPhase == Phase.TradingCards)
                {
                    if (Faction == Faction.Brown || Player.Ally == Faction.Brown)
                    {
                        if (Game.CurrentCardTradeOffer.Initiator == Faction)
                        {
                            return new GameStatus()
                            {
                                Description = "You are waiting for your ally to select a card in return ...",
                                WaitingForOthers = true
                            };
                        }
                        else
                        {
                            return new GameStatus()
                            {
                                Description = "Please select a card in return...",
                                WaitingForOthers = false
                            };
                        }
                    }
                    else
                    {
                        return new GameStatus()
                        {
                            Description = "{0} and their ally are currently trading cards...",
                            WaitingForOthers = true
                        };
                    }
                }

                switch (Game.CurrentMainPhase)
                {
                    case MainPhase.Started:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.AwaitingPlayers:
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Players may now join the game. As host, you can start the game when ready.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Waiting for the host to start the game...",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }
                            }
                        }
                        break;

                    case MainPhase.Setup:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.SelectingFactions:
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Players may now choose their faction... As host, you can skip to the next phase when ready.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Players may now choose their faction.",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.TradingFactions:
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Players may offer to trade factions with other players... As host, you can skip to the next phase when ready.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Players may now offer to trade factions. When two players both offer to trade with each other, they switch factions.",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.BluePredicting:
                                    {
                                        if (IAm(Faction.Blue))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please predict who will win the game and in which turn.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are predicting who will win the game and in which turn...", Faction.Blue),
                                                WaitingForOthers = true
                                            };
                                        }

                                    }

                                case Phase.BlackMulligan:
                                    {
                                        if (IAm(Faction.Black))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "If you drew two or more of your own leaders as traitors, you may draw a new set of traitors. Otherwise, pass.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} may draw a new set of traitors if they were dealt two or more of their own leaders...", Faction.Black),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.SelectingTraitors:
                                    {
                                        if (WaitingForMe)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Factions are now selecting one of four leaders they wish to keep as a traitor.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = WhoAreWeWaitingFor + " are currently selecting traitors...",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.PerformCustomSetup:
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("Please set up {0} initial {1} and force positions.", Game.NextFactionToPerformCustomSetup, Concept.Resource),
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("The host is setting up {0} initial {1} and force positions.", Game.NextFactionToPerformCustomSetup, Concept.Resource),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.YellowSettingUp:
                                    {
                                        if (IAm(Faction.Yellow))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please set up your initial force positions.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {

                                                Description = Skin.Current.Format("{0} are setting up their initial force positions...", Faction.Yellow),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.BlueSettingUp:
                                    {
                                        if (IAm(Faction.Blue))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please select your initial force position.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are selecting their initial force position...", Faction.Blue),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.GreySelectingCard:
                                    {
                                        if (IAm(Faction.Grey))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please select your starting Treachery Card.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are selecting their starting Treachery Card...", Faction.Grey),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }
                            }
                        }
                        break;

                    case MainPhase.Storm:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.HmsPlacement:
                                    {
                                        if (IAm(Faction.Grey))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please position the Hidden Mobile Stronghold.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "The Hidden Mobile Stronghold is being positioned...",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.DiallingStorm:
                                    {
                                        if (Game.HasBattleWheel.Contains(Player.Faction) && !Game.HasActedOrPassed.Contains(Player.Faction))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please dial a number to determine storm movement.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are determining storm movement", string.Join(", ", Game.HasBattleWheel.Where(f => !Game.HasActedOrPassed.Contains(f)).Select(f => Skin.Current.Describe(f)))),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.HmsMovement:
                                    {
                                        if (IAm(Faction.Grey))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You may move the Hidden Mobile Stronghold to a sector in an adjacent territory.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about moving the Hidden Mobile Stronghold...", Faction.Grey),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.MetheorAndStormSpell:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now use {0} or {1}... As host, you can skip to the next step when ready.", TreacheryCardType.Metheor, TreacheryCardType.StormSpell),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now use {0} or {1}...", TreacheryCardType.Metheor, TreacheryCardType.StormSpell),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.StormReport:

                                    var reportName = Game.CurrentTurn == 1 ? Skin.Current.Format("{0} and First {1}", MainPhase.Setup, MainPhase.Storm) : Skin.Current.Describe(MainPhase.Storm);
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now review the {0} report... As host, you can skip to the next step when ready.", reportName),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now review the {0} report...", reportName),
                                            WaitingForOthers = true
                                        };
                                    }
                            }
                        }
                        break;

                    case MainPhase.Blow:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.Thumper:
                                    if (Game.CurrentTurn > 1)
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("Factions may now use a {0} to call {1}... As host, you can skip to the next step when ready.", TreacheryCardType.Thumper, Concept.Monster),
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("Factions may now use a {0} to call {1}...", TreacheryCardType.Thumper, Concept.Monster),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }
                                    else
                                    {
                                        if (IsHost)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("You may now continue to the first {0} blow when ready.", Concept.Resource),
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("Waiting for the host to perform the first {0} blow.....", Concept.Resource),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.YellowSendingMonsterA:
                                case Phase.YellowSendingMonsterB:
                                    if (IAm(Faction.Yellow))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Please select where to send {0}.", Concept.Monster),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are selecting where to send {1}...", Faction.Yellow, Concept.Monster),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.YellowRidingMonsterA:
                                case Phase.YellowRidingMonsterB:
                                    if (IAm(Faction.Yellow))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Please select where to ride {0}.", Concept.Monster),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are selecting where to travel with {1}...", Faction.Yellow, Concept.Monster),
                                            WaitingForOthers = true
                                        };
                                    }
                                case Phase.BlueIntrudedByYellowRidingMonsterA:
                                case Phase.BlueIntrudedByYellowRidingMonsterB:
                                    if (IAm(Faction.Blue))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("You must decide what to do in response to an intrusion of {0}; be fighters or advisors?", Game.LastShippedOrMovedTo.Territory.Name),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are deciding what to do in response to an intrusion...", Faction.Blue),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.HarvesterA:
                                case Phase.HarvesterB:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now use a {0} to double the {1} blow in {2}... As host, you can skip to the next step when ready.", TreacheryCardType.Harvester, Concept.Resource, CurrentPhase == Phase.HarvesterA ? Game.LatestSpiceCardA : Game.LatestSpiceCardB),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now use a {0} to double the {1} blow in {2}...", TreacheryCardType.Harvester, Concept.Resource, CurrentPhase == Phase.HarvesterA ? Game.LatestSpiceCardA : Game.LatestSpiceCardB),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.AllianceA:
                                case Phase.AllianceB:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Factions may now make and break alliances. As host, you can skip to the next phase when ready.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Factions may now make and break alliances. If two factions both offer an alliance to each other, they become allies.",
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BlowReport:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions are reviewing the {0} report. As host, you can start the {1} phase when ready.", MainPhase.Blow, MainPhase.Charity),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now study the {0} report.", MainPhase.Blow),
                                            WaitingForOthers = true
                                        };
                                    }

                            }
                        }
                        break;

                    case MainPhase.Charity:
                        {
                            if (IsHost)
                            {
                                return new GameStatus()
                                {
                                    Description = "Factions may now claim charity if eligable. As host, you may end this step and start Bidding when ready.",
                                    WaitingForOthers = false
                                };
                            }
                            else if (IAm(Faction.Green))
                            {
                                return new GameStatus()
                                {
                                    Description = Skin.Current.Format("Factions may now claim charity if eligable. As {0}, you may start the Bidding phase when all factions are ready.", Faction.Green),
                                    WaitingForOthers = false
                                };
                            }
                            else
                            {
                                return new GameStatus()
                                {
                                    Description = "Factions may now claim charity if eligable.",
                                    WaitingForOthers = true
                                };
                            }
                        }

                    case MainPhase.Bidding:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.BlackMarketAnnouncement:
                                    {
                                        if (IAm(Faction.White))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You may now select a card to sell on the Black Markt, or pass.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about selling one of their cards on the Black Market...", Faction.White),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.BlackMarketBidding:

                                    if (Game.BlackMarketAuctionType != AuctionType.Silent)
                                    {
                                        if (Player == Game.BlackMarketBidSequence.CurrentPlayer)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please bid or pass.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about their bid...", Game.BlackMarketBidSequence.CurrentFaction),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }
                                    else
                                    {
                                        if (!Game.BlackMarketBids.Keys.Contains(Faction))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Please bid or pass.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Factions are thinking about their bids...",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.GreyRemovingCardFromBid:
                                    {
                                        if (IAm(Faction.Grey))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "Remove one card from the auction and put in on top or on the bottom of the Treachery Card deck.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about putting one card from the auction on top or on the bottom of the Treachery Card deck...", Faction.Grey),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.GreySwappingCard:
                                    {
                                        if (IAm(Faction.Grey))
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You may swap the next card on auction with a card from your hand.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about swapping the next card on auction with a card from their hand...", Faction.Grey),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.Bidding:
                                    if (Player == Game.BidSequence.CurrentPlayer)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Please bid or pass.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about their bid...", Game.BidSequence.CurrentFaction),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.ReplacingCardJustWon:
                                    {
                                        if (Player.Ally == Faction.Grey)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You may now discard the card you just won and draw a new card instead.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are thinking about replacing the card they just won with a new card from the deck...", Game.GetPlayer(Faction.Grey).Ally),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.WaitingForNextBiddingRound:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "You may start bidding on the next card when all factions are ready.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else if (IAm(Faction.Green))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "You may start bidding on the next card when all factions are ready.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Waiting for the next card to be put on auction..."),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BiddingReport:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions are reviewing the {0} report. As host you can start the {1} phase when ready.", MainPhase.Bidding, MainPhase.Resurrection),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now review the {0} report.", MainPhase.Bidding),
                                            WaitingForOthers = true
                                        };
                                    }

                            }
                            break;
                        }

                    case MainPhase.Resurrection:
                        switch (CurrentPhase)
                        {

                            case Phase.Resurrection:
                                if (IsHost)
                                {
                                    return new GameStatus()
                                    {
                                        Description = "Factions may now reclaim forces and leaders. As host, you may end this phase and start Shipment & Move when ready.",
                                        WaitingForOthers = false
                                    };
                                }
                                else
                                {
                                    return new GameStatus()
                                    {
                                        Description = "Factions may now reclaim forces and leaders. The host is waiting for everyone to be ready to enter the Shipment & Move phase...",
                                        WaitingForOthers = true
                                    };
                                }
                        }
                        break;

                    case MainPhase.ShipmentAndMove:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.NonOrangeShip:
                                    if (Game.ShipmentAndMoveSequence.CurrentPlayer == Player)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = string.Format("Your turn! Please decide to {0} forces or pass.", Game.ShipmentAndMoveSequence.CurrentFaction == Faction.Yellow ? "rally" : "ship"),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about {1} forces...", Game.ShipmentAndMoveSequence.CurrentFaction, Game.ShipmentAndMoveSequence.CurrentFaction == Faction.Yellow ? "rallying" : "shipping"),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.OrangeShip:
                                    if (IAm(Faction.Orange))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = !Game.EveryoneButOneActedOrPassed && Game.Applicable(Rule.OrangeDetermineShipment) ?
                                                "Please decide to ship now or delay your turn and let other factions go first." :
                                                "Your turn! Please decide to ship forces or pass.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = !Game.EveryoneButOneActedOrPassed && Game.Applicable(Rule.OrangeDetermineShipment) ?
                                            Skin.Current.Format("{0} are thinking deciding about shipping forces or delaying their turn...", Faction.Orange) :
                                            Skin.Current.Format("{0} are thinking about shipping forces...", Faction.Orange),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BlueAccompaniesNonOrange:
                                case Phase.BlueAccompaniesOrange:
                                    if (IAm(Faction.Blue))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Please decide if you want to accompany the latest shipment.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are deciding whether to accompany the latest shipment...", Faction.Blue),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BlueIntrudedByNonOrangeShip:
                                case Phase.BlueIntrudedByOrangeShip:
                                case Phase.BlueIntrudedByNonOrangeMove:
                                case Phase.BlueIntrudedByOrangeMove:
                                case Phase.BlueIntrudedByCaravan:
                                    if (IAm(Faction.Blue))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Please decide what to do in response to an intrusion of {0}; become fighters or advisors?", Game.LastShippedOrMovedTo.Territory.Name),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are deciding what to do in response to an intrusion...", Faction.Blue),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.NonOrangeMove:
                                    if (Game.ShipmentAndMoveSequence.CurrentPlayer == Player)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Please decide to move forces or pass.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about about moving forces.", Skin.Current.Describe(Game.ShipmentAndMoveSequence.CurrentFaction)),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.OrangeMove:
                                    if (IAm(Faction.Orange))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Please decide to move forces or pass.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about about moving forces.", Faction.Orange),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.ShipmentAndMoveConcluded:
                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Please enter the Battle Phase when all factions are ready.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "The host is waiting for everyone to be ready to enter the Battle phase...",
                                            WaitingForOthers = true
                                        };
                                    }
                            }
                        }
                        break;

                    case MainPhase.Battle:
                        {
                            switch (CurrentPhase)
                            {
                                case Phase.BattlePhase:
                                    if (Game.CurrentBattle == null)
                                    {
                                        var iAmAggressor = (Game.Aggressor == Player);

                                        if (iAmAggressor)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You are Aggressor! Please choose whom and where to battle.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are Aggressors! They are deciding whom and where to battle...", Game.Aggressor.Faction),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }
                                    else
                                    {
                                        var iAmAggressor = IAm(Game.CurrentBattle.Initiator);
                                        var iAmDefender = IAm(Game.CurrentBattle.Target);

                                        if (iAmAggressor)
                                        {
                                            if (Game.AggressorBattleAction == null)
                                            {
                                                return new GameStatus()
                                                {
                                                    Description = Skin.Current.Format("You are Aggressor against {0} in {1}! Please confirm your Battle Plan.", Game.CurrentBattle.Target, Game.CurrentBattle.Territory),
                                                    WaitingForOthers = false
                                                };
                                            }
                                            else
                                            {
                                                return new GameStatus()
                                                {
                                                    Description = Skin.Current.Format("You are waiting for {0} to defend {1}...", Game.CurrentBattle.Target, Game.CurrentBattle.Territory),
                                                    WaitingForOthers = true
                                                };
                                            }
                                        }
                                        else if (iAmDefender)
                                        {
                                            if (Game.DefenderBattleAction == null)
                                            {
                                                return new GameStatus()
                                                {
                                                    Description = Skin.Current.Format("You must defend against {0} Aggression in {1}! Please confirm your Battle Plan.", Game.CurrentBattle.Initiator, Game.CurrentBattle.Territory),
                                                    WaitingForOthers = false
                                                };
                                            }
                                            else
                                            {
                                                return new GameStatus()
                                                {
                                                    Description = Skin.Current.Format("You are waiting for {0} to attack {1}...", Game.CurrentBattle.Initiator, Game.CurrentBattle.Territory),
                                                    WaitingForOthers = true
                                                };
                                            }

                                        }
                                        else
                                        {
                                            return new GameStatus()
                                            {
                                                Description = Skin.Current.Format("{0} are defending against {1} aggression in {2}...", Game.CurrentBattle.Target, Game.CurrentBattle.Initiator, Game.CurrentBattle.Territory),
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.CallTraitorOrPass:
                                    {
                                        var iAmAggressor = IAm(Game.CurrentBattle.Initiator);
                                        var iAmDefender = IAm(Game.CurrentBattle.Target);

                                        if (Game.AggressorTraitorAction == null && iAmAggressor || Game.DefenderTraitorAction == null && iAmDefender)
                                        {
                                            return new GameStatus()
                                            {
                                                Description = "You may now decide to call TREACHERY if the enemy leader is a traitor under your command.",
                                                WaitingForOthers = false
                                            };
                                        }
                                        else
                                        {

                                            return new GameStatus()
                                            {
                                                Description = "Waiting for a faction to call TREACHERY...",
                                                WaitingForOthers = true
                                            };
                                        }
                                    }

                                case Phase.AvoidingAudit:
                                    if (IAm(Game.Auditee))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Do you wish to avoid being audited?",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about avoiding a scheduled audit...", Game.Auditee.Faction),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.Auditing:
                                    if (IAm(Faction.Brown))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "Conclude the audit when done inspecting your opponents cards.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Waiting for {0} to finish their audit...", Faction.Brown),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BattleConclusion:
                                    if (IAm(Game.BattleWinner))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "You won! Conclude the battle when done celebrating your victory.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are celebrating their victory in battle...", Game.BattleWinner),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.Facedancing:
                                    if (IAm(Faction.Purple))
                                    {
                                        return new GameStatus()
                                        {
                                            Description = "You may now reveal a leader to be one of your face dancers.",
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("{0} are thinking about revealing a face dancer...", Faction.Purple),
                                            WaitingForOthers = true
                                        };
                                    }

                                case Phase.BattleReport:

                                    if (IsHost)
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions are reviewing the {0} report. As host you can proceed the game when ready.", MainPhase.Battle),
                                            WaitingForOthers = false
                                        };
                                    }
                                    else
                                    {
                                        return new GameStatus()
                                        {
                                            Description = Skin.Current.Format("Factions may now review the {0} report.", MainPhase.Battle),
                                            WaitingForOthers = true
                                        };
                                    }

                            }
                        }
                        break;

                    case MainPhase.Collection:

                        switch (CurrentPhase)
                        {
                            case Phase.CollectionReport:

                                if (IsHost)
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("Please move to {1} when all factions are done reviewing the {0} Report.", MainPhase.Collection, MainPhase.Contemplate),
                                        WaitingForOthers = false
                                    };
                                }
                                else
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("Factions may review the {0} report until the host moves to {1}...", MainPhase.Collection, MainPhase.Contemplate),
                                        WaitingForOthers = true
                                    };
                                }

                        }
                        break;

                    case MainPhase.Contemplate:

                        switch (CurrentPhase)
                        {
                            case Phase.ReplacingFaceDancer:

                                if (IAm(Faction.Purple))
                                {
                                    return new GameStatus()
                                    {
                                        Description = "You may replace an unrevealed Face Dancer with a new one drawn from the Traitor Deck.",
                                        WaitingForOthers = false
                                    };
                                }
                                else
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("{0} are thinking about replacing one of their Face Dancers...", Faction.Purple),
                                        WaitingForOthers = true
                                    };
                                }
                            
                            case Phase.Contemplate:

                                if (IsHost)
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("You may now proceed with determining victories when ready.", Concept.Resource),
                                        WaitingForOthers = false
                                    };
                                }
                                else
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("Waiting for the host to start determining victories..."),
                                        WaitingForOthers = true
                                    };
                                }

                            case Phase.TurnConcluded:

                                if (IsHost)
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("Please start the next turn when all factions are done reviewing the {0} Report.", MainPhase.Contemplate),
                                        WaitingForOthers = false
                                    };
                                }
                                else
                                {
                                    return new GameStatus()
                                    {
                                        Description = Skin.Current.Format("Factions may review the {0} report until the host starts the next turn...", MainPhase.Contemplate),
                                        WaitingForOthers = true
                                    };
                                }

                        }
                        break;

                    case MainPhase.Ended:
                        return new GameStatus()
                        {
                            Description = "The game has ended.",
                            WaitingForOthers = false
                        };

                }

                return new GameStatus() { Description = "unknown phase" };
            }
        }

        public string WhoAreWeWaitingFor
        {
            get
            {
                if (Game.HasActedOrPassed != null)
                {
                    var result = "";
                    var waitingForPlayers = Game.Players.Where(p => !Game.HasActedOrPassed.Contains(p.Faction)).ToList();

                    for (int i = 0; i < waitingForPlayers.Count; i++)
                    {
                        var factionName = Skin.Current.Describe(waitingForPlayers[i].Faction);

                        if (i == 0)
                        {
                            result = factionName;
                        }
                        else if (i == waitingForPlayers.Count - 1)
                        {
                            result += " and " + factionName;
                        }
                        else
                        {
                            result += ", " + factionName;
                        }
                    }

                    return result;
                }
                else
                {
                    return "none";
                }
            }
        }

        public bool WaitingForMe
        {
            get
            {
                return !IsObserver && Game.HasActedOrPassed != null && !Game.HasActedOrPassed.Contains(Player.Faction);
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
                    case Phase.CallTraitorOrPass:
                    case Phase.BattleConclusion:
                    case Phase.Facedancing:
                        if (Game.CurrentBattle != null)
                        {
                            return new Territory[] { Game.CurrentBattle.Territory };
                        }
                        break;
                }

                return new Territory[] { };
            }
        }


        public bool HighlightPlayer(Player p)
        {
            return Game.CurrentPhase == Phase.Bidding && p == Game.BidSequence.CurrentPlayer ||
                   Game.CurrentPhase == Phase.BlackMarketBidding && Game.BlackMarketAuctionType != AuctionType.Silent && p == Game.BlackMarketBidSequence.CurrentPlayer ||
                   (Game.CurrentPhase == Phase.OrangeMove || Game.CurrentPhase == Phase.OrangeShip) && p.Faction == Faction.Orange ||
                   (Game.CurrentPhase == Phase.NonOrangeMove || Game.CurrentPhase == Phase.NonOrangeShip) && p == Game.ShipmentAndMoveSequence.CurrentPlayer ||
                   (Game.CurrentPhase == Phase.BlueAccompaniesOrange || Game.CurrentPhase == Phase.BlueAccompaniesNonOrange || Game.CurrentPhase == Phase.BlueIntrudedByOrangeMove || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeMove || Game.CurrentPhase == Phase.BlueIntrudedByOrangeShip || Game.CurrentPhase == Phase.BlueIntrudedByNonOrangeShip) && p.Faction == Faction.Blue ||
                   (Game.CurrentMainPhase == MainPhase.Battle) && p == Game.Aggressor;
        }
    }
}
