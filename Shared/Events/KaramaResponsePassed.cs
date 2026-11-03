namespace Treachery.Shared;

public class KaramaResponsePassed : GameEvent
{
    public KaramaResponsePassed() { }
    public KaramaResponsePassed(Game game, Faction initiator) : base(game, initiator) { }

    public override Message? Validate() =>
        Game.CurrentPhase != Phase.KaramaResponse || !Game.KaramaResponders.Contains(Initiator)
            ? Message.Express("You cannot respond to Karama now") : null;

    protected override void ExecuteConcreteEvent()
    {
        Game.KaramaResponders.Remove(Initiator);
        Log();
        if (Game.KaramaResponders.Count == 0) Game.FinishSecondEditionKaramaResponse();
    }

    public override Message GetMessage() => Message.Express(Initiator, " decline to cancel the named ability");
}
