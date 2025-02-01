/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class CardsDetermined : GameEvent
{
    #region Construction

    public CardsDetermined(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public CardsDetermined()
    {
    }

    #endregion Construction

    #region Properties

    public string _treacheryCardIds;

    [JsonIgnore]
    public IEnumerable<TreacheryCard> TreacheryCards
    {
        get => IdStringToObjects(_treacheryCardIds, TreacheryCardManager.Lookup);
        set => _treacheryCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
    }

    public string _whiteCardIds;

    [JsonIgnore]
    public IEnumerable<TreacheryCard> WhiteCards
    {
        get => IdStringToObjects(_whiteCardIds, TreacheryCardManager.Lookup);
        set => _whiteCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (TreacheryCards.Count() + WhiteCards.Count() < Game.Players.Sum(p => p.MaximumNumberOfCards) + 1) return Message.Express("Not enough cards selected");

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.TreacheryDeck = new Deck<TreacheryCard>(TreacheryCards, Game.Random);
        Game.TreacheryDeck.Shuffle();
        Game.Stone(Milestone.Shuffled);
        Game.WhiteCache = [..WhiteCards];
        Log(GetVerboseMessage());
        Game.Enter(Game.Version < 134, Game.EnterPhaseTradingFactions, Game.EnterSetupPhase);
    }

    public override Message GetMessage()
    {
        return Message.Express("Card decks were customized.");
    }

    private Message GetVerboseMessage()
    {
        if (WhiteCards.Any())
            return Message.Express("Treachery Cards: ", TreacheryCards, ". ", Faction.White, " Cards: ", WhiteCards);
        return Message.Express("Treachery Cards: ", TreacheryCards);
    }

    #endregion Execution
}