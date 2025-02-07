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

    public List<Faction> ResourceAuditedFactions { get; private set; } = new();
    internal Phase PhasePausedByClairvoyance { get; set; }
    public ClairVoyancePlayed LatestClairvoyance { get; internal set; }
    public ClairVoyanceQandA LatestClairvoyanceQandA { get; internal set; }
    public BattleInitiated LatestClairvoyanceBattle { get; internal set; }
    public int KarmaHmsMovesLeft { get; internal set; } = 2;
    public bool GreenKarma { get; internal set; } = false;
    public int PinkKarmaBonus { get; internal set; } = 0;
    public List<Faction> SecretsRemainHidden { get; private set; } = new();
    private List<FactionAdvantage> PreventedAdvantages { get; set; } = new();
    internal Phase PhaseBeforeSearchingDiscarded { get; set; }
    public JuicePlayed CurrentJuice { get; internal set; }
    internal bool BureaucratWasUsedThisPhase { get; set; }
    internal Phase PhaseBeforeBureaucratWasActivated { get; set; }
    public Faction TargetOfBureaucracy { get; internal set; }
    public NexusPlayed CurrentNexusPrescience { get; internal set; }
    public NexusPlayed CurrentYellowSecretAlly { get; internal set; }
    public NexusPlayed CurrentRedCunning { get; internal set; }
    public NexusPlayed CurrentOrangeCunning { get; internal set; }
    public NexusPlayed CurrentOrangeSecretAlly { get; internal set; }
    public NexusPlayed CurrentBlueCunning { get; internal set; }
    public NexusPlayed CurrentGreyCunning { get; internal set; }
    internal Faction WasVictimOfBureaucracy { get; set; }
    private bool BankerWasUsedThisPhase { get; set; }
    public BrownKarmaPrevention CurrentKarmaPrevention { get; internal set; } = null;
    public Planetology CurrentPlanetology { get; internal set; }
    public bool BlackTraitorWasCancelled { get; internal set; } = false;
    public bool FacedancerWasCancelled { get; internal set; } = false;

    #endregion State

    #region Events

    internal void RevealCurrentNoField(Player player, Location inLocation = null)
    {
        if (player != null && player.Faction == Faction.White)
        {
            var noFieldLocation = player.ForcesInLocations.FirstOrDefault(kvp => kvp.Value.AmountOfSpecialForces > 0).Key;

            if (noFieldLocation != null)
                if (inLocation == null || inLocation == noFieldLocation)
                {
                    LatestRevealedNoFieldValue = CurrentNoFieldValue;
                    player.SpecialForcesToReserves(noFieldLocation, 1);
                    var nrOfForces = Math.Min(player.ForcesInReserve, CurrentNoFieldValue);
                    player.ShipForces(noFieldLocation, nrOfForces);
                    Log(player.Faction, " reveal ", nrOfForces, FactionForce.White, " under ", FactionSpecialForce.White, " in ", noFieldLocation);

                    if (CurrentNoFieldValue == 0) FlipBlueAdvisorsWhenAlone();

                    CurrentNoFieldValue = -1;
                }
        }
    }

    internal void RevealCurrentNoField(Player player, Territory inTerritory)
    {
        if (player != null && player.Faction == Faction.White)
            foreach (var l in inTerritory.Locations) RevealCurrentNoField(player, l);
    }

    internal void ExchangeResourcesInBribe(Player from, Player target, int amount)
    {
        from.Resources -= amount;

        if (BribesDuringMentat)
        {
            if (from.Faction == Faction.Red && target.Faction == from.Ally)
                target.Resources += amount;
            else
                target.Bribes += amount;
        }
        else
        {
            target.Resources += amount;
        }
    }

    internal void RevokePlanIfNeeded(Faction f)
    {
        RevokePlan(CurrentBattle?.PlanOf(f));
    }

    internal void RevokePlan(Battle plan)
    {
        if (plan == AggressorPlan)
            AggressorPlan = null;
        else if (plan == DefenderPlan) DefenderPlan = null;
    }

    internal void Prevent(Faction initiator, FactionAdvantage advantage)
    {
        Log(initiator, " prevent ", advantage);

        if (!PreventedAdvantages.Contains(advantage)) PreventedAdvantages.Add(advantage);
    }

    internal void Allow(FactionAdvantage advantage)
    {
        if (PreventedAdvantages.Contains(advantage))
        {
            PreventedAdvantages.Remove(advantage);
            Log(advantage, " is no longer prevented");
        }
    }

    internal void ApplyBureaucracy(Faction payer, Faction receiver)
    {
        if (!BureaucratWasUsedThisPhase)
        {
            var bureaucrat = PlayerSkilledAs(LeaderSkill.Bureaucrat);
            if (bureaucrat != null && bureaucrat.Faction != payer && bureaucrat.Faction != receiver)
            {
                if (Version < 133) BureaucratWasUsedThisPhase = true;
                PhaseBeforeBureaucratWasActivated = CurrentPhase;
                TargetOfBureaucracy = receiver;
                Enter(Phase.Bureaucracy);
            }
        }
    }

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

    internal void PlayNexusCard(Player initiator, params object[] messageElements)
    {
        Stone(Milestone.NexusPlayed);

        string typeOfNexus;
        if (NexusPlayed.CanUseCunning(initiator))
            typeOfNexus = "Cunning";
        else if (NexusPlayed.CanUseBetrayal(this, initiator))
            typeOfNexus = "Betrayal";
        else
            typeOfNexus = "Secret Ally";

        if (messageElements != null && messageElements.Length > 0)
            Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus} to ", MessagePart.Express(messageElements));
        else
            Log(initiator.Faction, " play ", initiator.Nexus, $" Nexus {typeOfNexus}");

        DiscardNexusCard(initiator);
    }

    internal void LogPreventionByKarma(FactionAdvantage prevented)
    {
        Log(TreacheryCardType.Karma, " prevents ", prevented);
    }

    internal void LogPreventionByLowThreshold(FactionAdvantage prevented)
    {
        Log("Low Threshold prevents ", prevented);
    }

    #endregion Events

    #region Information

    public bool Prevented(FactionAdvantage advantage)
    {
        return PreventedAdvantages.Contains(advantage);
    }

    public bool BribesDuringMentat => !Applicable(Rule.BribesAreImmediate);

    public bool KarmaPrevented(Faction f)
    {
        return CurrentKarmaPrevention != null && CurrentKarmaPrevention.Target == f;
    }

    public bool JuiceForcesFirstPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoFirst;

    public bool JuiceForcesLastPlayer => CurrentJuice != null && CurrentJuice.Type == JuiceType.GoLast;

    public bool YieldsSecrets(Player p)
    {
        return !SecretsRemainHidden.Contains(p.Faction);
    }

    public bool YieldsSecrets(Faction f)
    {
        return !SecretsRemainHidden.Contains(f);
    }

    #endregion Information
}