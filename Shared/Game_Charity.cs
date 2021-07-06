/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public partial class Game
    {
        public bool ResourceTechTokenIncome;

        private void EnterCharityPhase()
        {
            CurrentMainPhase = MainPhase.Charity;
            CurrentReport = new Report(MainPhase.Charity);
            HasActedOrPassed.Clear();

            Monsters.Clear();
            ResourceTechTokenIncome = false;

            Allow(FactionAdvantage.YellowControlsMonster);
            Allow(FactionAdvantage.YellowProtectedFromMonster);

            var benegesserit = GetPlayer(Faction.Blue);
            if (benegesserit != null && Applicable(Rule.BlueAutoCharity))
            {
                if (!Prevented(FactionAdvantage.BlueCharity))
                {
                    HasActedOrPassed.Add(Faction.Blue);
                    benegesserit.Resources += 2;
                    CurrentReport.Add(Faction.Blue, "{0} claim 2 charity.", Faction.Blue);
                    RecentMilestones.Add(Milestone.CharityClaimed);
                }
                else
                {
                    CurrentReport.Add(Faction.Blue, "{0} are prevented from receiving 2 charity.", Faction.Blue);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueCharity);
                }
            }

            Enter(Phase.ClaimingCharity);
        }

        public void HandleEvent(CharityClaimed e)
        {
            HasActedOrPassed.Add(e.Initiator);

            int received = 2 - GetPlayer(e.Initiator).Resources;
            GetPlayer(e.Initiator).Resources = 2;

            CurrentReport.Add(e.Initiator, "{0} claim {1} charity.", e.Initiator, received);

            if (e.Initiator != Faction.Blue)
            {
                ResourceTechTokenIncome = true;
            }

            RecentMilestones.Add(Milestone.CharityClaimed);
        }
    }
}
