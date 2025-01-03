/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class ThoughtAnswered : GameEvent
{
    #region Construction

    public ThoughtAnswered(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public ThoughtAnswered()
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
        if (!ValidCards(Game, Player).Any()) return null;

        if (!ValidCards(Game, Player).Contains(Card)) return Message.Express("Select a valid card to show");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
    {
        if (p.Has(g.CurrentThought.Card))
            return new[] { g.CurrentThought.Card };
        return p.TreacheryCards;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (Card == null)
        {
            Log(Initiator, " don't own any cards");
        }
        else
        {
            LogTo(Game.CurrentThought.Initiator, "In response, ", Initiator, " show you: ", Card);
            Log("In response, ", Initiator, " show ", Game.CurrentThought.Initiator, " a card");
            Game.RegisterKnown(Game.CurrentThought.Initiator, Card);
        }

        Game.Enter(Phase.BattlePhase);
    }

    public override Message GetMessage()
    {
        if (Card == null)
            return Message.Express(Initiator, " don't have a card to show");
        return Message.Express(Initiator, " show one of their cards");
    }

    #endregion Execution
}