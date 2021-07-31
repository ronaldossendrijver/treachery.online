/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public bool RevivalTechTokenIncome;

        private void EnterRevivalPhase()
        {
            MainPhaseStart(MainPhase.Resurrection);
            HasActedOrPassed.Clear();
            Enter(Phase.Resurrection);
            Allow(FactionAdvantage.BlackFreeCard);
            Allow(FactionAdvantage.RedReceiveBid);
            Allow(FactionAdvantage.GreyAllyDiscardingCard);
            RevivalTechTokenIncome = false;
            FactionsThatTookFreeRevival.Clear();
        }

        public List<Faction> FactionsThatRevivedSpecialForcesThisTurn = new List<Faction>();
        public void HandleEvent(Revival r)
        {
            var initiator = GetPlayer(r.Initiator);

            //Payment
            var cost = Revival.DetermineCost(this, initiator, r.Hero, r.AmountOfForces, r.AmountOfSpecialForces);
            if (cost.CostForEmperor > 0)
            {
                var emperor = GetPlayer(Faction.Red);
                emperor.Resources -= cost.CostForEmperor;
            }
            initiator.Resources -= cost.TotalCostForPlayer;

            //Force revival
            initiator.ReviveForces(r.AmountOfForces);
            initiator.ReviveSpecialForces(r.AmountOfSpecialForces);

            if (r.AmountOfSpecialForces > 0)
            {
                FactionsThatRevivedSpecialForcesThisTurn.Add(r.Initiator);
            }

            //Register free revival
            bool usesFreeRevival = false;
            if (r.AmountOfForces + r.AmountOfSpecialForces > 0 && FreeRevivals(initiator) > 0)
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
            int purpleReceivedResourcesForFreeRevival = 0;
            int purpleReceivedResourcesForPaidForceRevival = 0;
            int purpleReceivedResourcesForPaidHeroRevival = 0;

            if (purple != null)
            {
                if (usesFreeRevival)
                {
                    purpleReceivedResourcesForFreeRevival = 1;
                }

                if (r.Initiator != Faction.Purple)
                {
                    purpleReceivedResourcesForPaidForceRevival = cost.TotalCostForForceRevival;
                    purpleReceivedResourcesForPaidHeroRevival = cost.CostToReviveHero;
                }

                if (Prevented(FactionAdvantage.PurpleReceiveRevive))
                {
                    purpleReceivedResourcesForFreeRevival = 0;
                    purpleReceivedResourcesForPaidForceRevival = 0;
                    CurrentReport.Add("{0} prevents {1} receiving {2} for this revival.", TreacheryCardType.Karma, Faction.Purple, Concept.Resource);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.PurpleReceiveRevive);
                }

                int totalProfits = purpleReceivedResourcesForFreeRevival + purpleReceivedResourcesForPaidForceRevival + purpleReceivedResourcesForPaidHeroRevival;

                purple.Resources += totalProfits;

                if (totalProfits >= 5)
                {
                    ApplyBureaucracy(initiator.Faction, Faction.Purple);
                }

                if (cost.Total - totalProfits >= 4)
                {
                    ActivateBanker();
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
                else
                {
                    ReviveHero(r.Hero);
                }

                if (r.AssignSkill)
                {
                    PrepareSkillAssignmentToRevivedLeader(r.Player, r.Hero as Leader);
                }

                if (AllowedEarlyRevivals.ContainsKey(r.Hero))
                {
                    AllowedEarlyRevivals.Remove(r.Hero);
                }
            }

            //Logging
            RecentMilestones.Add(Milestone.Revival);
            LogRevival(r, initiator, cost, purpleReceivedResourcesForFreeRevival + purpleReceivedResourcesForPaidForceRevival + purpleReceivedResourcesForPaidHeroRevival, asGhola);

            if (r.Initiator != Faction.Purple)
            {
                HasActedOrPassed.Add(r.Initiator);
            }
        }

        private void PrepareSkillAssignmentToRevivedLeader(Player player, Leader leader)
        {
            if (CurrentPhase != Phase.AssigningSkill)
            {
                PhaseBeforeSkillAssignment = CurrentPhase;
            }

            player.MostRecentlyRevivedLeader = leader;
            SkillDeck.Shuffle();
            RecentMilestones.Add(Milestone.Shuffled);
            player.SkillsToChooseFrom.Add(SkillDeck.Draw());
            player.SkillsToChooseFrom.Add(SkillDeck.Draw());
            Enter(Phase.AssigningSkill);
        }

        private void ReviveHero(IHero h)
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

        public List<Faction> FactionsThatTookFreeRevival = new List<Faction>();
        public int FreeRevivals(Player player)
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

                if (player.Ally == Faction.Yellow && player.Ally == Faction.Yellow && YellowAllowsThreeFreeRevivals)
                {
                    nrOfFreeRevivals = 3;
                }

                return nrOfFreeRevivals;
            }
        }


        public Faction[] FactionsWithIncreasedRevivalLimits { get; set; } = new Faction[] { };

        public void HandleEvent(SetIncreasedRevivalLimits e)
        {
            FactionsWithIncreasedRevivalLimits = e.Factions;
            CurrentReport.Add(e);
        }

        public int GetRevivalLimit(Game g, Player p)
        {
            if (p.Is(Faction.Purple) || (p.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival)))
            {
                return 100;
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

        private void LogRevival(Revival r, Player initiator, RevivalCost cost, int purpleReceivedResources, bool asGhola)
        {
            if (cost.CostForEmperor > 0)
            {
                if (r.AmountOfSpecialForces > 0)
                {
                    CurrentReport.Add(r.Initiator, "{0} revive {1}{2} {3} and {4} {5} for {6}, of which {7} pay {8}.{9}",
                        r.Initiator,
                        HeroRevivalMessage(r.Hero, asGhola),
                        r.AmountOfForces,
                        initiator.Force,
                        r.AmountOfSpecialForces,
                        initiator.SpecialForce,
                        cost.TotalCostForPlayer + cost.CostForEmperor,
                        Faction.Red,
                        cost.CostForEmperor,
                        RevivalCostReceivedByPurple(purpleReceivedResources));
                }
                else
                {
                    CurrentReport.Add(r.Initiator, "{0} revive {1}{2} {3} for {4}, of which {5} pay {6}.{7}",
                        r.Initiator,
                        HeroRevivalMessage(r.Hero, asGhola),
                        r.AmountOfForces,
                        initiator.Force,
                        cost.TotalCostForPlayer + cost.CostForEmperor,
                        Faction.Red,
                        cost.CostForEmperor,
                        RevivalCostReceivedByPurple(purpleReceivedResources));
                }
            }
            else
            {
                if (r.AmountOfSpecialForces > 0)
                {
                    CurrentReport.Add(r.Initiator, "{0} revive {1}{2} {3} and {4} {5} for {6}.{7}",
                        r.Initiator,
                        HeroRevivalMessage(r.Hero, asGhola),
                        r.AmountOfForces,
                        initiator.Force,
                        r.AmountOfSpecialForces,
                        initiator.SpecialForce,
                        cost.TotalCostForPlayer,
                        RevivalCostReceivedByPurple(purpleReceivedResources));
                }
                else
                {
                    CurrentReport.Add(r.Initiator, "{0} revive {1}{2} {3} for {4}.{5}",
                        r.Initiator,
                        HeroRevivalMessage(r.Hero, asGhola),
                        r.AmountOfForces,
                        initiator.Force,
                        cost.TotalCostForPlayer,
                        RevivalCostReceivedByPurple(purpleReceivedResources));
                }
            }
        }

        private MessagePart HeroRevivalMessage(IHero hero, bool asGhola)
        {
            if (hero != null)
            {
                return new MessagePart("{0}{1} and ", hero, asGhola ? " as Ghola" : "");
            }
            else
            {
                return new MessagePart("");
            }
        }

        private MessagePart RevivalCostReceivedByPurple(int v)
        {
            if (v > 0)
            {
                return new MessagePart(" {0} receive {1}.", Faction.Purple, v);
            }
            else
            {
                return new MessagePart("");
            }
        }


        public void HandleEvent(RaiseDeadPlayed r)
        {
            RecentMilestones.Add(Milestone.RaiseDead);
            CurrentReport.Add(r.GetMessage());
            var player = GetPlayer(r.Initiator);
            Discard(player, TreacheryCardType.RaiseDead);

            var purple = GetPlayer(Faction.Purple);
            if (purple != null)
            {
                purple.Resources += 1;
                CurrentReport.Add(Faction.Purple, "{0} receive 1 for revival by {1}.", Faction.Purple, TreacheryCardType.RaiseDead);
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
        }

        public RequestPurpleRevival CurrentPurpleRevivalRequest = null;
        public void HandleEvent(RequestPurpleRevival e)
        {
            CurrentReport.Add(e);
            CurrentPurpleRevivalRequest = e;
        }

        public Dictionary<IHero, int> AllowedEarlyRevivals = new Dictionary<IHero, int>();
        public void HandleEvent(AcceptOrCancelPurpleRevival e)
        {
            CurrentReport.Add(e);

            if (AllowedEarlyRevivals.ContainsKey(e.Hero))
            {
                AllowedEarlyRevivals.Remove(e.Hero);
            }

            if (!e.Cancel)
            {
                AllowedEarlyRevivals.Add(e.Hero, e.Price);
            }

            CurrentPurpleRevivalRequest = null;
        }

        private bool FreeRevivalPrevented(Faction f)
        {
            return CurrentFreeRevivalPrevention != null && CurrentFreeRevivalPrevention.Target == f;
        }

        public BrownFreeRevivalPrevention CurrentFreeRevivalPrevention { get; set; } = null;
        public void HandleEvent(BrownFreeRevivalPrevention e)
        {
            CurrentReport.Add(e);
            Discard(e.CardUsed());
            CurrentFreeRevivalPrevention = e;
            RecentMilestones.Add(Milestone.SpecialUselessPlayed);
        }
    }
}
