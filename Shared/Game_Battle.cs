/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        public PlayerSequence BattleSequence { get; internal set; }
        public BattleInitiated CurrentBattle { get; internal set; }
        public Battle AggressorBattleAction { get; internal set; }
        public TreacheryCalled AggressorTraitorAction { get; internal set; }
        public Battle DefenderBattleAction { get; internal set; }
        public TreacheryCalled DefenderTraitorAction { get; internal set; }
        public BattleOutcome BattleOutcome { get; internal set; }
        public Faction BattleWinner { get; internal set; }
        public Faction BattleLoser { get; internal set; }
        public int GreySpecialForceLossesToTake { get; internal set; }

        internal int NrOfBattlesFought { get; set; } = 0;
        internal TriggeredBureaucracy BattleTriggeredBureaucracy { get; set; }

        #endregion State

        #region BeginningOfBattlePhase

        

        #endregion

        #region BattleInitiation

        public BattleInitiated BattleAboutToStart { get; internal set; }

        public Faction CurrentPinkOrAllyFighter { get; internal set; }
        public int CurrentPinkBattleContribution { get; internal set; }

        

        

        internal void InitiateBattle()
        {
            CurrentReport = new Report(MainPhase.Battle);
            CurrentBattle = BattleAboutToStart;
            ChosenHMSAdvantage = StrongholdAdvantage.None;
            BattleOutcome = null;
            NrOfBattlesFought++;

            if (CurrentPinkOrAllyFighter == Faction.None)
            {
                Log(CurrentBattle.Initiator, " initiate battle with ", CurrentBattle.Target, " in ", CurrentBattle.Territory);
            }
            else
            {
                var initiatorIsWithPink = CurrentBattle.Initiator == Faction.Pink || CurrentBattle.Player.Ally == Faction.Pink;
                Log(CurrentBattle.Initiator,
                    MessagePart.ExpressIf(initiatorIsWithPink, CurrentBattle.Player.Ally),
                    " initiate battle with ",
                    CurrentBattle.Target,
                    MessagePart.ExpressIf(!initiatorIsWithPink, GetPlayer(CurrentBattle.Target).Ally),
                    " in ",
                    CurrentBattle.Territory,
                    ", where ",
                    CurrentPinkOrAllyFighter,
                    " will fight for their ally");
            }

            AnnounceHeroAvailability(BattleAboutToStart.AggressivePlayer);
            AnnounceHeroAvailability(BattleAboutToStart.DefendingPlayer);
            AssignBattleWheels(BattleAboutToStart.AggressivePlayer, BattleAboutToStart.DefendingPlayer);
        }

        private void AssignBattleWheels(params Player[] players)
        {
            HasBattleWheel.Clear();
            foreach (var p in players)
            {
                HasBattleWheel.Add(p.Faction);
            }
        }

        private void AnnounceHeroAvailability(Player p)
        {
            if (!Battle.ValidBattleHeroes(this, p).Any())
            {
                Log(p.Faction, " have no leaders available for this battle");
            }
        }

        #endregion

        #region VoiceAndPrescience

        public Voice CurrentVoice { get; internal set; } = null;
        public void HandleEvent(Voice e)
        {
            CurrentVoice = e;

            if (!IsPlaying(Faction.Blue))
            {
                PlayNexusCard(e.Player, "Secret Ally", " to use Voice");
            }

            if (CurrentBattle != null)
            {
                var opponent = CurrentBattle.OpponentOf(e.Player);

                if (opponent != null)
                {
                    RevokePlanIfNeeded(opponent.Faction);
                }
            }

            Log(e);
            Stone(Milestone.Voice);
        }

        public Prescience CurrentPrescience { get; internal set; } = null;
        public void HandleEvent(Prescience e)
        {
            CurrentPrescience = e;
            Log(e);
            Stone(Milestone.Prescience);
        }

        public Thought CurrentThought { get; internal set; }
        public void HandleEvent(Thought e)
        {
            CurrentThought = e;
            var opponent = CurrentBattle.OpponentOf(e.Initiator).Faction;
            Log(e.Initiator, " use their ", LeaderSkill.Thinker, " skill to ask ", opponent, " if they have a ", e.Card);
            Stone(Milestone.Prescience);
            Enter(Phase.Thought);
        }

        public void HandleEvent(ThoughtAnswered e)
        {
            if (e.Card == null)
            {
                Log(e.Initiator, " don't own any cards");
            }
            else
            {
                LogTo(CurrentThought.Initiator, "In response, ", e.Initiator, " show you a ", e.Card);
                RegisterKnown(CurrentThought.Initiator, e.Card);
            }

            Enter(Phase.BattlePhase);
        }

        #endregion

        #region BattlePlan

        public void HandleEvent(SwitchedSkilledLeader e)
        {
            var leader = SwitchedSkilledLeader.SwitchableLeader(this, e.Player);
            SetInFrontOfShield(leader, !IsInFrontOfShield(leader));
            Log(e.Initiator, " place ", Skill(leader), " ", leader, IsInFrontOfShield(leader) ? " in front of" : " behind", " their shield");
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
                Log(TreacheryCardType.Residual, " kills ", toKill);
            }
            else
            {
                Log(opponent.Faction, " have no available leaders to kill");
            }
        }


        public PortableAntidoteUsed CurrentPortableAntidoteUsed { get; private set; }
        public void HandleEvent(PortableAntidoteUsed e)
        {
            Log(e);
            CurrentPortableAntidoteUsed = e;
        }

        internal bool PoisonToothCancelled { get; set; } = false;
        public void HandleEvent(PoisonToothCancelled e)
        {
            PoisonToothCancelled = true;
            Log(e);
        }

        private RockWasMelted CurrentRockWasMelted { get; set; }
        public void HandleEvent(RockWasMelted e)
        {
            Log(e);
            if (Version < 146) Discard(e.Player, TreacheryCardType.Rockmelter);
            CurrentRockWasMelted = e;
            Enter(Phase.CallTraitorOrPass);
        }

        internal void RegisterKnownCards(Battle battle)
        {
            RegisterKnown(battle.Weapon);
            RegisterKnown(battle.Defense);
        }

        #endregion

        #region Treachery

        public void HandleEvent(TreacheryCalled e)
        {
            if (AggressorBattleAction.By(e.Initiator) || e.By(Faction.Black) && AreAllies(AggressorBattleAction.Initiator, Faction.Black))
            {
                AggressorTraitorAction = e;
                if (e.TraitorCalled)
                {
                    Log(e);
                    Stone(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(DefenderBattleAction.Hero);
                }
            }

            if (DefenderBattleAction.By(e.Initiator) || e.By(Faction.Black) && AreAllies(DefenderBattleAction.Initiator, Faction.Black))
            {
                DefenderTraitorAction = e;
                if (e.TraitorCalled)
                {
                    Log(e);
                    Stone(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(AggressorBattleAction.Hero);
                }
            }

            if (AggressorTraitorAction != null && DefenderTraitorAction != null)
            {
                var treachery = CurrentBattle.TreacheryOf(Faction.Black);

                if (Applicable(Rule.NexusCards) && treachery != null && treachery.Initiator == Faction.Black && treachery.TraitorCalled)
                {
                    Enter(Phase.CancellingTraitor);
                }
                else
                {
                    HandleRevealedBattlePlans();
                }
            }
        }



        #endregion

        #region BattleResolution

        internal void HandleRevealedBattlePlans()
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

            if (AggressorBattleAction.Initiator == BattleWinner) ResolveEffectOfOccupiedJacurutu(AggressorBattleAction, BattleOutcome.DefUndialedForces);
            if (DefenderBattleAction.Initiator == BattleWinner) ResolveEffectOfOccupiedJacurutu(DefenderBattleAction, BattleOutcome.AggUndialedForces);

            if (Version < 116) CaptureLeaderIfApplicable();

            FlipBeneGesseritWhenAloneOrWithPinkAlly();

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

        private void DiscardOneTimeCardsUsedInBattle(TreacheryCalled aggressorCall, TreacheryCalled defenderCall)
        {
            bool aggressorKeepsCards = aggressorCall.TreacherySucceeded(this) && !defenderCall.TreacherySucceeded(this);
            if (!aggressorKeepsCards)
            {
                DiscardOneTimeCards(AggressorBattleAction);
            }

            bool defenderKeepsCards = defenderCall.TreacherySucceeded(this) && !aggressorCall.TreacherySucceeded(this);
            if (!defenderKeepsCards)
            {
                DiscardOneTimeCards(DefenderBattleAction);
            }
        }

        private void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
        {
            BattleOutcome = DetermineBattleOutcome(agg, def, b.Territory);

            bool lasgunShield = !aggtrt.TreacherySucceeded(this) && !deftrt.TreacherySucceeded(this) && (agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield);

            ActivateSmuggler(aggtrt, deftrt, BattleOutcome, lasgunShield);

            HandleReinforcements(agg);
            HandleReinforcements(def);

            var aggressor = GetPlayer(agg.Initiator);
            var defender = GetPlayer(def.Initiator);

            if (aggtrt.TreacherySucceeded(this) || deftrt.TreacherySucceeded(this))
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
                HandleBattleOutcome(agg, def, b.Territory);
            }

            DetermineIfCapturedLeadersMustBeReleased();
        }

        private void HandleReinforcements(Battle plan)
        {
            if (plan.HasReinforcements)
            {
                int forcesToRemove = Math.Min(3, plan.Player.ForcesInReserve);
                plan.Player.RemoveForcesFromReserves(forcesToRemove);
                plan.Player.ForcesKilled += forcesToRemove;

                int specialForcesToRemove = 3 - forcesToRemove;
                if (specialForcesToRemove > 0)
                {
                    plan.Player.RemoveSpecialForcesFromReserves(specialForcesToRemove);
                    plan.Player.SpecialForcesKilled += specialForcesToRemove;
                }

                Log(
                    plan.Initiator,
                    MessagePart.ExpressIf(forcesToRemove > 0, forcesToRemove, plan.Player.Force),
                    MessagePart.ExpressIf(forcesToRemove > 0 && specialForcesToRemove > 0, " and "),
                    MessagePart.ExpressIf(specialForcesToRemove > 0, specialForcesToRemove, plan.Player.SpecialForce),
                    " reinforcements from reserves were killed");
            }
        }

        private void ActivateSmuggler(TreacheryCalled aggtrt, TreacheryCalled deftrt, BattleOutcome outcome, bool lasgunShield)
        {
            bool aggHeroSurvives = !deftrt.TreacherySucceeded(this) && (aggtrt.TreacherySucceeded(this) || !lasgunShield && !outcome.AggHeroKilled);
            bool defHeroSurvives = !aggtrt.TreacherySucceeded(this) && (deftrt.TreacherySucceeded(this) || !lasgunShield && !outcome.DefHeroKilled);

            if (aggHeroSurvives)
            {
                ActivateSmugglerIfApplicable(AggressorBattleAction.Player, AggressorBattleAction.Hero, DefenderBattleAction.Hero, CurrentBattle.Territory);
            }

            if (defHeroSurvives)
            {
                ActivateSmugglerIfApplicable(DefenderBattleAction.Player, DefenderBattleAction.Hero, AggressorBattleAction.Hero, CurrentBattle.Territory);
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

            if (CurrentPortableAntidoteUsed != null && CurrentPortableAntidoteUsed.Player == plan.Player)
            {
                Discard(CurrentPortableAntidoteUsed.Player.Card(TreacheryCardType.PortableAntidote));
            }

            if (Version >= 146 && CurrentRockWasMelted != null && CurrentRockWasMelted.Player == plan.Player)
            {
                Discard(CurrentRockWasMelted.Player.Card(TreacheryCardType.PortableAntidote));
            }
        }

        private void ActivateSandmasterIfApplicable(Battle plan)
        {
            var locationWithResources = CurrentBattle.Territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));

            if (locationWithResources != null && SkilledAs(plan.Hero, LeaderSkill.Sandmaster) && plan.Player.AnyForcesIn(CurrentBattle.Territory) > 0)
            {
                Log(LeaderSkill.Sandmaster, " adds ", Payment.Of(3), " to ", CurrentBattle.Territory);
                ChangeResourcesOnPlanet(locationWithResources, 3);
            }
        }

        private void ActivateSmugglerIfApplicable(Player player, IHero hero, IHero opponentHero, Territory territory)
        {
            if (SkilledAs(hero, LeaderSkill.Smuggler))
            {
                var locationWithResources = territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));
                if (locationWithResources != null)
                {
                    int collected = Math.Min(ResourcesOnPlanet[locationWithResources], hero.ValueInCombatAgainst(opponentHero));
                    if (collected > 0)
                    {
                        Log(player.Faction, LeaderSkill.Smuggler, " collects ", Payment.Of(collected), " from ", territory);
                        ChangeResourcesOnPlanet(locationWithResources, -collected);
                        player.Resources += collected;
                    }
                }
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

                DeciphererMayReplaceTraitor = leaderIsSkilled && BattleConcluded.ValidTraitorsToReplace(plan.Player).Any();
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
                Stone(Milestone.Shuffled);
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
                Log(Map.TueksSietch, " stronghold advantage: ", player.Faction, " collect ", Payment.Of(2), " for playing ", card);
                player.Resources += 2;
            }
        }

        private void ResolveEffectOfOwnedSietchTabr(Battle winnerPlan, Battle opponentPlan)
        {
            if (HasStrongholdAdvantage(winnerPlan.Initiator, StrongholdAdvantage.CollectResourcesForDial, CurrentBattle.Territory))
            {
                int collected = (int)Math.Floor(opponentPlan.Dial(this, winnerPlan.Initiator));
                if (collected > 0)
                {
                    Log(Map.SietchTabr, " stronghold advantage: ", winnerPlan.Initiator, " collect ", Payment.Of(collected), " from enemy force dial");
                    winnerPlan.Player.Resources += collected;
                }
            }
        }

        private void ResolveEffectOfOccupiedJacurutu(Battle winnerPlan, int opponentUndialedForces)
        {
            if (CurrentBattle.Territory == Map.Jacurutu.Territory)
            {
                if (opponentUndialedForces > 0)
                {
                    Log(winnerPlan.Initiator, " get ", Payment.Of(opponentUndialedForces), " from winning a fight in ", Map.Jacurutu);
                    winnerPlan.Player.Resources += opponentUndialedForces;
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
                Log(Auditee.Faction, " don't have cards to audit");
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        private void DetermineIfCapturedLeadersMustBeReleased()
        {
            var black = GetPlayer(Faction.Black);

            if (black != null)
            {
                //DetermineIfDeadLeaderMustBeReleased
                var deadCaptives = black.Leaders.Where(l => CapturedLeaders.ContainsKey(l) && !IsAlive(l)).ToList();
                foreach (var captive in deadCaptives)
                {
                    ReturnCapturedLeader(black, captive);
                }

                //DetermineIfLeaderUsedInBattleMustBeReleased
                var usedLeaderInBattle = CurrentBattle?.PlanOf(black)?.Hero;
                if (usedLeaderInBattle != null && usedLeaderInBattle is Leader leader && CapturedLeaders.ContainsKey(leader))
                {
                    ReturnCapturedLeader(black, leader);
                }

                //DetermineIfCapturedLeadersMustBeReleasedWhenBlackHasNoLeadersLeft
                if (!black.Leaders.Any(l => !CapturedLeaders.ContainsKey(l) && IsAlive(l)))
                {
                    var captives = black.Leaders.Where(l => CapturedLeaders.ContainsKey(l)).ToList();
                    foreach (var captive in captives)
                    {
                        ReturnCapturedLeader(black, captive);
                    }
                }
            }
        }

        private void ReturnCapturedLeader(Player currentOwner, Leader toReturn)
        {
            if (CapturedLeaders.TryGetValue(toReturn, out Faction value))
            {
                var originalPlayer = GetPlayer(value);
                originalPlayer.Leaders.Add(toReturn);
                currentOwner.Leaders.Remove(toReturn);
                CapturedLeaders.Remove(toReturn);
                if (IsSkilled(toReturn))
                {
                    SetInFrontOfShield(toReturn, true);
                }
                Log(toReturn, " returns to ", originalPlayer.Faction, " after working for ", currentOwner.Faction);
            }
        }

        private bool BrownLeaderWasRevealedAsTraitor
        {
            get
            {
                var brown = GetPlayer(Faction.Brown);
                if (brown != null && CurrentBattle.IsAggressorOrDefender(brown))
                {
                    return CurrentBattle.TreacheryOfOpponent(brown).TreacherySucceeded(this);
                }
                return false;
            }
        }

        internal bool BlackMustDecideToCapture => Version >= 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders) && !Prevented(FactionAdvantage.BlackCaptureLeader);

        #endregion

        #region BattleOutcome

        public bool LoserMayTryToAssassinate { get; internal set; } = false;

        public void HandleBattleOutcome(Battle agg, Battle def, Territory territory)
        {
            LogIf(BattleOutcome.AggHeroSkillBonus != 0, agg.Hero, " ", BattleOutcome.AggActivatedBonusSkill, " bonus: ", BattleOutcome.AggHeroSkillBonus);
            LogIf(BattleOutcome.DefHeroSkillBonus != 0, def.Hero, " ", BattleOutcome.DefActivatedBonusSkill, " bonus: ", BattleOutcome.DefHeroSkillBonus);

            LogIf(BattleOutcome.AggBattlePenalty != 0, agg.Hero, " ", BattleOutcome.DefActivatedPenaltySkill, " penalty: ", BattleOutcome.AggBattlePenalty);
            LogIf(BattleOutcome.DefBattlePenalty != 0, def.Hero, " ", BattleOutcome.AggActivatedPenaltySkill, " penalty: ", BattleOutcome.DefBattlePenalty);

            LogIf(BattleOutcome.AggMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.AggMessiahContribution);
            LogIf(BattleOutcome.DefMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.DefMessiahContribution);

            BattleWinner = BattleOutcome.Winner.Faction;
            BattleLoser = BattleOutcome.Loser.Faction;

            if (BattleOutcome.AggHeroKilled)
            {
                KillLeaderInBattle(agg.Hero, BattleOutcome.AggHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.AggHeroEffectiveStrength);
            }
            else
            {
                LogIf(BattleOutcome.AggSavedByCarthag, Map.Carthag, " stronghold advantage saves ", agg.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (BattleOutcome.DefHeroKilled)
            {
                KillLeaderInBattle(def.Hero, BattleOutcome.DefHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.DefHeroEffectiveStrength);
            }
            else
            {
                LogIf(BattleOutcome.DefSavedByCarthag, Map.Carthag, " stronghold advantage saves ", def.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (BattleInitiated.IsAggressorByJuice(this, def.Player.Faction))
            {
                Log(agg.Initiator, " (defending) strength: ", BattleOutcome.AggTotal);
                Log(def.Initiator, " (aggressor by ", TreacheryCardType.Juice, ") strength: ", BattleOutcome.DefTotal);
            }
            else
            {
                Log(agg.Initiator, " (aggressor) strength: ", BattleOutcome.AggTotal);
                Log(def.Initiator, " (defending) strength: ", BattleOutcome.DefTotal);
            }

            LoserMayTryToAssassinate = BattleLoser == Faction.Cyan && Applicable(Rule.CyanAssassinate) && !Assassinated.Any(l => l.Faction == BattleWinner) && BattleOutcome.WinnerBattlePlan.Hero is Leader && IsAlive(BattleOutcome.WinnerBattlePlan.Hero);

            Log(BattleOutcome.Winner.Faction, " WIN THE BATTLE");

            HandleHarassAndWithdraw(agg, territory);
            HandleHarassAndWithdraw(def, territory);

            bool loserMayRetreat =
                !BattleOutcome.LoserHeroKilled &&
                SkilledAs(BattleOutcome.LoserBattlePlan.Hero, LeaderSkill.Diplomat) &&
                (Retreat.MaxForces(this, BattleOutcome.Loser) > 0 || Retreat.MaxSpecialForces(this, BattleOutcome.Loser) > 0) &&
                Retreat.ValidTargets(this, BattleOutcome.Loser).Any();

            Enter(loserMayRetreat, Phase.Retreating, HandleLosses);
        }

        private void HandleHarassAndWithdraw(Battle plan, Territory territory)
        {
            if (plan.Weapon != null && plan.Weapon.Type == TreacheryCardType.HarassAndWithdraw ||
                plan.Defense != null && plan.Defense.Type == TreacheryCardType.HarassAndWithdraw)
            {
                var forceSupplier = Battle.DetermineForceSupplier(this, plan.Player);
                int undialledNormalForces = forceSupplier.ForcesIn(CurrentBattle.Territory) - plan.Forces - plan.ForcesAtHalfStrength;
                int undialledSpecialForces = forceSupplier.SpecialForcesIn(CurrentBattle.Territory) - plan.SpecialForces - plan.SpecialForcesAtHalfStrength;
                forceSupplier.ForcesToReserves(territory, undialledNormalForces, false);
                forceSupplier.ForcesToReserves(territory, undialledSpecialForces, true);

                if (undialledNormalForces + undialledSpecialForces > 0)
                {
                    Log(
                        plan.Initiator,
                        " withdraw ",
                        MessagePart.ExpressIf(undialledNormalForces > 0, undialledNormalForces, forceSupplier.Force),
                        MessagePart.ExpressIf(undialledNormalForces > 0 && undialledSpecialForces > 0, " and "),
                        MessagePart.ExpressIf(undialledSpecialForces > 0, undialledSpecialForces, forceSupplier.SpecialForce),
                        " to reserves");
                }
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

            bool heroStrengthCountsToTotal = !artilleryUsed && !(Version >= 145 && rockMelterUsed && !rockMelterUsedToKill);

            result.AggHeroEffectiveStrength = agg.Hero != null && heroStrengthCountsToTotal ? agg.Hero.ValueInCombatAgainst(def.Hero) : 0;
            result.DefHeroEffectiveStrength = def.Hero != null && heroStrengthCountsToTotal ? def.Hero.ValueInCombatAgainst(agg.Hero) : 0;

            if (heroStrengthCountsToTotal)
            {
                result.AggHeroSkillBonus = Battle.DetermineSkillBonus(this, agg, ref result.AggActivatedBonusSkill);
                result.AggBattlePenalty = !result.DefHeroKilled ? Battle.DetermineSkillPenalty(this, def, result.Aggressor, ref result.DefActivatedPenaltySkill) : 0;
                result.AggMessiahContribution = agg.Messiah && agg.Hero != null ? 2 : 0;

                result.DefHeroSkillBonus = Battle.DetermineSkillBonus(this, def, ref result.DefActivatedBonusSkill);
                result.DefBattlePenalty = !result.AggHeroKilled ? Battle.DetermineSkillPenalty(this, agg, result.Defender, ref result.AggActivatedPenaltySkill) : 0;
                result.DefMessiahContribution = def.Messiah && def.Hero != null ? 2 : 0;
            }

            int aggHeroContribution = result.AggHeroKilled || (Version < 145 && rockMelterUsed) ? 0 : result.AggHeroEffectiveStrength + result.AggHeroSkillBonus + result.AggMessiahContribution - result.AggBattlePenalty;
            int defHeroContribution = result.DefHeroKilled || (Version < 145 && rockMelterUsed) ? 0 : result.DefHeroEffectiveStrength + result.DefHeroSkillBonus + result.DefMessiahContribution - result.DefBattlePenalty;

            int aggPinkKarmaContribution = agg.Initiator == Faction.Pink ? PinkKarmaBonus : 0;
            int defPinkKarmaContribution = def.Initiator == Faction.Pink ? PinkKarmaBonus : 0;

            float aggForceDial;
            float defForceDial;

            var aggForceSupplier = Battle.DetermineForceSupplier(this, result.Aggressor);
            result.AggUndialedForces = aggForceSupplier.AnyForcesIn(CurrentBattle.Territory) - agg.TotalForces;

            var defForceSupplier = Battle.DetermineForceSupplier(this, result.Defender);
            result.DefUndialedForces = defForceSupplier.AnyForcesIn(CurrentBattle.Territory) - def.TotalForces;

            if (!rockMelterUsed)
            {
                aggForceDial = agg.Dial(this, result.Defender.Faction);
                defForceDial = def.Dial(this, result.Aggressor.Faction);
            }
            else
            {
                aggForceDial = result.AggUndialedForces;
                if (result.Aggressor.Faction == CurrentPinkOrAllyFighter) aggForceDial += (int)Math.Ceiling(0.5f * GetPlayer(Faction.Pink).AnyForcesIn(CurrentBattle.Territory));

                defForceDial = result.DefUndialedForces;
                if (result.Defender.Faction == CurrentPinkOrAllyFighter) defForceDial += (int)Math.Ceiling(0.5f * GetPlayer(Faction.Pink).AnyForcesIn(CurrentBattle.Territory));
            }

            result.AggReinforcementsContribution = agg.HasReinforcements ? 2 : 0;
            result.DefReinforcementsContribution = def.HasReinforcements ? 2 : 0;

            result.AggHomeworldContribution = agg.Player.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);
            result.DefHomeworldContribution = def.Player.GetHomeworldBattleContributionAndLasgunShieldLimit(territory);

            result.AggTotal = aggForceDial + aggHeroContribution + aggPinkKarmaContribution + result.AggHomeworldContribution + result.AggReinforcementsContribution;
            result.DefTotal = defForceDial + defHeroContribution + defPinkKarmaContribution + result.DefHomeworldContribution + result.DefReinforcementsContribution;

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

        private void HandleLosses()
        {
            ProcessWinnerLosses(CurrentBattle.Territory, BattleOutcome.Winner, BattleOutcome.WinnerBattlePlan, false);
            ProcessLoserLosses(CurrentBattle.Territory, BattleOutcome.Loser, BattleOutcome.LoserBattlePlan);
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

        private static void DetermineDeathBy(IHero hero, TreacheryCardType byWeapon, bool weaponHasEffect, ref bool heroIsKilled, ref TreacheryCardType causeOfDeath)
        {
            if (!heroIsKilled && hero != null && weaponHasEffect)
            {
                heroIsKilled = true;
                causeOfDeath = byWeapon;
            }
        }

        private void ProcessLoserLosses(Territory territory, Player loser, Battle loserGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            var forceSupplierOfLoser = Battle.DetermineForceSupplier(this, loser);
            if (forceSupplierOfLoser != loser)
            {
                Log(forceSupplierOfLoser.Faction, " lose all ", forceSupplierOfLoser.SpecialForcesIn(territory) + forceSupplierOfLoser.ForcesIn(territory), " forces in ", territory);
                forceSupplierOfLoser.KillAllForces(territory, true);
            }

            Log(loser.Faction, " lose all ", loser.AnyForcesIn(territory), " forces in ", territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit, MayKeepCardsAfterLosingBattle(loser));
            PayDialedSpice(loser, loserGambit, false);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                Stone(Milestone.Messiah);
            }
        }

        private bool MayKeepCardsAfterLosingBattle(Player p) => p.Ally == Faction.Cyan && CyanAllowsKeepingCards || p.Nexus == Faction.Cyan && NexusPlayed.CanUseSecretAlly(this, p);

        private bool DialledResourcesAreRefunded(Player p) => Applicable(Rule.YellowAllyGetsDialedResourcesRefunded) && p.Ally == Faction.Yellow && YellowRefundsBattleDial;

        private void PayDialedSpice(Player p, Battle plan, bool traitorWasRevealed)
        {
            int cost = plan.Cost(this);
            int costToBrown = p.Ally == Faction.Brown ? plan.AllyContributionAmount : 0;

            if (cost > 0)
            {
                int costForPlayer = cost - plan.AllyContributionAmount;
                int refundedResources = 0;

                if (costForPlayer > 0)
                {
                    p.Resources -= costForPlayer;

                    if (DialledResourcesAreRefunded(p))
                    {
                        Log(Payment.Of(costForPlayer), " dialled in battle will be refunded in the ", MainPhase.Contemplate, " phase");
                        refundedResources = costForPlayer;
                        p.Bribes += costForPlayer;
                    }

                    LogIf(HasStrongholdAdvantage(p.Faction, StrongholdAdvantage.FreeResourcesForBattles, CurrentBattle.Territory),
                        Map.Arrakeen, " stronghold advantage: supporting forces costs ", Payment.Of(2), " less");
                }

                if (plan.AllyContributionAmount > 0)
                {
                    p.AlliedPlayer.Resources -= plan.AllyContributionAmount;
                    if (Version >= 117) DecreasePermittedUseOfAllySpice(p.Faction, plan.AllyContributionAmount);
                }

                int receiverProfit = HandleBrownIncome(p, cost - costToBrown - refundedResources, traitorWasRevealed);

                if (cost - receiverProfit >= 4)
                {
                    ActivateBanker(p);
                }
            }

            if (plan.BankerBonus > 0)
            {
                p.Resources -= plan.BankerBonus;
                Log(p.Faction, " paid ", Payment.Of(plan.BankerBonus), " as ", LeaderSkill.Banker);
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
                        Log(Faction.Brown, " get ", Payment.Of(result), " from supported forces");

                        if (result >= 5)
                        {
                            BattleTriggeredBureaucracy = new TriggeredBureaucracy() { PaymentFrom = paidBy.Faction, PaymentTo = Faction.Brown };
                        }
                    }
                    else
                    {
                        LogPreventionByKarma(FactionAdvantage.BrownReceiveForcePayment);
                    }
                }
            }

            return result;
        }

        private void ProcessWinnerLosses(Territory territory, Player winner, Battle plan, bool traitorWasRevealed)
        {
            PayDialedSpice(winner, plan, traitorWasRevealed);
            ProcessWinnerForceLosses(territory, winner, plan);
        }

        private void ProcessWinnerForceLosses(Territory territory, Player winner, Battle plan)
        {
            var forceSupplier = Battle.DetermineForceSupplier(this, winner);
            if (CurrentPinkBattleContribution > 0)
            {
                var pink = GetPlayer(Faction.Pink);
                if (pink != null)
                {
                    pink.KillForces(territory, CurrentPinkBattleContribution, false, true);
                    Log(Faction.Pink, " lose ", CurrentPinkBattleContribution, pink.Force, " in ", territory);
                }
            }

            int specialForcesToLose = plan.SpecialForces + plan.SpecialForcesAtHalfStrength;
            int forcesToLose = plan.Forces + plan.ForcesAtHalfStrength;

            int specialForcesToSaveToReserves = 0;
            int forcesToSaveToReserves = 0;
            int specialForcesToSaveInTerritory = 0;
            int forcesToSaveInTerritory = 0;

            if (!MaySubstituteForceLosses(forceSupplier))
            {
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
            }

            if (specialForcesToSaveInTerritory + forcesToSaveInTerritory + specialForcesToSaveToReserves + forcesToSaveToReserves > 0)
            {
                if (specialForcesToSaveToReserves > 0) forceSupplier.ForcesToReserves(territory, specialForcesToSaveToReserves, true);

                if (forcesToSaveToReserves > 0) forceSupplier.ForcesToReserves(territory, forcesToSaveToReserves, false);

                Log(
                    LeaderSkill.Graduate,
                    " rescues ",
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0, forcesToSaveInTerritory, forceSupplier.Force),
                    MessagePart.ExpressIf(specialForcesToSaveInTerritory > 0, specialForcesToSaveInTerritory, forceSupplier.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0 || specialForcesToSaveInTerritory > 0, " on site"),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0 && specialForcesToSaveToReserves > 0, " and "),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0, forcesToSaveToReserves, forceSupplier.Force),
                    MessagePart.ExpressIf(specialForcesToSaveToReserves > 0, specialForcesToSaveToReserves, forceSupplier.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " to reserves"));
            }

            if (!MaySubstituteForceLosses(forceSupplier) || specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory == 0 || forceSupplier.ForcesIn(territory) <= plan.Forces + plan.ForcesAtHalfStrength)
            {
                int winnerForcesLost = forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory;
                int winnerSpecialForcesLost = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
                HandleForceLosses(territory, forceSupplier, winnerForcesLost, winnerSpecialForcesLost);
            }
            else
            {
                GreySpecialForceLossesToTake = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
            }
        }

        private bool MaySubstituteForceLosses(Player p) => p.Faction == Faction.Grey && (Version < 113 || !Prevented(FactionAdvantage.GreyReplacingSpecialForces));

        internal void HandleForceLosses(Territory territory, Player player, int forcesLost, int specialForcesLost)
        {
            bool hadMessiahBeforeLosses = player.MessiahAvailable;

            player.KillForces(territory, forcesLost, false, true);
            player.KillForces(territory, specialForcesLost, true, true);

            LogLosses(player, forcesLost, specialForcesLost);

            if (player.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                Stone(Milestone.Messiah);
            }
        }

        private void KillLeaderInBattle(IHero killedHero, TreacheryCardType causeOfDeath, Player winner, int heroValue)
        {
            Log(causeOfDeath, " kills ", killedHero, " → ", winner.Faction, " get ", Payment.Of(heroValue));
            if (killedHero is Leader) KillHero(killedHero);
            winner.Resources += heroValue;
        }

        private void LogLosses(Player player, int forcesLost, int specialForcesLost)
        {
            if (forcesLost > 0 || specialForcesLost > 0)
            {
                Log(
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

        #endregion

        #region NonBattleOutcomes

        private void TraitorCalled(BattleInitiated b, Battle agg, Battle def, TreacheryCalled deftrt, Player aggressor, Player defender, IHero aggLeader, IHero defLeader)
        {
            if (AggressorTraitorAction.TreacherySucceeded(this) && deftrt.TreacherySucceeded(this))
            {
                TwoTraitorsCalled(agg, def, aggressor, defender, b.Territory, aggLeader, defLeader);
            }
            else
            {
                var winner = AggressorTraitorAction.TreacherySucceeded(this) ? aggressor : defender;
                var loser = AggressorTraitorAction.TreacherySucceeded(this) ? defender : aggressor;
                var loserGambit = AggressorTraitorAction.TreacherySucceeded(this) ? def : agg;
                var winnerGambit = AggressorTraitorAction.TreacherySucceeded(this) ? agg : def;
                OneTraitorCalled(b.Territory, winner, loser, loserGambit, winnerGambit);
            }
        }

        private void OneTraitorCalled(Territory territory, Player winner, Player loser, Battle loserGambit, Battle winnerGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            var traitor = loserGambit.Hero;
            var traitorValue = traitor.ValueInCombatAgainst(winnerGambit.Hero);
            var traitorOwner = winner.Traitors.Any(t => t.IsTraitor(traitor)) ? winner.Faction : Faction.Black;

            Log(traitor, " is a ", traitorOwner, " traitor! ", loser.Faction, " lose everything");

            if (traitor is Leader)
            {
                Log("Treachery kills ", traitor, " → ", winner.Faction, " get ", Payment.Of(traitorValue));
                KillHero(traitor);
                winner.Resources += traitorValue;
            }

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            var forceSupplierOfLoser = Battle.DetermineForceSupplier(this, loser);
            if (forceSupplierOfLoser != loser)
            {
                Log(forceSupplierOfLoser.Faction, " lose all ", forceSupplierOfLoser.SpecialForcesIn(territory) + forceSupplierOfLoser.ForcesIn(territory), " forces in ", territory);
                forceSupplierOfLoser.KillAllForces(territory, true);
            }

            Log(loser.Faction, " lose all ", loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), " forces in ", territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit, MayKeepCardsAfterLosingBattle(loser));
            PayDialedSpice(loser, loserGambit, true);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                Stone(Milestone.Messiah);
            }
        }

        private void TwoTraitorsCalled(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            Log("Treachery kills both ", defLeader, " and ", aggLeader);
            KillHero(defLeader);
            KillHero(aggLeader);

            var forceSupplierOfDefender = Battle.DetermineForceSupplier(this, defender);
            if (forceSupplierOfDefender != defender)
            {
                Log(forceSupplierOfDefender.Faction, " lose all ", forceSupplierOfDefender.SpecialForcesIn(territory) + forceSupplierOfDefender.ForcesIn(territory), " forces in ", territory);
                forceSupplierOfDefender.KillAllForces(territory, true);
            }

            Log(defender.Faction, " lose all ", defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), " forces in ", territory);
            defender.KillAllForces(territory, true);

            var forceSupplierOfAggressor = Battle.DetermineForceSupplier(this, aggressor);
            if (forceSupplierOfAggressor != aggressor)
            {
                Log(forceSupplierOfAggressor.Faction, " lose all ", forceSupplierOfAggressor.SpecialForcesIn(territory) + forceSupplierOfAggressor.ForcesIn(territory), " forces in ", territory);
                forceSupplierOfAggressor.KillAllForces(territory, true);
            }

            Log(aggressor.Faction, " lose all ", aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), " forces in ", territory);
            aggressor.KillAllForces(territory, true);

            LoseCards(def, false);
            PayDialedSpice(defender, def, true);

            LoseCards(agg, false);
            PayDialedSpice(aggressor, agg, true);

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                Stone(Milestone.Messiah);
            }
        }

        private void LasgunShieldExplosion(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            Log("A ", TreacheryCardType.Laser, "/", TreacheryCardType.Shield, " explosion occurs!");
            Stone(Milestone.Explosion);

            if (aggLeader != null)
            {
                Log("The explosion kills ", aggLeader);
                KillHero(aggLeader);
            }

            if (defLeader != null)
            {
                Log("The explosion kills ", defLeader);
                KillHero(def.Hero);
            }

            if (agg.Messiah || def.Messiah)
            {
                Log("The explosion kills the ", Concept.Messiah);
                KillHero(LeaderManager.Messiah);
            }

            LoseCards(agg, false);
            PayDialedSpice(aggressor, agg, false);

            LoseCards(def, false);
            PayDialedSpice(defender, def, false);

            int removed = RemoveResources(territory);
            if (removed > 0)
            {
                Log("The explosion destroys ", Payment.Of(removed), " in ", territory);
            }

            KillAllForcesIn(territory, true);
            KillAmbassadorIn(territory);

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                Stone(Milestone.Messiah);
            }
        }

        private void KillAllForcesIn(Territory territory, bool inBattle)
        {
            foreach (var p in Players)
            {
                if (p.AnyForcesIn(territory) > 0)
                {
                    RevealCurrentNoField(p, territory);

                    int homeworldKillLimit = inBattle ? p.GetHomeworldBattleContributionAndLasgunShieldLimit(territory) : 0;
                    if (homeworldKillLimit == 0)
                    {
                        Log("All ", p.Faction, " forces in ", territory, " were killed");
                        p.KillAllForces(territory, inBattle);
                    }
                    else
                    {
                        int normalForcesToKill = Math.Min(p.ForcesIn(territory), homeworldKillLimit);
                        int specialForcesToKill = Math.Min(p.SpecialForcesIn(territory), homeworldKillLimit - normalForcesToKill);

                        if (normalForcesToKill > 0) p.KillForces(territory, normalForcesToKill, false, inBattle);
                        if (specialForcesToKill > 0) p.KillForces(territory, specialForcesToKill, true, inBattle);

                        Log(MessagePart.ExpressIf(normalForcesToKill > 0, normalForcesToKill, p.Force),
                            MessagePart.ExpressIf(normalForcesToKill > 0 && specialForcesToKill > 0, " and "),
                            MessagePart.ExpressIf(specialForcesToKill > 0, specialForcesToKill, p.SpecialForce),
                            " in ", territory, " were killed");
                    }
                }
            }
        }

        #endregion

        #region BattleConclusion

        internal bool BattleWasConcludedByWinner { get; set; } = false;

        public List<Leader> Assassinated { get; private set; } = new();

        public bool OccupationPreventsAlliance(Faction a, Faction b)
        {
            var playerA = GetPlayer(a);
            var playerB = GetPlayer(b);
            foreach (var t in playerA.ForcesOnPlanet.Select(kvp => kvp.Key.Territory).Distinct())
            {
                if (a == Faction.Blue && Applicable(Rule.AdvisorsDontConflictWithAlly) && playerA.ForcesIn(t) == 0) continue;
                if (b == Faction.Blue && Applicable(Rule.AdvisorsDontConflictWithAlly) && playerB.ForcesIn(t) == 0) continue;

                if (playerB.ForcesIn(t) > 0) return true;
            }

            return false;
        }

        public void HandleEvent(LoserConcluded e)
        {
            if (e.KeptCard != null)
            {
                if (SecretAllyAllowsKeepingCardsAfterLosingBattle)
                {
                    SecretAllyAllowsKeepingCardsAfterLosingBattle = false;
                    PlayNexusCard(e.Player, "Secret Ally", " to keep ", e.KeptCard);
                }
                else
                {
                    Log(e.Initiator, " keep ", e.KeptCard);
                }
            }

            foreach (var c in CardsToBeDiscardedByLoserAfterBattle.Where(c => c != e.KeptCard))
            {
                Discard(c);
            }

            CardsToBeDiscardedByLoserAfterBattle.Clear();

            Log(e);

            var winner = GetPlayer(BattleWinner);

            if (e.KarmaForcedKeptCardDecision == LoserConcluded.KARMA_DISCARD || e.KarmaForcedKeptCardDecision == LoserConcluded.KARMA_KEEP)
            {
                BattleWinnerMayChooseToDiscard = false;
                Discard(e.Player, TreacheryCardType.Karma);
                Stone(Milestone.Karma);

                foreach (var c in e.ForcedKeptOrDiscardedCards)
                {
                    if (e.KarmaForcedKeptCardDecision == LoserConcluded.KARMA_DISCARD)
                    {
                        Log("Using ", TreacheryCardType.Karma, ", ", e.Initiator, " force ", winner.Faction, " to discard ", c);
                        if (winner.Has(c)) Discard(c);
                    }
                    else if (e.KarmaForcedKeptCardDecision == LoserConcluded.KARMA_KEEP)
                    {
                        Log("Using ", TreacheryCardType.Karma, ", ", e.Initiator, " force ", winner.Faction, " to keep ", c);
                        if (TreacheryDiscardPile.Items.Contains(c))
                        {
                            TreacheryDiscardPile.Items.Remove(c);
                            winner.TreacheryCards.Add(c);
                        }
                    }
                }
            }

            if (e.Assassinate)
            {
                Stone(Milestone.Assassination);

                var assassinated = LoserConcluded.TargetOfAssassination(this, e.Player);

                Assassinated.Add(assassinated);
                e.Player.RevealedTraitors.Add(assassinated);

                if (!IsAlive(assassinated) || !winner.Leaders.Contains(assassinated))
                {
                    Log(e.Initiator, " reveal ", assassinated, " as their target of assassination...");
                }
                else
                {
                    Log(e.Initiator, " get ", Payment.Of(assassinated.CostToRevive), " by ASSASSINATING ", assassinated, "!");
                    e.Player.Resources += assassinated.CostToRevive;
                    KillHero(assassinated);
                }
            }

            LoserMayTryToAssassinate = false;

            if (BattleWasConcludedByWinner)
            {
                Enter(!IsPlaying(Faction.Purple) || BattleWinner == Faction.Purple, FinishBattle, Version <= 150, Phase.Facedancing, Phase.RevealingFacedancer);
            }
        }


        

        public Leader BlackVictim { get; internal set; }
        private void SelectVictimOfBlackWinner(Battle harkonnenAction, Battle victimAction)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var victim = GetPlayer(victimAction.Initiator);

            //Get all living leaders from the opponent that haven't fought in another territory this turn
            Deck<Leader> availableLeaders = new(victim.Leaders.Where(l => l.HeroType != HeroType.Auditor && LeaderState[l].Alive && CanJoinCurrentBattle(l)), Random);

            if (!availableLeaders.IsEmpty)
            {
                availableLeaders.Shuffle();
                BlackVictim = availableLeaders.Draw();
            }
            else
            {
                BlackVictim = null;
                Log(victim.Faction, " don't have any leaders for ", Faction.Black, " to capture or kill");
            }
        }

        public Dictionary<Leader, Faction> CapturedLeaders { get; private set; } = new Dictionary<Leader, Faction>();
        
                
        internal void FinishBattle()
        {
            if (AggressorBattleAction.Hero == Vidal && WhenToSetAsideVidal == VidalMoment.AfterUsedInBattle && !(AggressorTraitorAction.TreacherySucceeded(this) && !DefenderTraitorAction.TreacherySucceeded(this)))
            {
                SetAsideVidal();
            }

            if (DefenderBattleAction.Hero == Vidal && WhenToSetAsideVidal == VidalMoment.AfterUsedInBattle && !(DefenderTraitorAction.TreacherySucceeded(this) && !AggressorTraitorAction.TreacherySucceeded(this)))
            {
                SetAsideVidal();
            }

            ReturnSkilledLeadersInFrontOfShield();
            if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
            if (CurrentJuice != null && CurrentJuice.Type == JuiceType.Aggressor) CurrentJuice = null;
            CurrentDiplomacy = null;
            CurrentRockWasMelted = null;
            CurrentPortableAntidoteUsed = null;
            FinishDeciphererIfApplicable();
            if (NextPlayerToBattle == null) MainPhaseEnd();
            Enter(Phase.BattleReport);
        }

        private void ReturnSkilledLeadersInFrontOfShield()
        {
            foreach (var leader in LeaderState.Where(ls => ls.Key is Leader l && IsSkilled(l) && !ls.Value.InFrontOfShield).Select(ls => ls.Key as Leader))
            {
                var currentOwner = Players.FirstOrDefault(p => p.Leaders.Contains(leader));

                if (currentOwner == null || !CapturedLeaders.ContainsKey(leader) && !(currentOwner.Faction != Faction.Pink && leader.HeroType == HeroType.Vidal))
                {
                    SetInFrontOfShield(leader, true);

                    if (IsAlive(leader))
                    {
                        Log(Skill(leader), " ", leader, " is placed back in front of shield");
                    }
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

        

        

        #endregion

        #region FaceDancing
                

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

        #endregion

        #region PostBattle

        public Diplomacy CurrentDiplomacy { get; internal set; }
        

        public IList<TreacheryCard> AuditedCards { get; private set; }

        public Player Auditee
        {
            get
            {
                if (Applicable(Rule.BrownAuditor) && !Prevented(FactionAdvantage.BrownAudit))
                {
                    if (AggressorBattleAction != null && AggressorBattleAction.Hero != null && AggressorBattleAction.Hero.HeroType == HeroType.Auditor)
                    {
                        return DefenderBattleAction.Player;
                    }
                    else if (DefenderBattleAction != null && DefenderBattleAction.Hero != null && DefenderBattleAction.Hero.HeroType == HeroType.Auditor)
                    {
                        return AggressorBattleAction.Player;
                    }
                }

                return null;
            }
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

            Log(e);
            HandleLosses();
            FlipBeneGesseritWhenAloneOrWithPinkAlly();
            DetermineHowToProceedAfterRevealingBattlePlans();
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
                    LogPreventionByKarma(FactionAdvantage.BlackCaptureLeader);
                }
            }
        }

        internal void CaptureLeader()
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

        

        #endregion

        #region Support

        public Player NextPlayerToBattle
        {
            get
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    var playerToCheck = BattleSequence.CurrentPlayer;
                    if (Battle.BattlesToBeFought(this, playerToCheck).Any())
                    {
                        return playerToCheck;
                    }

                    BattleSequence.NextPlayer();
                }

                return null;
            }
        }


        private bool SecretAllyAllowsKeepingCardsAfterLosingBattle = false;
        public List<TreacheryCard> CardsToBeDiscardedByLoserAfterBattle { get; private set; } = new();

        private void LoseCards(Battle plan, bool mayChooseToKeepOne)
        {
            if (!(plan.Player.Ally == Faction.Cyan && CyanAllowsKeepingCards) && plan.Player.Nexus == Faction.Cyan && NexusPlayed.CanUseSecretAlly(this, plan.Player))
            {
                SecretAllyAllowsKeepingCardsAfterLosingBattle = true;
            }

            if (mayChooseToKeepOne)
            {
                if (plan.Weapon != null) CardsToBeDiscardedByLoserAfterBattle.Add(plan.Weapon);
                if (plan.Defense != null) CardsToBeDiscardedByLoserAfterBattle.Add(plan.Defense);
            }
            else
            {
                Discard(plan.Weapon);
                Discard(plan.Defense);
            }
        }

        public bool CanJoinCurrentBattle(IHero hero)
        {
            var currentTerritory = CurrentTerritory(hero);
            return currentTerritory == null || currentTerritory == CurrentBattle?.Territory;
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

        public bool BattleWinnerMayChooseToDiscard { get; set; } = true;

        #endregion
    }
}
