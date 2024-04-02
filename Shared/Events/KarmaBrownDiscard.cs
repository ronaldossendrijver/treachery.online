/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class KarmaBrownDiscard : GameEvent
{
    #region Construction

    public KarmaBrownDiscard(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaBrownDiscard()
    {
    }

    #endregion Construction

    #region Properties

    public string _cardIds;

    [JsonIgnore]
    public IEnumerable<TreacheryCard> Cards
    {
        get => IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
        set => _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        var karmaCardToUse = Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
        if (karmaCardToUse == null) return Message.Express("You don't have a ", TreacheryCardType.Karma, " card");
        if (Cards.Contains(karmaCardToUse)) return Message.Express("You can't select the ", TreacheryCardType.Karma, " you need to play to use this power");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Player p)
    {
        var karmaCardToUse = p.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
        return p.TreacheryCards.Where(c => c != karmaCardToUse);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
        Log();

        foreach (var card in Cards) Game.Discard(Player, card);

        Player.Resources += Cards.Count() * 3;
        Player.SpecialKarmaPowerUsed = true;
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, "Using ", TreacheryCardType.Karma, ", ", Initiator, " discard ", Cards.Select(c => MessagePart.Express(" ", c, " ")), "to get ", Payment.Of(Cards.Count() * 3));
    }

    #endregion Execution
}