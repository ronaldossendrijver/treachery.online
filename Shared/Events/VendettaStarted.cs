namespace Treachery.Shared;

public class VendettaStarted : GameEvent
{
    public VendettaStarted() { }
    public VendettaStarted(Game game, Faction initiator) : base(game, initiator) { }
    public int PowerCardId { get; set; } = -1;

    public override Message? Validate()
    {
        if (!Game.Applicable(Rule.SE_HarkonnenVendetta) || Initiator != Faction.Black || Player.SpecialKarmaPowerUsed ||
            Game.CurrentPhase != Phase.BiddingReport || !Karma.ValidPowerCards(Game, Player).Any())
            return Message.Express("Vendetta is only available at the end of bidding, once per game");
        if (!Karma.IsValidPowerCard(Game, Player, PowerCardId)) return Message.Express("Select a card to invoke Vendetta");
        return null;
    }

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Player, Karma.PowerCard(Game, Player, PowerCardId));
        Player.SpecialKarmaPowerUsed = true;
        Game.VendettaRecipients.Clear();
        foreach (var other in Game.Players.Where(p => p != Player && p.TreacheryCards.Count > 0))
        {
            Game.VendettaRecipients.Add(other.Faction);
            var card = other.TreacheryCards[Game.Random!.Next(other.TreacheryCards.Count)];
            Game.UnregisterKnown(other, [card]);
            other.TreacheryCards.Remove(card);
            Player.TreacheryCards.Add(card);
            Game.RegisterKnown(Player, card);
        }
        var extra = Game.DrawTreacheryCard() ?? throw new InvalidOperationException("Vendetta requires a card from the normal deck");
        Player.TreacheryCards.Add(extra);
        Game.RegisterKnown(Player, extra);
        Log();
        Game.Enter(Phase.VendettaReturningCards, false);
        Game.FinishVendettaIfComplete();
    }

    public override Message GetMessage() => Message.Express(Initiator, " invoke Vendetta");
}
