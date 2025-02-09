/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public partial class Game
{
    #region State

    public PlayerSequence BattleSequence { get; internal set; }
    public BattleInitiated BattleAboutToStart { get; internal set; }
    public BattleInitiated CurrentBattle { get; internal set; }
    public Battle AggressorPlan { get; internal set; }
    public TreacheryCalled AggressorTraitorAction { get; internal set; }
    public Battle DefenderPlan { get; internal set; }
    public TreacheryCalled DefenderTraitorAction { get; internal set; }
    public BattleOutcome BattleOutcome { get; internal set; }
    public Faction BattleWinner { get; internal set; }
    public Faction BattleLoser { get; internal set; }
    internal int NrOfBattlesFought { get; set; }

    public Faction CurrentPinkOrAllyFighter { get; internal set; }
    public int CurrentPinkBattleContribution { get; internal set; }

    public Voice CurrentVoice { get; internal set; }
    public Prescience CurrentPrescience { get; internal set; }
    public Thought CurrentThought { get; internal set; }
    public StrongholdAdvantage ChosenHmsAdvantage { get; internal set; }

    public PortableAntidoteUsed CurrentPortableAntidoteUsed { get; internal set; }
    internal bool PoisonToothCancelled { get; set; }
    internal RockWasMelted CurrentRockWasMelted { get; set; }

    public List<IHero> TraitorsDeciphererCanLookAt { get; } = new();
    public bool DeciphererMayReplaceTraitor { get; private set; }
    public bool LoserMayTryToAssassinate { get; internal set; }
    public bool BattleWinnerMayChooseToDiscard { get; internal set; } = true;
    internal bool SecretAllyAllowsKeepingCardsAfterLosingBattle { get; set; }
    public List<TreacheryCard> CardsToBeDiscardedByLoserAfterBattle { get; } = new();
    public Diplomacy CurrentDiplomacy { get; internal set; }
    public List<TreacheryCard> AuditedCards { get; } = new();
    public Leader BlackVictim { get; internal set; }
    public int GreySpecialForceLossesToTake { get; internal set; }
    internal TriggeredBureaucracy BattleTriggeredBureaucracy { get; set; }
    internal TreacheryCard CardUsedByDiplomat { get; set; }
    internal bool AuditorSurvivedBattle { get; set; }

    #endregion State

    #region BattleInitiation

    internal void InitiateBattle()
    {
        CurrentBattle = BattleAboutToStart;
        ChosenHmsAdvantage = StrongholdAdvantage.None;
        BattleOutcome = null;
        NrOfBattlesFought++;

        AnnounceHeroAvailability(CurrentBattle.AggressivePlayer);
        AnnounceHeroAvailability(CurrentBattle.DefendingPlayer);
        AssignBattleWheels(CurrentBattle.AggressivePlayer, CurrentBattle.DefendingPlayer);
    }

    private void AssignBattleWheels(params Player[] players)
    {
        HasBattleWheel.Clear();
        foreach (var p in players) HasBattleWheel.Add(p.Faction);
    }

    private void AnnounceHeroAvailability(Player p)
    {
        if (!Battle.ValidBattleHeroes(this, p).Any()) Log(p.Faction, " have no leaders available for this battle");
    }

    #endregion

    #region BattleResolution

    internal void HandleRevealedBattlePlans()
    {
        ResolveEffectOfOwnedTueksSietch(AggressorPlan);
        ResolveEffectOfOwnedTueksSietch(DefenderPlan);

        DiscardOneTimeCardsUsedInBattle(AggressorTraitorAction, DefenderTraitorAction);

        ResolveBattle(CurrentBattle, AggressorPlan, DefenderPlan, AggressorTraitorAction, DefenderTraitorAction);

        if (CardUsedByDiplomat != null)
        {
            Discard(CardUsedByDiplomat);
            CardUsedByDiplomat = null;
        }

        if (AggressorPlan.Initiator == BattleWinner) ActivateDeciphererIfApplicable(AggressorPlan);
        if (DefenderPlan.Initiator == BattleWinner) ActivateDeciphererIfApplicable(DefenderPlan);

        if (AggressorPlan.Initiator == BattleWinner) ActivateSandmasterIfApplicable(AggressorPlan);
        if (DefenderPlan.Initiator == BattleWinner) ActivateSandmasterIfApplicable(DefenderPlan);

        if (AggressorPlan.Initiator == BattleWinner) ResolveEffectOfOwnedSietchTabr(AggressorPlan, DefenderPlan);
        if (DefenderPlan.Initiator == BattleWinner) ResolveEffectOfOwnedSietchTabr(DefenderPlan, AggressorPlan);

        if (AggressorPlan.Initiator == BattleWinner) ResolveEffectOfOccupiedJacurutu(AggressorPlan, BattleOutcome.DefUndialedForces);
        if (DefenderPlan.Initiator == BattleWinner) ResolveEffectOfOccupiedJacurutu(DefenderPlan, BattleOutcome.AggUndialedForces);

        if (Version < 116) CaptureLeaderIfApplicable();

        FlipBlueAdvisorsWhenAlone();
        if (Version >= 162) DetermineOccupation(CurrentBattle.Territory);
            
        if (BattleTriggeredBureaucracy != null)
        {
            ApplyBureaucracy(BattleTriggeredBureaucracy.PaymentFrom, BattleTriggeredBureaucracy.PaymentTo);
            BattleTriggeredBureaucracy = null;
        }

        if (CurrentPhase != Phase.Retreating) DetermineHowToProceedAfterRevealingBattlePlans();
    }

    private void DiscardOneTimeCardsUsedInBattle(TreacheryCalled aggressorCall, TreacheryCalled defenderCall)
    {
        var aggressorKeepsCards = aggressorCall.Succeeded && !defenderCall.Succeeded;
        if (!aggressorKeepsCards) DiscardOneTimeCards(AggressorPlan);

        var defenderKeepsCards = defenderCall.Succeeded && !aggressorCall.Succeeded;
        if (!defenderKeepsCards) DiscardOneTimeCards(DefenderPlan);
    }

    private void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
    {
        BattleOutcome = Battle.DetermineBattleOutcome(agg, def, b.Territory, this);

        var lasgunShield = !aggtrt.Succeeded && !deftrt.Succeeded && (agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield);

        ActivateSmuggler(aggtrt, deftrt, BattleOutcome, lasgunShield);

        HandleReinforcements(agg);
        HandleReinforcements(def);

        if (aggtrt.Succeeded || deftrt.Succeeded)
        {
            TraitorCalled(b, agg, def, deftrt, agg.Hero, def.Hero);
        }
        else if (lasgunShield)
        {
            LasgunShieldExplosion(agg, def, agg.Player, def.Player, b.Territory, agg.Hero, def.Hero);
        }
        else
        {
            SetHeroLocations(agg, b.Territory);
            SetHeroLocations(def, b.Territory);
            HandleBattleOutcome(agg, def, b.Territory);
        }

        DetermineIfCapturedLeadersMustBeReleased();
        AuditorSurvivedBattle = (agg.Hero?.HeroType == HeroType.Auditor && IsAlive(agg.Hero)) || (def.Hero?.HeroType == HeroType.Auditor && IsAlive(def.Hero));
    }

    internal bool BlackMustDecideToCapture => Version >= 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders) && !Prevented(FactionAdvantage.BlackCaptureLeader);

    private void CaptureLeaderIfApplicable()
    {
        if (Version < 116 && BattleWinner == Faction.Black && Applicable(Rule.BlackCapturesOrKillsLeaders))
        {
            if (!Prevented(FactionAdvantage.BlackCaptureLeader))
                CaptureLeader();
            else
                LogPreventionByKarma(FactionAdvantage.BlackCaptureLeader);
        }
    }

    internal void CaptureLeader()
    {
        if (AggressorPlan.By(BattleWinner))
            SelectVictimOfBlackWinner(AggressorPlan, DefenderPlan);
        else
            SelectVictimOfBlackWinner(DefenderPlan, AggressorPlan);
    }

    private void HandleReinforcements(Battle plan)
    {
        if (plan.HasReinforcements)
        {
            var forcesToRemove = Math.Min(3, plan.Player.ForcesInReserve);
            plan.Player.AddForcesToReserves(-forcesToRemove);
            plan.Player.ForcesKilled += forcesToRemove;

            var specialForcesToRemove = 3 - forcesToRemove;
            if (specialForcesToRemove > 0)
            {
                plan.Player.AddSpecialForcesToReserves(-specialForcesToRemove);
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
        var aggHeroSurvives = !deftrt.Succeeded && (aggtrt.Succeeded || (!lasgunShield && !outcome.AggHeroKilled));
        var defHeroSurvives = !aggtrt.Succeeded && (deftrt.Succeeded || (!lasgunShield && !outcome.DefHeroKilled));

        if (aggHeroSurvives) ActivateSmugglerIfApplicable(AggressorPlan.Player, AggressorPlan.Hero, DefenderPlan.Hero, CurrentBattle.Territory);

        if (defHeroSurvives) ActivateSmugglerIfApplicable(DefenderPlan.Player, DefenderPlan.Hero, AggressorPlan.Hero, CurrentBattle.Territory);
    }

    private void DiscardOneTimeCards(Battle plan)
    {
        if (plan.Hero != null && plan.Hero is TreacheryCard) Discard(plan.Hero as TreacheryCard);

        if (plan.Weapon != null)
            if (plan.Weapon.IsArtillery ||
                plan.Weapon.IsMirrorWeapon ||
                plan.Weapon.IsRockMelter ||
                (plan.Weapon.IsPoisonTooth && !PoisonToothCancelled) ||
                !(plan.Weapon.IsWeapon || plan.Weapon.IsDefense || plan.Weapon.IsUseless) ||
                (CurrentDiplomacy != null && plan.Initiator == CurrentDiplomacy.Initiator && plan.Weapon == CurrentDiplomacy.Card))
                Discard(plan.Weapon);

        if (plan.Defense != null)
            if (plan.Defense.IsPortableAntidote || 
                (Version >= 158 && !(plan.Defense.IsWeapon || plan.Defense.IsDefense || plan.Defense.IsUseless)))
                Discard(plan.Defense);

        if (CurrentPortableAntidoteUsed != null && CurrentPortableAntidoteUsed.Player == plan.Player) Discard(CurrentPortableAntidoteUsed.Player.Card(TreacheryCardType.PortableAntidote));

        if (Version >= 146 && CurrentRockWasMelted != null && CurrentRockWasMelted.Player == plan.Player) Discard(CurrentRockWasMelted.Player.Card(TreacheryCardType.PortableAntidote));
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
                var collected = Math.Min(ResourcesOnPlanet[locationWithResources], hero.ValueInCombatAgainst(opponentHero));
                if (collected > 0)
                {
                    Log(player.Faction, LeaderSkill.Smuggler, " collects ", Payment.Of(collected), " from ", territory);
                    ChangeResourcesOnPlanet(locationWithResources, -collected);
                    player.Resources += collected;
                }
            }
        }
    }

    private void ActivateDeciphererIfApplicable(Battle plan)
    {
        var playerIsSkilled = SkilledAs(plan.Player, LeaderSkill.Decipherer);
        var leaderIsSkilled = SkilledAs(plan.Hero, LeaderSkill.Decipherer);

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
            foreach (var item in TraitorsDeciphererCanLookAt) TraitorDeck.PutOnTop(item);

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
            var collected = (int)Math.Floor(opponentPlan.Dial(this, winnerPlan.Initiator));
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
            if (opponentUndialedForces > 0)
            {
                Log(winnerPlan.Initiator, " get ", Payment.Of(opponentUndialedForces), " from winning a fight in ", Map.Jacurutu);
                winnerPlan.Player.Resources += opponentUndialedForces;
            }
    }

    internal void DetermineHowToProceedAfterRevealingBattlePlans()
    {
        if (Auditee != null && !BrownLeaderWasRevealedAsTraitor)
        {
            PrepareAudit();
        }
        else
        {
            Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            LetFactionsDiscardSurplusCards();
        }
    }

    private void PrepareAudit()
    {
        var auditableCards = new Deck<TreacheryCard>(AuditCancelled.GetCardsThatMayBeAudited(this), Random);

        if (auditableCards.Items.Count > 0)
        {
            var nrOfAuditedCards = AuditCancelled.GetNumberOfCardsThatMayBeAudited(this);
            AuditedCards.Clear();
            auditableCards.Shuffle();
            for (var i = 0; i < nrOfAuditedCards; i++) AuditedCards.Add(auditableCards.Draw());

            Enter(Phase.AvoidingAudit);
        }
        else
        {
            Log(Auditee.Faction, " don't have cards to audit");
            Enter(BattleWinner == Faction.None, FinishBattle, BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
        }
            
        LetFactionsDiscardSurplusCards();
    }

    private void DetermineIfCapturedLeadersMustBeReleased()
    {
        var black = GetPlayer(Faction.Black);

        if (black != null)
        {
            //DetermineIfDeadLeaderMustBeReleased
            var deadCaptives = black.Leaders.Where(l => CapturedLeaders.ContainsKey(l) && !IsAlive(l)).ToList();
            foreach (var captive in deadCaptives) ReturnCapturedLeader(black, captive);

            //DetermineIfLeaderUsedInBattleMustBeReleased
            var usedLeaderInBattle = CurrentBattle?.PlanOf(black)?.Hero;
            if (usedLeaderInBattle != null && usedLeaderInBattle is Leader leader && CapturedLeaders.ContainsKey(leader)) ReturnCapturedLeader(black, leader);

            //DetermineIfCapturedLeadersMustBeReleasedWhenBlackHasNoLeadersLeft
            if (!black.Leaders.Any(l => !CapturedLeaders.ContainsKey(l) && IsAlive(l)))
            {
                var captives = black.Leaders.Where(l => CapturedLeaders.ContainsKey(l)).ToList();
                foreach (var captive in captives) ReturnCapturedLeader(black, captive);
            }
        }
    }

    private void ReturnCapturedLeader(Player currentOwner, Leader toReturn)
    {
        if (CapturedLeaders.TryGetValue(toReturn, out var value))
        {
            var originalPlayer = GetPlayer(value);
            originalPlayer.Leaders.Add(toReturn);
            currentOwner.Leaders.Remove(toReturn);
            CapturedLeaders.Remove(toReturn);
            if (IsSkilled(toReturn)) SetInFrontOfShield(toReturn, true);
            Log(toReturn, " returns to ", originalPlayer.Faction, " after working for ", currentOwner.Faction);
        }
    }

    private bool BrownLeaderWasRevealedAsTraitor
    {
        get
        {
            var brown = GetPlayer(Faction.Brown);
            if (brown != null && CurrentBattle.IsAggressorOrDefender(brown)) return CurrentBattle.TreacheryOfOpponent(brown).Succeeded;
            return false;
        }
    }

    #endregion

    #region BattleOutcome

    private void HandleBattleOutcome(Battle agg, Battle def, Territory territory)
    {
        LogIf(BattleOutcome.AggHeroSkillBonus != 0, agg.Hero, " ", BattleOutcome.AggActivatedBonusSkill, " bonus: ", BattleOutcome.AggHeroSkillBonus);
        LogIf(BattleOutcome.DefHeroSkillBonus != 0, def.Hero, " ", BattleOutcome.DefActivatedBonusSkill, " bonus: ", BattleOutcome.DefHeroSkillBonus);

        LogIf(BattleOutcome.AggBattlePenalty != 0, agg.Hero, " ", BattleOutcome.DefActivatedPenaltySkill, " penalty: ", BattleOutcome.AggBattlePenalty);
        LogIf(BattleOutcome.DefBattlePenalty != 0, def.Hero, " ", BattleOutcome.AggActivatedPenaltySkill, " penalty: ", BattleOutcome.DefBattlePenalty);

        LogIf(BattleOutcome.AggMessiahContribution > 0, agg.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.AggMessiahContribution);
        LogIf(BattleOutcome.DefMessiahContribution > 0, def.Hero, " ", Concept.Messiah, " bonus: ", BattleOutcome.DefMessiahContribution);

        BattleWinner = BattleOutcome.Winner.Faction;
        BattleLoser = BattleOutcome.Loser.Faction;

        if (BattleOutcome.AggHeroKilled)
            KillLeaderInBattle(agg.Hero, BattleOutcome.AggHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.AggHeroEffectiveStrength);
        else
            LogIf(BattleOutcome.AggSavedByCarthag, Map.Carthag, " stronghold advantage saves ", agg.Hero, " from death by ", TreacheryCardType.Poison);

        if (BattleOutcome.DefHeroKilled)
            KillLeaderInBattle(def.Hero, BattleOutcome.DefHeroCauseOfDeath, BattleOutcome.Winner, BattleOutcome.DefHeroEffectiveStrength);
        else
            LogIf(BattleOutcome.DefSavedByCarthag, Map.Carthag, " stronghold advantage saves ", def.Hero, " from death by ", TreacheryCardType.Poison);

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

        var loserMayRetreat =
            !BattleOutcome.LoserHeroKilled &&
            SkilledAs(BattleOutcome.LoserBattlePlan.Hero, LeaderSkill.Diplomat) &&
            Retreat.MaxTotalForces(this, BattleOutcome.Loser) > 0 &&
            (Retreat.MaxForces(this, BattleOutcome.Loser) > 0 || Retreat.MaxSpecialForces(this, BattleOutcome.Loser) > 0) &&
            Retreat.ValidTargets(this, BattleOutcome.Loser).Any();

        Enter(loserMayRetreat, Phase.Retreating, HandleLosses);
    }

    private void HandleHarassAndWithdraw(Battle plan, Territory territory)
    {
        if ((plan.Weapon != null && plan.Weapon.Type == TreacheryCardType.HarassAndWithdraw) ||
            (plan.Defense != null && plan.Defense.Type == TreacheryCardType.HarassAndWithdraw))
        {
            var forceSupplier = Battle.DetermineForceSupplier(this, plan.Player);
            var undialledNormalForces = forceSupplier.ForcesIn(CurrentBattle.Territory) - plan.Forces - plan.ForcesAtHalfStrength;
            var undialledSpecialForces = forceSupplier.SpecialForcesIn(CurrentBattle.Territory) - plan.SpecialForces - plan.SpecialForcesAtHalfStrength;
            forceSupplier.ForcesToReserves(territory, undialledNormalForces, false);
            forceSupplier.ForcesToReserves(territory, undialledSpecialForces, true);

            if (undialledNormalForces + undialledSpecialForces > 0)
                Log(
                    plan.Initiator,
                    " withdraw ",
                    MessagePart.ExpressIf(undialledNormalForces > 0, undialledNormalForces, forceSupplier.Force),
                    MessagePart.ExpressIf(undialledNormalForces > 0 && undialledSpecialForces > 0, " and "),
                    MessagePart.ExpressIf(undialledSpecialForces > 0, undialledSpecialForces, forceSupplier.SpecialForce),
                    " to reserves");
        }
    }

    internal void HandleLosses()
    {
        ProcessWinnerLosses(CurrentBattle.Territory, BattleOutcome.Winner, BattleOutcome.WinnerBattlePlan, false);
        ProcessLoserLosses(CurrentBattle.Territory, BattleOutcome.Loser, BattleOutcome.LoserBattlePlan);
    }

    internal bool IsProtectedByCarthagAdvantage(Battle plan, Territory territory)
    {
        return HasStrongholdAdvantage(plan.Initiator, StrongholdAdvantage.CountDefensesAsAntidote, territory) &&
               !plan.HasPoison && !plan.HasPoisonTooth && plan.Defense != null && plan.Defense.IsDefense;
    }

    private void ProcessLoserLosses(Territory territory, Player loser, Battle loserGambit)
    {
        var hadMessiahBeforeLosses = loser.MessiahAvailable;
        var forceSupplier = Battle.DetermineForceSupplier(this, loser);
        ProcessPinkOccupationLoserLosses(territory, loser);

        Log(forceSupplier.Faction, " lose all ", forceSupplier.AnyForcesIn(territory), " forces ", InOrOn(territory), territory);
        PayDialedSpice(loser, loserGambit, false);
        forceSupplier.KillAllForces(territory, true);
        LoseCards(loserGambit, MayKeepCardsAfterLosingBattle(loser));
        
        if (loser.MessiahAvailable && !hadMessiahBeforeLosses) Stone(Milestone.Messiah);
    }

    private void ProcessPinkOccupationWinnerLosses(Territory territory, Player winner)
    {
        if (CurrentPinkBattleContribution > 0 && winner.OrAllyIs(Faction.Pink))
        {
            var pink = GetPlayer(Faction.Pink);
            if (pink != null)
            {
                pink.KillForces(territory, CurrentPinkBattleContribution, false, true);
                Log(Faction.Pink, " lose ", CurrentPinkBattleContribution, pink.Force, InOrOn(territory), territory);
            }
        }
    }

    private void ProcessPinkOccupationLoserLosses(Territory territory, Player loser)
    {
        if (CurrentPinkBattleContribution > 0 && loser.OrAllyIs(Faction.Pink))
        {
            var pink = GetPlayer(Faction.Pink);
            if (pink != null)
            {
                Log(Faction.Pink, " lose all ", pink.AnyForcesIn(territory), " forces ", InOrOn(territory), territory);
                pink.KillAllForces(territory, true);
            }
        }
    }

    private bool MayKeepCardsAfterLosingBattle(Player p)
    {
        return (p.Ally == Faction.Cyan && CyanAllowsKeepingCards) ||
               (p.Nexus == Faction.Cyan && NexusPlayed.CanUseSecretAlly(this, p));
    }

    private bool DialledResourcesAreRefunded(Player p)
    {
        return Applicable(Rule.YellowAllyGetsDialedResourcesRefunded) && p.Ally == Faction.Yellow &&
               YellowRefundsBattleDial;
    }

    private void PayDialedSpice(Player p, Battle plan, bool traitorWasRevealed)
    {
        var cost = plan.Cost(this, out var paidByArrakeen);
        var costToBrown = p.Ally == Faction.Brown ? plan.AllyContributionAmount : 0;

        if (paidByArrakeen > 0) Log(Map.Arrakeen, " stronghold advantage supports ", Payment.Of(paidByArrakeen));

        if (cost + paidByArrakeen > 0)
        {
            var costForPlayer = cost - plan.AllyContributionAmount;
            var refundedResources = 0;

            if (costForPlayer > 0)
            {
                p.Resources -= costForPlayer;

                if (DialledResourcesAreRefunded(p))
                {
                    Log(Payment.Of(costForPlayer), " dialled in battle will be refunded in the ", MainPhase.Contemplate, " phase");
                    refundedResources = costForPlayer;
                    p.Bribes += costForPlayer;
                }
            }

            if (plan.AllyContributionAmount > 0)
            {
                p.AlliedPlayer.Resources -= plan.AllyContributionAmount;
                if (Version >= 117) DecreasePermittedUseOfAllySpice(p.Faction, plan.AllyContributionAmount);
            }

            var dialledResourcesRelevantForBrown = cost - costToBrown - refundedResources;
            if (Version >= 155) dialledResourcesRelevantForBrown += paidByArrakeen;

            var receiverProfit = HandleBrownIncome(p, dialledResourcesRelevantForBrown, traitorWasRevealed);

            if (cost - receiverProfit >= 4) ActivateBanker(p);
        }

        if (plan.BankerBonus > 0)
        {
            p.Resources -= plan.BankerBonus;
            Log(p.Faction, " paid ", Payment.Of(plan.BankerBonus), " as ", LeaderSkill.Banker);
        }
    }

    private int HandleBrownIncome(Player paidBy, int costsExcludingPaymentByBrownAlly, bool traitorWasRevealed)
    {
        var result = 0;

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

                    if (result >= 5) BattleTriggeredBureaucracy = new TriggeredBureaucracy { PaymentFrom = paidBy.Faction, PaymentTo = Faction.Brown };
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
        ProcessPinkOccupationWinnerLosses(territory, winner);

        var specialForcesToLose = plan.SpecialForces + plan.SpecialForcesAtHalfStrength;
        var forcesToLose = plan.Forces + plan.ForcesAtHalfStrength;

        var specialForcesToSaveToReserves = 0;
        var forcesToSaveToReserves = 0;
        var specialForcesToSaveInTerritory = 0;
        var forcesToSaveInTerritory = 0;

        var nrOfSpecialForceLossesThatCanBeReplacedByRemainingNormalForces = Math.Min(specialForcesToLose,
            forceSupplier.ForcesIn(territory) - plan.Forces - plan.ForcesAtHalfStrength);
        
        if (!MaySubstituteForceLosses(forceSupplier) || Version >= 164 && nrOfSpecialForceLossesThatCanBeReplacedByRemainingNormalForces == 0)
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
                MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " and "),
                MessagePart.ExpressIf(forcesToSaveToReserves > 0, forcesToSaveToReserves, forceSupplier.Force),
                MessagePart.ExpressIf(specialForcesToSaveToReserves > 0, specialForcesToSaveToReserves, forceSupplier.SpecialForce),
                MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " to reserves"));
        }

        if (!MaySubstituteForceLosses(forceSupplier) || specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory == 0 || forceSupplier.ForcesIn(territory) <= plan.Forces + plan.ForcesAtHalfStrength)
        {
            var winnerForcesLost = forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory;
            var winnerSpecialForcesLost = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
            HandleForceLosses(territory, forceSupplier, winnerForcesLost, winnerSpecialForcesLost);
        }
        else
        {
            GreySpecialForceLossesToTake = specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory;
        }
    }

        

    private bool MaySubstituteForceLosses(Player p)
    {
        return p.Faction == Faction.Grey && (Version < 113 || !Prevented(FactionAdvantage.GreyReplacingSpecialForces));
    }

    internal void HandleForceLosses(Territory territory, Player player, int forcesLost, int specialForcesLost)
    {
        var hadMessiahBeforeLosses = player.MessiahAvailable;

        player.KillForces(territory, forcesLost, false, true);
        player.KillForces(territory, specialForcesLost, true, true);

        LogLosses(player, forcesLost, specialForcesLost);

        if (player.MessiahAvailable && !hadMessiahBeforeLosses) Stone(Milestone.Messiah);
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
            Log(
                player.Faction,
                " lose ",
                MessagePart.ExpressIf(forcesLost > 0, forcesLost, player.Force),
                MessagePart.ExpressIf(specialForcesLost > 0, specialForcesLost, player.SpecialForce),
                " during battle ");
    }

    private void SetHeroLocations(Battle b, Territory territory)
    {
        if (b.Hero != null && b.Hero is Leader) LeaderState[b.Hero].CurrentTerritory = territory;

        if (b.Messiah) LeaderState[LeaderManager.Messiah].CurrentTerritory = territory;
    }

    #endregion

    #region NonBattleOutcomes

    private void TraitorCalled(BattleInitiated b, Battle agg, Battle def, TreacheryCalled deftrt, IHero aggLeader, IHero defLeader)
    {
        if (AggressorTraitorAction.Succeeded && deftrt.Succeeded)
        {
            TwoTraitorsCalled(agg, def, agg.Player, def.Player, b.Territory, aggLeader, defLeader);
        }
        else
        {
            var winner = AggressorTraitorAction.Succeeded ? agg.Player : def.Player;
            var loser = AggressorTraitorAction.Succeeded ? def.Player : agg.Player;
            var loserGambit = AggressorTraitorAction.Succeeded ? def : agg;
            var winnerGambit = AggressorTraitorAction.Succeeded ? agg : def;
            OneTraitorCalled(b.Territory, winner, loser, loserGambit, winnerGambit);
        }
    }

    private void OneTraitorCalled(Territory territory, Player winner, Player loser, Battle loserGambit, Battle winnerGambit)
    {
        var hadMessiahBeforeLosses = loser.MessiahAvailable;

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

        ProcessPinkOccupationLoserLosses(territory, loser);

        if (forceSupplierOfLoser != loser)
        {
            Log(forceSupplierOfLoser.Faction, " lose all ", forceSupplierOfLoser.SpecialForcesIn(territory) + forceSupplierOfLoser.ForcesIn(territory), " forces ", InOrOn(territory), territory);
            forceSupplierOfLoser.KillAllForces(territory, true);
        }

        Log(loser.Faction, " lose all ", loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), " forces ", InOrOn(territory), territory);
        loser.KillAllForces(territory, true);
        LoseCards(loserGambit, MayKeepCardsAfterLosingBattle(loser));
        PayDialedSpice(loser, loserGambit, true);

        if (loser.MessiahAvailable && !hadMessiahBeforeLosses) Stone(Milestone.Messiah);
    }

    private void TwoTraitorsCalled(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
    {
        var hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

        Log("Treachery kills both ", defLeader, " and ", aggLeader);
        KillHero(defLeader);
        KillHero(aggLeader);

        var forceSupplierOfDefender = Battle.DetermineForceSupplier(this, defender);
        ProcessPinkOccupationLoserLosses(territory, defender);
        if (forceSupplierOfDefender != defender)
        {
            Log(forceSupplierOfDefender.Faction, " lose all ", forceSupplierOfDefender.SpecialForcesIn(territory) + forceSupplierOfDefender.ForcesIn(territory), " forces ", InOrOn(territory), territory);
            forceSupplierOfDefender.KillAllForces(territory, true);
        }

        Log(defender.Faction, " lose all ", defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), " forces ", InOrOn(territory), territory);
        defender.KillAllForces(territory, true);

        var forceSupplierOfAggressor = Battle.DetermineForceSupplier(this, aggressor);
        ProcessPinkOccupationLoserLosses(territory, aggressor);
        if (forceSupplierOfAggressor != aggressor)
        {
            Log(forceSupplierOfAggressor.Faction, " lose all ", forceSupplierOfAggressor.SpecialForcesIn(territory) + forceSupplierOfAggressor.ForcesIn(territory), " forces ", InOrOn(territory), territory);
            forceSupplierOfAggressor.KillAllForces(territory, true);
        }

        Log(aggressor.Faction, " lose all ", aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), " forces ", InOrOn(territory), territory);
        aggressor.KillAllForces(territory, true);

        LoseCards(def, false);
        PayDialedSpice(defender, def, true);

        LoseCards(agg, false);
        PayDialedSpice(aggressor, agg, true);

        if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses) Stone(Milestone.Messiah);
    }

    private string InOrOn(Territory t)
    {
        return t.IsHomeworld ? " on " : " in ";
    }

    private void LasgunShieldExplosion(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
    {
        var hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

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

        var removed = RemoveResources(territory);
        if (removed > 0) Log("The explosion destroys ", Payment.Of(removed), " in ", territory);

        KillAllForcesIn(territory, true);
        KillAmbassadorIn(territory);

        if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses) Stone(Milestone.Messiah);
    }

    internal void KillAllForcesIn(Territory territory, bool inBattle)
    {
        foreach (var p in Players)
            if (p.AnyForcesIn(territory) > 0)
            {
                RevealCurrentNoField(p, territory);

                var homeworldKillLimit = inBattle ? p.GetHomeworldBattleContributionAndLasgunShieldLimit(territory) : 0;
                if (homeworldKillLimit == 0)
                {
                    Log("All ", p.Faction, " forces in ", territory, " were killed");
                    p.KillAllForces(territory, inBattle);
                }
                else
                {
                    var normalForcesToKill = Math.Min(p.ForcesIn(territory), homeworldKillLimit);
                    var specialForcesToKill = Math.Min(p.SpecialForcesIn(territory), homeworldKillLimit - normalForcesToKill);

                    if (normalForcesToKill > 0) p.KillForces(territory, normalForcesToKill, false, inBattle);
                    if (specialForcesToKill > 0) p.KillForces(territory, specialForcesToKill, true, inBattle);

                    Log(MessagePart.ExpressIf(normalForcesToKill > 0, normalForcesToKill, p.Force),
                        MessagePart.ExpressIf(normalForcesToKill > 0 && specialForcesToKill > 0, " and "),
                        MessagePart.ExpressIf(specialForcesToKill > 0, specialForcesToKill, p.SpecialForce),
                        " in ", territory, " were killed");
                }
            }
    }

    #endregion

    #region BattleConclusion

    internal bool BattleWasConcludedByWinner { get; set; } = false;

    public List<Leader> Assassinated { get; } = new();

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

    public Dictionary<Leader, Faction> CapturedLeaders { get; } = new();

    internal void FinishBattle()
    {
        if (AggressorPlan.Hero == Vidal && WhenToSetAsideVidal == VidalMoment.AfterUsedInBattle && !(AggressorTraitorAction.Succeeded && !DefenderTraitorAction.Succeeded)) SetAsideVidal();

        if (DefenderPlan.Hero == Vidal && WhenToSetAsideVidal == VidalMoment.AfterUsedInBattle && !(DefenderTraitorAction.Succeeded && !AggressorTraitorAction.Succeeded)) SetAsideVidal();

        ReturnSkilledLeadersInFrontOfShieldAfterBattle();
        if (Version >= 162) DetermineOccupation(CurrentBattle.Territory);
        if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
        if (CurrentJuice != null && CurrentJuice.Type == JuiceType.Aggressor) CurrentJuice = null;
        CurrentDiplomacy = null;
        CurrentRockWasMelted = null;
        CurrentPortableAntidoteUsed = null;
        FinishDeciphererIfApplicable();
        if (NextPlayerToBattle == null) MainPhaseEnd();
        Enter(Phase.BattleReport);
    }

    internal void DetermineOccupation(Territory territory)
    {
        foreach (var location in territory.Locations) DetermineOccupation(location);
    }

    internal void DetermineOccupation(Location location)
    {
        if (location is Homeworld hw)
        {
            var previousOccupier = OccupierOf(hw.World);
            var solePlayerOnPlanet = BattalionsIn(hw).Count() == 1 ? GetPlayer(BattalionsIn(hw).First().Faction) : null;

            if (solePlayerOnPlanet != null)
            {
                HomeworldOccupation.Remove(hw);

                if (!solePlayerOnPlanet.IsNative(hw))
                {
                    HomeworldOccupation.Add(hw, solePlayerOnPlanet.Faction);
                    Log(solePlayerOnPlanet.Faction, " now occupy ", hw);
                }
                else if (previousOccupier != null)
                {
                    Log(previousOccupier.Faction, " no longer occupy ", hw);
                }

                CheckIfShipmentPermissionsShouldBeRevoked();

                if (hw.World == World.Pink) CheckIfOccupierTakesVidal(previousOccupier);
            }
        }
    }

    private void ReturnSkilledLeadersInFrontOfShieldAfterBattle()
    {
        foreach (var leader in LeaderState.Where(ls => ls.Key is Leader l && IsSkilled(l) && !ls.Value.InFrontOfShield).Select(ls => ls.Key as Leader))
        {
            var currentOwner = Players.FirstOrDefault(p => p.Leaders.Contains(leader));

            if (currentOwner == null || (!CapturedLeaders.ContainsKey(leader) && !(currentOwner.Faction != Faction.Pink && leader.HeroType == HeroType.Vidal) && CurrentBattle.IsAggressorOrDefender(currentOwner)))
            {
                SetInFrontOfShield(leader, true);

                if (IsAlive(leader)) Log(Skill(leader), " ", leader, " is placed back in front of shield");
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

    #region PostBattle

    private void LoseCards(Battle plan, bool mayChooseToKeepOne)
    {
        if (!(plan.Player.Ally == Faction.Cyan && CyanAllowsKeepingCards) && plan.Player.Nexus == Faction.Cyan && NexusPlayed.CanUseSecretAlly(this, plan.Player)) SecretAllyAllowsKeepingCardsAfterLosingBattle = true;

        if (mayChooseToKeepOne)
        {
            if (plan.Weapon != null && (Version < 159 || plan.Player.Has(plan.Weapon))) CardsToBeDiscardedByLoserAfterBattle.Add(plan.Weapon);
            if (plan.Defense != null && (Version < 159 || plan.Player.Has(plan.Defense))) CardsToBeDiscardedByLoserAfterBattle.Add(plan.Defense);
        }
        else
        {
            Discard(plan.Weapon);
            Discard(plan.Defense);
        }
    }

    #endregion

    #region Information

    public Player NextPlayerToBattle
    {
        get
        {
            for (var i = 0; i < Players.Count; i++)
            {
                var playerToCheck = BattleSequence.CurrentPlayer;
                if (Battle.BattlesToBeFought(this, playerToCheck).Any()) return playerToCheck;

                BattleSequence.NextPlayer();
            }

            return null;
        }
    }

    public Player Auditee
    {
        get
        {
            if (Applicable(Rule.BrownAuditor) && !Prevented(FactionAdvantage.BrownAudit))
            {
                if (AggressorPlan != null && AggressorPlan.Hero != null && AggressorPlan.Hero.HeroType == HeroType.Auditor)
                    return DefenderPlan.Player;
                if (DefenderPlan != null && DefenderPlan.Hero != null && DefenderPlan.Hero.HeroType == HeroType.Auditor) return AggressorPlan.Player;
            }

            return null;
        }
    }

    public IHero WinnerHero
    {
        get
        {
            if (BattleWinner != Faction.None)
            {
                var winnerGambit = BattleWinner == AggressorPlan.Initiator ? AggressorPlan : DefenderPlan;
                return winnerGambit.Hero;
            }

            return null;
        }
    }

    public Battle WinnerBattleAction
    {
        get
        {
            if (AggressorPlan != null && AggressorPlan.Initiator == BattleWinner) return AggressorPlan;
            if (DefenderPlan != null && DefenderPlan.Initiator == BattleWinner) return DefenderPlan;

            return null;
        }
    }

    public bool CanJoinCurrentBattle(IHero hero)
    {
        var currentTerritory = LeaderState[hero].CurrentTerritory;
        return currentTerritory == null || currentTerritory == CurrentBattle?.Territory;
    }

    #endregion Information
}