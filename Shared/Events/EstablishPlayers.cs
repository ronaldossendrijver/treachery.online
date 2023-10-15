/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class EstablishPlayers : GameEvent
    {
        #region Construction

        public EstablishPlayers(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public EstablishPlayers()
        {
        }

        #endregion Construction

        #region Properties

        public int Seed { get; set; }

        public int MaximumNumberOfPlayers { get; set; } = 6;

        public int MaximumTurns { get; set; } = 10;

        public Rule[] ApplicableRules { get; set; }

        public string _gameName = "";

        [JsonIgnore]
        public string GameName
        {
            get => _gameName == null || _gameName == "" ? string.Format("{0}'s Game", Players.FirstOrDefault()) : _gameName;
            set => _gameName = value;
        }

        public string _players = "";

        [JsonIgnore]
        public IEnumerable<string> Players
        {
            get => _players == "" ? Array.Empty<string>() : _players.Split('>');
            set => _players = string.Join('>', value);
        }

        public string _factionsInPlay = "";

        [JsonIgnore]
        public List<Faction> FactionsInPlay
        {
            get => _factionsInPlay == null || _factionsInPlay.Length == 0 ? new List<Faction>() : _factionsInPlay.Split(',').Select(f => Enum.Parse<Faction>(f)).ToList();
            set => _factionsInPlay = string.Join(',', value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Game.CurrentPhase != Phase.AwaitingPlayers) return Message.Express("Invalid game phase");

            int extraSpotsForBots =
                (ApplicableRules.Contains(Rule.PurpleBot) && FactionsInPlay.Contains(Faction.Purple) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlackBot) && FactionsInPlay.Contains(Faction.Black) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.OrangeBot) && FactionsInPlay.Contains(Faction.Orange) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.RedBot) && FactionsInPlay.Contains(Faction.Red) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreenBot) && FactionsInPlay.Contains(Faction.Green) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlueBot) && FactionsInPlay.Contains(Faction.Blue) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.YellowBot) && FactionsInPlay.Contains(Faction.Yellow) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreyBot) && FactionsInPlay.Contains(Faction.Grey) ? 1 : 0);

            if (Players.Count() + extraSpotsForBots > FactionsInPlay.Count) return Message.Express("More factions required");
            if (ApplicableRules.Contains(Rule.FillWithBots) && FactionsInPlay.Count < MaximumNumberOfPlayers) return Message.Express("More factions required");

            int nrOfBots =
                (ApplicableRules.Contains(Rule.PurpleBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlackBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.OrangeBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.RedBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreenBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.YellowBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.GreyBot) ? 1 : 0) +
                (ApplicableRules.Contains(Rule.BlueBot) ? 1 : 0);

            if (Players.Count() + nrOfBots == 0 && !ApplicableRules.Contains(Rule.FillWithBots)) return Message.Express("At least one player required");
            if (Players.Count() + nrOfBots < 2 && !ApplicableRules.Contains(Rule.FillWithBots)) return Message.Express("At least two players required");
            if (MaximumNumberOfPlayers < 2) return Message.Express("At least two players required");
            if (Players.Count() + nrOfBots > MaximumNumberOfPlayers) return Message.Express("Too many players");
            if (FactionsInPlay.Any(f => !AvailableFactions().Contains(f))) return Message.Express("Invalid faction");

            return null;
        }

        public static int GetMaximumNumberOfPlayers()
        {
            return AvailableFactions().Count();
        }

        public static int GetMaximumNumberOfTurns()
        {
            return 20;
        }

        public static IEnumerable<Faction> AvailableFactions()
        {
            var result = new List<Faction>();

            if (Game.ExpansionLevel >= 0)
            {
                result.Add(Faction.Green);
                result.Add(Faction.Black);
                result.Add(Faction.Yellow);
                result.Add(Faction.Red);
                result.Add(Faction.Orange);
                result.Add(Faction.Blue);
            }

            if (Game.ExpansionLevel >= 1)
            {
                result.Add(Faction.Grey);
                result.Add(Faction.Purple);
            }

            if (Game.ExpansionLevel >= 2)
            {
                result.Add(Faction.Brown);
                result.Add(Faction.White);
            }

            if (Game.ExpansionLevel >= 3)
            {
                result.Add(Faction.Pink);
                result.Add(Faction.Cyan);
            }

            return result;
        }

        public static IEnumerable<RuleGroup> AvailableRuleGroups()
        {
            var result = new List<RuleGroup>();

            if (Game.ExpansionLevel >= 0)
            {
                result.Add(RuleGroup.CoreAdvanced);
                result.Add(RuleGroup.CoreBasicExceptions);
                result.Add(RuleGroup.CoreAdvancedExceptions);
            }

            if (Game.ExpansionLevel >= 1)
            {
                result.Add(RuleGroup.ExpansionIxAndBtBasic);
                result.Add(RuleGroup.ExpansionIxAndBtAdvanced);
            }

            if (Game.ExpansionLevel >= 2)
            {
                result.Add(RuleGroup.ExpansionBrownAndWhiteBasic);
                result.Add(RuleGroup.ExpansionBrownAndWhiteAdvanced);
            }

            if (Game.ExpansionLevel >= 3)
            {
                result.Add(RuleGroup.ExpansionPinkAndCyanBasic);
                result.Add(RuleGroup.ExpansionPinkAndCyanAdvanced);
            }

            if (Game.ExpansionLevel >= 0)
            {
                result.Add(RuleGroup.House);
            }

            return result;
        }

        #endregion Validation

        #region Execution

        public override Message Execute(bool performValidation, bool isHost)
        {
            if (performValidation)
            {
                var result = Validate();
                if (result == null)
                {
                    Game.PerformPreEventTasks(this);
                    ExecuteConcreteEvent();
                    Game.PerformPostEventTasks(this, true);
                }
                return result;
            }
            else
            {
                Game.PerformPreEventTasks(this);
                ExecuteConcreteEvent();
                Game.PerformPostEventTasks(this, true);
                return null;
            }
        }

        protected override void ExecuteConcreteEvent()
        {
            if (Game.CurrentPhase != Phase.AwaitingPlayers) return;

            Game.CurrentReport = new Report(MainPhase.Setup);

            Game.Stone(Milestone.GameStarted);
            Log("Game started!");

            Game.CurrentMainPhase = MainPhase.Setup;

            Game.Seed = Seed;
            Game.Name = GameName;
            Game.Random = new Random(Seed);

            Game.AllRules = ApplicableRules.ToList();
            Game.Rules = ApplicableRules.Where(r => Game.GetRuleGroup(r) != RuleGroup.Bots).ToList();
            Game.RulesForBots = ApplicableRules.Where(r => Game.GetRuleGroup(r) == RuleGroup.Bots).ToList();
            Game.Rules.AddRange(Game.GetRulesInGroup(RuleGroup.CoreBasic, Game.ExpansionLevel));

            Game.Ruleset = Game.DetermineApproximateRuleset(FactionsInPlay, Game.Rules, Game.ExpansionLevel);
            Log("Ruleset: ", Game.Ruleset);

            var customRules = Game.GetCustomRules().ToList();
            LogIf(customRules.Any(), "House rules: ", customRules);

            if (Game.Applicable(Rule.ExpansionTreacheryCards))
            {
                if (!Game.Rules.Contains(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal)) Game.Rules.Add(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal);
                if (!Game.Rules.Contains(Rule.ExpansionTreacheryCardsPBandSS)) Game.Rules.Add(Rule.ExpansionTreacheryCardsPBandSS);
                if (!Game.Rules.Contains(Rule.ExpansionTreacheryCardsAmal)) Game.Rules.Add(Rule.ExpansionTreacheryCardsAmal);
            }

            Game.ResourceCardDeck = CreateAndShuffleResourceCardDeck();
            Game.TreacheryDeck = TreacheryCardManager.CreateTreacheryDeck(Game, Game.Random);
            CreateDiscoveryTokens();

            if (!Game.Applicable(Rule.CustomDecks))
            {
                Game.TreacheryDeck.Shuffle();
                Game.Stone(Milestone.Shuffled);
            }

            Game.TreacheryDiscardPile = new Deck<TreacheryCard>(Game.Random);
            Game.ResourceCardDiscardPileA = new Deck<ResourceCard>(Game.Random);
            Game.ResourceCardDiscardPileB = new Deck<ResourceCard>(Game.Random);

            if (Game.Applicable(Rule.NexusCards))
            {
                CreateNexusDeck();
            }

            CreateTerrorTokens();
            Game.UnassignedAmbassadors = new Deck<Ambassador>(AvailableFactions().Where(f => f != Faction.Cyan).Select(f => Game.AmbassadorOf(f)), Game.Random);

            Game.OrangeAllowsShippingDiscount = true;
            Game.PurpleAllowsRevivalDiscount = true;
            Game.GreyAllowsReplacingCards = true;
            Game.RedWillPayForExtraRevival = 0;
            Game.YellowWillProtectFromMonster = true;
            Game.YellowAllowsThreeFreeRevivals = true;
            Game.YellowSharesPrescience = true;
            Game.YellowRefundsBattleDial = true;
            Game.GreenSharesPrescience = true;
            Game.BlueAllowsUseOfVoice = true;
            Game.WhiteAllowsUseOfNoField = true;

            Game.MaximumNumberOfTurns = MaximumTurns;
            Game.MaximumNumberOfPlayers = MaximumNumberOfPlayers;

            Log("The maximum number of turns is: ", Game.MaximumNumberOfTurns);

            Game.FactionsInPlay = FactionsInPlay;

            AddPlayersToGame();

            FillEmptySeatsWithBots();
            RemoveClaimedFactions();

            Game.Enter(Game.Applicable(Rule.PlayersChooseFactions), Phase.SelectingFactions, Game.AssignFactionsAndEnterFactionTrade);
        }

        private void CreateTerrorTokens()
        {
            Game.UnplacedTerrorTokens = new List<TerrorType> {
                TerrorType.Assassination,
                TerrorType.Atomics,
                TerrorType.Extortion,
                TerrorType.Robbery,
                TerrorType.Sabotage,
                TerrorType.SneakAttack
            };
        }

        private void CreateNexusDeck()
        {
            Game.NexusCardDeck = new Deck<Faction>(AvailableFactions(), Game.Random);
            Game.NexusCardDeck.Shuffle();
        }

        private void RemoveClaimedFactions()
        {
            foreach (var f in Game.Players.Where(p => !p.Is(Faction.None)).Select(p => p.Faction))
            {
                Game.FactionsInPlay.Remove(f);
            }
        }

        private Deck<ResourceCard> CreateAndShuffleResourceCardDeck()
        {
            var result = new Deck<ResourceCard>(Game.Random);
            foreach (var c in Map.GetResourceCardsInPlay(Game))
            {
                result.PutOnTop(c);
            }

            Game.Stone(Milestone.Shuffled);
            result.Shuffle();
            return result;
        }



        private void AddPlayersToGame()
        {
            if (Game.Version < 113)
            {
                AddBots();
            }

            foreach (var newPlayer in Players)
            {
                var p = new Player(Game, newPlayer);
                if (!Game.Players.Contains(p))
                {
                    Game.Players.Add(p);
                    Log(p.Name, " joined the game");
                }
                else
                {
                    Log(p.Name, " is already in the game");
                }
            }
        }

        private void AddBots()
        {
            //Can be removed later, this was replaced by filling empty seats with bots.
            if (Game.Applicable(Rule.OrangeBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Edric*"), Faction.Orange, true));
            if (Game.Applicable(Rule.RedBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Shaddam IV*"), Faction.Red, true));
            if (Game.Applicable(Rule.BlackBot)) Game.Players.Add(new Player(Game, UniquePlayerName("The Baron*"), Faction.Black, true));
            if (Game.Applicable(Rule.PurpleBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Scytale*"), Faction.Purple, true));
            if (Game.Applicable(Rule.BlueBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Mother Mohiam*"), Faction.Blue, true));
            if (Game.Applicable(Rule.GreenBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Paul Atreides*"), Faction.Green, true));
            if (Game.Applicable(Rule.YellowBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Liet Kynes*"), Faction.Yellow, true));
            if (Game.Applicable(Rule.GreyBot)) Game.Players.Add(new Player(Game, UniquePlayerName("Prince Rhombur*"), Faction.Grey, true));
        }

        private string UniquePlayerName(string name)
        {
            var result = name;
            while (Game.Players.Any(p => p.Name == result))
            {
                result += "'";
            }
            return result;
        }

        private void CreateDiscoveryTokens()
        {
            Game.YellowDiscoveryTokens = new Deck<DiscoveryToken>(Game.Random);
            Game.OrangeDiscoveryTokens = new Deck<DiscoveryToken>(Game.Random);

            if (Game.Applicable(Rule.DiscoveryTokens))
            {
                Game.YellowDiscoveryTokens.Items.Add(DiscoveryToken.Jacurutu);
                Game.YellowDiscoveryTokens.Items.Add(DiscoveryToken.Shrine);
                Game.YellowDiscoveryTokens.Items.Add(DiscoveryToken.TestingStation);
                Game.YellowDiscoveryTokens.Items.Add(DiscoveryToken.Cistern);
                Game.YellowDiscoveryTokens.Shuffle();

                Game.OrangeDiscoveryTokens.Items.Add(DiscoveryToken.ProcessingStation);
                Game.OrangeDiscoveryTokens.Items.Add(DiscoveryToken.CardStash);
                Game.OrangeDiscoveryTokens.Items.Add(DiscoveryToken.ResourceStash);
                Game.OrangeDiscoveryTokens.Items.Add(DiscoveryToken.Flight);
                Game.OrangeDiscoveryTokens.Shuffle();
            }
        }

        private void FillEmptySeatsWithBots()
        {
            if (Game.Applicable(Rule.FillWithBots))
            {
                if (Game.Version <= 125)
                {
                    var available = new Deck<Faction>(FactionsInPlay.Where(f => !Game.IsPlaying(f)), Game.Random);
                    available.Shuffle();

                    while (Game.Players.Count < MaximumNumberOfPlayers)
                    {
                        var bot = available.Draw() switch
                        {
                            Faction.Black => new Player(Game, UniquePlayerName("The Baron*"), Faction.Black, true),
                            Faction.Blue => new Player(Game, UniquePlayerName("Mother Mohiam*"), Faction.Blue, true),
                            Faction.Green => new Player(Game, UniquePlayerName("Paul Atreides*"), Faction.Green, true),
                            Faction.Yellow => new Player(Game, UniquePlayerName("Liet Kynes*"), Faction.Yellow, true),
                            Faction.Red => new Player(Game, UniquePlayerName("Shaddam IV*"), Faction.Red, true),
                            Faction.Orange => new Player(Game, UniquePlayerName("Edric*"), Faction.Orange, true),
                            Faction.Grey => new Player(Game, UniquePlayerName("Prince Rhombur*"), Faction.Grey, true),
                            Faction.Purple => new Player(Game, UniquePlayerName("Scytale*"), Faction.Purple, true),
                            Faction.Brown => new Player(Game, UniquePlayerName("Brown*"), Faction.Brown, true),
                            Faction.White => new Player(Game, UniquePlayerName("White*"), Faction.White, true),
                            Faction.Pink => new Player(Game, UniquePlayerName("Pink*"), Faction.Pink, true),
                            Faction.Cyan => new Player(Game, UniquePlayerName("Cyan*"), Faction.Cyan, true),
                            _ => new Player(Game, UniquePlayerName("The Baron*"), Faction.Black, true)
                        };

                        Game.Players.Add(bot);
                    }
                }
                else
                {
                    int botNr = 1;

                    while (Game.Players.Count < MaximumNumberOfPlayers)
                    {
                        Game.Players.Add(new Player(Game, UniquePlayerName(string.Format("Bot{0}", botNr++)), Faction.None, true));
                    }
                }
            }
        }

        #endregion Execution
    }
}
