/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface IBid
    {
        public int TotalAmount { get; }
        public int Amount { get; }
        public int AllyContributionAmount { get; }
        public int RedContributionAmount { get; }
        public Faction Initiator { get; }
        public Player Player { get; }
        public bool Passed { get; }

        public Message GetMessage();
    }
}
