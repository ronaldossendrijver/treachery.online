/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class WhiteGaveCard : GameEvent
{
    #region Construction

    public WhiteGaveCard(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public WhiteGaveCard()
    {
    }

    #endregion Construction

    #region Properties

    public int _cardId = -1;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Lookup.Find(_cardId);
        set => _cardId = TreacheryCardManager.Lookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Player p)
    {
        return p.TreacheryCards.Where(c => c.Rules.Contains(Rule.WhiteTreacheryCards));
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var target = Player.AlliedPlayer;

        Player.TreacheryCards.Remove(Card);
        Game.RegisterKnown(Player, Card);
        target.TreacheryCards.Add(Card);

        foreach (var p in Game.Players.Where(p => p != Player && p != target))
        {
            Game.UnregisterKnown(p, Player.TreacheryCards);
            Game.UnregisterKnown(p, target.TreacheryCards);
        }

        Log();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " give a card to their ally");
    }

    #endregion Execution
}