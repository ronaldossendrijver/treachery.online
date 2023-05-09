/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RevivalCost
    {
        public int CostForForceRevivalForPlayer { get; set; }
        public int CostForEmperor { get; set; }
        public int CostToReviveHero { get; set; }
        public bool CanBePaid { get; set; }
        public bool IncludesCostsForSpecialForces { get; set; }
        public int NumberOfForcesRevivedForFree { get; set; }

        public int TotalCost => TotalCostForPlayer + CostForEmperor;
        public int TotalCostForPlayer => CostForForceRevivalForPlayer + CostToReviveHero;
        public int TotalCostForForceRevival => CostForForceRevivalForPlayer + CostForEmperor;
    }
}
