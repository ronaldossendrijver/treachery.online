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

    public PlayerSequence ShipmentAndMoveSequence { get; internal set; }
    public ILocationEvent LastShipmentOrMovement { get; internal set; }
    private Queue<Intrusion> Intrusions { get; } = new();
    public bool ShipsTechTokenIncome { get; internal set; }
    public List<IPlacement> RecentMoves { get; } = new();
    public int CurrentNoFieldValue { get; internal set; } = -1;
    public int LatestRevealedNoFieldValue { get; internal set; } = -1;
    public Dictionary<Faction, ShipmentPermission> ShipmentPermissions { get; } = new();
    internal List<Faction> FactionsWithOrnithoptersAtStartOfMovement { get; set; }
    internal bool BeginningOfShipmentAndMovePhase { get; set; }
    internal KarmaShipmentPrevention CurrentKarmaShipmentPrevention { get; set; }
    internal List<Territory> ChosenDestinationsWithAllies { get; } = new();
    internal bool CurrentPlayerMayPerformExtraMove { get; set; }
    internal bool BlueMayAccompany { get; set; }
    internal Phase PhaseBeforeCaravanCausedIntrusion { get; set; }
    internal Phase PhaseBeforeRevivalCausedIntrusion { get; set; }
    internal FlightUsed CurrentFlightUsed { get; set; }
    internal FlightDiscoveryUsed CurrentFlightDiscoveryUsed { get; set; }
    internal AmbassadorActivated CurrentAmbassadorActivated { get; set; }
    public Faction AllianceByAmbassadorOfferedTo { get; internal set; }
    internal Phase PausedAmbassadorPhase { get; set; }
    private Player PlayerToSetAsideVidal { get; set; }
    private VidalMoment WhenToSetAsideVidal { get; set; }
    internal Phase PausedTerrorPhase { get; set; }
    public bool AllianceByTerrorWasOffered { get; internal set; } = false;
    public Faction AllianceByTerrorOfferedTo { get; internal set; }
    public bool InOrangeCunningShipment { get; internal set; }
    public List<Territory> CurrentBlockedTerritories { get; } = new();
    internal bool BrownHasExtraMove { get; set; }

    #endregion State

    internal void StartShipAndMoveSequence()
    {
        if (ShipmentAndMoveSequence.CurrentFaction == Faction.Orange && OrangeMayShipOutOfTurnOrder) ShipmentAndMoveSequence.NextPlayer();

        Enter(JuiceForcesFirstPlayer && CurrentJuice.Initiator != Faction.Orange, Phase.NonOrangeShip, IsPlaying(Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip, Phase.NonOrangeShip);
    }

    internal void PerformMoveFromLocations(Player initiator, Dictionary<Location, Battalion> forceLocations, ILocationEvent evt, bool asAdvisors, bool byCaravan)
    {
        LastShipmentOrMovement = evt;
        var wasOccupiedBeforeMove = IsOccupied(evt.To.Territory);
        var dist = DetermineMaximumMoveDistance(initiator, forceLocations.Values);

        foreach (var fromTerritory in forceLocations.Keys.Select(l => l.Territory).Distinct())
        {
            var totalNumberOfForces = 0;
            var totalNumberOfSpecialForces = 0;

            foreach (var fl in forceLocations.Where(fl => fl.Key.Territory == fromTerritory))
                if (Version <= 150 || fl.Value.TotalAmountOfForces > 0)
                {
                    PerformMoveFromLocation(initiator, fl.Key, fl.Value, evt.To, ref totalNumberOfForces, ref totalNumberOfSpecialForces);
                    CheckSandmaster(initiator, evt.To, dist, fl);
                }

            if (initiator.Is(Faction.Blue)) initiator.FlipForces(evt.To, wasOccupiedBeforeMove && asAdvisors);

            if (totalNumberOfForces > 0 || totalNumberOfSpecialForces > 0) LogMove(initiator, fromTerritory, evt.To, totalNumberOfForces, totalNumberOfSpecialForces, wasOccupiedBeforeMove && asAdvisors, byCaravan);
        }

        Stone(Milestone.Move);
        FlipBeneGesseritWhenAlone();
    }

    private void CheckSandmaster(Player initiator, Location to, int dist, KeyValuePair<Location, Battalion> fl)
    {
        if (SkilledAs(initiator, LeaderSkill.Sandmaster))
        {
            var paths = Map.FindPaths(fl.Key, to, dist, initiator.Faction == Faction.Yellow && Applicable(Rule.YellowMayMoveIntoStorm), initiator.Faction, this);
            var mostSpice = 0;
            List<Location> pathWithMostSpice = null;
            foreach (var p in paths)
            {
                var amountOfSpiceLocations = p.Where(l => ResourcesOnPlanet.ContainsKey(l)).Distinct().Count();
                if (amountOfSpiceLocations > mostSpice)
                {
                    mostSpice = amountOfSpiceLocations;
                    pathWithMostSpice = p;
                }
            }

            if (mostSpice > 0)
            {
                foreach (var loc in pathWithMostSpice)
                    if (ResourcesOnPlanet.ContainsKey(loc)) ChangeResourcesOnPlanet(loc, -1);

                initiator.Resources += mostSpice;
                Log(initiator.Faction, " ", LeaderSkill.Sandmaster, " collects ", Payment.Of(mostSpice), " along the way");
            }
        }
    }

    internal void PerformMoveFromLocation(Player initiator, Location from, Battalion battalion, Location to, ref int totalNumberOfForces, ref int totalNumberOfSpecialForces)
    {
        var mustMoveThroughStorm = MustMoveThroughStorm(initiator, from, to, battalion);
        if (IsInStorm(to) || mustMoveThroughStorm)
        {
            int killCount;
            if (initiator.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowProtectedFromStorm) && Applicable(Rule.YellowStormLosses))
            {
                killCount = 0;
                initiator.MoveForces(from, to, battalion.AmountOfForces);
                initiator.MoveSpecialForces(from, to, battalion.AmountOfSpecialForces);
                StormLossesToTake.Add(new LossToTake { Location = to, Amount = TakeLosses.HalfOf(battalion.AmountOfForces, battalion.AmountOfSpecialForces), Faction = battalion.Faction });
            }
            else
            {
                killCount = battalion.AmountOfForces + battalion.AmountOfSpecialForces;
                initiator.KillForces(from, battalion.AmountOfForces, battalion.AmountOfSpecialForces, false);
            }

            if (killCount > 0) Log(killCount, initiator.Faction, "forces are killed by the storm while travelling");
        }
        else
        {
            initiator.MoveForces(from, to, battalion.AmountOfForces);
            initiator.MoveSpecialForces(from, to, battalion.AmountOfSpecialForces);
        }

        totalNumberOfForces += battalion.AmountOfForces;
        totalNumberOfSpecialForces += battalion.AmountOfSpecialForces;
    }

    private bool MustMoveThroughStorm(Player initiator, Location from, Location to, Battalion moved)
    {
        if (from == null || to == null) return false;

        var max = DetermineMaximumMoveDistance(initiator, new[] { moved });
        var targetsAvoidingStorm = Map.FindNeighbours(from, max, false, initiator.Faction, this);
        var targetsIgnoringStorm = Map.FindNeighbours(from, max, true, initiator.Faction, this);
        return !targetsAvoidingStorm.Contains(to) && targetsIgnoringStorm.Contains(to);
    }

    internal void LogMove(Player initiator, Territory from, Location to, int forceAmount, int specialForceAmount, bool asAdvisors, bool byCaravan)
    {
        Log(
            CaravanMessage(byCaravan),
            initiator.Faction,
            " move ",
            MessagePart.ExpressIf(forceAmount > 0, forceAmount, initiator.Force),
            MessagePart.ExpressIf(specialForceAmount > 0, specialForceAmount, initiator.SpecialForce),
            " from ",
            from,
            " to ",
            to,
            MessagePart.ExpressIf(initiator.Is(Faction.Blue) && Applicable(Rule.BlueAdvisors), " as ", asAdvisors ? initiator.SpecialForce : initiator.Force));
    }

    private static MessagePart CaravanMessage(bool byCaravan)
    {
        return MessagePart.ExpressIf(byCaravan, "By ", TreacheryCardType.Caravan, ", ");
    }

    internal void TakeVidal(Player p, VidalMoment whenToSetAside)
    {
        var vidal = Vidal;

        var currentOwner = OwnerOf(vidal);
        if (currentOwner != null)
        {
            currentOwner.Leaders.Remove(vidal);

            if (IsAlive(vidal)) Log(currentOwner.Faction, " lose ", vidal);

            CapturedLeaders.Remove(vidal);
        }

        p.Leaders.Add(vidal);
        PlayerToSetAsideVidal = p;
        WhenToSetAsideVidal = whenToSetAside;

        Log(p.Faction, " take ", vidal);

        SetInFrontOfShield(vidal, false);
    }

    internal void LetPlayerDiscardTreacheryCardOfChoice(Faction f)
    {
        PhaseBeforeDiscarding = CurrentPhase;
        FactionsThatMustDiscard.Add(f);
        Enter(Phase.Discarding);
    }

    internal void KillAmbassadorIn(Territory territory)
    {
        var ambassador = AmbassadorIn(territory);
        if (ambassador != Ambassador.None)
        {
            var pink = GetPlayer(Faction.Pink);
            AmbassadorsOnPlanet.Remove(territory);
            pink.Ambassadors.Add(ambassador);
            Log("The ambassador in ", territory, " returns to ", Faction.Pink);
        }
    }

    internal void DetermineNextShipmentAndMoveSubPhase()
    {
        //Console.WriteLine("** DetermineNextShipmentAndMoveSubPhase **");

        /*Console.WriteLine("* Before cleanup: *");
        foreach (var i in Intrusions)
        {
            Console.WriteLine($"- {i.GetType()} {i.Type} {i.TriggeringEvent.To} {i.TriggeringEvent.Initiator} {i.Territory}");
        }*/

        CleanupObsoleteIntrusions();

        /*Console.WriteLine("* After cleanup: *");
        foreach (var i in Intrusions)
        {
            Console.WriteLine($"- {i.GetType()} {i.Type} {i.TriggeringEvent.To} {i.TriggeringEvent.Initiator} {i.Territory}");
        }*/

        if (BlueMayAccompany)
        {
            if (Prevented(FactionAdvantage.BlueAccompanies))
            {
                LogPreventionByKarma(FactionAdvantage.BlueAccompanies);
                BlueMayAccompany = false;
                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueAccompanies);
            }
            else if (HasLowThreshold(Faction.Blue))
            {
                BlueMayAccompany = false;
            }
        }

        if (StormLossesToTake.Count > 0)
        {
            PhaseBeforeStormLoss = CurrentPhase;
            Enter(Phase.StormLosses);
        }
        else if (Intrusions.Any())
        {
            switch (Intrusions.Peek().Type)
            {
                case IntrusionType.BlueIntrusion:

                    DetermineNextShipmentAndMoveSubPhaseOnBlueIntruded();
                    break;

                case IntrusionType.Terror:

                    DetermineNextShipmentAndMoveSubPhaseOnTerrorTriggered();
                    break;

                case IntrusionType.Ambassador:

                    DetermineNextShipmentAndMoveSubPhaseOnAmbassadorTriggered();
                    break;
            }
        }
        else
        {
            DetermineNextShipmentAndMoveSubPhaseOnNoIntrusion();
        }
    }

    private void CleanupObsoleteIntrusions()
    {
        if (Intrusions.Any())
        {

            var i = 0;
            var search = true;
            while (search)
            {
                i++;
                if (i > 100) throw new Exception("Stuck");

                if (LastBlueIntrusion != null && GetPlayer(Faction.Blue).ForcesIn(LastBlueIntrusion.Territory) == 0)
                    DequeueIntrusion(IntrusionType.BlueIntrusion);
                else if (LastTerrorTrigger != null && !TerrorIn(LastTerrorTrigger.Territory).Any())
                    DequeueIntrusion(IntrusionType.Terror);
                else if (LastAmbassadorTrigger != null && AmbassadorIn(LastAmbassadorTrigger.Territory) == Ambassador.None)
                    DequeueIntrusion(IntrusionType.Ambassador);
                else
                    search = false;
            }
        }
    }

    private void DetermineNextShipmentAndMoveSubPhaseOnNoIntrusion()
    {
        switch (CurrentPhase)
        {
            case Phase.YellowRidingMonsterA:
            case Phase.BlueIntrudedByYellowRidingMonsterA:
            case Phase.TerrorTriggeredByYellowRidingMonsterA:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                EndWormRideDuringPhase(Phase.YellowRidingMonsterA);
                break;

            case Phase.YellowRidingMonsterB:
            case Phase.BlueIntrudedByYellowRidingMonsterB:
            case Phase.TerrorTriggeredByYellowRidingMonsterB:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                EndWormRideDuringPhase(Phase.YellowRidingMonsterB);
                break;

            case Phase.OrangeShip:
            case Phase.BlueIntrudedByOrangeShip:
            case Phase.TerrorTriggeredByOrangeShip:
            case Phase.AmbassadorTriggeredByOrangeShip:
                Enter(IsPlaying(Faction.Blue) && BlueMayAccompany, Phase.BlueAccompaniesOrangeShip, !InOrangeCunningShipment, Phase.OrangeMove, Phase.NonOrangeMove);
                break;

            case Phase.BlueAccompaniesOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                Enter(!InOrangeCunningShipment, Phase.OrangeMove, Phase.NonOrangeMove);
                break;

            case Phase.NonOrangeShip:
            case Phase.BlueIntrudedByNonOrangeShip:
            case Phase.TerrorTriggeredByNonOrangeShip:
            case Phase.AmbassadorTriggeredByNonOrangeShip:
                Enter(IsPlaying(Faction.Blue) && BlueMayAccompany, Phase.BlueAccompaniesNonOrangeShip, Phase.NonOrangeMove);
                break;

            case Phase.BlueAccompaniesNonOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                Enter(Phase.NonOrangeMove);
                break;

            case Phase.OrangeMove:
            case Phase.BlueIntrudedByOrangeMove:
            case Phase.TerrorTriggeredByOrangeMove:
            case Phase.AmbassadorTriggeredByOrangeMove:
                DetermineNextSubPhaseAfterOrangeShipAndMove();
                break;

            case Phase.NonOrangeMove:
            case Phase.BlueIntrudedByNonOrangeMove:
            case Phase.TerrorTriggeredByNonOrangeMove:
            case Phase.AmbassadorTriggeredByNonOrangeMove:
                DetermineNextSubPhaseAfterNonOrangeMove();
                break;

            case Phase.BlueIntrudedByCaravan:
            case Phase.TerrorTriggeredByCaravan:
            case Phase.AmbassadorTriggeredByCaravan:
                Enter(PhaseBeforeCaravanCausedIntrusion);
                break;

            case Phase.BlueIntrudedByRevival:
            case Phase.TerrorTriggeredByRevival:
            case Phase.AmbassadorTriggeredByRevival:
            {
                //TODO This code is really ugly. Only way to solve this is to put ALL interrupting effects (including skill assignment) on one stack.
                var nextPhase = PhaseBeforeRevivalCausedIntrusion;
                if (PhaseBeforeRevivalCausedIntrusion is Phase.AssigningSkill && !SkillAssigned.PlayersMustChooseLeaderSkills(this))
                {
                    nextPhase = PhaseBeforeSkillAssignment;
                    if (nextPhase == CurrentPhase)
                    {
                        nextPhase = Phase.Resurrection;
                    }
                }

                Enter(nextPhase);
                break;
            }

        }
    }

    private void DetermineNextShipmentAndMoveSubPhaseOnBlueIntruded()
    {
        switch (CurrentPhase)
        {
            case Phase.YellowRidingMonsterA:
            case Phase.BlueIntrudedByYellowRidingMonsterA:
            case Phase.TerrorTriggeredByYellowRidingMonsterA:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                Enter(Phase.BlueIntrudedByYellowRidingMonsterA);
                break;

            case Phase.YellowRidingMonsterB:
            case Phase.BlueIntrudedByYellowRidingMonsterB:
            case Phase.TerrorTriggeredByYellowRidingMonsterB:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                Enter(Phase.BlueIntrudedByYellowRidingMonsterB);
                break;

            case Phase.OrangeShip:
            case Phase.BlueIntrudedByOrangeShip:
            case Phase.TerrorTriggeredByOrangeShip:
            case Phase.AmbassadorTriggeredByOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                Enter(Phase.BlueIntrudedByOrangeShip);
                break;

            case Phase.NonOrangeShip:
            case Phase.BlueIntrudedByNonOrangeShip:
            case Phase.TerrorTriggeredByNonOrangeShip:
            case Phase.AmbassadorTriggeredByNonOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                Enter(Phase.BlueIntrudedByNonOrangeShip);
                break;

            case Phase.OrangeMove:
            case Phase.BlueIntrudedByOrangeMove:
            case Phase.TerrorTriggeredByOrangeMove:
            case Phase.AmbassadorTriggeredByOrangeMove:
                Enter(Phase.BlueIntrudedByOrangeMove);
                break;

            case Phase.NonOrangeMove:
            case Phase.BlueIntrudedByNonOrangeMove:
            case Phase.TerrorTriggeredByNonOrangeMove:
            case Phase.AmbassadorTriggeredByNonOrangeMove:
                Enter(Phase.BlueIntrudedByNonOrangeMove);
                break;

            case Phase.BlueIntrudedByCaravan:
            case Phase.TerrorTriggeredByCaravan:
            case Phase.AmbassadorTriggeredByCaravan:
                Enter(Phase.BlueIntrudedByCaravan);
                break;

            case Phase.BlueIntrudedByRevival:
            case Phase.TerrorTriggeredByRevival:
            case Phase.AmbassadorTriggeredByRevival:
                Enter(Phase.BlueIntrudedByRevival);
                break;

            default:
                throw new Exception($"Blue intrusion triggered during undefined phase: {CurrentPhase}");
        }
    }

    private void DetermineNextShipmentAndMoveSubPhaseOnTerrorTriggered()
    {
        switch (CurrentPhase)
        {
            case Phase.YellowRidingMonsterA:
            case Phase.BlueIntrudedByYellowRidingMonsterA:
            case Phase.TerrorTriggeredByYellowRidingMonsterA:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                Enter(Phase.TerrorTriggeredByYellowRidingMonsterA);
                break;

            case Phase.YellowRidingMonsterB:
            case Phase.BlueIntrudedByYellowRidingMonsterB:
            case Phase.TerrorTriggeredByYellowRidingMonsterB:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                Enter(Phase.TerrorTriggeredByYellowRidingMonsterB);
                break;

            case Phase.OrangeShip:
            case Phase.BlueIntrudedByOrangeShip:
            case Phase.TerrorTriggeredByOrangeShip:
            case Phase.AmbassadorTriggeredByOrangeShip:
                Enter(Phase.TerrorTriggeredByOrangeShip);
                break;

            case Phase.BlueAccompaniesOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                Enter(Phase.TerrorTriggeredByOrangeShip);
                break;

            case Phase.NonOrangeShip:
            case Phase.BlueIntrudedByNonOrangeShip:
            case Phase.TerrorTriggeredByNonOrangeShip:
            case Phase.AmbassadorTriggeredByNonOrangeShip:
                Enter(Phase.TerrorTriggeredByNonOrangeShip);
                break;

            case Phase.BlueAccompaniesNonOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                Enter(Phase.TerrorTriggeredByNonOrangeShip);
                break;

            case Phase.OrangeMove:
            case Phase.BlueIntrudedByOrangeMove:
            case Phase.TerrorTriggeredByOrangeMove:
            case Phase.AmbassadorTriggeredByOrangeMove:
                Enter(Phase.TerrorTriggeredByOrangeMove);
                break;

            case Phase.NonOrangeMove:
            case Phase.BlueIntrudedByNonOrangeMove:
            case Phase.TerrorTriggeredByNonOrangeMove:
            case Phase.AmbassadorTriggeredByNonOrangeMove:
                Enter(Phase.TerrorTriggeredByNonOrangeMove);
                break;

            case Phase.BlueIntrudedByCaravan:
            case Phase.TerrorTriggeredByCaravan:
            case Phase.AmbassadorTriggeredByCaravan:
                Enter(Phase.TerrorTriggeredByCaravan);
                break;

            case Phase.BlueIntrudedByRevival:
            case Phase.TerrorTriggeredByRevival:
            case Phase.AmbassadorTriggeredByRevival:
                Enter(Phase.TerrorTriggeredByRevival);
                break;

            default:
                throw new Exception($"Terror triggered during undefined phase: {CurrentPhase}");
        }
    }

    private void DetermineNextShipmentAndMoveSubPhaseOnAmbassadorTriggered()
    {
        switch (CurrentPhase)
        {
            case Phase.YellowRidingMonsterA:
            case Phase.BlueIntrudedByYellowRidingMonsterA:
            case Phase.TerrorTriggeredByYellowRidingMonsterA:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterA:
                Enter(Phase.AmbassadorTriggeredByYellowRidingMonsterA);
                break;

            case Phase.YellowRidingMonsterB:
            case Phase.BlueIntrudedByYellowRidingMonsterB:
            case Phase.TerrorTriggeredByYellowRidingMonsterB:
            case Phase.AmbassadorTriggeredByYellowRidingMonsterB:
                Enter(Phase.AmbassadorTriggeredByYellowRidingMonsterB);
                break;

            case Phase.OrangeShip:
            case Phase.BlueIntrudedByOrangeShip:
            case Phase.TerrorTriggeredByOrangeShip:
            case Phase.AmbassadorTriggeredByOrangeShip:
                Enter(Phase.AmbassadorTriggeredByOrangeShip);
                break;

            case Phase.BlueAccompaniesOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip:
                Enter(Phase.AmbassadorTriggeredByBlueAccompaniesOrangeShip);
                break;

            case Phase.NonOrangeShip:
            case Phase.BlueIntrudedByNonOrangeShip:
            case Phase.TerrorTriggeredByNonOrangeShip:
            case Phase.AmbassadorTriggeredByNonOrangeShip:
                Enter(Phase.AmbassadorTriggeredByNonOrangeShip);
                break;

            case Phase.BlueAccompaniesNonOrangeShip:
            case Phase.TerrorTriggeredByBlueAccompaniesNonOrangeShip:
            case Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip:
                Enter(Phase.AmbassadorTriggeredByBlueAccompaniesNonOrangeShip);
                break;

            case Phase.OrangeMove:
            case Phase.BlueIntrudedByOrangeMove:
            case Phase.TerrorTriggeredByOrangeMove:
            case Phase.AmbassadorTriggeredByOrangeMove:
                Enter(Phase.AmbassadorTriggeredByOrangeMove);
                break;

            case Phase.NonOrangeMove:
            case Phase.BlueIntrudedByNonOrangeMove:
            case Phase.TerrorTriggeredByNonOrangeMove:
            case Phase.AmbassadorTriggeredByNonOrangeMove:
                Enter(Phase.AmbassadorTriggeredByNonOrangeMove);
                break;

            case Phase.BlueIntrudedByCaravan:
            case Phase.TerrorTriggeredByCaravan:
            case Phase.AmbassadorTriggeredByCaravan:
                Enter(Phase.AmbassadorTriggeredByCaravan);
                break;

            case Phase.BlueIntrudedByRevival:
            case Phase.TerrorTriggeredByRevival:
            case Phase.AmbassadorTriggeredByRevival:
                Enter(Phase.AmbassadorTriggeredByRevival);
                break;

            default:
                throw new Exception($"Ambassador triggered during undefined phase: {CurrentPhase}");
        }
    }

    //private bool IsFirst(Faction a, Faction b) => !PlayerSequence.IsAfter(this, GetPlayer(a), GetPlayer(b));

    internal void CheckIfForcesShouldBeDestroyedByAllyPresence(Player p)
    {
        if (p.Ally != Faction.None && p.Faction != Faction.Pink && p.Ally != Faction.Pink)
        {
            //Forces that must be destroyed because moves ended where allies are
            foreach (var t in ChosenDestinationsWithAllies)
                if (p.AnyForcesIn(t) > 0)
                {
                    Log("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
                    RevealCurrentNoField(p, t);
                    p.KillAllForces(t, false);
                }

            if (HasActedOrPassed.Contains(p.Ally))
            {
                //Forces that must me destroyed if both the player and his ally have moved

                var playerTerritories = Applicable(Rule.AdvisorsDontConflictWithAlly) ? p.OccupiedTerritories : p.TerritoriesWithForces;
                var allyTerritories = Applicable(Rule.AdvisorsDontConflictWithAlly) ? GetPlayer(p.Ally).OccupiedTerritories : GetPlayer(p.Ally).TerritoriesWithForces;

                foreach (var t in playerTerritories.Intersect(allyTerritories).ToList())
                    if (t != Map.PolarSink.Territory)
                    {
                        Log("All ", p.Faction, " forces in ", t, " were killed due to ally presence");
                        RevealCurrentNoField(p, t);
                        p.KillAllForces(t, false);
                    }
            }
        }
    }

    public bool ThreatenedByAllyPresence(Player p, Territory t)
    {
        if (t == Map.PolarSink.Territory) return false;

        var ally = GetPlayer(p.Ally);
        if (ally == null) return false;

        if (p.Faction == Faction.Pink || p.Ally == Faction.Pink) return false;

        if (Applicable(Rule.AdvisorsDontConflictWithAlly))
        {
            if (p.Is(Faction.Blue) && !p.Occupies(t))
                return false;
            if (p.Ally == Faction.Blue && !ally.Occupies(t))
                return false;
            return ally.AnyForcesIn(t) > 0;
        }

        return ally.AnyForcesIn(t) > 0;
    }

    private void DetermineNextSubPhaseAfterNonOrangeMove()
    {
        if (CurrentPlayerMayPerformExtraMove)
        {
            CurrentPlayerMayPerformExtraMove = false;
            Enter(Phase.NonOrangeMove);
        }
        else
        {
            Enter(
                EveryoneActedOrPassed, ConcludeShipmentAndMove,
                IsPlaying(Faction.Orange) && !HasActedOrPassed.Any(p => p == Faction.Orange) && OrangeMayShipOutOfTurnOrder, Phase.OrangeShip,
                Phase.NonOrangeShip);
        }
    }

    private void DetermineNextSubPhaseAfterOrangeShipAndMove()
    {
        if (CurrentPlayerMayPerformExtraMove)
        {
            CurrentPlayerMayPerformExtraMove = false;
            Enter(Phase.OrangeMove);
        }
        else if (!InOrangeCunningShipment && CurrentOrangeCunning != null && CurrentOrangeCunning.By(Faction.Orange))
        {
            InOrangeCunningShipment = true;
            Enter(Phase.OrangeShip);
        }
        else
        {
            Enter(!EveryoneActedOrPassed, Phase.NonOrangeShip, ConcludeShipmentAndMove);
        }
    }

    private void ConcludeShipmentAndMove()
    {
        CheckIfCyanGainsVidal();
        CurrentKarmaShipmentPrevention = null;
        MainPhaseEnd();
        Enter(Phase.ShipmentAndMoveConcluded);
        ReceiveShipsTechIncome();
        BrownHasExtraMove = false;
        CurrentBlueCunning = null;
    }

    private void CheckIfCyanGainsVidal()
    {
        var cyan = GetPlayer(Faction.Cyan);
        if (cyan != null && !Prevented(FactionAdvantage.CyanGainingVidal))
        {
            var vidal = Vidal;

            if (VidalIsAlive)
            {
                var pink = GetPlayer(Faction.Pink);
                var nrOfBattlesInStrongholds = Battle.BattlesToBeFought(this, cyan).Select(batt => batt.Territory)
                    .Where(t => (pink == null || pink.AnyForcesIn(t) == 0) && (t.IsStronghold || IsSpecialStronghold(t))).Distinct().Count();

                if (nrOfBattlesInStrongholds >= 2 && !VidalIsCapturedOrGhola && OccupierOf(World.Pink) == null) TakeVidal(cyan, VidalMoment.EndOfTurn);
            }
        }
    }

    private void ReceiveShipsTechIncome()
    {
        if (ShipsTechTokenIncome)
        {
            var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Ships));
            if (techTokenOwner != null)
            {
                var amount = techTokenOwner.TechTokens.Count;
                techTokenOwner.Resources += amount;
                Log(techTokenOwner.Faction, " receive ", Payment.Of(amount), " from ", TechToken.Ships);
            }
        }
    }

    #region Intrusions

    internal bool CheckIntrusion(ILocationEvent e)
    {
        CheckBlueIntrusion(e, e.Initiator, e.To.Territory);
        CheckAmbassadorTriggered(e);
        CheckTerrorTriggered(e);
        return Intrusions.Count > 0;
    }

    private void CheckBlueIntrusion(ILocationEvent e, Faction initiator, Territory territory)
    {
        var bgPlayer = GetPlayer(Faction.Blue);
        if (bgPlayer != null &&
            territory != Map.PolarSink.Territory &&
            !territory.IsHomeworld &&
            Applicable(Rule.BlueAdvisors) &&
            initiator != bgPlayer.Ally &&
            initiator != Faction.Blue &&
            bgPlayer.ForcesIn(territory) > 0)
        {
            if (Prevented(FactionAdvantage.BlueIntrusion))
            {
                LogPreventionByKarma(FactionAdvantage.BlueIntrusion);
                if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueIntrusion);
            }
            else
            {
                QueueIntrusion(e, IntrusionType.BlueIntrusion);
            }
        }
    }

    private void CheckAmbassadorTriggered(ILocationEvent e)
    {
        var pinkPlayer = GetPlayer(Faction.Pink);
        if (pinkPlayer != null &&
            (Version < 160 || e.Initiator != Faction.Blue || e.ForcesAddedToLocation > 0) &&
            e.Initiator != Faction.Pink &&
            e.Initiator != pinkPlayer.Ally &&
            AmbassadorIn(e.To.Territory) != Ambassador.None &&
            AmbassadorIn(e.To.Territory) != AmbassadorOf(e.Initiator))
            QueueIntrusion(e, IntrusionType.Ambassador);
    }

    private void CheckTerrorTriggered(ILocationEvent e)
    {
        var cyanPlayer = GetPlayer(Faction.Cyan);
        if (cyanPlayer != null &&
            (e.TotalAmountOfForcesAddedToLocation >= 3 || !cyanPlayer.HasLowThreshold()) &&
            e.Initiator != Faction.Cyan &&
            e.Initiator != cyanPlayer.Ally &&
            !IsOccupiedByFactionOrTheirAlly(World.Cyan, e.Initiator) &&
            TerrorIn(e.To.Territory).Any())
            QueueIntrusion(e, IntrusionType.Terror);
    }

    private void QueueIntrusion(ILocationEvent e, IntrusionType type)
    {
        Intrusions.Enqueue(new Intrusion(e, type));
    }

    internal void DequeueIntrusion(IntrusionType type)
    {
        if (Intrusions.Count == 0)
            throw new ArgumentException("No intrusions to dequeue");
        if (Intrusions.Peek().Type != type) throw new ArgumentException($"Wrong intrusion type: actual:{type} vs expected:{Intrusions.Peek().Type}");

        Intrusions.Dequeue();
    }

    #endregion Intrusions


    #region Information

    public Leader Vidal => LeaderState.Keys.FirstOrDefault(h => h.HeroType == HeroType.Vidal) as Leader;
    public bool VidalIsAlive => Vidal != null && IsAlive(Vidal);

    public bool VidalIsCapturedOrGhola
    {
        get
        {
            var playerWithVidal = Players.FirstOrDefault(p => p.Leaders.Any(l => l.HeroType == HeroType.Vidal));
            return playerWithVidal != null && (CapturedLeaders.Keys.Any(h => h.HeroType == HeroType.Vidal) || playerWithVidal.Is(Faction.Purple));
        }
    }


    public bool PreventedFromShipping(Faction f)
    {
        return CurrentKarmaShipmentPrevention != null && CurrentKarmaShipmentPrevention.Target == f;
    }

    public bool HasShipmentPermission(Player p, ShipmentPermission permission)
    {
        return ShipmentPermissions.TryGetValue(p.Faction, out var permissions) &&
               (permissions & permission) == permission;
    }

    public Intrusion LastBlueIntrusion => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.BlueIntrusion ? Intrusions.Peek() : null;

    public Intrusion LastTerrorTrigger => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.Terror ? Intrusions.Peek() : null;

    public Intrusion LastAmbassadorTrigger => Intrusions.Count > 0 && Intrusions.Peek().Type == IntrusionType.Ambassador ? Intrusions.Peek() : null;

    internal bool OccupiesArrakeenOrCarthag(Player p)
    {
        return p.Occupies(Map.Arrakeen) || p.Occupies(Map.Carthag);
    }

    internal static Ambassador AmbassadorOf(Faction faction)
    {
        return (Ambassador)(int)faction;
    }

    public bool OrangeMayDelay => OrangeMayShipOutOfTurnOrder && Players.Count - HasActedOrPassed.Count > (JuiceForcesLastPlayer && !HasActedOrPassed.Contains(CurrentJuice.Initiator) ? 2 : 1);

    internal bool OrangeMayShipOutOfTurnOrder => Applicable(Rule.OrangeDetermineShipment) && (Version < 113 || !Prevented(FactionAdvantage.OrangeDetermineMoveMoment));

    #endregion Information
}