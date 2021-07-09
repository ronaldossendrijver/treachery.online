/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public BattleInitiated CurrentBattle { get; private set; } = null;
        public Battle AggressorBattleAction { get; private set; } = null;
        public TreacheryCalled AggressorTraitorAction { get; private set; }
        public Battle DefenderBattleAction { get; private set; } = null;
        public TreacheryCalled DefenderTraitorAction { get; private set; }

        public Faction BattleWinner { get; private set; }
        public Faction BattleLoser { get; private set; }
        public int GreySpecialForceLossesToTake { get; private set; }

        public int NrOfBattlesFought = 0;

        private void EnterBattlePhase()
        {
            MainPhaseStart(MainPhase.Battle);
            NrOfBattlesFought = 0;
            if (KarmaHmsMovesLeft != 2) KarmaHmsMovesLeft = 0;
            AllowMovePhaseFactionAdvantages();
            ResetBattle();
            Enter(Aggressor != null, Phase.BattlePhase, EnterSpiceCollectionPhase);
        }

        private void AllowMovePhaseFactionAdvantages()
        {
            Allow(FactionAdvantage.BlueAccompanies);
            Allow(FactionAdvantage.GreenSpiceBlowPrescience);
            Allow(FactionAdvantage.BlueNoFlipOnIntrusion);
            Allow(FactionAdvantage.YellowExtraMove);
            Allow(FactionAdvantage.YellowProtectedFromStorm);
            Allow(FactionAdvantage.OrangeDetermineMoveMoment);
            Allow(FactionAdvantage.OrangeSpecialShipments);
            Allow(FactionAdvantage.OrangeShipmentsDiscount);
            Allow(FactionAdvantage.OrangeShipmentsDiscountAlly);
            Allow(FactionAdvantage.PurpleRevivalDiscount);
            Allow(FactionAdvantage.PurpleRevivalDiscountAlly);
            Allow(FactionAdvantage.GreyCyborgExtraMove);
        }

        public Player Aggressor
        {
            get
            {
                BattleSequence.Start(this, false);

                for (int i = 0; i < Players.Count; i++)
                {
                    var playerToCheck = BattleSequence.CurrentPlayer;
                    if (Battle.MustFight(this, playerToCheck))
                    {
                        return playerToCheck;
                    }

                    BattleSequence.NextPlayer(this, false);
                }

                return null;
            }
        }


        public void HandleEvent(BattleInitiated b)
        {
            CurrentReport = new Report(MainPhase.Battle);
            CurrentBattle = b;
            NrOfBattlesFought++;
            CurrentReport.Add(b.GetMessage());

            if (!Battle.ValidBattleHeroes(this, b.Defender).Any())
            {
                CurrentReport.Add(b.Target, "{0} don't have leaders available for this battle.", b.Target);
            }

            if (!Battle.ValidBattleHeroes(this, b.Player).Any())
            {
                CurrentReport.Add(b.Initiator, "{0} don't have leaders available for this battle.", b.Initiator);
            }

            HasBattleWheel.Clear();
            HasBattleWheel.Add(b.Initiator);
            HasBattleWheel.Add(b.Target);
        }

        public Voice CurrentVoice { get; private set; } = null;
        public void HandleEvent(Voice e)
        {
            MainPhaseMiddle();

            CurrentVoice = e;

            if (CurrentBattle != null)
            {

                var opponent = CurrentBattle.OpponentOf(e.Player);

                if (opponent != null)
                {
                    RevokePlanIfNeeded(opponent.Faction);
                }
            }

            CurrentReport.Add(e.GetMessage());
            RecentMilestones.Add(Milestone.Voice);
        }

        public Prescience CurrentPrescience { get; private set; } = null;
        public void HandleEvent(Prescience e)
        {
            MainPhaseMiddle();

            CurrentPrescience = e;
            CurrentReport.Add(e.GetMessage());
            RecentMilestones.Add(Milestone.Prescience);
        }

        public void HandleEvent(Battle b)
        {
            MainPhaseMiddle();

            if (b.Initiator == CurrentBattle.Initiator)
            {
                AggressorBattleAction = b;
            }
            else if (b.Initiator == CurrentBattle.Target)
            {
                DefenderBattleAction = b;
            }

            if (AggressorBattleAction != null && DefenderBattleAction != null)
            {
                if (Version < 43) DiscardOneTimeCardsUsedInBattle(null, null);
                CurrentReport.Add(AggressorBattleAction.GetBattlePlanMessage());
                CurrentReport.Add(DefenderBattleAction.GetBattlePlanMessage());
                RegisterKnownCards(AggressorBattleAction);
                RegisterKnownCards(DefenderBattleAction);
                Enter(Phase.CallTraitorOrPass);

                if (Version >= 69)
                {
                    if (CurrentBattle.Initiator == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
                    {
                        AggressorTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
                    }
                    else if (CurrentBattle.Target == Faction.Purple && (GetPlayer(Faction.Purple).Ally != Faction.Black || Prevented(FactionAdvantage.BlackCallTraitorForAlly)))
                    {
                        DefenderTraitorAction = new TreacheryCalled(this) { Initiator = Faction.Purple, TraitorCalled = false };
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
            bool aggressorKeepsCards = Version >= 73 && aggressorCall.TraitorCalled && !defenderCall.TraitorCalled;
            if (!aggressorKeepsCards)
            {
                if (AggressorBattleAction.Hero != null && AggressorBattleAction.Hero is TreacheryCard)
                {
                    Discard(AggressorBattleAction.Hero as TreacheryCard);
                }

                if (AggressorBattleAction.Weapon != null && (
                AggressorBattleAction.Weapon.IsArtillery ||
                AggressorBattleAction.Weapon.IsPoisonTooth && !PoisonToothCancelled
                ))
                {
                    Discard(AggressorBattleAction.Weapon);
                }
            }

            bool defenderKeepsCards = Version >= 73 && defenderCall.TraitorCalled && !aggressorCall.TraitorCalled;
            if (!defenderKeepsCards)
            {
                if (DefenderBattleAction.Hero != null && DefenderBattleAction.Hero is TreacheryCard)
                {
                    Discard(DefenderBattleAction.Hero as TreacheryCard);
                }

                if (DefenderBattleAction.Weapon != null && (
                    DefenderBattleAction.Weapon.IsArtillery ||
                    DefenderBattleAction.Weapon.IsPoisonTooth && !PoisonToothCancelled
                    ))
                {
                    Discard(DefenderBattleAction.Weapon);
                }
            }
        }

        public void HandleEvent(BattleRevision e)
        {
            if (CurrentBattle != null)
            {
                if (e.By(CurrentBattle.Initiator))
                {
                    AggressorBattleAction = null;
                }
                else if (e.By(CurrentBattle.Target))
                {
                    DefenderBattleAction = null;
                }
            }
        }

        private bool PoisonToothCancelled = false;
        public void HandleEvent(PoisonToothCancelled e)
        {
            PoisonToothCancelled = true;
            CurrentReport.Add(e.GetMessage());
        }

        public void HandleEvent(TreacheryCalled e)
        {
            if (AggressorBattleAction.By(e.Initiator) || (e.By(Faction.Black) && Allies(AggressorBattleAction.Initiator, Faction.Black)))
            {
                AggressorTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Add(e.GetMessage());
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(DefenderBattleAction.Hero);
                }
            }

            if (DefenderBattleAction.By(e.Initiator) || (e.By(Faction.Black) && Allies(DefenderBattleAction.Initiator, Faction.Black)))
            {
                DefenderTraitorAction = e;
                if (e.TraitorCalled)
                {
                    CurrentReport.Add(e.GetMessage());
                    RecentMilestones.Add(Milestone.TreacheryCalled);
                    e.Player.RevealedTraitors.Add(AggressorBattleAction.Hero);
                }
            }

            if (AggressorTraitorAction != null && DefenderTraitorAction != null)
            {
                if (Version >= 43) DiscardOneTimeCardsUsedInBattle(AggressorTraitorAction, DefenderTraitorAction);
                ResolveBattle(CurrentBattle, AggressorBattleAction, DefenderBattleAction, AggressorTraitorAction, DefenderTraitorAction);
                CaptureLeaderIfApplicable(e);
                FlipBeneGesseritWhenAlone();
                Enter(Version < 43 || BattleWinner != Faction.None, Phase.BattleConclusion, FinishBattle);
            }
        }

        private void CaptureLeaderIfApplicable(TreacheryCalled e)
        {
            if (BattleWinner == Faction.Black && (Version >= 40 || e.By(Faction.Black)) && Applicable(Rule.BlackCapturesOrKillsLeaders))
            {
                if (!Prevented(FactionAdvantage.BlackCaptureLeader))
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
                else
                {
                    CurrentReport.Add(Faction.Black, "{0} prevents {1} from capturing a leader.", TreacheryCardType.Karma, Faction.Black);
                }
            }
        }

        public void HandleEvent(BattleConcluded e)
        {
            if (Version < 43)
            {
                HasActedOrPassed.Add(e.Initiator);

                if (e.Initiator == BattleWinner)
                {
                    HandleWinnerConclusion(e);
                }

                if (HasActedOrPassed.Contains(AggressorBattleAction.Initiator) && HasActedOrPassed.Contains(DefenderBattleAction.Initiator))
                {
                    if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
                    Enter(IsPlaying(Faction.Purple) && BattleWinner != Faction.Purple, Phase.Facedancing, FinishBattle);
                }
            }
            else
            {
                HandleWinnerConclusion(e);
                Enter(IsPlaying(Faction.Purple) && BattleWinner != Faction.Purple, Phase.Facedancing, FinishBattle);
            }
        }

        public void HandleEvent(FaceDanced f)
        {
            if (f.FaceDancerCalled)
            {
                var initiator = GetPlayer(f.Initiator);
                var facedancer = initiator.FaceDancers.FirstOrDefault(f => WinnerHero.IsFaceDancer(f));
                CurrentReport.Add(f.Initiator, "{0} reveal {1} as one of their Face Dancers!", f.Initiator, facedancer);

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

                if (Version >= 81)
                {
                    FlipBeneGesseritWhenAlone();
                }
            }
            else
            {
                CurrentReport.Add(f.Initiator, "{0} don't reveal a Face Dancer.", f.Initiator);
            }

            FinishBattle();
        }

        private void ReplaceForces(FaceDanced f, Player initiator)
        {
            if (Version < 56 || Version >= 80)
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

                    CurrentReport.Add(f.Initiator, "{0} {1} forces go back to reserves and are replaced by {2} {3} forces ({4} from reserves{5}).", nrOfRemovedForces, winner.Faction, f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Initiator, f.ForcesFromReserve, DetermineSourceLocations(f));
                }
            }
            else
            {
                int nrOfRemovedForces = FaceDanced.MaximumNumberOfForces(this, initiator);

                if (nrOfRemovedForces > 0)
                {
                    foreach (var player in Players.Where(p => p.Faction != f.Initiator))
                    {
                        player.ForcesToReserves(CurrentBattle.Territory);
                    }

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

                    CurrentReport.Add(f.Initiator, "{0} forces go back to reserves and are replaced by {1} {2} forces ({3} from reserves{4}).", nrOfRemovedForces, f.TargetForceLocations.Sum(b => b.Value.TotalAmountOfForces), f.Initiator, f.ForcesFromReserve, DetermineSourceLocations(f));
                }
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
            CurrentReport.Add(f.Initiator, "{0} draw 3 new Face Dancers.", f.Initiator);
        }

        private MessagePart DetermineSourceLocations(FaceDanced f)
        {
            if (f.ForceLocations.Count == 0)
            {
                return new MessagePart("");
            }
            else
            {
                return new MessagePart(", {0}", string.Join(", ", f.ForceLocations.Select(fl => string.Format("{0} from {1}", fl.Value.AmountOfForces + fl.Value.AmountOfSpecialForces, fl.Key))));
            }
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
            if (!Applicable(Rule.FullPhaseKarma)) AllowPreventedBattleFactionAdvantages();
            if (Aggressor == null) MainPhaseEnd();
            Enter(Phase.BattleReport);

        }

        private void AllowPreventedBattleFactionAdvantages()
        {
            if (Version >= 88 || AggressorBattleAction.By(Faction.Green) || DefenderBattleAction.By(Faction.Green))
            {
                Allow(FactionAdvantage.GreenUseMessiah);
                Allow(FactionAdvantage.GreenBattlePlanPrescience);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Blue) || DefenderBattleAction.By(Faction.Blue))
            {
                Allow(FactionAdvantage.BlueUsingVoice);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Yellow) || DefenderBattleAction.By(Faction.Yellow))
            {
                Allow(FactionAdvantage.YellowSpecialForceBonus);
                Allow(FactionAdvantage.YellowNotPayingForBattles);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Red) || DefenderBattleAction.By(Faction.Red))
            {
                Allow(FactionAdvantage.RedSpecialForceBonus);
            }

            if (Version >= 88 || AggressorBattleAction.By(Faction.Grey) || DefenderBattleAction.By(Faction.Grey))
            {
                Allow(FactionAdvantage.GreySpecialForceBonus);
            }

            if (Version >= 88 || GetPlayer(AggressorBattleAction.Initiator).Ally == Faction.Black || GetPlayer(DefenderBattleAction.Initiator).Ally == Faction.Black)
            {
                Allow(FactionAdvantage.BlackCallTraitorForAlly);
                Allow(FactionAdvantage.BlackCaptureLeader);
            }
        }

        private void HandleWinnerConclusion(BattleConcluded e)
        {
            var winner = GetPlayer(e.Initiator);

            foreach (var c in e.DiscardedCards)
            {
                CurrentReport.Add(e.Initiator, "{0} discard {1}.", e.Initiator, c);
                winner.TreacheryCards.Remove(c);
                TreacheryDiscardPile.PutOnTop(c);
            }

            DecideFateOfCapturedLeader(e);
            TakeTechToken(e, winner);
            ProcessGreyForceLossesAndSubstitutions(e, winner);
        }

        private void ProcessGreyForceLossesAndSubstitutions(BattleConcluded e, Player winner)
        {
            if (GreySpecialForceLossesToTake > 0)
            {
                var winnerGambit = WinnerBattleAction;
                int winnerForcesLost = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength + e.SpecialForceLossesReplaced;
                int winnerSpecialForcesLost = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength - e.SpecialForceLossesReplaced;
                TakeLosses(CurrentBattle.Territory, winner, winnerForcesLost, winnerSpecialForcesLost);
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
                    CurrentReport.Add(e.Initiator, "{0} take {1} from {2}.", e.Initiator, e.StolenToken, BattleLoser);
                }
            }
        }

        private void DecideFateOfCapturedLeader(BattleConcluded e)
        {
            if (e.By(Faction.Black) && Applicable(Rule.BlackCapturesOrKillsLeaders) && BlackVictim != null)
            {
                if (e.Initiator == CurrentBattle.Aggressor)
                {
                    CaptureOrAssassinateLeader(AggressorBattleAction, DefenderBattleAction, e.CaptureDecision);
                }
                else
                {
                    CaptureOrAssassinateLeader(DefenderBattleAction, AggressorBattleAction, e.CaptureDecision);
                }
            }
        }

        private void ResetBattle()
        {
            CurrentBattle = null;
            CurrentPrescience = null;
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
        }

        public void ResolveBattle(BattleInitiated b, Battle agg, Battle def, TreacheryCalled aggtrt, TreacheryCalled deftrt)
        {
            var aggressor = GetPlayer(agg.Initiator);
            var defender = GetPlayer(def.Initiator);

            if (Version <= 93)
            {
                SetHeroLocations(agg, b.Territory);
                SetHeroLocations(def, b.Territory);
            }

            if (aggtrt.TraitorCalled || deftrt.TraitorCalled)
            {
                TraitorCalled(b, agg, def, deftrt, aggressor, defender, agg.Hero, def.Hero);
            }
            else if ((agg.HasLaser || def.HasLaser) && (agg.HasShield || def.HasShield))
            {
                LasgunShieldExplosion(agg, def, aggressor, defender, b.Territory, agg.Hero, def.Hero);
            }
            else
            {
                if (Version >= 94)
                {
                    SetHeroLocations(agg, b.Territory);
                    SetHeroLocations(def, b.Territory);
                }

                DetermineBattleOutcome(agg, def, aggressor, defender, b.Territory, agg.Hero, def.Hero);
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
                if (hero != null && hero is Leader capturedLeader && harkonnen.Leaders.Contains(capturedLeader) && PreviousOwners.ContainsKey(capturedLeader))
                {
                    ReturnLeader(harkonnen, capturedLeader);
                }

                if (!harkonnen.Leaders.Any(l => PreviousOwners.ContainsKey(l) && IsAlive(l)))
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
            if (PreviousOwners.ContainsKey(toReturn))
            {
                Player originalPlayer = GetPlayer(PreviousOwners[toReturn]);
                originalPlayer.Leaders.Add(toReturn);
                currentOwner.Leaders.Remove(toReturn);
                PreviousOwners.Remove(toReturn);
                CurrentReport.Add(originalPlayer.Faction, "{0} returns to {1} after service for {2}", toReturn, originalPlayer.Faction, currentOwner.Faction);
            }
        }

        private void DetermineBattleOutcome(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggHero, IHero defHero)
        {
            bool poisonToothUsed = !PoisonToothCancelled && (agg.HasPoisonTooth || def.HasPoisonTooth);
            bool artilleryUsed = agg.HasArtillery || def.HasArtillery;

            var aggHeroKilled = false;
            var aggHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(agg, def, aggHero, poisonToothUsed, artilleryUsed, ref aggHeroKilled, ref aggHeroCauseOfDeath);

            bool defHeroKilled = false;
            var defHeroCauseOfDeath = TreacheryCardType.None;
            DetermineCauseOfDeath(def, agg, defHero, poisonToothUsed, artilleryUsed, ref defHeroKilled, ref defHeroCauseOfDeath);

            int aggHeroEffectiveStrength = (aggHero != null && !artilleryUsed) ? aggHero.ValueInCombatAgainst(defHero) : 0;
            int aggHeroContribution = !aggHeroKilled ? aggHeroEffectiveStrength : 0;

            int defHeroEffectiveStrength = (defHero != null && !artilleryUsed) ? defHero.ValueInCombatAgainst(aggHero) : 0;
            int defHeroContribution = (defHero != null && !defHeroKilled) ? defHeroEffectiveStrength : 0;

            var aggMessiahContribution = aggressor.Is(Faction.Green) && agg.Messiah && agg.Hero != null && !aggHeroKilled && !artilleryUsed ? 2 : 0;
            var defMessiahContribution = defender.Is(Faction.Green) && def.Messiah && def.Hero != null && !defHeroKilled && !artilleryUsed ? 2 : 0;

            float aggForceDial = agg.Dial(this, defender.Faction);
            float defForceDial = def.Dial(this, aggressor.Faction);

            float aggTotal = aggForceDial + aggHeroContribution + aggMessiahContribution;
            float defTotal = defForceDial + defHeroContribution + defMessiahContribution;

            var winner = (aggTotal >= defTotal) ? aggressor : defender;
            var winnerGambit = (winner == aggressor) ? agg : def;

            var loser = (aggTotal >= defTotal) ? defender : aggressor;
            var loserGambit = (loser == aggressor) ? agg : def;

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            if (aggHeroKilled)
            {
                KillLeaderInBattle(aggHero, defHero, aggHeroCauseOfDeath, winner, aggHeroEffectiveStrength);
            }

            if (defHeroKilled)
            {
                KillLeaderInBattle(defHero, aggHero, defHeroCauseOfDeath, winner, defHeroEffectiveStrength);
            }

            CurrentReport.Add(aggressor.Faction, "Strength of the {0} (aggressor) army: {1}.", aggressor.Faction, aggTotal);
            CurrentReport.Add(defender.Faction, "Strength of the {0} (defending) army: {1}.", defender.Faction, defTotal);
            CurrentReport.Add(winner.Faction, "{0} WIN THE BATTLE.", winner.Faction);

            ProcessWinnerLosses(territory, winner, winnerGambit);
            ProcessLoserLosses(territory, loser, loserGambit);
        }

        private void DetermineCauseOfDeath(Battle wheel, Battle opponent, IHero theHero, bool poisonToothUsed, bool artilleryUsed, ref bool heroDies, ref TreacheryCardType causeOfDeath)
        {
            DetermineDeathBy(theHero, TreacheryCardType.ArtilleryStrike, artilleryUsed && !wheel.HasShield, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.PoisonTooth, poisonToothUsed && !wheel.HasNonAntidotePoisonDefense, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Laser, opponent.HasLaser, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Poison, opponent.HasPoison && !wheel.HasAntidote, ref heroDies, ref causeOfDeath);
            DetermineDeathBy(theHero, TreacheryCardType.Projectile, opponent.HasProjectile && !wheel.HasProjectileDefense, ref heroDies, ref causeOfDeath);
        }

        private void DetermineDeathBy(IHero hero, TreacheryCardType byWeapon, bool weaponHasEffect, ref bool heroIsKilled, ref TreacheryCardType causeOfDeath)
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

            CurrentReport.Add(loser.Faction, "{0} lose all ({1}) forces in {2}.", loser.Faction, loser.AnyForcesIn(territory), territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void PayDialedSpice(Player p, Battle b)
        {
            int cost = b.Cost(this);

            if (cost > 0)
            {
                p.Resources -= cost;
            }
        }

        private void ProcessWinnerLosses(Territory territory, Player winner, Battle winnerGambit)
        {
            PayDialedSpice(winner, winnerGambit);

            if (winner.Faction != Faction.Grey || winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength == 0 || winner.ForcesIn(territory) <= winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength)
            {
                int winnerForcesLost = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength;
                int winnerSpecialForcesLost = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength;
                TakeLosses(territory, winner, winnerForcesLost, winnerSpecialForcesLost);
            }
            else
            {
                GreySpecialForceLossesToTake = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength;
            }
        }

        private void TakeLosses(Territory territory, Player player, int forcesLost, int specialForcesLost)
        {
            bool hadMessiahBeforeLosses = player.MessiahAvailable;

            player.KillForces(territory, forcesLost, false, true);
            player.KillForces(territory, specialForcesLost, true, true);
            LogLosses(player, forcesLost, specialForcesLost);

            if (player.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void KillLeaderInBattle(IHero killedHero, IHero opposingHero, TreacheryCardType causeOfDeath, Player winner, int heroValue)
        {
            var earned = (causeOfDeath == TreacheryCardType.ArtilleryStrike) ? 0 : killedHero.ValueInCombatAgainst(opposingHero);
            if (Version >= 45) earned = heroValue;

            CurrentReport.Add(winner.Faction, "{1} kills {0}. {2} earn {3}.", killedHero, causeOfDeath, winner.Faction, earned);
            RecentMilestones.Add(Milestone.LeaderKilled);
            if (killedHero is Leader) KillHero(killedHero as Leader);
            winner.Resources += earned;
        }

        private void LogLosses(Player player, int forcesLost, int specialForcesLost)
        {
            if (specialForcesLost > 0)
            {
                CurrentReport.Add(player.Faction, "{0} lose {1} {2} and {3} {4} during battle.", player.Faction, forcesLost, player.Force, specialForcesLost, player.SpecialForce);
            }
            else if (forcesLost > 0)
            {
                CurrentReport.Add(player.Faction, "{0} lose {1} {2} during battle.", player.Faction, forcesLost, player.Force);
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

            CurrentReport.Add(traitorOwner, "{0} is a {1} traitor! {2} lose everything.", traitor, traitorOwner, loser.Faction);

            RecentMilestones.Add(Milestone.LeaderKilled);

            if (traitor is Leader)
            {
                CurrentReport.Add(loser.Faction, "Treachery kills {0}. {1} earn {2}.", traitor, winner.Faction, traitorValue);
                KillHero(traitor);
                winner.Resources += traitorValue;
            }

            BattleWinner = winner.Faction;
            BattleLoser = loser.Faction;

            CurrentReport.Add(loser.Faction, "{0} lose all ({1}) forces in {2}.", loser.Faction, loser.SpecialForcesIn(territory) + loser.ForcesIn(territory), territory);
            loser.KillAllForces(territory, true);
            LoseCards(loserGambit);
            PayDialedSpice(loser, loserGambit);

            if (loser.MessiahAvailable && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void TwoTraitorsCalled(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            CurrentReport.Add("Both leaders are traitors, everyone gets killed!");
            RecentMilestones.Add(Milestone.LeaderKilled);

            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Add(defender.Faction, "Treachery kills {0}.", defLeader);
            KillHero(defLeader);
            CurrentReport.Add(aggressor.Faction, "Treachery kills {0}.", aggLeader);
            KillHero(aggLeader);
            CurrentReport.Add(defender.Faction, "{0} lose all ({1}) forces in {2}.", defender.Faction, defender.SpecialForcesIn(territory) + defender.ForcesIn(territory), territory);
            defender.KillAllForces(territory, true);
            CurrentReport.Add(aggressor.Faction, "{0} lose all ({1}) forces in {2}.", aggressor.Faction, aggressor.SpecialForcesIn(territory) + aggressor.ForcesIn(territory), territory);
            aggressor.KillAllForces(territory, true);

            LoseCards(def);
            PayDialedSpice(defender, def);

            LoseCards(agg);
            PayDialedSpice(aggressor, agg);

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LasgunShieldExplosion(Battle agg, Battle def, Player aggressor, Player defender, Territory territory, IHero aggLeader, IHero defLeader)
        {
            bool hadMessiahBeforeLosses = aggressor.MessiahAvailable || defender.MessiahAvailable;

            CurrentReport.Add(Faction.None, "A {0}/{1} explosion occurs!", TreacheryCardType.Laser, TreacheryCardType.Shield);
            RecentMilestones.Add(Milestone.Explosion);

            if (aggLeader != null)
            {
                CurrentReport.Add(aggressor.Faction, "The explosion kills {0}.", aggLeader);
                KillHero(aggLeader);
            }

            if (defLeader != null)
            {
                CurrentReport.Add(defender.Faction, "The explosion kills {0}.", defLeader);
                KillHero(def.Hero);
            }

            if (agg.Messiah || def.Messiah)
            {
                CurrentReport.Add(aggressor.Faction, "The explosion kills the {0}.", Concept.Messiah);
                KillHero(LeaderManager.Messiah);
            }

            LoseCards(agg);
            PayDialedSpice(aggressor, agg);

            LoseCards(def);
            PayDialedSpice(defender, def);

            int removed = RemoveResources(territory);
            if (removed > 0)
            {
                CurrentReport.Add(Faction.None, "The explosion destroys {0} {1} in {2}.", removed, Concept.Resource, territory);
            }

            foreach (var p in Players)
            {
                int numberOfForces = p.AnyForcesIn(territory);
                if (numberOfForces > 0)
                {
                    CurrentReport.Add(p.Faction, "The explosion kills {0} {1} forces in {2}.", numberOfForces, p.Faction, territory);
                    p.KillAllForces(territory, true);
                }
            }

            if ((aggressor.MessiahAvailable || defender.MessiahAvailable) && !hadMessiahBeforeLosses)
            {
                RecentMilestones.Add(Milestone.Messiah);
            }
        }

        private void LoseCards(Battle gambit)
        {
            Discard(gambit.Weapon);
            Discard(gambit.Defense);
        }

        public bool CanJoinCurrentBattle(IHero hero)
        {
            var currentTerritory = CurrentTerritory(hero);
            return currentTerritory == null || currentTerritory == CurrentBattle.Territory;
        }

        public Leader BlackVictim = null;
        private void SelectVictimOfBlackWinner(Battle harkonnenAction, Battle victimAction)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var victim = GetPlayer(victimAction.Initiator);

            //Get all living leaders from the opponent that haven't fought in another territory this turn
            Deck<Leader> availableLeaders = new Deck<Leader>(
                victim.Leaders.Where(l => LeaderState[l].Alive && CanJoinCurrentBattle(l)), Random);

            if (!availableLeaders.IsEmpty)
            {
                availableLeaders.Shuffle();
                BlackVictim = availableLeaders.Draw();
            }
            else
            {
                BlackVictim = null;
                CurrentReport.Add(Faction.Black, "{0} don't have any leaders {1} can capture or kill.", victim.Faction, Faction.Black);
            }
        }

        private Dictionary<Leader, Faction> PreviousOwners = new Dictionary<Leader, Faction>();
        private void CaptureOrAssassinateLeader(Battle harkonnenAction, Battle victimAction, CaptureDecision decision)
        {
            var harkonnen = GetPlayer(harkonnenAction.Initiator);
            var target = GetPlayer(victimAction.Initiator);

            if (decision == CaptureDecision.Capture)
            {
                CurrentReport.Add(Faction.Black, "{0} capture a leader!", Faction.Black);
                harkonnen.Leaders.Add(BlackVictim);
                target.Leaders.Remove(BlackVictim);
                PreviousOwners.Add(BlackVictim, target.Faction);
            }
            else if (decision == CaptureDecision.Kill)
            {
                CurrentReport.Add(Faction.Black, "{0} kill a leader for 2!", Faction.Black);
                RecentMilestones.Add(Milestone.LeaderKilled);
                AssassinateLeader(BlackVictim);
                harkonnen.Resources += 2;
            }
            else if (decision == CaptureDecision.DontCapture)
            {
                CurrentReport.Add(Faction.Black, "{0} decide not to capture or kill a leader.", Faction.Black);
            }
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
    }
}
