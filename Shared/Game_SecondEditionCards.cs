namespace Treachery.Shared;

public partial class Game
{
    public IEnumerable<TreacheryCard> ValidSecondEditionKaramaPowerCards(Player player) => Karma.ValidPowerCards(this, player);

    public List<Faction> VendettaRecipients { get; } = [];

    internal void FinishVendettaIfComplete()
    {
        var black = GetPlayer(Faction.Black) ?? throw new InvalidEventException();
        if (VendettaRecipients.Count == 0 && black.TreacheryCards.Count <= 8)
            Enter(Phase.BiddingReport, false);
    }
}
