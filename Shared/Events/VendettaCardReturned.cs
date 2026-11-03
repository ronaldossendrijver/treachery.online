namespace Treachery.Shared;

public class VendettaCardReturned : GameEvent
{
    public VendettaCardReturned() { }
    public VendettaCardReturned(Game game, Faction initiator) : base(game, initiator) { }
    public int CardId { get; set; } = -1;
    [JsonIgnore]
    public TreacheryCard? Card => TreacheryCardManager.Lookup.Find(CardId);

    public override Message? Validate() =>
        Game.CurrentPhase != Phase.VendettaReturningCards || Initiator != Faction.Black || Card == null || !Player.Has(Card)
            ? Message.Express("Select a card from your hand to return or discard") : null;

    protected override void ExecuteConcreteEvent()
    {
        if (Game.VendettaRecipients.Count > 0)
        {
            var target = Game.GetPlayer(Game.VendettaRecipients[0]) ?? throw new InvalidEventException();
            Game.VendettaRecipients.RemoveAt(0);
            Game.UnregisterKnown(Player, [Card!]);
            Player.TreacheryCards.Remove(Card!);
            target.TreacheryCards.Add(Card!);
            Game.RegisterKnown(Player, Card!);
            Game.RegisterKnown(target, Card!);
        }
        else Game.Discard(Player, Card);
        Log();
        Game.FinishVendettaIfComplete();
    }

    public override Message GetMessage() => Message.Express(Initiator, " distribute a Vendetta card");
}
