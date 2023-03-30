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
        #region BeginningOfRevival

        internal bool RevivalTechTokenIncome { get; set; }

        public List<Faction> FactionsThatTookFreeRevival { get; private set; } = new List<Faction>();

        internal bool PurpleStartedRevivalWithLowThreshold { get; set; }

        
        #endregion

        #region Revival

        public List<Faction> FactionsThatRevivedSpecialForcesThisTurn { get; private set; } = new List<Faction>();
        public void HandleEvent(Revival r)
        {
            var initiator = GetPlayer(r.Initiator);

            if (r.UsesRedSecretAlly)
            {
                PlayNexusCard(r.Player, "Secret Ally", "revive ", 3, " additional forces beyond revival limits for free");
            }

            //Payment
            var cost = Revival.DetermineCost(this, initiator, r.Hero, r.AmountOfForces, r.AmountOfSpecialForces, r.ExtraForcesPaidByRed, r.ExtraSpecialForcesPaidByRed, r.UsesRedSecretAlly);
            if (cost.CostForEmperor > 0)
            {
                var emperor = GetPlayer(Faction.Red);
                emperor.Resources -= cost.CostForEmperor;
            }
            initiator.Resources -= cost.TotalCostForPlayer;

            int highThresholdBonus = r.Initiator == Faction.Grey && HasHighThreshold(Faction.Grey) ? Math.Max(0, Math.Min(2, r.Player.ForcesKilled - r.AmountOfForces - r.ExtraForcesPaidByRed)) : 0;

            //Force revival
            initiator.ReviveForces(r.AmountOfForces + r.ExtraForcesPaidByRed + highThresholdBonus);
            initiator.ReviveSpecialForces(r.AmountOfSpecialForces + r.ExtraSpecialForcesPaidByRed);

            if (r.AmountOfSpecialForces > 0)
            {
                FactionsThatRevivedSpecialForcesThisTurn.Add(r.Initiator);
            }

            //Register free revival
            bool usesFreeRevival = false;
            if (r.AmountOfForces + r.AmountOfSpecialForces > 0 && FreeRevivals(initiator, r.UsesRedSecretAlly) > 0)
            {
                usesFreeRevival = true;
                FactionsThatTookFreeRevival.Add(r.Initiator);
            }

            //Tech token activated?
            if (usesFreeRevival && r.Initiator != Faction.Purple)
            {
                RevivalTechTokenIncome = true;
            }

            //Purple income
            var purple = GetPlayer(Faction.Purple);
            int totalProfitsForPurple = 0;
            if (purple != null)
            {
                if (usesFreeRevival && !PurpleStartedRevivalWithLowThreshold)
                {
                    totalProfitsForPurple += 1;
                }

                if (r.Initiator != Faction.Purple)
                {
                    totalProfitsForPurple += cost.TotalCostForForceRevival;
                    totalProfitsForPurple += cost.CostToReviveHero;
                }

                if (totalProfitsForPurple > 0 && Prevented(FactionAdvantage.PurpleReceiveRevive))
                {
                    totalProfitsForPurple = 0;
                    LogPreventionByKarma(FactionAdvantage.PurpleReceiveRevive);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.PurpleReceiveRevive);
                }

                purple.Resources += totalProfitsForPurple;

                if (totalProfitsForPurple >= 5)
                {
                    ApplyBureaucracy(initiator.Faction, Faction.Purple);
                }

                if (cost.Total - totalProfitsForPurple >= 4)
                {
                    ActivateBanker(initiator);
                }
            }

            //Hero revival
            bool asGhola = false;
            if (r.Hero != null)
            {
                if (initiator.Faction != r.Hero.Faction && r.Hero is Leader)
                {
                    asGhola = true;
                    ReviveGhola(initiator, r.Hero as Leader);
                }
                else if (purple != null && purple.Leaders.Contains(r.Hero) && IsAlive(r.Hero))
                {
                    //Transfer of ghola
                    purple.Leaders.Remove(r.Hero as Leader);
                    initiator.Leaders.Add(r.Hero as Leader);
                }
                else
                {
                    ReviveHero(r.Hero);
                }

                if (r.AssignSkill)
                {
                    PrepareSkillAssignmentToRevivedLeader(r.Player, r.Hero as Leader);
                }

                EarlyRevivalsOffers.Remove(r.Hero);
            }

            if (r.Location != null)
            {
                if (r.Initiator == Faction.Yellow)
                {
                    initiator.ShipSpecialForces(r.Location, 1);
                    Log(r.Initiator, " place ", FactionSpecialForce.Yellow, " in ", r.Location);
                }
                else if (r.Initiator == Faction.Purple)
                {
                    initiator.ShipForces(r.Location, r.AmountOfForces + r.ExtraForcesPaidByRed);
                    Log(r.Initiator, " place ", r.AmountOfForces + r.ExtraForcesPaidByRed, FactionForce.Purple, " in ", r.Location);
                }
            }

            //Logging
            Stone(Milestone.Revival);
            LogRevival(r, initiator, cost, totalProfitsForPurple, asGhola, highThresholdBonus);

            if (r.Initiator != Faction.Purple)
            {
                HasActedOrPassed.Add(r.Initiator);
            }
        }

        internal void PrepareSkillAssignmentToRevivedLeader(Player player, Leader leader)
        {
            if (CurrentPhase != Phase.AssigningSkill)
            {
                PhaseBeforeSkillAssignment = CurrentPhase;
            }

            player.MostRecentlyRevivedLeader = leader;
            SkillDeck.Shuffle();
            Stone(Milestone.Shuffled);
            player.SkillsToChooseFrom.Add(SkillDeck.Draw());
            player.SkillsToChooseFrom.Add(SkillDeck.Draw());
            Enter(Phase.AssigningSkill);
        }

        internal void ReviveHero(IHero h)
        {
            LeaderState[h].Revive();
            LeaderState[h].CurrentTerritory = null;
        }

        private void ReviveGhola(Player initiator, Leader l)
        {
            LeaderState[l].Revive();
            LeaderState[l].CurrentTerritory = null;

            var currentOwner = Players.FirstOrDefault(p => p.Leaders.Contains(l));
            if (currentOwner != null)
            {
                currentOwner.Leaders.Remove(l);
                initiator.Leaders.Add(l);
            }
        }

        public int FreeRevivals(Player player, bool usesRedSecretAlly)
        {
            if (FactionsThatTookFreeRevival.Contains(player.Faction) || FreeRevivalPrevented(player.Faction))
            {
                return 0;
            }
            else
            {
                int nrOfFreeRevivals = 0;

                switch (player.Faction)
                {
                    case Faction.Yellow: nrOfFreeRevivals = 3; break;
                    case Faction.Green: nrOfFreeRevivals = 2; break;
                    case Faction.Black: nrOfFreeRevivals = 2; break;
                    case Faction.Red: nrOfFreeRevivals = 1; break;
                    case Faction.Orange: nrOfFreeRevivals = 1; break;
                    case Faction.Blue: nrOfFreeRevivals = 1; break;
                    case Faction.Grey: nrOfFreeRevivals = 1; break;
                    case Faction.Purple: nrOfFreeRevivals = 2; break;

                    case Faction.Brown: nrOfFreeRevivals = 0; break;
                    case Faction.White: nrOfFreeRevivals = 2; break;
                    case Faction.Pink: nrOfFreeRevivals = 2; break;
                    case Faction.Cyan: nrOfFreeRevivals = 2; break;
                }

                if (CurrentRecruitsPlayed != null)
                {
                    nrOfFreeRevivals *= 2;
                }

                if (player.Ally == Faction.Yellow && player.Ally == Faction.Yellow && YellowAllowsThreeFreeRevivals)
                {
                    nrOfFreeRevivals = 3;
                }

                if (CurrentYellowNexus != null && CurrentYellowNexus.Player == player)
                {
                    nrOfFreeRevivals = 3;
                }

                if (usesRedSecretAlly)
                {
                    nrOfFreeRevivals += 3;
                }

                if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(player))
                {
                    nrOfFreeRevivals += 1;
                }

                return nrOfFreeRevivals;
            }
        }

        private bool GetsExtraCharityAndFreeRevivalDueToLowThreshold(Player player) =>
            !(player.Is(Faction.Red) || player.Is(Faction.Brown)) && player.HasLowThreshold() ||
            player.Is(Faction.Red) && player.HasLowThreshold(World.Red) ||
            player.Is(Faction.Brown) && player.HasLowThreshold() && OccupierOf(World.Brown) == null;

        private void LogRevival(Revival r, Player initiator, RevivalCost cost, int purpleReceivedResources, bool asGhola, int highThresholdBonus)
        {
            int totalAmountOfForces = r.AmountOfForces + r.ExtraForcesPaidByRed + highThresholdBonus;

            Log(
            r.Initiator,
                " revive ",
                MessagePart.ExpressIf(r.Hero != null, r.Hero),
                MessagePart.ExpressIf(asGhola, " as Ghola"),
                MessagePart.ExpressIf(r.Hero != null && totalAmountOfForces + r.AmountOfSpecialForces + r.ExtraSpecialForcesPaidByRed > 0, " and "),
                MessagePart.ExpressIf(totalAmountOfForces > 0, totalAmountOfForces, initiator.Force),
                MessagePart.ExpressIf(r.AmountOfSpecialForces + r.ExtraSpecialForcesPaidByRed > 0, r.AmountOfSpecialForces + r.ExtraSpecialForcesPaidByRed, initiator.SpecialForce),
                " for ",
                Payment.Of(cost.TotalCostForPlayer + cost.CostForEmperor),
                MessagePart.ExpressIf(r.ExtraForcesPaidByRed > 0 || r.ExtraSpecialForcesPaidByRed > 0, " (", Payment.Of(cost.CostForEmperor, Faction.Red), ")"),
                MessagePart.ExpressIf(purpleReceivedResources > 0, " → ", Faction.Purple, " get ", Payment.Of(purpleReceivedResources)));
        }

        #endregion

        #region RevivalEvents

        public Faction[] FactionsWithIncreasedRevivalLimits { get; internal set; } = Array.Empty<Faction>();

        public void HandleEvent(SetIncreasedRevivalLimits e)
        {
            FactionsWithIncreasedRevivalLimits = e.Factions;
            Log(e);
        }

        public int GetRevivalLimit(Game g, Player p)
        {
            if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            {
                return 100;
            }
            else if (CurrentRecruitsPlayed != null)
            {
                return 7;
            }
            else if (FactionsWithIncreasedRevivalLimits.Contains(p.Faction))
            {
                return 5;
            }
            else
            {
                return 3;
            }
        }

        public void HandleEvent(RaiseDeadPlayed r)
        {
            Stone(Milestone.RaiseDead);
            Log(r);
            var player = GetPlayer(r.Initiator);
            Discard(player, TreacheryCardType.RaiseDead);

            var purple = GetPlayer(Faction.Purple);
            if (purple != null)
            {
                purple.Resources += 1;
                Log(Faction.Purple, " get ", Payment.Of(1), " for revival by ", TreacheryCardType.RaiseDead);
            }

            if (r.Hero != null)
            {
                if (r.Initiator != r.Hero.Faction && r.Hero is Leader)
                {
                    ReviveGhola(player, r.Hero as Leader);
                }
                else
                {
                    ReviveHero(r.Hero);
                }

                if (r.AssignSkill)
                {
                    PrepareSkillAssignmentToRevivedLeader(r.Player, r.Hero as Leader);
                }
            }
            else
            {
                player.ReviveForces(r.AmountOfForces);
                player.ReviveSpecialForces(r.AmountOfSpecialForces);

                if (r.AmountOfSpecialForces > 0)
                {
                    FactionsThatRevivedSpecialForcesThisTurn.Add(r.Initiator);
                }
            }

            if (r.Location != null && r.Initiator == Faction.Yellow)
            {
                player.ShipSpecialForces(r.Location, 1);
                Log(r.Initiator, " place ", FactionSpecialForce.Yellow, " in ", r.Location);
            }
        }

        public List<RequestPurpleRevival> CurrentRevivalRequests { get; private set; } = new();
        public void HandleEvent(RequestPurpleRevival e)
        {
            var existingRequest = CurrentRevivalRequests.FirstOrDefault(r => r.Hero == e.Hero);
            if (existingRequest != null)
            {
                CurrentRevivalRequests.Remove(existingRequest);
            }

            Log(e);
            CurrentRevivalRequests.Add(e);
        }

        public Dictionary<IHero, int> EarlyRevivalsOffers { get; private set; } = new();

        public bool IsAllowedEarlyRevival(IHero h) => EarlyRevivalsOffers.ContainsKey(h) && EarlyRevivalsOffers[h] < int.MaxValue;


        private bool FreeRevivalPrevented(Faction f)
        {
            return CurrentFreeRevivalPrevention != null && CurrentFreeRevivalPrevention.Target == f;
        }

        public BrownFreeRevivalPrevention CurrentFreeRevivalPrevention { get; internal set; } = null;

        public bool PreventedFromReviving(Faction f) => CurrentKarmaRevivalPrevention != null && CurrentKarmaRevivalPrevention.Target == f;

        internal KarmaRevivalPrevention CurrentKarmaRevivalPrevention = null;
        public void HandleEvent(KarmaRevivalPrevention e)
        {
            CurrentKarmaRevivalPrevention = e;
            Discard(e.Player, Karma.ValidKarmaCards(this, e.Player).FirstOrDefault());
            e.Player.SpecialKarmaPowerUsed = true;
            Log(e);
            Stone(Milestone.Karma);
        }

        public int AmbassadorsPlacedThisTurn { get; internal set; } = 0;

        public Ambassador AmbassadorIn(Territory t) => AmbassadorsOnPlanet.TryGetValue(t, out Ambassador value) ? value : Ambassador.None;

        #endregion

        #region EndOfRevival

        

        #endregion
    }
}
