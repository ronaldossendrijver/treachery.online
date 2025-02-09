namespace Treachery.Client.OtherComponents;

public abstract partial class BiddingPanel<TBid> where TBid : GameEvent, new()
{
    protected abstract string AuctionDescription { get; }

    protected abstract bool ShowRepeatAutoPass { get; }

    protected abstract bool CanUseKarma { get; }

    protected abstract bool CanUseRedSecretAlly { get; }

    protected abstract int MaxAllyBidAmount { get; }

    protected abstract int MaxBidAmount { get; }
}