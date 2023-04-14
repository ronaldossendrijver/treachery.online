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
        public List<Faction> ResourceAuditedFactions { get; private set; } = new();

        public void HandleEvent(WhiteRevealedNoField e)
        {
            RevealCurrentNoField(e.Player);
        }

        internal void RevealCurrentNoField(Player player, Location inLocation = null)
        {
            if (player != null && player.Faction == Faction.White)
            {
                var noFieldLocation = player.ForcesInLocations.FirstOrDefault(kvp => kvp.Value.AmountOfSpecialForces > 0).Key;

                if (noFieldLocation != null)
                {
                    if (inLocation == null || inLocation == noFieldLocation)
                    {
                        LatestRevealedNoFieldValue = CurrentNoFieldValue;
                        player.SpecialForcesToReserves(noFieldLocation, 1);
                        int nrOfForces = Math.Min(player.ForcesInReserve, CurrentNoFieldValue);
                        player.ShipForces(noFieldLocation, nrOfForces);
                        Log(player.Faction, " reveal ", nrOfForces, FactionForce.White, " under ", FactionSpecialForce.White, " in ", noFieldLocation);

                        if (CurrentNoFieldValue == 0)
                        {
                            FlipBeneGesseritWhenAloneOrWithPinkAlly();
                        }

                        CurrentNoFieldValue = -1;
                    }
                }
            }
        }

        internal void RevealCurrentNoField(Player player, Territory inTerritory)
        {
            if (player != null && player.Faction == Faction.White)
            {
                foreach (var l in inTerritory.Locations)
                {
                    RevealCurrentNoField(player, l);
                }
            }
        }

        internal Phase PhaseBeforeSearchingDiscarded { get; set; }

        internal void ExchangeResourcesInBribe(Player from, Player target, int amount)
        {
            from.Resources -= amount;

            if (BribesDuringMentat)
            {
                if (from.Faction == Faction.Red && target.Faction == from.Ally)
                {
                    target.Resources += amount;
                }
                else
                {
                    target.Bribes += amount;
                }
            }
            else
            {
                target.Resources += amount;
            }
        }

        internal Phase PhasePausedByClairvoyance;
        public ClairVoyancePlayed LatestClairvoyance { get; internal set; }
        public ClairVoyanceQandA LatestClairvoyanceQandA { get; internal set; }
        public BattleInitiated LatestClairvoyanceBattle { get; internal set; }

        public int KarmaHmsMovesLeft { get; internal set; } = 2;

        public bool GreenKarma { get; internal set; } = false;

        public int PinkKarmaBonus { get; internal set; } = 0;

        public List<Faction> SecretsRemainHidden { get; private set; } = new();

        public bool YieldsSecrets(Player p)
        {
            return !SecretsRemainHidden.Contains(p.Faction);
        }

        public bool YieldsSecrets(Faction f)
        {
            return !SecretsRemainHidden.Contains(f);
        }

        internal void RevokePlanIfNeeded(Faction f)
        {
            RevokePlan(CurrentBattle?.PlanOf(f));
        }

        internal void RevokePlan(Battle plan)
        {
            if (plan == AggressorBattleAction)
            {
                AggressorBattleAction = null;
            }
            else if (plan == DefenderBattleAction)
            {
                DefenderBattleAction = null;
            }
        }

        private List<FactionAdvantage> PreventedAdvantages = new();

        internal void Prevent(Faction initiator, FactionAdvantage advantage)
        {
            Log(initiator, " prevent ", advantage);

            if (!PreventedAdvantages.Contains(advantage))
            {
                PreventedAdvantages.Add(advantage);
            }
        }

        internal void Allow(FactionAdvantage advantage)
        {
            if (PreventedAdvantages.Contains(advantage))
            {
                PreventedAdvantages.Remove(advantage);
                Log(advantage, " is no longer prevented");
            }
        }

        public bool Prevented(FactionAdvantage advantage) => PreventedAdvantages.Contains(advantage);

        public bool BribesDuringMentat => !Applicable(Rule.BribesAreImmediate);

        public bool KarmaPrevented(Faction f)
        {
            return CurrentKarmaPrevention != null && CurrentKarmaPrevention.Target == f;
        }

        public BrownKarmaPrevention CurrentKarmaPrevention { get; internal set; } = null;

        public bool JuiceForcesFirstPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoFirst;

        public bool JuiceForcesLastPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoLast;


        public JuicePlayed CurrentJuice { get; internal set; }

        private bool BureaucratWasUsedThisPhase { get; set; } = false;
        private Phase _phaseBeforeBureaucratWasActivated;
        public Faction TargetOfBureaucracy { get; internal set; }

        internal void ApplyBureaucracy(Faction payer, Faction receiver)
        {
            if (!BureaucratWasUsedThisPhase)
            {
                var bureaucrat = PlayerSkilledAs(LeaderSkill.Bureaucrat);
                if (bureaucrat != null && bureaucrat.Faction != payer && bureaucrat.Faction != receiver)
                {
                    if (Version < 133) BureaucratWasUsedThisPhase = true;
                    _phaseBeforeBureaucratWasActivated = CurrentPhase;
                    TargetOfBureaucracy = receiver;
                    Enter(Phase.Bureaucracy);
                }
            }
        }

        internal Faction WasVictimOfBureaucracy { get; set; }
        public void HandleEvent(Bureaucracy e)
        {
            Log(e.GetDynamicMessage());
            if (!e.Passed)
            {
                Stone(Milestone.Bureaucracy);
                BureaucratWasUsedThisPhase = true;
                GetPlayer(TargetOfBureaucracy).Resources -= 2;
                WasVictimOfBureaucracy = TargetOfBureaucracy;
            }
            Enter(_phaseBeforeBureaucratWasActivated);
            TargetOfBureaucracy = Faction.None;
        }

        private bool BankerWasUsedThisPhase { get; set; } = false;

        internal void ActivateBanker(Player playerWhoPaid)
        {
            if (!BankerWasUsedThisPhase)
            {
                var banker = PlayerSkilledAs(LeaderSkill.Banker);
                if (banker != null && banker != playerWhoPaid)
                {
                    BankerWasUsedThisPhase = true;
                    Log(banker.Faction, " will receive ", Payment.Of(1), " from ", LeaderSkill.Banker, " at ", MainPhase.Collection);
                    banker.BankedResources += 1;
                }
            }
        }

        public Planetology CurrentPlanetology { get; internal set; }

        public bool BlackTraitorWasCancelled { get; internal set; } = false;
        public bool FacedancerWasCancelled { get; internal set; } = false;
        
        internal void PlayNexusCard(Player initiator, params object[] messageElements)
        {
            Stone(Milestone.NexusPlayed);

            string typeOfNexus;
            if (NexusPlayed.CanUseCunning(initiator))
            {
                typeOfNexus = "Cunning";
            }
            else if (NexusPlayed.CanUseBetrayal(this, initiator))
            {
                typeOfNexus = "Betrayal";
            }
            else
            {
                typeOfNexus = "Secret Ally";
            }

            if (messageElements != null && messageElements.Length > 0)
            {
                Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus} to ", MessagePart.Express(messageElements));
            }
            else
            {
                Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus}");
            }

            DiscardNexusCard(initiator);
        }

        public NexusPlayed CurrentGreenNexus { get; internal set; }
        public NexusPlayed CurrentYellowNexus { get; internal set; }
        public NexusPlayed CurrentRedNexus { get; internal set; }
        public NexusPlayed CurrentOrangeNexus { get; internal set; }
        public NexusPlayed CurrentBlueNexus { get; internal set; }
        public NexusPlayed CurrentGreyNexus { get; internal set; }
        
        internal void LogPreventionByKarma(FactionAdvantage prevented)
        {
            Log(TreacheryCardType.Karma, " prevents ", prevented);
        }

        internal void LogPreventionByLowThreshold(FactionAdvantage prevented)
        {
            Log("Low Threshold prevents ", prevented);
        }

        public bool CharityIsCancelled => EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped;
    }

    class TriggeredBureaucracy
    {
        internal Faction PaymentFrom;
        internal Faction PaymentTo;
    }
}
