namespace Treachery.Client.GameEventComponents
{
    public abstract partial class PlacementComponent<PlacementEventType> where PlacementEventType : PlacementEvent, new()
    {
        protected abstract bool InformAboutCaravan { get; }

        protected abstract string Title { get; }

        protected abstract bool MayPass { get; }
    }
}
