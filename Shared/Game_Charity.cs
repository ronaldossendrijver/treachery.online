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
            MainPhaseStart(MainPhase.Charity);
            HasActedOrPassed.Clear();

            Monsters.Clear();
            ResourceTechTokenIncome = false;

            Allow(FactionAdvantage.YellowControlsMonster);
            Allow(FactionAdvantage.YellowProtectedFromMonster);

            if (!Prevented(FactionAdvantage.BrownControllingCharity))
            {
                var brown = GetPlayer(Faction.Brown);
                if (brown != null)
                {
                    int toCollect = Players.Count * 2 * CurrentCharityMultiplier;
                    CurrentReport.Add(Faction.Brown, "{0} collect {1} {2}.", Faction.Brown, toCollect, Concept.Resource);
                }
            }
            else
            {
                CurrentReport.Add(Faction.Brown, "{0} prevents {1}", Faction.Brown, FactionAdvantage.BrownControllingCharity);
            }

            var blue = GetPlayer(Faction.Blue);
            if (blue != null && Applicable(Rule.BlueAutoCharity))
            {
                if (!Prevented(FactionAdvantage.BlueCharity))
                {
                    HasActedOrPassed.Add(Faction.Blue);
                    GiveCharity(blue, 2 * CurrentCharityMultiplier);
                    RecentMilestones.Add(Milestone.CharityClaimed);
                }
                else
                {
                    CurrentReport.Add(Faction.Blue, "{0} are prevented from receiving charity.", Faction.Blue);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueCharity);
                }
            }

            Enter(Phase.ClaimingCharity);
        }

        private int CurrentCharityMultiplier => EconomicsStatus == BrownEconomicsStatus.Double || EconomicsStatus == BrownEconomicsStatus.DoubleFlipped ? 2 : 1;

        private void GiveCharity(Player to, int amount)
        {
            var brown = GetPlayer(Faction.Brown);

            to.Resources += amount;
            if (brown != null && !Prevented(FactionAdvantage.BrownControllingCharity))
            {
                if (brown.Resources >= amount)
                {
                    brown.Resources -= amount;
                    CurrentReport.Add(to.Faction, "{0} receive {1} charity from {2}.", to.Faction, amount, Faction.Brown);
                }
                else
                {
                    CurrentReport.Add(to.Faction, "{0} are unable to give {1} {2}.", Faction.Brown, amount, Concept.Resource);
                }
            }
            else
            {
                CurrentReport.Add(to.Faction, "{0} claim {1} charity.", to.Faction, amount);
            }
        }

        public void HandleEvent(CharityClaimed e)
        {
            HasActedOrPassed.Add(e.Initiator);

            GiveCharity(e.Player, (2 - e.Player.Resources) * CurrentCharityMultiplier);

            if (e.Initiator != Faction.Blue)
            {
                ResourceTechTokenIncome = true;
            }

            RecentMilestones.Add(Milestone.CharityClaimed);
        }
    }
}
