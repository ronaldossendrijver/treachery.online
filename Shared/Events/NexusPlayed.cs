/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Treachery.Shared
{
    public class NexusPlayed : GameEvent, ILocationEvent
    {
        #region Construction

        public NexusPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public NexusPlayed()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Faction { get; set; }

        public PrescienceAspect GreenPrescienceAspect { get; set; }

        public int PurpleForces { get; set; }

        public int PurpleSpecialForces { get; set; }

        public int _purpleHeroId = -1;

        [JsonIgnore]
        public IHero PurpleHero
        {
            get => LeaderManager.HeroLookup.Find(_purpleHeroId);
            set => _purpleHeroId = LeaderManager.HeroLookup.GetId(value);
        }

        public bool PurpleAssignSkill { get; set; } = false;

        public int _brownCardId;

        [JsonIgnore]
        public TreacheryCard BrownCard
        {
            get => TreacheryCardManager.Get(_brownCardId);
            set => _brownCardId = TreacheryCardManager.GetId(value);
        }

        public int _pinkTerritoryId;

        [JsonIgnore]
        public Territory PinkTerritory
        {
            get => Game.Map.TerritoryLookup.Find(_pinkTerritoryId);
            set => _pinkTerritoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        public Faction PinkFaction { get; set; }

        public int _cyanTerritoryId;

        [JsonIgnore]
        public Territory CyanTerritory
        {
            get => Game.Map.TerritoryLookup.Find(_cyanTerritoryId);
            set => _cyanTerritoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        public int PurpleNumberOfSpecialForcesInLocation { get; set; }

        public int _purpleLocationId = -1;

        [JsonIgnore]
        public Location PurpleLocation 
        { 
            get => Game.Map.LocationLookup.Find(_purpleLocationId); 
            set => _purpleLocationId = Game.Map.LocationLookup.GetId(value); 
        }

        [JsonIgnore]
        public Location To => PurpleLocation;

        [JsonIgnore]
        public int TotalAmountOfForcesAddedToLocation => PurpleNumberOfSpecialForcesInLocation;

        [JsonIgnore]
        public bool IsCunning => Initiator == Faction;

        [JsonIgnore]
        public bool IsSecretAlly => !Game.IsPlaying(Faction);

        [JsonIgnore]
        public bool IsBetrayal => !(IsCunning || IsSecretAlly);

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            switch (Faction)
            {
                case Faction.None: return Message.Express("Invalid Nexus faction");

                case Faction.Green:
                    if ((IsCunning || IsSecretAlly) && GreenPrescienceAspect == PrescienceAspect.None) return Message.Express("Invalid battle plan element");
                    break;

                case Faction.Purple:
                    if (IsSecretAlly)
                    {
                        if (PurpleHero != null && !ValidPurpleHeroes(Game, Player).Contains(PurpleHero)) return Message.Express("Invalid leader");
                        if (PurpleForces > ValidPurpleMaxAmount(Game, Player, false)) return Message.Express("You can't revive that many ", Player.Force);
                        if (PurpleSpecialForces > ValidPurpleMaxAmount(Game, Player, true)) return Message.Express("You can't revive that many ", Player.SpecialForce);
                        if (DeterminePurpleCost() > Player.Resources) return Message.Express("You can't pay that much");
                        if (PurpleForces + PurpleSpecialForces > 5) return Message.Express("You can't revive that many forces");
                        if (PurpleAssignSkill && PurpleHero == null) return Message.Express("You must revive a leader to assign a skill to");
                        if (PurpleAssignSkill && !Revival.MayAssignSkill(Game, Player, PurpleHero)) return Message.Express("You can't assign a skill to this leader");

                        if (PurpleLocation != null)
                        {
                            if (!Revival.ValidRevivedForceLocations(Game, Player).Contains(PurpleLocation)) return Message.Express("You can't place revived forces there");

                            if (PurpleNumberOfSpecialForcesInLocation > Revival.NumberOfSpecialForcesThatMayBePlacedOnPlanet(Player, PurpleSpecialForces))
                            {
                                return Message.Express("You can't place that many forces directly on the planet");
                            }
                        }
                    }
                    break;

                case Faction.Brown:
                    if (IsCunning)
                    {
                        if (BrownCard == null) return Message.Express("Select a ", TreacheryCardType.Useless, " card to discard");
                        if (BrownCard != null && !ValidBrownCards(Player).Contains(BrownCard)) return Message.Express("Invalid card");
                    }
                    break;

                case Faction.Pink:
                    if (IsBetrayal)
                    {
                        if (PinkTerritory == null) return Message.Express("Select a territory");
                        if (!ValidPinkTerritories(Game).Contains(PinkTerritory)) return Message.Express("Invalid territory");
                    }
                    else if (IsSecretAlly)
                    {
                        if (!ValidPinkFactions(Game).Contains(PinkFaction)) return Message.Express("Invalid faction");
                    }
                    break;

                case Faction.Cyan:
                    if (IsBetrayal && !ValidCyanTerritories(Game).Contains(CyanTerritory)) return Message.Express("This territory contains no terror token");
                    break;


            }

            return null;
        }

        public static bool CanUseCunning(Player p) => p.Nexus != Faction.None && p.Nexus == p.Faction;

        public static bool CanUseSecretAlly(Game g, Player p) => p.Nexus != Faction.None && !g.IsPlaying(p.Nexus);

        public static bool CanUseBetrayal(Game g, Player p) => p.Nexus != Faction.None && p.Nexus != p.Faction && g.IsPlaying(p.Nexus);

        public static bool IsApplicable(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.NexusCards || g.CurrentPhaseIsUnInterruptable)
            {
                return false;
            }

            bool cunning = CanUseCunning(p);
            bool secretAlly = CanUseSecretAlly(g, p);
            bool betrayal = CanUseBetrayal(g, p);

            bool gameIsInBattle = g.CurrentPhase == Phase.BattlePhase && g.CurrentBattle != null;
            bool isCurrentlyFormulatingBattlePlan = gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(p) && (g.DefenderBattleAction == null || g.AggressorBattleAction == null);

            return (p.Nexus) switch
            {
                Faction.Green when betrayal => gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(Faction.Green),
                Faction.Green when cunning || secretAlly => isCurrentlyFormulatingBattlePlan,

                Faction.Black when betrayal => g.CurrentPhase == Phase.CancellingTraitor,
                Faction.Black when cunning => true,
                Faction.Black when secretAlly => g.CurrentMainPhase == MainPhase.Contemplate,

                Faction.Yellow when betrayal => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.ShipmentAndMove,
                Faction.Yellow when secretAlly => g.CurrentMainPhase == MainPhase.Blow || g.CurrentMainPhase == MainPhase.Resurrection,

                Faction.Red when betrayal => g.CurrentMainPhase == MainPhase.Bidding || gameIsInBattle && g.Applicable(Rule.RedSpecialForces) && g.CurrentBattle.IsAggressorOrDefender(Faction.Red),
                Faction.Red when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Orange when betrayal => g.CurrentMainPhase == MainPhase.ShipmentAndMove && g.RecentlyPaid != null && g.HasRecentPaymentFor(typeof(Shipment)),
                Faction.Orange when cunning => g.CurrentPhase == Phase.OrangeMove && !g.InOrangeCunningShipment,
                Faction.Orange when secretAlly => g.CurrentPhase == Phase.NonOrangeShip,

                Faction.Blue when betrayal => gameIsInBattle && g.CurrentBattle.IsAggressorOrDefender(Faction.Blue),
                Faction.Blue when cunning => g.CurrentMainPhase == MainPhase.ShipmentAndMove,

                Faction.Grey when betrayal => g.CurrentMainPhase == MainPhase.Bidding && g.CurrentPhase < Phase.BiddingReport,
                Faction.Grey when cunning => isCurrentlyFormulatingBattlePlan,

                Faction.Purple when betrayal => g.CurrentPhase == Phase.Facedancing,
                Faction.Purple when cunning => true,
                Faction.Purple when secretAlly => g.CurrentPhase == Phase.Resurrection,

                Faction.Brown when betrayal => true,
                Faction.Brown when secretAlly => g.CurrentMainPhase == MainPhase.Collection && ValidBrownCards(p).Any() || g.CurrentPhase == Phase.BattleConclusion && g.CurrentBattle != null && p.Faction == g.BattleWinner,

                Faction.White when betrayal => g.CurrentMainPhase == MainPhase.Bidding && g.WhiteBiddingJustFinished && g.CardJustWon != null,

                Faction.Pink when betrayal => g.CurrentMainPhase < MainPhase.ShipmentAndMove && ValidPinkTerritories(g).Any(),
                Faction.Pink when cunning || secretAlly => true,

                Faction.Cyan when betrayal => ValidCyanTerritories(g).Any(),

                _ => false
            };
        }

        public static int ValidPurpleMaxAmount(Game g, Player p, bool specialForces)
        {
            if (specialForces)
            {
                if (p.Faction == Faction.Red || p.Faction == Faction.Yellow)
                {
                    if (g.FactionsThatRevivedSpecialForcesThisTurn.Contains(p.Faction))
                    {
                        return 0;
                    }
                    else
                    {
                        return Math.Min(p.SpecialForcesKilled, 1);
                    }
                }
                else
                {
                    return Math.Min(p.SpecialForcesKilled, 5);
                }
            }
            else
            {
                return Math.Min(p.ForcesKilled, 5);
            }
        }

        public static IEnumerable<IHero> ValidPurpleHeroes(Game game, Player player) => RaiseDeadPlayed.ValidHeroes(game, player);

        public static IEnumerable<TreacheryCard> ValidBrownCards(Player player) => player.TreacheryCards.Where(c => c.Type == TreacheryCardType.Useless);

        public static IEnumerable<Territory> ValidPinkTerritories(Game g)
        {
            var pink = g.GetPlayer(Faction.Pink);

            if (pink != null && pink.HasAlly)
            {
                return g.Map.Territories(false).Where(t => pink.AnyForcesIn(t) > 0 && pink.AlliedPlayer.AnyForcesIn(t) > 0);
            }

            return Array.Empty<Territory>();
        }

        public static IEnumerable<Faction> ValidPinkFactions(Game g) => g.Players.Where(p => p.Faction != Faction.Pink && p.Faction != Faction.Purple).Select(p => p.Faction);

        public static IEnumerable<Territory> ValidCyanTerritories(Game g) => g.TerrorOnPlanet.Values.Distinct();

        public static bool MaySelectLocationForRevivedForces(Game game, Player player, int specialForces) =>
            player.Is(Faction.Yellow) && specialForces >= 1 && player.HasHighThreshold() && Revival.ValidRevivedForceLocations(game, player).Any();

        public int DeterminePurpleCost()
        {
            return DeterminePurpleCost(PurpleForces, PurpleSpecialForces);
        }

        public static int DeterminePurpleCost(int Forces, int SpecialForces)
        {
            return (Forces + SpecialForces);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (IsCunning)
            {
                HandleCunning();
            }
            else if (IsSecretAlly)
            {
                HandleSecretAlly();
            }
            else
            {
                HandleBetrayal();
            }
        }

        private void HandleBetrayal()
        {
            switch (Faction)
            {
                case Faction.Green:
                    Game.PlayNexusCard(Player);
                    Game.Prevent(Initiator, FactionAdvantage.GreenBattlePlanPrescience);
                    Game.CurrentPrescience = null;
                    break;

                case Faction.Black:
                    var traitor = Game.CurrentBattle.PlanOfOpponent(Faction.Black).Hero;
                    GetPlayer(Faction.Black).Traitors.Remove(traitor);
                    Game.TraitorDeck.Items.Add(traitor);
                    Game.TraitorDeck.Shuffle();
                    Game.Stone(Milestone.Shuffled);
                    Game.BlackTraitorWasCancelled = true;
                    Game.PlayNexusCard(Player, "cancel the ", Faction.Black, " traitor call");
                    Game.Enter(Phase.CallTraitorOrPass);
                    Game.HandleRevealedBattlePlans();
                    break;

                case Faction.Yellow:
                    Game.PlayNexusCard(Player);
                    if (Game.CurrentMainPhase == MainPhase.Blow)
                    {
                        Game.Prevent(Initiator, FactionAdvantage.YellowRidesMonster);
                    }
                    else if (Game.CurrentMainPhase == MainPhase.ShipmentAndMove)
                    {
                        Game.Prevent(Initiator, FactionAdvantage.YellowExtraMove);
                    }
                    break;

                case Faction.Red:
                    Game.PlayNexusCard(Player);
                    if (Game.CurrentMainPhase == MainPhase.Bidding)
                    {
                        Game.Prevent(Initiator, FactionAdvantage.RedReceiveBid);
                    }
                    else if (Game.CurrentMainPhase == MainPhase.Battle && Game.Applicable(Rule.RedSpecialForces))
                    {
                        Game.Prevent(Initiator, FactionAdvantage.RedSpecialForceBonus);
                    }
                    break;

                case Faction.Orange:
                    Game.PlayNexusCard(Player);
                    foreach (var p in Game.StoredRecentlyPaid)
                    {
                        object from = p.To == Faction.None ? "the Bank" : p.To;
                        Log(Player, "get ", p.Amount, " from ", from);
                        if (p.To != Faction.None)
                        {
                            var getFrom = GetPlayer(p.To);
                            if (getFrom != null)
                            {
                                getFrom.Resources -= p.Amount;
                            }
                        }

                        Player.Resources += p.Amount;
                    }

                    if (Game.TargetOfBureaucracy == Faction.Orange)
                    {
                        Game.TargetOfBureaucracy = Initiator;
                    }

                    break;

                case Faction.Blue:
                    Game.PlayNexusCard(Player);
                    Game.Prevent(Initiator, FactionAdvantage.BlueUsingVoice);
                    Game.CurrentVoice = null;
                    break;

                case Faction.Grey:
                    Game.PlayNexusCard(Player);
                    if (Game.CurrentPhase < Phase.GreySelectingCard)
                    {
                        Game.Prevent(Initiator, FactionAdvantage.GreySelectingCardsOnAuction);
                    }
                    else if (Game.CurrentPhase > Phase.GreySelectingCard && Game.CurrentPhase < Phase.BiddingReport)
                    {
                        Game.Prevent(Initiator, FactionAdvantage.GreySwappingCard);
                    }
                    break;

                case Faction.Purple:
                    Game.FacedancerWasCancelled = true;
                    Game.PlayNexusCard(Player, "cancel the ", Faction.Purple, " face dancer reveal");
                    Game.FinishBattle();
                    break;

                case Faction.Brown:
                    Game.PlayNexusCard(Player, "force ", Faction.Brown, " to discard one of their treachery cards at random");
                    var victimPlayer = GetPlayer(Faction.Brown);
                    if (victimPlayer.TreacheryCards.Any())
                    {
                        Game.Discard(victimPlayer.TreacheryCards.RandomOrDefault(Game.Random));
                    }
                    else
                    {
                        Log(victimPlayer.Faction, " have no treachery cards to discard");
                    }
                    break;

                case Faction.White:
                    var paymentToWhite = Game.StoredRecentlyPaid.FirstOrDefault(p => p.To == Faction.White);
                    var white = GetPlayer(Faction.White);

                    if (paymentToWhite != null)
                    {
                        var amountReceived = paymentToWhite.Amount - (Game.WasVictimOfBureaucracy == Faction.White ? 2 : 0);
                        Game.PlayNexusCard(Player, "let ", Faction.White, " lose the payment of ", Payment.Of(amountReceived), " they just received");
                        white.Resources -= amountReceived;
                    }
                    else if (white.Has(Game.CardJustWon))
                    {
                        Game.PlayNexusCard(Player, "force ", Faction.White, " to discard the card they just won");
                        Game.Discard(white, Game.CardJustWon);
                    }
                    break;

                case Faction.Pink:
                    var pinksAlly = Game.GetPlayer(Faction.Pink).AlliedPlayer;
                    pinksAlly.ForcesToReserves(PinkTerritory);
                    Game.PlayNexusCard(Player, "return all ", pinksAlly.Faction, " forces in ", PinkTerritory, " to reserves");
                    Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
                    break;

                case Faction.Cyan:
                    var terrorToRemove = Game.TerrorIn(CyanTerritory).RandomOrDefault(Game.Random);
                    Game.TerrorOnPlanet.Remove(terrorToRemove);
                    Game.PlayNexusCard(Player, "remove a terror token from ", CyanTerritory);
                    break;

            }
        }

        private void HandleCunning()
        {
            switch (Faction)
            {
                case Faction.Green:
                    Game.CurrentNexusPrescience = this;
                    Game.PlayNexusCard(Player, "see their opponent's ", GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    Game.PhaseBeforeDiscardingTraitor = Game.CurrentPhase;
                    Game.FactionThatMustDiscardTraitor = Initiator;
                    Game.NumberOfTraitorsToDiscard = 1;
                    Game.PlayNexusCard(Player, "draw a new traitor");
                    Player.Traitors.Add(Game.TraitorDeck.Draw());
                    Game.Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Red:
                    Game.CurrentRedCunning = this;
                    Game.PlayNexusCard(Player, "let 5 ", FactionForce.Red, " count as ", FactionSpecialForce.Red, " during this battle");
                    break;

                case Faction.Orange:
                    Game.CurrentOrangeCunning = this;
                    Game.PlayNexusCard(Player, "perform an extra shipment after their move");
                    break;

                case Faction.Blue:
                    Game.CurrentBlueCunning = this;
                    Game.PlayNexusCard(Player, "be able to flip advisor to fighters during ", MainPhase.ShipmentAndMove);
                    break;

                case Faction.Grey:
                    Game.CurrentGreyCunning = this;
                    Game.PlayNexusCard(Player, "let ", FactionForce.Grey, " be full strength during this battle");
                    break;

                case Faction.Purple:
                    var purple = GetPlayer(Faction.Purple);
                    Game.PlayNexusCard(Player, "replace their ", purple.RevealedDancers.Count, " revealed face dancers");
                    if (purple.RevealedDancers.Count > 0)
                    {
                        for (int i = 0; i < purple.RevealedDancers.Count; i++)
                        {
                            purple.FaceDancers.Add(Game.TraitorDeck.Draw());
                        }

                        Game.TraitorDeck.Items.AddRange(purple.RevealedDancers);
                        Game.TraitorDeck.Shuffle();

                        foreach (var dancer in purple.RevealedDancers)
                        {
                            purple.FaceDancers.Remove(dancer);
                        }
                        purple.RevealedDancers.Clear();

                        Game.Stone(Milestone.Shuffled);
                    }
                    break;

                case Faction.Pink:
                    if (!Game.IsAlive(Game.Vidal))
                    {
                        Game.Revive(Player, Game.Vidal);

                        if (PurpleAssignSkill)
                        {
                            Game.PrepareSkillAssignmentToRevivedLeader(Player, Game.Vidal);
                        }
                    }
                    Game.TakeVidal(Player, VidalMoment.EndOfTurn);
                    Game.PlayNexusCard(Player, "take ", Game.Vidal, " this turn");
                    break;
            }
        }

        private void HandleSecretAlly()
        {
            switch (Faction)
            {
                case Faction.Green:
                    Game.CurrentNexusPrescience = this;
                    Game.PlayNexusCard(Player, "see their opponent's ", GreenPrescienceAspect);
                    break;

                case Faction.Black:
                    Game.PhaseBeforeDiscardingTraitor = Game.CurrentPhase;
                    Game.FactionThatMustDiscardTraitor = Initiator;
                    Game.NumberOfTraitorsToDiscard = 2;
                    Game.PlayNexusCard(Player, "draw two new traitors");
                    Player.Traitors.Add(Game.TraitorDeck.Draw());
                    Player.Traitors.Add(Game.TraitorDeck.Draw());
                    Game.Enter(Phase.DiscardingTraitor);
                    break;

                case Faction.Yellow:
                    Game.CurrentYellowSecretAlly = this;
                    if (Game.CurrentMainPhase == MainPhase.Blow)
                    {
                        Game.PlayNexusCard(Player, "prevent their forces from being devoured by ", Concept.Monster);
                    }
                    else if (Game.CurrentMainPhase == MainPhase.Resurrection)
                    {
                        Game.PlayNexusCard(Player, "increase their free revival to 3");
                    }
                    break;

                case Faction.Orange:
                    Game.CurrentOrangeSecretAlly = this;
                    Game.PlayNexusCard(Player, "be able to ship as ", Faction.Orange);
                    break;

                case Faction.Purple:
                    Game.Stone(Milestone.RaiseDead);
                    var player = GetPlayer(Initiator);

                    player.ReviveForces(PurpleForces);
                    player.ReviveSpecialForces(PurpleSpecialForces);

                    if (PurpleSpecialForces > 0)
                    {
                        Game.FactionsThatRevivedSpecialForcesThisTurn.Add(Initiator);
                    }

                    if (PurpleHero != null)
                    {
                        Game.Revive(Player, PurpleHero);

                        if (PurpleAssignSkill)
                        {
                            Game.PrepareSkillAssignmentToRevivedLeader(player, PurpleHero as Leader);
                        }

                    }

                    Game.PlayNexusCard(Player, "revive ",
                        MessagePart.ExpressIf(PurpleHero != null, PurpleHero),
                        MessagePart.ExpressIf(PurpleHero != null && PurpleForces + PurpleSpecialForces > 0, " and "),
                        MessagePart.ExpressIf(PurpleForces > 0, PurpleForces, " ", player.Force),
                        MessagePart.ExpressIf(PurpleForces > 0 && PurpleSpecialForces > 0, " and "),
                        MessagePart.ExpressIf(PurpleSpecialForces > 0, PurpleSpecialForces, " ", player.SpecialForce));

                    if (PurpleLocation != null && Initiator == Faction.Yellow)
                    {
                        player.ShipSpecialForces(PurpleLocation, 1);
                        Log(Initiator, " place ", FactionSpecialForce.Yellow, " in ", PurpleLocation);
                    }

                    break;

                case Faction.Brown:
                    if (Game.CurrentMainPhase == MainPhase.Collection)
                    {
                        Game.PlayNexusCard(Player, "discard a ", TreacheryCardType.Useless, " card to get ", Payment.Of(2));
                        Game.Discard(Player, BrownCard);
                        Player.Resources += 2;
                    }
                    else if (Game.CurrentPhase == Phase.BattleConclusion)
                    {
                        var auditee = Game.CurrentBattle.OpponentOf(Initiator);
                        var recentBattlePlan = Game.CurrentBattle.PlanOf(auditee);
                        var auditableCards = auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);

                        Game.PlayNexusCard(Player, "see a random treachery card in the ", auditee.Faction, " hand");

                        if (auditableCards.Any())
                        {
                            var auditedCard = auditableCards.RandomOrDefault(Game.Random);
                            Game.RegisterKnown(Player, auditedCard);
                            LogTo(Initiator, "You see: ", auditedCard);
                            LogTo(auditee.Faction, "You showed them: ", auditedCard);
                        }
                        else
                        {
                            Log(Game.Auditee.Faction, " don't have cards to audit");
                        }
                    }
                    break;

                case Faction.Pink:
                    Game.PlayNexusCard(Player, "force ", PinkFaction, " to reveal if they have an ", Faction.Pink, " traitor");
                    Log(PinkFaction, " reveal to ", Initiator, " if they have a ", Initiator, " traitor");
                    var hasTraitor = GetPlayer(PinkFaction).Traitors.Any(t => t.Faction == Initiator);
                    LogTo(Initiator, PinkFaction, " reveal that they ", hasTraitor ? "DO" : "DON'T", " have a ", Initiator, " traitor ");
                    LogTo(PinkFaction, " you revealed to them that you ", hasTraitor ? "DO" : "DON'T", " have a ", Initiator, " traitor ");
                    break;

            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " play a Nexus card");
        }

        #endregion Execution
    }
}
