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
        #region State

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

        private int NrOfBattlesFought { get; set; } = 0;
        private TriggeredBureaucracy BattleTriggeredBureaucracy { get; set; }

        #endregion State

        #region BeginningOfBattlePhase

        private void EnterBattlePhase()
        {
            MainPhaseStart(MainPhase.Battle);
            NrOfBattlesFought = 0;
            BattleSequence = new PlayerSequence(this);
            if (KarmaHmsMovesLeft != 2) KarmaHmsMovesLeft = 0;
            ResetBattle();
            Enter(NextPlayerToBattle == null, EnterSpiceCollectionPhase, Version >= 107, Phase.BeginningOfBattle, Phase.BattlePhase);
        }

        #endregion

        #region BattleInitiation

        public void HandleEvent(BattleInitiated b)
        {
            CurrentReport = new Report(MainPhase.Battle);
            CurrentBattle = b;
            ChosenHMSAdvantage = StrongholdAdvantage.None;
            BattleOutcome = null;
            NrOfBattlesFought++;
            Log(b);
            AnnounceHeroAvailability(b.AggressivePlayer);
            AnnounceHeroAvailability(b.DefendingPlayer);
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

        private void AnnounceHeroAvailability(Player p)
        {
            if (!Battle.ValidBattleHeroes(this, p).Any())
            {
                Log(p.Faction, " have no leaders available for this battle");
            }
        }

        #endregion

        #region VoiceAndPrescience

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

            Log(e);
            RecentMilestones.Add(Milestone.Voice);
        }

        public Prescience CurrentPrescience { get; private set; } = null;
        public void HandleEvent(Prescience e)
        {
            CurrentPrescience = e;
            Log(e);
            RecentMilestones.Add(Milestone.Prescience);
        }

        public Thought CurrentThought { get; private set; }
        public void HandleEvent(Thought e)
        {
            CurrentThought = e;
            var opponent = CurrentBattle.OpponentOf(e.Initiator).Faction;
            Log(e.Initiator, " use their ", LeaderSkill.Thinker, " skill to ask ", opponent, " if they have a ", e.Card);
            Enter(Phase.Thought);
        }

        public void HandleEvent(ThoughtAnswered e)
        {
            Log(e);
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

        public void HandleEvent(HMSAdvantageChosen e)
        {
            Log(e);
            ChosenHMSAdvantage = e.Advantage;
        }

        public void HandleEvent(SwitchedSkilledLeader e)
        {
            var leader = e.Player.Leaders.FirstOrDefault(l => Skilled(l) && !CapturedLeaders.ContainsKey(l));
            SwitchInFrontOfShield(leader);
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
                RecentMilestones.Add(Milestone.LeaderKilled);
                Log(TreacheryCardType.Residual, " kills ", toKill);
            }
            else
            {
                Log(opponent.Faction, " have no available leaders to kill");
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

                Log(AggressorBattleAction.GetBattlePlanMessage());
                Log(DefenderBattleAction.GetBattlePlanMessage());

                RegisterKnownCards(AggressorBattleAction);
                RegisterKnownCards(DefenderBattleAction);

                PassPurpleTraitorAction();

                Enter(AggressorBattleAction.HasRockMelter || DefenderBattleAction.HasRockMelter, Phase.MeltingRock, Phase.CallTraitorOrPass);
            }
        }

        public PortableAntidoteUsed CurrentPortableAntidoteUsed { get; private set; }
        public void HandleEvent(PortableAntidoteUsed e)
        {
            Log(e);
            CurrentPortableAntidoteUsed = e;
        }

        private bool PoisonToothCancelled { get; set; } = false;
        public void HandleEvent(PoisonToothCancelled e)
        {
            PoisonToothCancelled = true;
            Log(e);
        }

        private RockWasMelted CurrentRockWasMelted { get; set; }
        public void HandleEvent(RockWasMelted e)
        {
            Log(e);
            Discard(e.Player, TreacheryCardType.Rockmelter);
            CurrentRockWasMelted = e;
            Enter(Phase.CallTraitorOrPass);
        }

        private void RegisterKnownCards(Battle battle)
        {
            RegisterKnown(battle.Weapon);
            RegisterKnown(battle.Defense);
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
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(DefenderBattleAction.Hero);
                }
            }

            if (DefenderBattleAction.By(e.Initiator) || e.By(Faction.Black) && AreAllies(DefenderBattleAction.Initiator, Faction.Black))
            {
                DefenderTraitorAction = e;
                if (e.TraitorCalled)
                {
                    Log(e);
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(AggressorBattleAction.Hero);
                }
            }

            if (AggressorTraitorAction != null && DefenderTraitorAction != null)
            {
                HandleRevealedBattlePlans();
            }
        }

        #endregion

        #region BattleResolution

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

        private void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
        {
            BattleOutcome = DetermineBattleOutcome(agg, def, b.Territory);

            bool lasgunShield = !aggtrt.TraitorCalled && !deftrt.TraitorCalled && (agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield);

            ActivateSmuggler(aggtrt, deftrt, BattleOutcome, lasgunShield);

            var aggressor = GetPlayer(agg.Initiator);
            var defender = GetPlayer(def.Initiator);

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
                HandleBattleOutcome(agg, def);
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

        private void ActivateSmuggler(TreacheryCalled aggtrt, TreacheryCalled deftrt, BattleOutcome outcome, bool lasgunShield)
        {
            bool aggHeroSurvives = !deftrt.TraitorCalled && (aggtrt.TraitorCalled || !lasgunShield && !outcome.AggHeroKilled);
            bool defHeroSurvives = !aggtrt.TraitorCalled && (deftrt.TraitorCalled || !lasgunShield && !outcome.DefHeroKilled);

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

            if (CurrentPortableAntidoteUsed != null)
            {
                Discard(CurrentPortableAntidoteUsed.Player.Card(TreacheryCardType.PortableAntidote));
            }
        }

        private void ActivateSandmasterIfApplicable(Battle plan)
        {
            var locationWithResources = CurrentBattle.Territory.Locations.FirstOrDefault(l => ResourcesOnPlanet.ContainsKey(l));

            if (locationWithResources != null && SkilledAs(plan.Hero, LeaderSkill.Sandmaster) && plan.Player.AnyForcesIn(CurrentBattle.Territory) > 0)
            {
                Log(LeaderSkill.Sandmaster, " adds ", Payment(3), " to ", CurrentBattle.Territory);
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
                        Log(player.Faction, LeaderSkill.Smuggler, " collects ", Payment(collected), " from ", territory);
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

                DeciphererMayReplaceTraitor = plan.Initiator != Faction.Purple && leaderIsSkilled && BattleConcluded.ValidTraitorsToReplace(plan.Player).Any();
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
                Log(Map.TueksSietch, " stronghold advantage: ", player.Faction, " collect ", Payment(2), " for playing ", card);
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
                    Log(Map.SietchTabr, " stronghold advantage: ", playerPlan.Initiator, " collect ", Payment(collected), " from enemy force dial");
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
                Log(Auditee.Faction, " don't have cards to audit");
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
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
                    return CurrentBattle.TreacheryOfOpponent(brown).TraitorCalled;
                }
                return false;
            }
        }

        private bool BlackMustDecideToCapture => Version >= 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders) && !Prevented(FactionAdvantage.BlackCaptureLeader);

        #endregion

        #region BattleOutcome

        public void HandleBattleOutcome(Battle agg, Battle def)
        {
            LogIf(this.BattleOutcome.AggHeroSkillBonus != 0, agg.Hero, " ", this.BattleOutcome.AggActivatedBonusSkill, " bonus: ", this.BattleOutcome.AggHeroSkillBonus);
            LogIf(this.BattleOutcome.DefHeroSkillBonus != 0, def.Hero, " ", this.BattleOutcome.DefActivatedBonusSkill, " bonus: ", this.BattleOutcome.DefHeroSkillBonus);

            LogIf(this.BattleOutcome.AggBattlePenalty != 0, agg.Hero, " ", this.BattleOutcome.DefActivatedPenaltySkill, " penalty: ", this.BattleOutcome.AggBattlePenalty);
            LogIf(this.BattleOutcome.DefBattlePenalty != 0, def.Hero, " ", this.BattleOutcome.AggActivatedPenaltySkill, " penalty: ", this.BattleOutcome.DefBattlePenalty);

            LogIf(this.BattleOutcome.AggMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", this.BattleOutcome.AggMessiahContribution);
            LogIf(this.BattleOutcome.DefMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", this.BattleOutcome.DefMessiahContribution);

            BattleWinner = this.BattleOutcome.Winner.Faction;
            BattleLoser = this.BattleOutcome.Loser.Faction;

            if (this.BattleOutcome.AggHeroKilled)
            {
                KillLeaderInBattle(agg.Hero, this.BattleOutcome.AggHeroCauseOfDeath, this.BattleOutcome.Winner, this.BattleOutcome.AggHeroEffectiveStrength);
            }
            else
            {
                LogIf(this.BattleOutcome.AggSavedByCarthag, Map.Carthag, " stronghold advantage saves ", agg.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (this.BattleOutcome.DefHeroKilled)
            {
                KillLeaderInBattle(def.Hero, this.BattleOutcome.DefHeroCauseOfDeath, this.BattleOutcome.Winner, this.BattleOutcome.DefHeroEffectiveStrength);
            }
            else
            {
                LogIf(this.BattleOutcome.DefSavedByCarthag, Map.Carthag, " stronghold advantage saves ", def.Hero, " from death by ", TreacheryCardType.Poison);
            }

            if (BattleInitiated.IsAggressorByJuice(this, def.Player.Faction))
            {
                Log(agg.Initiator, " (defending) strength: ", this.BattleOutcome.AggTotal);
                Log(def.Initiator, " (aggressor by ", TreacheryCardType.Juice, ") strength: ", this.BattleOutcome.DefTotal);
            }
            else
            {
                Log(agg.Initiator, " (aggressor) strength: ", this.BattleOutcome.AggTotal);
                Log(def.Initiator, " (defending) strength: ", this.BattleOutcome.DefTotal);
            }

            Log(this.BattleOutcome.Winner.Faction, " WIN THE BATTLE");

            bool loserMayRetreat =
                !BattleOutcome.LoserHeroKilled &&
                SkilledAs(BattleOutcome.LoserBattlePlan.Hero, LeaderSkill.Diplomat) &&
                (Retreat.MaxForces(this, BattleOutcome.Loser) > 0 || Retreat.MaxSpecialForces(this, BattleOutcome.Loser) > 0) &&
                Retreat.ValidTargets(this, BattleOutcome.Loser).Any();

            Enter(loserMayRetreat, Phase.Retreating, HandleForceLosses);
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

            int aggHeroContribution = result.AggHeroKilled || (Version < 145 && rockMelterUsed)? 0 : result.AggHeroEffectiveStrength + result.AggHeroSkillBonus + result.AggMessiahContribution - result.AggBattlePenalty;
            int defHeroContribution = result.DefHeroKilled || (Version < 145 && rockMelterUsed)? 0 : result.DefHeroEffectiveStrength + result.DefHeroSkillBonus + result.DefMessiahContribution - result.DefBattlePenalty;

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

            result.AggTotal = aggForceDial + aggHeroContribution ;
            result.DefTotal = defForceDial + defHeroContribution ;

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

            Log(loser.Faction, " lose all ", loser.AnyForcesIn(territory), " forces in ", territory);
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
                    LogIf(HasStrongholdAdvantage(p.Faction, StrongholdAdvantage.FreeResourcesForBattles, CurrentBattle.Territory),
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
                Log(p.Faction, " paid ", Payment(plan.BankerBonus), " as ", LeaderSkill.Banker);
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
                        Log(Faction.Brown, " get ", Payment(result), " from supported forces");

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

                Log(
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

        private void KillLeaderInBattle(IHero killedHero, TreacheryCardType causeOfDeath, Player winner, int heroValue)
        {
            Log(causeOfDeath, " kills ", killedHero, " → ", winner.Faction, " get ", Payment(heroValue));
            RecentMilestones.Add(Milestone.LeaderKilled);
            if (killedHero is Leader) KillHero(killedHero as Leader);
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

        private void OneTraitorCalled(Territory territory, Player winner, Player loser, Battle loserGambit, Battle winnerGambit)
        {
            bool hadMessiahBeforeLosses = loser.MessiahAvailable;

            var traitor = loserGambit.Hero;
            var traitorValue = traitor.ValueInCombatAgainst(winnerGambit.Hero);
            var traitorOwner = winner.Traitors.Any(t => t.IsTraitor(traitor)) ? winner.Faction : Faction.Black;

            Log(traitor, " is a ", traitorOwner, " traitor! ", loser.Faction, " lose everything");

            RecentMilestones.Add(Milestone.LeaderKilled);

            if (traitor is Leader)
            {
                Log("Treachery kills ", traitor, " → ", winner.Faction, " get ", Payment(traitorValue));
                KillHero(traitor);
                winner.Resources += traitorValue;
            }

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            Log(loser.Faction, " lose all ", loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), " forces in ", territory);
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

            Log("Treachery kills both ", defLeader, " and ", aggLeader);
            KillHero(defLeader);
            KillHero(aggLeader);
            Log(defender.Faction, " lose all ", defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), " forces in ", territory);
            defender.KillAllForces(territory, true);
            Log(aggressor.Faction, " lose all ", aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), " forces in ", territory);
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

            Log("A ", TreacheryCardType.Laser, "/", TreacheryCardType.Shield, " explosion occurs!");
            RecentMilestones.Add(Milestone.Explosion);

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

            LoseCards(agg);
            PayDialedSpice(aggressor, agg, false);

            LoseCards(def);
            PayDialedSpice(defender, def, false);

            int removed = RemoveResources(territory);
            if (removed > 0)
            {
                Log("The explosion destroys ", Payment(removed), " in ", territory);
            }

            foreach (var p in Players)
            {
                RevealCurrentNoField(p, territory);

                int numberOfForces = p.AnyForcesIn(territory);
                if (numberOfForces > 0)
                {
                    Log("The explosion kills all ", numberOfForces, p.Faction, " forces in ", territory);
                    p.KillAllForces(territory, true);
                }
            }

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        #endregion

        #region BattleConclusion

        public void HandleEvent(BattleConcluded e)
        {
            var winner = GetPlayer(e.Initiator);

            foreach (var c in e.DiscardedCards)
            {
                Log(e.Initiator, " discard ", c);
                winner.TreacheryCards.Remove(c);
                TreacheryDiscardPile.PutOnTop(c);
            }

            if (TraitorsDeciphererCanLookAt.Count > 0)
            {
                Log(e.Initiator, " look at ", TraitorsDeciphererCanLookAt.Count, " leaders in the traitor deck");
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

        public Leader BlackVictim { get; private set; }
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
                Log(victim.Faction, " don't have any leaders for ", Faction.Black, " to capture or kill");
            }
        }

        public Dictionary<Leader, Faction> CapturedLeaders { get; private set; } = new Dictionary<Leader, Faction>();
        private void CaptureOrAssassinateLeader(Battle harkonnenAction, Battle victimAction, CaptureDecision decision)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var target = GetPlayer(victimAction.Initiator);

            if (decision == CaptureDecision.Capture)
            {
                Log(Faction.Black, " capture a leader!");
                harkonnen.Leaders.Add(BlackVictim);
                target.Leaders.Remove(BlackVictim);
                SetInFrontOfShield(BlackVictim, false);
                CapturedLeaders.Add(BlackVictim, target.Faction);
            }
            else if (decision == CaptureDecision.Kill)
            {
                Log(Faction.Black, " kill a leader for ", Payment(2));
                RecentMilestones.Add(Milestone.LeaderKilled);
                AssassinateLeader(BlackVictim);
                harkonnen.Resources += 2;
            }
            else if (decision == CaptureDecision.DontCapture)
            {
                Log(Faction.Black, " decide not to capture or kill a leader");
            }
        }

        private void DeciphererReplacesTraitors(BattleConcluded e)
        {
            Log(e.Initiator, " replaced ", e.ReplacedTraitor, " by another traitor from the deck");

            e.Player.Traitors.Add(e.NewTraitor);
            TraitorsDeciphererCanLookAt.Remove(e.NewTraitor);

            e.Player.Traitors.Remove(e.ReplacedTraitor);
            TraitorDeck.PutOnTop(e.ReplacedTraitor);

            RecentMilestones.Add(Milestone.Shuffled);
            TraitorDeck.Shuffle();
        }


        private void FinishBattle()
        {
            GreenKarma = false;
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
            foreach (var ls in LeaderState)
            {
                if (ls.Key is Leader l && Skilled(l) && !CapturedLeaders.ContainsKey(l) && !ls.Value.InFrontOfShield)
                {
                    ls.Value.InFrontOfShield = true;

                    if (IsAlive(l))
                    {
                        Log(Skill(l), " ", l, " is placed back in front of shield");
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

        private void ProcessGreyForceLossesAndSubstitutions(BattleConcluded e, Player winner)
        {
            if (GreySpecialForceLossesToTake > 0)
            {
                var plan = CurrentBattle.PlanOf(winner);
                var territory = CurrentBattle.Territory;

                var winnerGambit = WinnerBattleAction;
                int forcesToLose = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength + e.SpecialForceLossesReplaced;
                int specialForcesToLose = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength - e.SpecialForceLossesReplaced;

                Log(winner.Faction, " substitute ", e.SpecialForceLossesReplaced, winner.SpecialForce, " losses by ", winner.Force, " losses");

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

                    Log(
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
                    Log(e.Initiator, " steal ", e.StolenToken, " from ", BattleLoser);
                }
            }
        }

        #endregion

        #region FaceDancing

        public void HandleEvent(FaceDanced f)
        {
            if (f.FaceDancerCalled)
            {
                var initiator = GetPlayer(f.Initiator);
                var facedancer = initiator.FaceDancers.FirstOrDefault(f => WinnerHero.IsFaceDancer(f));
                Log(f.Initiator, " reveal ", facedancer, " as one of their Face Dancers!");

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
                Log(f.Initiator, " don't reveal a Face Dancer");
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

                Log(nrOfRemovedForces, " ", winner.Faction, " forces go back to reserves and are replaced by ", f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Player.Force, " (", f.ForcesFromReserve, " from reserves", DetermineSourceLocations(f), ")");
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
            Log(f.Initiator, " draw 3 new Face Dancers.");
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

        #endregion

        #region PostBattle

        public Diplomacy CurrentDiplomacy { get; private set; }
        public void HandleEvent(Diplomacy e)
        {
            Log(e.GetDynamicMessage());
            CurrentDiplomacy = e;
        }

        public IList<TreacheryCard> AuditedCards { get; private set; }

        public void HandleEvent(Captured e)
        {
            Log(e);
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
            HandleForceLosses();
            FlipBeneGesseritWhenAlone();
            DetermineHowToProceedAfterRevealingBattlePlans();
        }

        public void HandleEvent(AuditCancelled e)
        {
            Log(e.GetDynamicMessage());

            if (e.Cancelled)
            {
                e.Player.Resources -= e.Cost();
                GetPlayer(Faction.Brown).Resources += e.Cost();
            }

            if (!e.Cancelled)
            {
                Enter(Phase.Auditing);
                LogTo(e.Initiator, Faction.Brown, " see: ", AuditedCards);
            }
            else
            {
                Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        public void HandleEvent(Audited e)
        {
            Log(e);

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

        #endregion

        #region Support

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

        public Battle WinnerBattleAction
        {
            get
            {
                if (AggressorBattleAction != null && AggressorBattleAction.Initiator == BattleWinner) return AggressorBattleAction;
                if (DefenderBattleAction != null && DefenderBattleAction.Initiator == BattleWinner) return DefenderBattleAction;

                return null;
            }
        }

        #endregion
    }
}
