/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public PlayerSequence BattleSequence { get; private set; }
        public BattleInitiated CurrentBattle { get; private set; }
        public Battle AggressorBattleAction { get; private set; }
        public TreacheryCalled AggressorTraitorAction { get; private set; }
        public Battle DefenderBattleAction { get; private set; }
        public TreacheryCalled DefenderTraitorAction { get; private set; }
        public BattleOutcome BattleOutcome { get; private set; }
        public Faction BattleWinner { get; private set; }
        public Faction BattleLoser { get; private set; }
        public int GreySpecialForceLossesToTake { get; private set; }
        public int NrOfBattlesFought { get; private set; } = 0;

        private TriggeredBureaucracy BattleTriggeredBureaucracy { get; set; }

        private void EnterBattlePhase()
        {
            MainPhaseStart(MainPhase.Battle);
            NrOfBattlesFought = 0;
            BattleSequence = new PlayerSequence(this);
            if (KarmaHmsMovesLeft != 2) KarmaHmsMovesLeft = 0;
            ResetBattle();
            Enter(NextPlayerToBattle == null, EnterSpiceCollectionPhase, Version >= 107, Phase.BeginningOfBattle, Phase.BattlePhase);
        }

        public Player NextPlayerToBattle
        {
            get
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    var playerToCheck = BattleSequence.CurrentPlayer;
                    if (Battle.MustFight(this, playerToCheck))
                    {
                        return playerToCheck;
                    }

                    BattleSequence.NextPlayer();
                }

                return null;
            }
        }

        public void HandleEvent(BattleInitiated b)
        {
            CurrentReport = new Report(MainPhase.Battle);
            CurrentBattle = b;
            ChosenHMSAdvantage = StrongholdAdvantage.None;
            BattleOutcome = null;
            NrOfBattlesFought++;
            CurrentReport.Express(b);
            CheckHeroAvailability(b.AggressivePlayer);
            CheckHeroAvailability(b.DefendingPlayer);
            AssignBattleWheels(b.AggressivePlayer, b.DefendingPlayer);
        }

        private void AssignBattleWheels(params Player[] players)
        {
            HasBattleWheel.Clear();
            foreach (var p in players)
            {
                HasBattleWheel.Add(p.Faction);
            }
        }

        private void CheckHeroAvailability(Player p)
        {
            if (!Battle.ValidBattleHeroes(this, p).Any())
            {
                CurrentReport.Express(p.Faction, " have no leaders available for this battle");
            }
        }

        public Voice CurrentVoice { get; private set; } = null;
        public void HandleEvent(Voice e)
        {
            CurrentVoice = e;

            if (CurrentBattle != null)
            {
                var opponent = CurrentBattle.OpponentOf(e.Player);

                if (opponent != null)
                {
                    RevokePlanIfNeeded(opponent.Faction);
                }
            }

            CurrentReport.Express(e);
            RecentMilestones.Add(Milestone.Voice);
        }

        public Prescience CurrentPrescience { get; private set; } = null;
        public void HandleEvent(Prescience e)
        {
            CurrentPrescience = e;
            CurrentReport.Express(e);
            RecentMilestones.Add(Milestone.Prescience);
        }

        public void HandleEvent(Battle b)
        {
            if (b.Initiator == CurrentBattle.Aggressor)
            {
                AggressorBattleAction = b;
            }
            else if (b.Initiator == CurrentBattle.Defender)
            {
                DefenderBattleAction = b;
            }

            if (AggressorBattleAction != null && DefenderBattleAction != null)
            {
                RevealCurrentNoField(AggressorBattleAction.Player, CurrentBattle.Territory);
                RevealCurrentNoField(DefenderBattleAction.Player, CurrentBattle.Territory);

                CurrentReport.Express(AggressorBattleAction.GetBattlePlanMessage());
                CurrentReport.Express(DefenderBattleAction.GetBattlePlanMessage());

                RegisterKnownCards(AggressorBattleAction);
                RegisterKnownCards(DefenderBattleAction);

                PassPurpleTraitorAction();

                Enter(AggressorBattleAction.HasRockMelter || DefenderBattleAction.HasRockMelter, Phase.MeltingRock, Phase.CallTraitorOrPass);
            }
        }

        private void PassPurpleTraitorAction()
        {
            if (CurrentBattle.Aggressor == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
            {
                AggressorTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
            }
            else if (CurrentBattle.Defender == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
            {
                DefenderTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
            }
        }

        private void ActivateSmuggler(Player player, IHero hero, IHero opponentHero, Territory territory)
        {
            if (SkilledAs(hero, LeaderSkill.Smuggler))
            {
                var locationWithResources = territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));
                if (locationWithResources != null)
                {
                    int collected = Math.Min(ResourcesOnPlanet[locationWithResources], hero.ValueInCombatAgainst(opponentHero));
                    if (collected > 0)
                    {
                        CurrentReport.Express(player.Faction, LeaderSkill.Smuggler, " collects ", Payment(collected), " from ", territory);
                        ChangeResourcesOnPlanet(locationWithResources, -collected);
                        player.Resources += collected;
                    }
                }
            }
        }

        private void RegisterKnownCards(Battle battle)
        {
            RegisterKnown(battle.Weapon);
            RegisterKnown(battle.Defense);
        }

        private void DiscardOneTimeCardsUsedInBattle(TreacheryCalled aggressorCall, TreacheryCalled defenderCall)
        {
            bool aggressorKeepsCards = aggressorCall.TraitorCalled && !defenderCall.TraitorCalled;
            if (!aggressorKeepsCards)
            {
                DiscardOneTimeCards(AggressorBattleAction);
            }

            bool defenderKeepsCards = defenderCall.TraitorCalled && !aggressorCall.TraitorCalled;
            if (!defenderKeepsCards)
            {
                DiscardOneTimeCards(DefenderBattleAction);
            }
        }

        private void DiscardOneTimeCards(Battle plan)
        {
            if (plan.Hero != null && plan.Hero is TreacheryCard)
            {
                Discard(plan.Hero as TreacheryCard);
            }

            if (plan.Weapon != null && !(plan.Weapon.IsWeapon || plan.Weapon.IsDefense || plan.Weapon.IsUseless))
            {
                Discard(plan.Weapon);
            }
            else if (CurrentDiplomacy != null && plan.Initiator == CurrentDiplomacy.Initiator && plan.Weapon == CurrentDiplomacy.Card)
            {
                Discard(plan.Weapon);
            }
            else if (plan.Weapon != null && (
                plan.Weapon.IsArtillery ||
                plan.Weapon.IsMirrorWeapon ||
                plan.Weapon.IsRockmelter ||
                plan.Weapon.IsPoisonTooth && !PoisonToothCancelled))
            {
                Discard(plan.Weapon);
            }

            if (plan.Defense != null && plan.Defense.IsPortableAntidote)
            {
                Discard(plan.Defense);
            }
            else if (CurrentDiplomacy != null && plan.Initiator == CurrentDiplomacy.Initiator && plan.Defense == CurrentDiplomacy.Card)
            {
                Discard(plan.Defense);
            }

            if (CurrentPortableAntidoteUsed != null)
            {
                Discard(CurrentPortableAntidoteUsed.Player.Card(TreacheryCardType.PortableAntidote));
            }
        }

        public void HandleEvent(BattleRevision e)
        {
            if (CurrentBattle != null)
            {
                if (e.By(CurrentBattle.Aggressor))
                {
                    AggressorBattleAction = null;
                }
                else if (e.By(CurrentBattle.Defender))
                {
                    DefenderBattleAction = null;
                }
            }
        }

        private bool PoisonToothCancelled { get; set; } = false;
        public void HandleEvent(PoisonToothCancelled e)
        {
            PoisonToothCancelled = true;
            CurrentReport.Express(e);
        }

        public PortableAntidoteUsed CurrentPortableAntidoteUsed { get; private set; }
        public void HandleEvent(PortableAntidoteUsed e)
        {
            CurrentReport.Express(e);
            CurrentPortableAntidoteUsed = e;
        }

        public Diplomacy CurrentDiplomacy { get; private set; }
        public void HandleEvent(Diplomacy e)
        {
            CurrentReport.Express(e.GetDynamicMessage());
            CurrentDiplomacy = e;
        }

        public void HandleEvent(ResidualPlayed e)
        {
            Discard(e.Player, TreacheryCardType.Residual);

            var opponent = CurrentBattle.OpponentOf(e.Initiator);
            var leadersToKill = new Deck<IHero>(opponent.Leaders.Where(l => LeaderState[l].Alive && CanJoinCurrentBattle(l)), Random);
            leadersToKill.Shuffle();

            if (!leadersToKill.IsEmpty)
            {
                var toKill = leadersToKill.Draw();
                var opponentPlan = CurrentBattle.PlanOf(opponent);
                if (opponentPlan != null && opponentPlan.Hero == toKill)
                {
                    RevokePlan(opponentPlan);
                }

                KillHero(toKill);
                CurrentReport.Express(TreacheryCardType.Residual, " kills ", toKill);
            }
            else
            {
                CurrentReport.Express(opponent.Faction, " have no available leaders to kill");
            }
        }

        public IList<TreacheryCard> AuditedCards;
        public void HandleEvent(TreacheryCalled e)
        {
            if (AggressorBattleAction.By(e.Initiator) || e.By(Faction.Black) && AreAllies(AggressorBattleAction.Initiator, Faction.Black))
            {
                AggressorTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Express(e);
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(DefenderBattleAction.Hero);
                }
            }

            if (DefenderBattleAction.By(e.Initiator) || e.By(Faction.Black) && AreAllies(DefenderBattleAction.Initiator, Faction.Black))
            {
                DefenderTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Express(e);
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(AggressorBattleAction.Hero);
                }
            }

            if (AggressorTraitorAction != null && DefenderTraitorAction != null)
            {
                HandleRevealedBattlePlans();
            }
        }

        private RockWasMelted CurrentRockWasMelted { get; set; }
        public void HandleEvent(RockWasMelted e)
        {
            CurrentReport.Express(e);
            Discard(e.Player, TreacheryCardType.Rockmelter);
            CurrentRockWasMelted = e;
            Enter(Phase.CallTraitorOrPass);
        }

        private void HandleRevealedBattlePlans()
        {
            ResolveEffectOfOwnedTueksSietch(AggressorBattleAction);
            ResolveEffectOfOwnedTueksSietch(DefenderBattleAction);

            DiscardOneTimeCardsUsedInBattle(AggressorTraitorAction, DefenderTraitorAction);
            ResolveBattle(CurrentBattle, AggressorBattleAction, DefenderBattleAction, AggressorTraitorAction, DefenderTraitorAction);

            if (AggressorBattleAction.Initiator == BattleWinner) ActivateDeciphererIfApplicable(AggressorBattleAction);
            if (DefenderBattleAction.Initiator == BattleWinner) ActivateDeciphererIfApplicable(DefenderBattleAction);

            if (AggressorBattleAction.Initiator == BattleWinner) ActivateSandmasterIfApplicable(AggressorBattleAction);
            if (DefenderBattleAction.Initiator == BattleWinner) ActivateSandmasterIfApplicable(DefenderBattleAction);

            if (AggressorBattleAction.Initiator == BattleWinner) ResolveEffectOfOwnedSietchTabr(AggressorBattleAction, DefenderBattleAction);
            if (DefenderBattleAction.Initiator == BattleWinner) ResolveEffectOfOwnedSietchTabr(DefenderBattleAction, AggressorBattleAction);

            if (Version < 116) CaptureLeaderIfApplicable();

            FlipBeneGesseritWhenAlone();

            if (BattleTriggeredBureaucracy != null)
            {
                ApplyBureaucracy(BattleTriggeredBureaucracy.PaymentFrom, BattleTriggeredBureaucracy.PaymentTo);
                BattleTriggeredBureaucracy = null;
            }

            if (CurrentPhase != Phase.Retreating)
            {
                DetermineHowToProceedAfterRevealingBattlePlans();
            }
        }

        private void ActivateSandmasterIfApplicable(Battle plan)
        {
            var locationWithResources = CurrentBattle.Territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));

            if (locationWithResources != null && SkilledAs(plan.Hero, LeaderSkill.Sandmaster) && plan.Player.AnyForcesIn(CurrentBattle.Territory) > 0)
            {
                CurrentReport.Express(LeaderSkill.Sandmaster, " adds ", Payment(3), " to ", CurrentBattle.Territory);
                ChangeResourcesOnPlanet(locationWithResources, 3);
            }
        }

        public List<IHero> TraitorsDeciphererCanLookAt { get; private set; } = new List<IHero>();
        public bool DeciphererMayReplaceTraitor { get; private set; } = false;

        private void ActivateDeciphererIfApplicable(Battle plan)
        {
            bool playerIsSkilled = SkilledAs(plan.Player, LeaderSkill.Decipherer);
            bool leaderIsSkilled = SkilledAs(plan.Hero, LeaderSkill.Decipherer);

            if (playerIsSkilled || leaderIsSkilled)
            {
                var traitor = TraitorDeck.Draw();
                TraitorsDeciphererCanLookAt.Add(traitor);
                plan.Player.KnownNonTraitors.Add(traitor);

                traitor = TraitorDeck.Draw();
                TraitorsDeciphererCanLookAt.Add(traitor);
                plan.Player.KnownNonTraitors.Add(traitor);

                DeciphererMayReplaceTraitor = plan.Initiator != Faction.Purple && leaderIsSkilled && BattleConcluded.ValidTraitorsToReplace(this, plan.Player).Any();
            }
        }

        private void FinishDeciphererIfApplicable()
        {
            if (TraitorsDeciphererCanLookAt.Count > 0)
            {
                foreach (var item in TraitorsDeciphererCanLookAt)
                {
                    TraitorDeck.PutOnTop(item);
                }

                TraitorDeck.Shuffle();
                RecentMilestones.Add(Milestone.Shuffled);
                TraitorsDeciphererCanLookAt.Clear();
            }
        }

        private void ResolveEffectOfOwnedTueksSietch(Battle playerPlan)
        {
            if (HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CollectResourcesForUseless, CurrentBattle.Territory))
            {
                CollectTueksSietchBonus(playerPlan.Player, playerPlan.Weapon);
                CollectTueksSietchBonus(playerPlan.Player, playerPlan.Defense);
            }
        }

        private void CollectTueksSietchBonus(Player player, TreacheryCard card)
        {
            if (card != null && card.Type == TreacheryCardType.Useless)
            {
                CurrentReport.Express(Map.TueksSietch, " stronghold advantage: ", player.Faction, " collect ", Payment(2), " for playing ", card);
                player.Resources += 2;
            }
        }

        private void ResolveEffectOfOwnedSietchTabr(Battle playerPlan, Battle opponentPlan)
        {
            if (HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CollectResourcesForDial, CurrentBattle.Territory))
            {
                int collected = (int)Math.Floor(opponentPlan.Dial(this, playerPlan.Initiator));
                if (collected > 0)
                {
                    CurrentReport.Express(Map.SietchTabr, " stronghold advantage: ", playerPlan.Initiator, " collect ", Payment(collected), " from enemy force dial");
                    playerPlan.Player.Resources += collected;
                }
            }
        }

        private void DetermineHowToProceedAfterRevealingBattlePlans()
        {
            if (Auditee != null && !BrownLeaderWasRevealedAsTraitor)
            {
                PrepareAudit();
            }
            else
            {
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        private void PrepareAudit()
        {
            var auditableCards = new Deck<TreacheryCard>(AuditCancelled.GetCardsThatMayBeAudited(this), Random);

            if (auditableCards.Items.Count > 0)
            {
                var nrOfAuditedCards = AuditCancelled.GetNumberOfCardsThatMayBeAudited(this);
                AuditedCards = new List<TreacheryCard>();
                auditableCards.Shuffle();
                for (int i = 0; i < nrOfAuditedCards; i++)
                {
                    AuditedCards.Add(auditableCards.Draw());
                }

                Enter(Phase.AvoidingAudit);
            }
            else
            {
                CurrentReport.Express(Auditee.Faction, " don't have cards to audit");
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        private bool BrownLeaderWasRevealedAsTraitor
        {
            get
            {
                var brown = GetPlayer(Faction.Brown);
                if (brown != null && CurrentBattle.IsAggressorOrDefender(brown))
                {
                    return CurrentBattle.TreacheryOfOpponent(brown).TraitorCalled;
                }
                return false;
            }
        }


        private bool BlackMustDecideToCapture => Version >= 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders) && !Prevented(FactionAdvantage.BlackCaptureLeader);

        public void HandleEvent(Captured e)
        {
            CurrentReport.Express(e);
            if (!e.Passed)
            {
                if (Version > 125 && Prevented(FactionAdvantage.BlackCaptureLeader))
                {
                    LogPrevention(FactionAdvantage.BlackCaptureLeader);
                }
                else
                {
                    CaptureLeader();
                }
            }

            Enter(Phase.BattleConclusion);
        }

        public Player Auditee
        {
            get
            {
                if (Applicable(Rule.BrownAuditor) && !Prevented(FactionAdvantage.BrownAudit))
                {
                    if (AggressorBattleAction.Hero != null && AggressorBattleAction.Hero.HeroType == HeroType.Auditor)
                    {
                        return DefenderBattleAction.Player;
                    }
                    else if (DefenderBattleAction.Hero != null && DefenderBattleAction.Hero.HeroType == HeroType.Auditor)
                    {
                        return AggressorBattleAction.Player;
                    }
                }

                return null;
            }
        }

        public void HandleEvent(AuditCancelled e)
        {
            CurrentReport.Express(e.GetDynamicMessage());

            if (e.Cancelled)
            {
                e.Player.Resources -= e.Cost();
                GetPlayer(Faction.Brown).Resources += e.Cost();
            }

            if (!e.Cancelled)
            {
                Enter(Phase.Auditing);
                CurrentReport.ExpressTo(e.Initiator, Faction.Brown, " see: ", AuditedCards);
            }
            else
            {
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        public void HandleEvent(Audited e)
        {
            CurrentReport.Express(e);

            foreach (var card in AuditedCards)
            {
                RegisterKnown(e.Player, card);
            }

            Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
        }

        private void CaptureLeaderIfApplicable()
        {
            if (Version < 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders))
            {
                if (!Prevented(FactionAdvantage.BlackCaptureLeader))
                {
                    CaptureLeader();
                }
                else
                {
                    LogPrevention(FactionAdvantage.BlackCaptureLeader);
                }
            }
        }

        private void CaptureLeader()
        {
            if (AggressorBattleAction.By(BattleWinner))
            {
                SelectVictimOfBlackWinner(AggressorBattleAction, DefenderBattleAction);
            }
            else
            {
                SelectVictimOfBlackWinner(DefenderBattleAction, AggressorBattleAction);
            }
        }

        public void HandleEvent(BattleConcluded e)
        {
            var winner = GetPlayer(e.Initiator);

            foreach (var c in e.DiscardedCards)
            {
                CurrentReport.Express(e.Initiator, " discard ", c);
                winner.TreacheryCards.Remove(c);
                TreacheryDiscardPile.PutOnTop(c);
            }

            if (TraitorsDeciphererCanLookAt.Count > 0)
            {
                CurrentReport.Express(e.Initiator, " look at ", TraitorsDeciphererCanLookAt.Count, " leaders in the traitor deck");
            }

            if (e.ReplacedTraitor != null && e.NewTraitor != null)
            {
                DeciphererReplacesTraitors(e);
            }

            DecideFateOfCapturedLeader(e);
            TakeTechToken(e, winner);
            ProcessGreyForceLossesAndSubstitutions(e, winner);
            Enter(IsPlaying(Faction.Purple) && BattleWinner != Faction.Purple, Phase.Facedancing, FinishBattle);
        }

        private void DeciphererReplacesTraitors(BattleConcluded e)
        {
            CurrentReport.Express(e.Initiator, " replaced ", e.ReplacedTraitor, " by another traitor from the deck");

            e.Player.Traitors.Add(e.NewTraitor);
            TraitorsDeciphererCanLookAt.Remove(e.NewTraitor);

            e.Player.Traitors.Remove(e.ReplacedTraitor);
            TraitorDeck.PutOnTop(e.ReplacedTraitor);

            RecentMilestones.Add(Milestone.Shuffled);
            TraitorDeck.Shuffle();
        }

        public void HandleEvent(FaceDanced f)
        {
            if (f.FaceDancerCalled)
            {
                var initiator = GetPlayer(f.Initiator);
                var facedancer = initiator.FaceDancers.FirstOrDefault(f => WinnerHero.IsFaceDancer(f));
                CurrentReport.Express(f.Initiator, " reveal ", facedancer, " as one of their Face Dancers!");

                RecentMilestones.Add(Milestone.FaceDanced);

                if (facedancer is Leader && IsAlive(facedancer))
                {
                    KillHero(facedancer);
                }

                if (BattleWinner == Faction.Black)
                {
                    ReturnCapturedLeaders(GetPlayer(BattleWinner), facedancer);
                }

                foreach (var p in Players)
                {
                    if (!p.KnownNonTraitors.Contains(facedancer)) p.KnownNonTraitors.Add(facedancer);
                }

                if (!initiator.RevealedDancers.Contains(facedancer))
                {
                    initiator.RevealedDancers.Add(facedancer);
                }

                if (!initiator.HasUnrevealedFaceDancers)
                {
                    ReplaceFacedancers(f, initiator);
                }

                ReplaceForces(f, initiator);

                FlipBeneGesseritWhenAlone();
            }
            else
            {
                CurrentReport.Express(f.Initiator, " don't reveal a Face Dancer");
            }

            FinishBattle();
        }

        private void ReplaceForces(FaceDanced f, Player initiator)
        {
            var winner = GetPlayer(BattleWinner);
            int nrOfRemovedForces = winner.AnyForcesIn(CurrentBattle.Territory);

            if (nrOfRemovedForces > 0)
            {
                winner.ForcesToReserves(CurrentBattle.Territory);

                initiator.ForcesInReserve -= f.ForcesFromReserve;
                foreach (var fl in f.ForceLocations)
                {
                    var location = fl.Key;
                    initiator.ChangeForces(location, -fl.Value.AmountOfForces);
                    initiator.ChangeSpecialForces(location, -fl.Value.AmountOfSpecialForces);
                }

                foreach (var fl in f.TargetForceLocations)
                {
                    var location = fl.Key;
                    initiator.ChangeForces(location, fl.Value.AmountOfForces);
                    initiator.ChangeSpecialForces(location, fl.Value.AmountOfSpecialForces);
                }

                CurrentReport.Express(nrOfRemovedForces, " ", winner.Faction, " forces go back to reserves and are replaced by ", f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Player.Force, " (", f.ForcesFromReserve, " from reserves", DetermineSourceLocations(f), ")");
            }
        }

        private void ReplaceFacedancers(FaceDanced f, Player purple)
        {
            TraitorDeck.Items.AddRange(purple.FaceDancers);
            purple.FaceDancers.Clear();
            purple.RevealedDancers.Clear();
            TraitorDeck.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            for (int i = 0; i < 3; i++)
            {
                purple.FaceDancers.Add(TraitorDeck.Draw());
            }
            CurrentReport.Express(f.Initiator, " draw 3 new Face Dancers.");
        }

        private MessagePart DetermineSourceLocations(FaceDanced f)
        {
            return MessagePart.ExpressIf(f.ForceLocations.Count > 0, f.ForceLocations.Select(fl => MessagePart.Express(", ", fl.Value.AmountOfForces, " from ", fl.Key)));
        }

        public IHero WinnerHero
        {
            get
            {
                if (BattleWinner != Faction.None)
                {
                    var winnerGambit = BattleWinner == AggressorBattleAction.Initiator ? AggressorBattleAction : DefenderBattleAction;
                    return winnerGambit.Hero;
                }

                return null;
            }
        }

        private void FinishBattle()
        {
            GreenKarma = false;
            PutSkilledLeadersInFrontOfShield();
            if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
            if (CurrentJuice != null && CurrentJuice.Type == JuiceType.Aggressor) CurrentJuice = null;
            CurrentDiplomacy = null;
            CurrentRockWasMelted = null;
            CurrentPortableAntidoteUsed = null;
            FinishDeciphererIfApplicable();
            if (NextPlayerToBattle == null) MainPhaseEnd();
            Enter(Phase.BattleReport);
        }

        private void PutSkilledLeadersInFrontOfShield()
        {
            foreach (var ls in LeaderState)
            {
                if (ls.Key is Leader l && Skilled(l) && !CapturedLeaders.ContainsKey(l) && !ls.Value.InFrontOfShield)
                {
                    CurrentReport.Express(Skill(l), " ", l, " is placed back in front of shield");
                    ls.Value.InFrontOfShield = true;
                }
            }
        }

        private void AllowPreventedBattleFactionAdvantages()
        {
            Allow(FactionAdvantage.GreenUseMessiah);
            Allow(FactionAdvantage.GreenBattlePlanPrescience);
            Allow(FactionAdvantage.BlueUsingVoice);
            Allow(FactionAdvantage.YellowSpecialForceBonus);
            Allow(FactionAdvantage.YellowNotPayingForBattles);
            Allow(FactionAdvantage.RedSpecialForceBonus);
            Allow(FactionAdvantage.GreySpecialForceBonus);
            Allow(FactionAdvantage.GreyReplacingSpecialForces);
            Allow(FactionAdvantage.BlackCallTraitorForAlly);
            Allow(FactionAdvantage.BlackCaptureLeader);
            Allow(FactionAdvantage.BrownReceiveForcePayment);
        }

        private void ProcessGreyForceLossesAndSubstitutions(BattleConcluded e, Player winner)
        {
            if (GreySpecialForceLossesToTake > 0)
            {
                var plan = CurrentBattle.PlanOf(winner);
                var territory = CurrentBattle.Territory;

                var winnerGambit = WinnerBattleAction;
                int forcesToLose = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength + e.SpecialForceLossesReplaced;
                int specialForcesToLose = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength - e.SpecialForceLossesReplaced;

                CurrentReport.Express(winner.Faction, " substitute ", e.SpecialForceLossesReplaced, winner.SpecialForce, " losses by ", winner.Force, " losses");

                int specialForcesToSaveToReserves = 0;
                int forcesToSaveToReserves = 0;
                int specialForcesToSaveInTerritory = 0;
                int forcesToSaveInTerritory = 0;

                if (SkilledAs(plan.Hero, LeaderSkill.Graduate))
                {
                    specialForcesToSaveInTerritory = Math.Min(specialForcesToLose, 1);
                    forcesToSaveInTerritory = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveInTerritory));

                    specialForcesToSaveToReserves = Math.Max(0, Math.Min(specialForcesToLose - specialForcesToSaveInTerritory - forcesToSaveInTerritory, 2));
                    forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose - forcesToSaveInTerritory, 2 - specialForcesToSaveToReserves));
                }
                else if (SkilledAs(winner, LeaderSkill.Graduate))
                {
                    specialForcesToSaveToReserves = Math.Min(specialForcesToLose, 1);
                    forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveToReserves));
                }

                if (specialForcesToSaveInTerritory + forcesToSaveInTerritory + specialForcesToSaveToReserves + forcesToSaveToReserves > 0)
                {
                    if (specialForcesToSaveToReserves > 0) winner.ForcesToReserves(territory, specialForcesToSaveToReserves, true);

                    if (forcesToSaveToReserves > 0) winner.ForcesToReserves(territory, forcesToSaveToReserves, false);

                    CurrentReport.Express(
                        LeaderSkill.Graduate,
                        " saves ",
                        MessagePart.ExpressIf(forcesToSaveInTerritory > 0, forcesToSaveInTerritory, winner.Force),
                        MessagePart.ExpressIf(specialForcesToSaveInTerritory > 0, specialForcesToSaveInTerritory, winner.SpecialForce),
                        MessagePart.ExpressIf(forcesToSaveInTerritory > 0 || specialForcesToSaveInTerritory > 0, " in ", territory),

                        MessagePart.ExpressIf(forcesToSaveInTerritory > 0 || specialForcesToSaveInTerritory > 0 && forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " and "),
                        MessagePart.ExpressIf(forcesToSaveToReserves > 0, forcesToSaveToReserves, winner.Force),
                        MessagePart.ExpressIf(specialForcesToSaveToReserves > 0, specialForcesToSaveToReserves, winner.SpecialForce),
                        MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " to reserves"));
                }

                HandleLoserLosses(territory, winner,
                    forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory,
                    specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory);
            }
        }

        private void TakeTechToken(BattleConcluded e, Player winner)
        {
            if (e.StolenToken != TechToken.None)
            {
                var loser = GetPlayer(BattleLoser);
                if (loser.TechTokens.Contains(e.StolenToken))
                {
                    loser.TechTokens.Remove(e.StolenToken);
                    winner.TechTokens.Add(e.StolenToken);
                    CurrentReport.Express(e.Initiator, " steal ", e.StolenToken, " from ", BattleLoser);
                }
            }
        }

        private void DecideFateOfCapturedLeader(BattleConcluded e)
        {
            if (e.By(Faction.Black) && Applicable(Rule.BlackCapturesOrKillsLeaders) && BlackVictim != null)
            {
                if (Version > 125 && Prevented(FactionAdvantage.BlackCaptureLeader))
                {
                    LogPrevention(FactionAdvantage.BlackCaptureLeader);
                }
                else
                {
                    if (e.Initiator == CurrentBattle.Aggressor)
                    {
                        CaptureOrAssassinateLeader(AggressorBattleAction, DefenderBattleAction, e.DecisionToCapture);
                    }
                    else
                    {
                        CaptureOrAssassinateLeader(DefenderBattleAction, AggressorBattleAction, e.DecisionToCapture);
                    }
                }
            }
        }

        private void ResetBattle()
        {
            CurrentBattle = null;
            CurrentPrescience = null;
            CurrentThought = null;
            CurrentVoice = null;
            BlackVictim = null;
            AggressorBattleAction = null;
            DefenderBattleAction = null;
            AggressorTraitorAction = null;
            DefenderTraitorAction = null;
            PoisonToothCancelled = false;
            GreySpecialForceLossesToTake = 0;
            BattleWinner = Faction.None;
            BattleLoser = Faction.None;
            HasActedOrPassed.Clear();
            BattleTriggeredBureaucracy = null;
        }

        private void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
        {
            var aggressor = GetPlayer(agg.Initiator);
            var defender = GetPlayer(def.Initiator);

            var outcome = DetermineBattleOutcome(agg, def, b.Territory);

            bool lasgunShield = !aggtrt.TraitorCalled && !deftrt.TraitorCalled && (agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield);
            bool aggHeroSurvives = !deftrt.TraitorCalled && (aggtrt.TraitorCalled || !lasgunShield && !outcome.AggHeroKilled);
            bool defHeroSurvives = !aggtrt.TraitorCalled && (deftrt.TraitorCalled || !lasgunShield && !outcome.DefHeroKilled);

            if (aggHeroSurvives)
            {
                ActivateSmuggler(AggressorBattleAction.Player, AggressorBattleAction.Hero, DefenderBattleAction.Hero, CurrentBattle.Territory);
            }

            if (defHeroSurvives)
            {
                ActivateSmuggler(DefenderBattleAction.Player, DefenderBattleAction.Hero, AggressorBattleAction.Hero, CurrentBattle.Territory);
            }

            if (aggtrt.TraitorCalled || deftrt.TraitorCalled)
            {
                TraitorCalled(b, agg, def, deftrt, aggressor, defender, agg.Hero, def.Hero);
            }
            else if (lasgunShield)
            {
                LasgunShieldExplosion(agg, def, aggressor, defender, b.Territory, agg.Hero, def.Hero);
            }
            else
            {
                SetHeroLocations(agg, b.Territory);
                SetHeroLocations(def, b.Territory);

                DetermineAndHandleBattleOutcome(agg, def, b.Territory);
            }

            if (aggressor.Is(Faction.Black))
            {
                ReturnCapturedLeaders(aggressor, agg.Hero);
            }
            else if (defender.Is(Faction.Black))
            {
                ReturnCapturedLeaders(defender, def.Hero);
            }
        }

        private void TraitorCalled(BattleInitiated b, Battle agg, Battle def, TreacheryCalled deftrt, Player aggressor, Player defender, IHero aggLeader, IHero defLeader)
        {
            if (AggressorTraitorAction.TraitorCalled && deftrt.TraitorCalled)
            {
                TwoTraitorsCalled(agg, def, aggressor, defender, b.Territory, aggLeader, defLeader);
            }
            else
            {
                var winner = AggressorTraitorAction.TraitorCalled ? aggressor : defender;
                var loser = AggressorTraitorAction.TraitorCalled ? defender : aggressor;
                var loserGambit = AggressorTraitorAction.TraitorCalled ? def : agg;
                var winnerGambit = AggressorTraitorAction.TraitorCalled ? agg : def;
                OneTraitorCalled(b.Territory, winner, loser, loserGambit, winnerGambit);
            }
        }

        private void ReturnCapturedLeaders(Player harkonnen, IHero hero)
        {
            if (harkonnen != null)
            {
                //Captured leader used in battle
                if (hero != null && hero is Leader capturedLeader && harkonnen.Leaders.Contains(capturedLeader) && CapturedLeaders.ContainsKey(capturedLeader))
                {
                    ReturnLeader(harkonnen, capturedLeader);
                }

                if (!harkonnen.Leaders.Any(l => CapturedLeaders.ContainsKey(l) && IsAlive(l)))
                {
                    Leader toReturn;
                    while ((toReturn = harkonnen.Leaders.FirstOrDefault(c => c.Faction != Faction.Black)) != null)
                    {
                        ReturnLeader(harkonnen, toReturn);
                    }
                }
            }
        }

        private void ReturnLeader(Player currentOwner, Leader toReturn)
        {
            if (CapturedLeaders.ContainsKey(toReturn))
            {
                Player originalPlayer = GetPlayer(CapturedLeaders[toReturn]);
                originalPlayer.Leaders.Add(toReturn);
                currentOwner.Leaders.Remove(toReturn);
                CapturedLeaders.Remove(toReturn);
                if (Skilled(toReturn))
                {
                    SetInFrontOfShield(toReturn, true);
                }
                CurrentReport.Express(toReturn, " returns to ", originalPlayer.Faction, " after working for ", currentOwner.Faction);
            }
        }

        public BattleOutcome DetermineBattleOutcome(Battle agg, Battle def, Territory territory)
        {
            var result = new BattleOutcome
            {
                Aggressor = agg.Player,
                Defender = def.Player
            };

            //Determine result

            agg.ActivateDynamicWeapons(def.Weapon, def.Defense);
            def.ActivateDynamicWeapons(agg.Weapon, agg.Defense);

            bool poisonToothUsed = !PoisonToothCancelled && (agg.HasPoisonTooth || def.HasPoisonTooth);
            bool artilleryUsed = agg.HasArtillery || def.HasArtillery;
            bool rockMelterUsed = agg.HasRockMelter || def.HasRockMelter;
            bool rockMelterUsedToKill = CurrentRockWasMelted != null && CurrentRockWasMelted.Kill;

            result.AggHeroKilled = false;
            result.AggHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(
                agg, def, agg.Hero, poisonToothUsed, artilleryUsed, rockMelterUsed && rockMelterUsedToKill, territory,
                ref result.AggHeroKilled, ref result.AggHeroCauseOfDeath, ref result.AggSavedByCarthag);

            result.DefHeroKilled = false;
            result.DefHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(
                def, agg, def.Hero, poisonToothUsed, artilleryUsed, rockMelterUsed && rockMelterUsedToKill, territory,
                ref result.DefHeroKilled, ref result.DefHeroCauseOfDeath, ref result.DefSavedByCarthag);

            int aggHeroSkillBonus = Battle.DetermineSkillBonus(this, agg, out result.AggActivatedBonusSkill);
            result.AggHeroEffectiveStrength = (agg.Hero != null && !artilleryUsed) ? agg.Hero.ValueInCombatAgainst(def.Hero) : 0;
            int aggHeroContribution = !result.AggHeroKilled && !rockMelterUsed ? result.AggHeroEffectiveStrength + aggHeroSkillBonus : 0;

            int defHeroSkillBonus = Battle.DetermineSkillBonus(this, def, out result.DefActivatedBonusSkill);
            result.DefHeroEffectiveStrength = (def.Hero != null && !artilleryUsed) ? def.Hero.ValueInCombatAgainst(agg.Hero) : 0;
            int defHeroContribution = !result.DefHeroKilled && !rockMelterUsed ? result.DefHeroEffectiveStrength + defHeroSkillBonus : 0;

            int aggSkillPenalty = Battle.DetermineSkillPenalty(this, def, result.Aggressor, out result.DefActivatedPenaltySkill);
            result.AggBattlePenalty = !result.DefHeroKilled && !rockMelterUsed ? aggSkillPenalty : 0;

            int defSkillPenalty = Battle.DetermineSkillPenalty(this, agg, result.Defender, out result.AggActivatedPenaltySkill);
            result.DefBattlePenalty = !result.AggHeroKilled && !rockMelterUsed ? defSkillPenalty : 0;

            var aggMessiahContribution = result.Aggressor.Is(Faction.Green) && agg.Messiah && agg.Hero != null && !result.AggHeroKilled && !artilleryUsed ? 2 : 0;
            var defMessiahContribution = result.Defender.Is(Faction.Green) && def.Messiah && def.Hero != null && !result.DefHeroKilled && !artilleryUsed ? 2 : 0;

            float aggForceDial;
            float defForceDial;

            if (!rockMelterUsed)
            {
                aggForceDial = agg.Dial(this, result.Defender.Faction);
                defForceDial = def.Dial(this, result.Aggressor.Faction);
            }
            else
            {
                aggForceDial = result.Aggressor.AnyForcesIn(CurrentBattle.Territory) - agg.TotalForces;
                defForceDial = result.Defender.AnyForcesIn(CurrentBattle.Territory) - def.TotalForces;
            }

            result.AggTotal = aggForceDial + aggHeroContribution + aggMessiahContribution - result.AggBattlePenalty;
            result.DefTotal = defForceDial + defHeroContribution + defMessiahContribution - result.DefBattlePenalty;

            agg.DeactivateDynamicWeapons();
            def.DeactivateDynamicWeapons();

            bool aggressorWinsTies = true;
            if (HasStrongholdAdvantage(result.Defender.Faction, StrongholdAdvantage.WinTies, CurrentBattle.Territory))
            {
                aggressorWinsTies = false;
            }

            if (BattleInitiated.IsAggressorByJuice(this, result.Defender.Faction) && !HasStrongholdAdvantage(result.Aggressor.Faction, StrongholdAdvantage.WinTies, CurrentBattle.Territory))
            {
                aggressorWinsTies = false;
            }

            if (aggressorWinsTies)
            {
                result.Winner = (result.AggTotal >= result.DefTotal) ? result.Aggressor : result.Defender;
            }
            else
            {
                result.Winner = (result.DefTotal >= result.AggTotal) ? result.Defender : result.Aggressor;
            }

            result.WinnerBattlePlan = (result.Winner == result.Aggressor) ? agg : def;
            result.Loser = result.Winner == result.Aggressor ? result.Defender : result.Aggressor;
            result.LoserBattlePlan = (result.Loser == result.Aggressor) ? agg : def;

            return result;
        }

        public void DetermineAndHandleBattleOutcome(Battle agg, Battle def, Territory territory)
        {
            BattleOutcome = DetermineBattleOutcome(agg, def, territory);

            CurrentReport.ExpressIf(BattleOutcome.AggHeroSkillBonus != 0, agg.Hero, " ", BattleOutcome.AggActivatedBonusSkill, " bonus: ", BattleOutcome.AggHeroSkillBonus);
            CurrentReport.ExpressIf(BattleOutcome.DefHeroSkillBonus != 0, def.Hero, " ", BattleOutcome.DefActivatedBonusSkill, " bonus: ", BattleOutcome.DefHeroSkillBonus);

            CurrentReport.ExpressIf(BattleOutcome.AggBattlePenalty != 0, agg.Hero, " ", BattleOutcome.DefActivatedPenaltySkill, " penalty: ", BattleOutcome.AggBattlePenalty);
            CurrentReport.ExpressIf(BattleOutcome.DefBattlePenalty != 0, def.Hero, " ", BattleOutcome.AggActivatedPenaltySkill, " penalty: ", BattleOutcome.DefBattlePenalty);

            CurrentReport.ExpressIf(BattleOutcome.AggMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.AggMessiahContribution);
            CurrentReport.ExpressIf(BattleOutcome.DefMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.DefMessiahContribution);

            BattleWinner = BattleOutcome.Winner.Faction;
            BattleLoser = BattleOutcome.Loser.Faction;

            if (BattleOutcome.AggHeroKilled)
            {
                KillLeaderInBattle(agg.Hero, def.Hero, BattleOutcome.AggHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.AggHeroEffectiveStrength);
            }
            else
            {
                CurrentReport.ExpressIf(BattleOutcome.AggSavedByCarthag, Map.Carthag, " stronghold advantage saves ", agg.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (BattleOutcome.DefHeroKilled)
            {
                KillLeaderInBattle(def.Hero, agg.Hero, BattleOutcome.DefHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.DefHeroEffectiveStrength);
            }
            else
            {
                CurrentReport.ExpressIf(BattleOutcome.DefSavedByCarthag, Map.Carthag, " stronghold advantage saves ", def.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (BattleInitiated.IsAggressorByJuice(this, def.Player.Faction))
            {
                CurrentReport.Express(agg.Initiator, " (defending) strength: ", BattleOutcome.AggTotal);
                CurrentReport.Express(def.Initiator, " (aggressor by ", TreacheryCardType.Juice, ") strength: ", BattleOutcome.DefTotal);
            }
            else
            {
                CurrentReport.Express(agg.Initiator, " (aggressor) strength: ", BattleOutcome.AggTotal);
                CurrentReport.Express(def.Initiator, " (defending) strength: ", BattleOutcome.DefTotal);
            }

            CurrentReport.Express(BattleOutcome.Winner.Faction, " WIN THE BATTLE");

            bool loserMayRetreat = 
                !BattleOutcome.LoserHeroKilled &&
                SkilledAs(BattleOutcome.LoserBattlePlan.Hero, LeaderSkill.Diplomat) && 
                (Retreat.MaxForces(this, BattleOutcome.Loser) > 0 || Retreat.MaxSpecialForces(this, BattleOutcome.Loser) > 0) && 
                Retreat.ValidTargets(this, BattleOutcome.Loser).Any();

            Enter(loserMayRetreat, Phase.Retreating, HandleForceLosses);
        }

        private void HandleForceLosses()
        {
            ProcessWinnerLosses(CurrentBattle.Territory, BattleOutcome.Winner, BattleOutcome.WinnerBattlePlan, false);
            ProcessLoserLosses(CurrentBattle.Territory, BattleOutcome.Loser, BattleOutcome.LoserBattlePlan, false);
        }

        private void DetermineCauseOfDeath(Battle playerPlan, Battle opponentPlan, IHero theHero, bool poisonToothUsed, bool artilleryUsed, bool rockMelterWasUsedToKill, Territory battleTerritory, ref bool heroDies, ref TreacheryCardType causeOfDeath, ref bool savedByCarthag)
        {
            heroDies = false;
            causeOfDeath = TreacheryCardType.None;
            bool isProtectedByCarthagAdvantage = HasStrongholdAdvantage(playerPlan.Initiator, StrongholdAdvantage.CountDefensesAsAntidote, battleTerritory) && !playerPlan.HasPoison && !playerPlan.HasPoisonTooth && playerPlan.Defense != null && playerPlan.Defense.IsDefense;
            savedByCarthag = isProtectedByCarthagAdvantage && opponentPlan.HasPoison && !playerPlan.HasAntidote;

            DetermineDeathBy(theHero, TreacheryCardType.Rockmelter, rockMelterWasUsedToKill, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.ArtilleryStrike, artilleryUsed && !playerPlan.HasShield, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.PoisonTooth, poisonToothUsed && !playerPlan.HasNonAntidotePoisonDefense, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Laser, opponentPlan.HasLaser, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Poison, opponentPlan.HasPoison && !(playerPlan.HasAntidote || isProtectedByCarthagAdvantage), ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Projectile, opponentPlan.HasProjectile && !playerPlan.HasProjectileDefense, ref heroDies, ref causeOfDeath);
        }

        private void DetermineDeathBy(IHero hero, TreacheryCardType byWeapon, bool weaponHasEffect, ref bool heroIsKilled, ref TreacheryCardType causeOfDeath)
        {
            if (!heroIsKilled && hero != null && weaponHasEffect)
            {
                heroIsKilled = true;
                causeOfDeath = byWeapon;
            }
        }

        private void ProcessLoserLosses(Territory territory, Player loser, Battle loserGambit, bool traitorWasRevealed)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            CurrentReport.Express(loser.Faction, " lose all ", loser.AnyForcesIn(territory), " forces in ", territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit, traitorWasRevealed);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void PayDialedSpice(Player p, Battle plan, bool traitorWasRevealed)
        {
            int cost = plan.Cost(this);
            int costToBrown = p.Ally == Faction.Brown ? plan.AllyContributionAmount : 0;


            if (cost > 0)
            {
                int costForPlayer = cost - plan.AllyContributionAmount;

                if (costForPlayer > 0)
                {
                    p.Resources -= costForPlayer;
                    CurrentReport.ExpressIf(HasStrongholdAdvantage(p.Faction, StrongholdAdvantage.FreeResourcesForBattles, CurrentBattle.Territory),
                        Map.Arrakeen, " stronghold advantage: supporting forces costs ", Payment(2), " less");
                }

                if (plan.AllyContributionAmount > 0)
                {
                    p.AlliedPlayer.Resources -= plan.AllyContributionAmount;
                    if (Version >= 117) DecreasePermittedUseOfAllySpice(p.Faction, plan.AllyContributionAmount);
                }

                int receiverProfit = HandleBrownIncome(p, cost - costToBrown, traitorWasRevealed);

                if (cost - receiverProfit >= 4)
                {
                    ActivateBanker(p);
                }
            }

            if (plan.BankerBonus > 0)
            {
                p.Resources -= plan.BankerBonus;
                CurrentReport.Express(p.Faction, " paid ", Payment(plan.BankerBonus), " for as ", LeaderSkill.Banker);
            }
        }

        private int HandleBrownIncome(Player paidBy, int costsExcludingPaymentByBrownAlly, bool traitorWasRevealed)
        {
            int result = 0;

            var brown = GetPlayer(Faction.Brown);
            if (brown != null && paidBy.Faction != Faction.Brown && (Version < 126 || !traitorWasRevealed))
            {
                result = (int)Math.Floor(0.5f * costsExcludingPaymentByBrownAlly);

                if (result > 0)
                {
                    if (!Prevented(FactionAdvantage.BrownReceiveForcePayment))
                    {
                        brown.Resources += result;
                        CurrentReport.Express(Faction.Brown, " get ", Payment(result), " from supported forces");

                        if (result >= 5)
                        {
                            BattleTriggeredBureaucracy = new TriggeredBureaucracy() { PaymentFrom = paidBy.Faction, PaymentTo = Faction.Brown };
                        }
                    }
                    else
                    {
                        LogPrevention(FactionAdvantage.BrownReceiveForcePayment);
                    }
                }
            }

            return result;
        }

        private void ProcessWinnerLosses(Territory territory, Player winner, Battle plan, bool traitorWasRevealed)
        {
            PayDialedSpice(winner, plan, traitorWasRevealed);
            ProcessForceLosses(territory, winner, plan);
        }

        private void ProcessForceLosses(Territory territory, Player player, Battle plan)
        {
            int specialForcesToLose = plan.SpecialForces + plan.SpecialForcesAtHalfStrength;
            int forcesToLose = plan.Forces + plan.ForcesAtHalfStrength;

            int specialForcesToSaveToReserves = 0;
            int forcesToSaveToReserves = 0;
            int specialForcesToSaveInTerritory = 0;
            int forcesToSaveInTerritory = 0;

            if (!MaySubstituteForceLosses(player))
            {
                if (SkilledAs(plan.Hero, LeaderSkill.Graduate))
                {
                    specialForcesToSaveInTerritory = Math.Min(specialForcesToLose, 1);
                    forcesToSaveInTerritory = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveInTerritory));

                    specialForcesToSaveToReserves = Math.Max(0, Math.Min(specialForcesToLose - specialForcesToSaveInTerritory - forcesToSaveInTerritory, 2));
                    forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose - forcesToSaveInTerritory, 2 - specialForcesToSaveToReserves));
                }
                else if (SkilledAs(player, LeaderSkill.Graduate))
                {
                    specialForcesToSaveToReserves = Math.Min(specialForcesToLose, 1);
                    forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveToReserves));
                }
            }

            if (specialForcesToSaveInTerritory + forcesToSaveInTerritory + specialForcesToSaveToReserves + forcesToSaveToReserves > 0)
            {
                if (specialForcesToSaveToReserves > 0) player.ForcesToReserves(territory, specialForcesToSaveToReserves, true);

                if (forcesToSaveToReserves > 0) player.ForcesToReserves(territory, forcesToSaveToReserves, false);

                CurrentReport.Express(
                    LeaderSkill.Graduate,
                    " rescues ",
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0, forcesToSaveInTerritory, player.Force),
                    MessagePart.ExpressIf(specialForcesToSaveInTerritory > 0, specialForcesToSaveInTerritory, player.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0 || specialForcesToSaveInTerritory > 0, " on site and "),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0, forcesToSaveToReserves, player.Force),
                    MessagePart.ExpressIf(specialForcesToSaveToReserves > 0, specialForcesToSaveToReserves, player.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " to reserves"));
            }

            if (!MaySubstituteForceLosses(player) || specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory == 0 || player.ForcesIn(territory) <= plan.Forces + plan.ForcesAtHalfStrength)
            {
                int winnerForcesLost = forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory;
                int winnerSpecialForcesLost = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
                HandleLoserLosses(territory, player, winnerForcesLost, winnerSpecialForcesLost);
            }
            else
            {
                GreySpecialForceLossesToTake = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
            }
        }

        private bool MaySubstituteForceLosses(Player p) => p.Faction == Faction.Grey && (Version < 113 || !Prevented(FactionAdvantage.GreyReplacingSpecialForces));

        private void HandleLoserLosses(Territory territory, Player loser, int forcesLost, int specialForcesLost)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            loser.KillForces(territory, forcesLost, false, true);
            loser.KillForces(territory, specialForcesLost, true, true);
            LogLosses(loser, forcesLost, specialForcesLost);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void KillLeaderInBattle(IHero killedHero, IHero opposingHero, TreacheryCardType causeOfDeath, Player winner, int heroValue)
        {
            CurrentReport.Express(causeOfDeath, " kills ", killedHero, " → ", winner.Faction, " collect ", Payment(heroValue));
            RecentMilestones.Add(Milestone.LeaderKilled);
            if (killedHero is Leader) KillHero(killedHero as Leader);
            winner.Resources += heroValue;
        }

        private void LogLosses(Player player, int forcesLost, int specialForcesLost)
        {
            if (forcesLost > 0 || specialForcesLost > 0)
            {
                CurrentReport.Express(
                    player.Faction,
                    " lose ",
                    MessagePart.ExpressIf(forcesLost > 0, forcesLost, player.Force),
                    MessagePart.ExpressIf(specialForcesLost > 0, specialForcesLost, player.SpecialForce),
                    " during battle ");
            }
        }

        private void SetHeroLocations(Battle b, Territory territory)
        {
            if (b.Hero != null && b.Hero is Leader)
            {
                LeaderState[b.Hero].CurrentTerritory = territory;
            }

            if (b.Messiah)
            {
                LeaderState[LeaderManager.Messiah].CurrentTerritory = territory;
            }
        }

        private void OneTraitorCalled(Territory territory, Player winner, Player loser, Battle loserGambit, Battle winnerGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            var traitor = loserGambit.Hero;
            var traitorValue = traitor.ValueInCombatAgainst(winnerGambit.Hero);
            var traitorOwner = winner.Traitors.Any(t => t.IsTraitor(traitor)) ? winner.Faction : Faction.Black;

            CurrentReport.Express(traitor, " is a ", traitorOwner, " traitor! ", loser.Faction, " lose everything");

            RecentMilestones.Add(Milestone.LeaderKilled);

            if (traitor is Leader)
            {
                CurrentReport.Express("Treachery kills ", traitor, ". ", winner.Faction, " collect ", Payment(traitorValue));
                KillHero(traitor);
                winner.Resources += traitorValue;
            }

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            CurrentReport.Express(loser.Faction, " lose all ", loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), " forces in ", territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit, true);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void TwoTraitorsCalled(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            RecentMilestones.Add(Milestone.LeaderKilled);

            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Express("Treachery kills both ", defLeader, " and ", aggLeader);
            KillHero(defLeader);
            KillHero(aggLeader);
            CurrentReport.Express(defender.Faction, " lose all ", defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), " forces in ", territory);
            defender.KillAllForces(territory, true);
            CurrentReport.Express(aggressor.Faction, " lose all ", aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), " forces in ", territory);
            aggressor.KillAllForces(territory, true);

            LoseCards(def);
            PayDialedSpice(defender, def, true);

            LoseCards(agg);
            PayDialedSpice(aggressor, agg, true);

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LasgunShieldExplosion(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Express("A ", TreacheryCardType.Laser, "/", TreacheryCardType.Shield, " explosion occurs!");
            RecentMilestones.Add(Milestone.Explosion);

            if (aggLeader != null)
            {
                CurrentReport.Express("The explosion kills ", aggLeader);
                KillHero(aggLeader);
            }

            if (defLeader != null)
            {
                CurrentReport.Express("The explosion kills ", defLeader);
                KillHero(def.Hero);
            }

            if (agg.Messiah || def.Messiah)
            {
                CurrentReport.Express("The explosion kills the ", Concept.Messiah);
                KillHero(LeaderManager.Messiah);
            }

            LoseCards(agg);
            PayDialedSpice(aggressor, agg, false);

            LoseCards(def);
            PayDialedSpice(defender, def, false);

            int removed = RemoveResources(territory);
            if (removed > 0)
            {
                CurrentReport.Express("The explosion destroys ", Payment(removed), " in ", territory);
            }

            foreach (var p in Players)
            {
                RevealCurrentNoField(p, territory);

                int numberOfForces = p.AnyForcesIn(territory);
                if (numberOfForces > 0)
                {
                    CurrentReport.Express("The explosion kills all ", numberOfForces, p.Faction, " forces in ", territory);
                    p.KillAllForces(territory, true);
                }
            }

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LoseCards(Battle plan)
        {
            Discard(plan.Weapon);
            Discard(plan.Defense);
        }

        public bool CanJoinCurrentBattle(IHero hero)
        {
            var currentTerritory = CurrentTerritory(hero);
            return currentTerritory == null || currentTerritory == CurrentBattle?.Territory;
        }

        public Leader BlackVictim { get; set; }
        private void SelectVictimOfBlackWinner(Battle harkonnenAction, Battle victimAction)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var victim = GetPlayer(victimAction.Initiator);

            //Get all living leaders from the opponent that haven't fought in another territory this turn
            Deck<Leader> availableLeaders = new Deck<Leader>(
                victim.Leaders.Where(l => l.HeroType != HeroType.Auditor && LeaderState[l].Alive && CanJoinCurrentBattle(l)), Random);

            if (!availableLeaders.IsEmpty)
            {
                availableLeaders.Shuffle();
                BlackVictim = availableLeaders.Draw();
            }
            else
            {
                BlackVictim = null;
                CurrentReport.Express(victim.Faction, " don't have any leaders for ", Faction.Black, " to capture or kill");
            }
        }

        public Dictionary<Leader, Faction> CapturedLeaders { get; private set; } = new Dictionary<Leader, Faction>();
        private void CaptureOrAssassinateLeader(Battle harkonnenAction, Battle victimAction, CaptureDecision decision)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var target = GetPlayer(victimAction.Initiator);

            if (decision == CaptureDecision.Capture)
            {
                CurrentReport.Express(Faction.Black, " capture a leader!");
                harkonnen.Leaders.Add(BlackVictim);
                target.Leaders.Remove(BlackVictim);
                SetInFrontOfShield(BlackVictim, false);
                CapturedLeaders.Add(BlackVictim, target.Faction);
            }
            else if (decision == CaptureDecision.Kill)
            {
                CurrentReport.Express(Faction.Black, " kill a leader for ", Payment(2));
                RecentMilestones.Add(Milestone.LeaderKilled);
                AssassinateLeader(BlackVictim);
                harkonnen.Resources += 2;
            }
            else if (decision == CaptureDecision.DontCapture)
            {
                CurrentReport.Express(Faction.Black, " decide not to capture or kill a leader");
            }
        }

        public void HandleEvent(SwitchedSkilledLeader e)
        {
            var leader = e.Player.Leaders.FirstOrDefault(l => Skilled(l) && !CapturedLeaders.ContainsKey(l));
            SwitchInFrontOfShield(leader);
            CurrentReport.Express(e.Initiator, " place ", Skill(leader), " ", leader, IsInFrontOfShield(leader) ? " in front of" : " behind", " their shield");
        }

        public Battle WinnerBattleAction
        {
            get
            {
                if (AggressorBattleAction != null && AggressorBattleAction.Initiator == BattleWinner) return AggressorBattleAction;
                if (DefenderBattleAction != null && DefenderBattleAction.Initiator == BattleWinner) return DefenderBattleAction;

                return null;
            }
        }

        public Thought CurrentThought { get; private set; }
        public void HandleEvent(Thought e)
        {
            CurrentThought = e;
            var opponent = CurrentBattle.OpponentOf(e.Initiator).Faction;
            CurrentReport.Express("By ", LeaderSkill.Thinker, ", ", e.Initiator, " ask ", opponent, " if they have a ", e.Card);

            Enter(Phase.Thought);
        }

        public void HandleEvent(ThoughtAnswered e)
        {
            CurrentReport.Express(e);
            if (e.Card == null)
            {
                CurrentReport.Express(e.Initiator, " don't own any cards", e.Initiator);
            }
            else
            {
                CurrentReport.ExpressTo(CurrentThought.Initiator, "In response, ", e.Initiator, " show you a ", e.Card);
                RegisterKnown(CurrentThought.Initiator, e.Card);
            }

            Enter(Phase.BattlePhase);
        }

        public void HandleEvent(HMSAdvantageChosen e)
        {
            CurrentReport.Express(e);
            ChosenHMSAdvantage = e.Advantage;
        }

        public void HandleEvent(Retreat e)
        {
            int forcesToMove = e.Forces;
            foreach (var l in CurrentBattle.Territory.Locations.Where(l => e.Player.ForcesIn(l) > 0).ToArray())
            {
                if (forcesToMove == 0) break;
                int toMoveFromHere = Math.Min(forcesToMove, e.Player.ForcesIn(l));
                e.Player.MoveForces(l, e.Location, toMoveFromHere);
                forcesToMove -= toMoveFromHere;
            }

            int specialForcesToMove = e.SpecialForces;
            foreach (var l in CurrentBattle.Territory.Locations.Where(l => e.Player.SpecialForcesIn(l) > 0).ToArray())
            {
                if (specialForcesToMove == 0) break;
                int toMoveFromHere = Math.Min(specialForcesToMove, e.Player.SpecialForcesIn(l));
                e.Player.MoveSpecialForces(l, e.Location, toMoveFromHere);
                specialForcesToMove -= toMoveFromHere;
            }

            CurrentReport.Express(e);
            HandleForceLosses();
            FlipBeneGesseritWhenAlone();
            DetermineHowToProceedAfterRevealingBattlePlans();
        }
    }
}
