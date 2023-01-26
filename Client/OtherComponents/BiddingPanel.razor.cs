using Treachery.Shared;

namespace Treachery.Client.OtherComponents
{
    public abstract partial class BiddingPanel<BidType> where BidType : GameEvent, new()
    {
        protected abstract string AuctionDescription { get; }

        protected abstract bool ShowRepeatAutoPass { get; }

        protected abstract bool CanUseKarma { get; }

        protected abstract bool CanUseRedCunning { get; }

        protected abstract int MaxAllyBidAmount { get; }

        protected abstract int MaxBidAmount { get; }
    }
}
