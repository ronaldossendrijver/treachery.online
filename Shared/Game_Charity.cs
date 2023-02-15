/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;
using System.Numerics;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        private bool ResourceTechTokenIncome { get; set; }

        #endregion

        #region BeginningOfCharity

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

        #endregion

        #region Charity

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
                LogPreventionByKarma(FactionAdvantage.BrownControllingCharity);
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
                    LogPreventionByKarma(FactionAdvantage.BlueCharity);
                    if (!Applicable(Rule.FullPhaseKarma)) Allow(FactionAdvantage.BlueCharity);
                }
            }

            MainPhaseMiddle();
            Enter(Phase.ClaimingCharity);
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

        private void GiveCharity(Player to, int basicAmount)
        {
            int homeworldBonus = 0;
            if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(to))
            {
                homeworldBonus = 1;
            }
            
            var brown = GetPlayer(Faction.Brown);

            to.Resources += basicAmount + homeworldBonus;
            if (brown != null && !Prevented(FactionAdvantage.BrownControllingCharity))
            {
                if (brown.Resources >= basicAmount)
                {
                    brown.Resources -= basicAmount;
                    Log(to.Faction, " get ", Payment(basicAmount), " from ", Faction.Brown, MessagePart.ExpressIf(homeworldBonus > 0, " and ", Payment(homeworldBonus), " from the bank"));
                }
                else
                {
                    Log(to.Faction, " are unable to claim ", Payment(basicAmount), " from ", Faction.Brown, MessagePart.ExpressIf(homeworldBonus > 0, " but get ", Payment(homeworldBonus), " from the bank"));
                }
            }
            else
            {
                Log(to.Faction, " claim ", Payment(basicAmount + homeworldBonus));
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

        #endregion

        #region EndOfCharity

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

        #endregion
    }
}
