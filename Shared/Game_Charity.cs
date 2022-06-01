/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public bool ResourceTechTokenIncome { get; private set; }

        private void EnterCharityPhase()
        {
            MainPhaseStart(MainPhase.Charity);
            HasActedOrPassed.Clear();
            Monsters.Clear();
            ResourceTechTokenIncome = false;
            Allow(FactionAdvantage.YellowControlsMonster);
            Allow(FactionAdvantage.YellowProtectedFromMonster);

            if (Version < 122)
            {
                StartClaimingCharity();
            }
            else
            {
                Enter(Phase.BeginningOfCharity);
            }
        }

        private void StartClaimingCharity()
        {
            if (!Prevented(FactionAdvantage.BrownControllingCharity))
            {
                var brown = GetPlayer(Faction.Brown);
                if (brown != null)
                {
                    int toCollect = Players.Count * 2 * CurrentCharityMultiplier;
                    brown.Resources += toCollect;
                    Log(Faction.Brown, " collect ", Payment(toCollect));
                }
            }
            else
            {
                LogPrevention(FactionAdvantage.BrownControllingCharity);
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
                    LogPrevention(FactionAdvantage.BlueCharity);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueCharity);
                }
            }

            MainPhaseMiddle();
            Enter(Phase.ClaimingCharity);
        }

        private void EndCharityPhase()
        {
            ReceiveResourceTechIncome();

            if (Version < 122)
            {
                EnterBiddingPhase();
            }
            else
            {
                if (Version >= 132) MainPhaseEnd();
                Enter(Phase.CharityReport);
            }
        }

        private void ReceiveResourceTechIncome()
        {
            if (ResourceTechTokenIncome)
            {
                var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(TechToken.Resources));
                if (techTokenOwner != null)
                {
                    var amount = techTokenOwner.TechTokens.Count;
                    techTokenOwner.Resources += amount;
                    Log(techTokenOwner.Faction, " receive ", Payment(amount), " from ", TechToken.Resources);
                }
            }
        }

        private int CurrentCharityMultiplier
        {
            get
            {
                if (EconomicsStatus == BrownEconomicsStatus.Double || EconomicsStatus == BrownEconomicsStatus.DoubleFlipped)
                {
                    return 2;
                }
                else if (EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        private void GiveCharity(Player to, int amount)
        {
            var brown = GetPlayer(Faction.Brown);

            to.Resources += amount;
            if (brown != null && !Prevented(FactionAdvantage.BrownControllingCharity))
            {
                if (brown.Resources >= amount)
                {
                    brown.Resources -= amount;
                    Log(to.Faction, " claim ", Payment(amount), " from ", Faction.Brown);
                }
                else
                {
                    Log(to.Faction, " are unable to claim ", Payment(amount), " from ", Faction.Brown);
                }
            }
            else
            {
                Log(to.Faction, " claim ", Payment(amount));
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
