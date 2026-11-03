namespace Treachery.Shared;

public class HarkonnenTraitorsRedrawn : PassableGameEvent
{
    public HarkonnenTraitorsRedrawn() { }
    public HarkonnenTraitorsRedrawn(Game game, Faction initiator) : base(game, initiator) { }

    public List<int> LeaderIds { get; set; } = [];

    public static IEnumerable<IHero> ValidLeaders(Player player)
        => player.Traitors.Where(l => l.Faction == player.Faction);

    public static int MaximumRedraw(Game game, Player player)
        => Math.Min(ValidLeaders(player).Count(), game.TraitorDeck!.Items.Count);

    public override Message? Validate()
    {
        if (Initiator != Faction.Black) return Message.Express("Only Harkonnen may redraw traitors");
        if (Passed && LeaderIds.Count != 0) return Message.Express("Do not select traitors when passing");
        if (!Passed && (LeaderIds.Count == 0 || LeaderIds.Count > MaximumRedraw(Game, Player)))
            return Message.Express("Select between 1 and ", MaximumRedraw(Game, Player), " of your own leaders");
        if (LeaderIds.Distinct().Count() != LeaderIds.Count ||
            LeaderIds.Any(id => !ValidLeaders(Player).Any(l => LeaderManager.HeroLookup.GetId(l) == id)))
            return Message.Express("Only your own traitor cards may be redrawn");
        return null;
    }

    protected override void ExecuteConcreteEvent()
    {
        var returned = Player.Traitors.Where(l => LeaderIds.Contains(LeaderManager.HeroLookup.GetId(l))).ToList();
        foreach (var hero in returned)
        {
            Player.Traitors.Remove(hero);
            Log(Initiator, " reveal and set aside ", hero);
        }
        foreach (var hero in returned)
        {
            var replacement = Game.TraitorDeck!.Draw();
            Player.Traitors.Add(replacement);
            LogTo(Initiator, "Your replacement traitor is ", replacement);
            foreach (var player in Game.Players) player.KnownNonTraitors.Add(hero);
        }
        Game.TraitorDeck!.Items.AddRange(returned);
        if (returned.Count > 0)
        {
            Game.TraitorDeck.Shuffle();
            Game.Stone(Milestone.Shuffled);
        }
        else Log(Initiator, " keep their initial traitors");
        Game.EnterSelectTraitors();
    }
}
