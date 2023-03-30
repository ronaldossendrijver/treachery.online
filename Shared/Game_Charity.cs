/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        #region State

        internal bool ResourceTechTokenIncome { get; set; }

        #endregion

        #region BeginningOfCharity

        

        #endregion

        #region Charity

        

        public void ReceiveTechIncome(TechToken token)
        {
            var techTokenOwner = Players.FirstOrDefault(p => p.TechTokens.Contains(token));
            if (techTokenOwner != null)
            {
                var amount = techTokenOwner.TechTokens.Count;
                techTokenOwner.Resources += amount;
                Log(techTokenOwner.Faction, " receive ", Payment.Of(amount), " from ", token);
            }
        }

        internal int CurrentCharityMultiplier
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

        internal void GiveCharity(Player to, int basicAmount)
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
                    Log(to.Faction, " get ", Payment.Of(basicAmount), " from ", Faction.Brown, MessagePart.ExpressIf(homeworldBonus > 0, " and ", Payment.Of(homeworldBonus), " from the bank"));
                }
                else
                {
                    Log(to.Faction, " are unable to claim ", Payment.Of(basicAmount), " from ", Faction.Brown, MessagePart.ExpressIf(homeworldBonus > 0, " but get ", Payment.Of(homeworldBonus), " from the bank"));
                }
            }
            else
            {
                Log(to.Faction, " claim ", Payment.Of(basicAmount + homeworldBonus));
            }
        }

        

        #endregion

        #region EndOfCharity

        

        #endregion
    }
}
