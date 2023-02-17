/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class RevivalCost
    {
        public int TotalCostForPlayer;
        public int CostForForceRevivalForPlayer;
        public int CostForEmperor;
        public int CostToReviveHero;
        public bool CanBePaid;
        public bool IncludesCostsForSpecialForces;
        public int NumberOfForcesRevivedForFree;

        public RevivalCost(Game g, Player initiator, IHero hero, int amountOfForces, int amountOfSpecialForces, int extraForcesPaidByRed, int extraSpecialForcesPaidByRed, bool usesRedSecretAlly)
        {
            if (g.Version >= 124)
            {
                CostForEmperor = initiator.Ally == Faction.Red ? Revival.DetermineCostOfForcesForRed(g, initiator.AlliedPlayer, initiator.Faction, extraForcesPaidByRed, extraSpecialForcesPaidByRed) : 0;
                CostForForceRevivalForPlayer = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces, usesRedSecretAlly, out int nrOfPaidSpecialForces, out int numberOfForcesRevivedForFree);
                IncludesCostsForSpecialForces = nrOfPaidSpecialForces > 0;
                NumberOfForcesRevivedForFree = numberOfForcesRevivedForFree;
            }
            else
            {
                int costForForceRevival = GetPriceOfForceRevival(g, initiator, amountOfForces, amountOfSpecialForces, usesRedSecretAlly, out int nrOfPaidSpecialForces, out int numberOfForcesRevivedForFree);
                var amountPaidForByEmperor = Revival.ValidMaxRevivalsByRed(g, initiator);
                var emperor = g.GetPlayer(Faction.Red);
                var emperorsSpice = emperor != null ? emperor.Resources : 0;

                CostForEmperor = DetermineCostForEmperor(g, initiator.Faction, costForForceRevival, amountOfForces, amountOfSpecialForces, emperorsSpice, amountPaidForByEmperor);
                CostForForceRevivalForPlayer = costForForceRevival - CostForEmperor;

                IncludesCostsForSpecialForces = nrOfPaidSpecialForces > 0;
                NumberOfForcesRevivedForFree = numberOfForcesRevivedForFree;
            }

            CostToReviveHero = Revival.GetPriceOfHeroRevival(g, initiator, hero);
            TotalCostForPlayer = CostForForceRevivalForPlayer + CostToReviveHero;
            CanBePaid = initiator.Resources >= TotalCostForPlayer;
        }

        public static int GetPriceOfForceRevival(Game g, Player initiator, int amountOfForces, int amountOfSpecialForces, bool usesRedSecretAlly, out int nrOfPaidSpecialForces, out int numberOfForcesRevivedForFree)
        {
            int nrOfFreeRevivals = g.FreeRevivals(initiator, usesRedSecretAlly);
            nrOfPaidSpecialForces = initiator.Is(Faction.Red) && initiator.HasLowThreshold(World.RedStar) ? amountOfSpecialForces : Math.Max(0, amountOfSpecialForces - nrOfFreeRevivals);

            int nrOfFreeRevivalsLeft = nrOfFreeRevivals - (amountOfSpecialForces - nrOfPaidSpecialForces);
            int nrOfPaidNormalForces = Math.Max(0, amountOfForces - nrOfFreeRevivalsLeft);
            numberOfForcesRevivedForFree = amountOfForces + amountOfSpecialForces - nrOfPaidSpecialForces - nrOfPaidNormalForces;

            int priceOfSpecialForces = initiator.Is(Faction.Grey) ? 3 : 2;
            int priceOfNormalForces = initiator.Is(Faction.Brown) && !g.Prevented(FactionAdvantage.BrownRevival) ? 1 : 2;
            var cost = nrOfPaidSpecialForces * priceOfSpecialForces + nrOfPaidNormalForces * priceOfNormalForces;

            if (Revival.MayReviveWithDiscount(g, initiator))
            {
                cost = (int)Math.Ceiling(0.5 * cost);
            }

            return cost;
        }


        public int Total => TotalCostForPlayer + CostForEmperor;

        public int TotalCostForForceRevival => CostForForceRevivalForPlayer + CostForEmperor;

        public static int DetermineCostForEmperor(Game g, Faction initiator, int totalCostForForceRevival, int amountOfForces, int amountOfSpecialForces, int emperorsSpice, int amountPaidForByEmperor)
        {
            int priceOfSpecialForces = initiator == Faction.Grey ? 3 : 2;
            int priceOfNormalForces = initiator == Faction.Brown && !g.Prevented(FactionAdvantage.BrownRevival) && g.Version >= 122 ? 1 : 2;

            int specialForcesPaidByEmperor = 0;
            while (
                (specialForcesPaidByEmperor + 1) <= amountOfSpecialForces &&
                (specialForcesPaidByEmperor + 1) * priceOfSpecialForces <= emperorsSpice &&
                specialForcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            {
                specialForcesPaidByEmperor++;
            }

            int forcesPaidByEmperor = 0;
            while (
                (forcesPaidByEmperor + 1) <= amountOfForces &&
                specialForcesPaidByEmperor * priceOfSpecialForces + (forcesPaidByEmperor + 1) * priceOfNormalForces <= emperorsSpice &&
                specialForcesPaidByEmperor + forcesPaidByEmperor + 1 <= amountPaidForByEmperor)
            {
                forcesPaidByEmperor++;
            }

            int costForEmperor = specialForcesPaidByEmperor * priceOfSpecialForces + forcesPaidByEmperor * priceOfNormalForces;
            return Math.Min(totalCostForForceRevival, Math.Min(costForEmperor, emperorsSpice));
        }
    }
}
