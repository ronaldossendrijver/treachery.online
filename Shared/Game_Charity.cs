/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Shared;

public partial class Game
{
    #region State

    internal bool ResourceTechTokenIncome { get; set; }
    public bool CharityIsCancelled => EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped;

    #endregion State

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
                return 2;
            if (EconomicsStatus == BrownEconomicsStatus.Cancel || EconomicsStatus == BrownEconomicsStatus.CancelFlipped)
                return 0;
            return 1;
        }
    }

    internal void GiveCharity(Player to, int basicAmount)
    {
        var homeworldBonus = 0;
        if (GetsExtraCharityAndFreeRevivalDueToLowThreshold(to)) homeworldBonus = 1;

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

    #endregion Charity
}