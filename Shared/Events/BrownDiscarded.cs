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

public class BrownDiscarded : GameEvent
{
    #region Construction

    public BrownDiscarded(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BrownDiscarded()
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
        if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Player p)
    {
        return p.TreacheryCards.Where(c =>
            (c.Type == TreacheryCardType.Useless && !p.HasHighThreshold()) ||
            (c.Type != TreacheryCardType.Projectile && c.Type != TreacheryCardType.Poison && p.TreacheryCards.Count(toCount => toCount.Type == c.Type) > 1));
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Card);
        Log();

        if (Card.Type == TreacheryCardType.Useless)
            Player.Resources += 2;
        else
            Player.Resources += 3;

        Game.Stone(Milestone.ResourcesReceived);
    }

    public override Message GetMessage()
    {
        if (Card.Type == TreacheryCardType.Useless)
            return Message.Express(Initiator, " get ", Payment.Of(2), " by discarding a ", TreacheryCardType.Useless, " card");
        return Message.Express(Initiator, " get ", Payment.Of(3), " by discarding a duplicate ", Card);
    }

    #endregion Execution
}