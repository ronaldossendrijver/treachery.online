/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class DiscardedTaken : GameEvent
{
    #region Construction

    public DiscardedTaken(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DiscardedTaken()
    {
    }

    #endregion Construction

    #region Properties

    public int _cardId;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Get(_cardId);
        set => _cardId = TreacheryCardManager.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidCards(Game, Player).Contains(Card)) return Message.Express("Invalid card");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
    {
        return g.RecentlyDiscarded.Where(kvp => kvp.Value != p.Faction && g.TreacheryDiscardPile.Items.Contains(kvp.Key)).Select(kvp => kvp.Key);
    }

    public static bool CanBePlayed(Game g, Player p)
    {
        return !g.CurrentPhaseCannotBeInterrupted && p.Has(TreacheryCardType.TakeDiscarded) && ValidCards(g, p).Any();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        Game.RecentlyDiscarded.Remove(Card);
        Game.TreacheryDiscardPile.Items.Remove(Card);
        Player.TreacheryCards.Add(Card);
        Game.Discard(Player, TreacheryCardType.TakeDiscarded);
        Game.Stone(Milestone.CardWonSwapped);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use ", TreacheryCardType.TakeDiscarded, " to acquire the discarded ", Card);
    }

    #endregion Execution
}