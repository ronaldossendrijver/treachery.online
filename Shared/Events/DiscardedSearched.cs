/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class DiscardedSearched : GameEvent
{
    #region Construction

    public DiscardedSearched(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DiscardedSearched()
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
        if (!ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Game g)
    {
        return g.TreacheryDiscardPile.Items;
    }

    public static bool CanBePlayed(Player p)
    {
        return p.Has(TreacheryCardType.SearchDiscarded);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();

        foreach (var p in Game.Players) Game.UnregisterKnown(p, Game.TreacheryDiscardPile.Items);

        Game.TreacheryDiscardPile.Items.Remove(Card);
        Player.TreacheryCards.Add(Card);
        Game.TreacheryDiscardPile.Shuffle();
        Game.Discard(Player, TreacheryCardType.SearchDiscarded);
        Game.Enter(Game.PhaseBeforeSearchingDiscarded);
        Game.Stone(Milestone.Shuffled);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " take one card and the Treachery Discard Pile is then shuffled");
    }

    #endregion Execution
}