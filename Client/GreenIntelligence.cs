/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Client;

public class GreenIntelligence
{
    public string TrackedSpiceCard { get; set; }

    public Dictionary<Faction, Dictionary<int, int>> TrackedTreacheryCards { get; }

    private readonly Dictionary<Tuple<Faction, int>, int> _trackedTraitors = new();
    private readonly Dictionary<int, int> _trackedDiscardedTraitors = new();
    private readonly List<int> _discardedCards = [];
    private readonly List<int> _removedCards = [];
    private readonly TreacheryCard[] _cardsInPlay;
    private readonly IGameService _client;

    public GreenIntelligence(IGameService client)
    {
        _client = client;
        var whiteCards = client.Game.IsPlaying(Faction.White) ? TreacheryCardManager.GetWhiteCards().ToArray() : [];
        _cardsInPlay = TreacheryCardManager.GetCardsInPlay(client.Game).Union(whiteCards).ToArray();
        TrackedTreacheryCards = new Dictionary<Faction, Dictionary<int, int>>();
        foreach (var p in client.Game.Players)
        {
            var cardsOfPlayer = new Dictionary<int, int>();
            for (var i = 0; i < p.MaximumNumberOfCards; i++) cardsOfPlayer.Add(i, DefaultSelectedCard(p.Faction, i));
            TrackedTreacheryCards.Add(p.Faction, cardsOfPlayer);
        }
    }

    private static int DefaultSelectedCard(Faction p, int cardNr)
    {
        if (cardNr == 0) return TreacheryCard.Unknown;
        if (cardNr == 1 && p == Faction.Black) return TreacheryCard.Unknown;
        return TreacheryCard.None;
    }

    public IEnumerable<TreacheryCard> DiscardedCards => _discardedCards.Select(id => TreacheryCardManager.Lookup.Find(id));

    public IEnumerable<TreacheryCard> RemovedCards => _removedCards.Select(id => TreacheryCardManager.Lookup.Find(id));

    public IEnumerable<TreacheryCard> AvailableDistinctCards(Faction f, int cardNumber)
    {
        var result = new List<TreacheryCard>();

        var cardsSelectedElsewhere = AllTrackedCardsExcept(f, cardNumber);

        foreach (var c in _cardsInPlay)
            if (result.All(added => _client.CurrentSkin.Describe(added) != _client.CurrentSkin.Describe(c)) && !_removedCards.Contains(c.Id) && !_discardedCards.Contains(c.Id) && !cardsSelectedElsewhere.Contains(c.Id)) result.Add(c);

        return result;
    }

    private List<int> AllTrackedCardsExcept(Faction f, int cardNumber)
    {
        var result = new List<int>();
        foreach (var kvp in TrackedTreacheryCards)
        {
            var cardsOfPlayer = kvp.Value;
            result.AddRange(kvp.Key != f
                ? cardsOfPlayer.Values
                : cardsOfPlayer.Where(c => c.Key != cardNumber).Select(c => c.Value));
        }
        return result;
    }

    public int GetSelectedTraitor(Faction f, int nr)
    {
        var key = new Tuple<Faction, int>(f, nr);
        return _trackedTraitors.GetValueOrDefault(key, -1);
    }

    public void ChangeSelectedTraitor(Faction f, int cardNumber, int? leaderId)
    {
        var key = new Tuple<Faction, int>(f, cardNumber);
        if (_trackedTraitors.ContainsKey(key)) _trackedTraitors.Remove(key);

        if (leaderId != null) _trackedTraitors.Add(key, (int)leaderId);
    }

    public int GetDiscardedTraitor(int nr)
    {
        return _trackedDiscardedTraitors.GetValueOrDefault(nr, -1);
    }

    public void ChangeDiscardedTraitor(int nr, int? leaderId)
    {
        if (_trackedDiscardedTraitors.ContainsKey(nr)) _trackedDiscardedTraitors.Remove(nr);

        if (leaderId != null) _trackedDiscardedTraitors.Add(nr, (int)leaderId);
    }

    public void Discard(Faction f, int cardNumber)
    {
        var current = TreacheryCardManager.Lookup.Find(TrackedTreacheryCards[f][cardNumber]);
        TrackedTreacheryCards[f][cardNumber] = TreacheryCard.None;

        if (current.Type == TreacheryCardType.Metheor)
            _removedCards.Add(current.Id);
        else
            _discardedCards.Add(current.Id);
    }

    public void SetNotDiscarded(TreacheryCard card)
    {
        if (card.Type == TreacheryCardType.Metheor)
            _removedCards.Remove(card.Id);
        else
            _discardedCards.Remove(card.Id);
    }

    public void ClearDiscarded()
    {
        _discardedCards.Clear();
    }

    private static void Write(ref string target, object item)
    {
        if (item == null)
            target += ';';
        else
            target = target + item + ';';
    }

    public override string ToString()
    {
        var result = "";

        //Treachery cards
        foreach (var faction in TrackedTreacheryCards.Keys)
        foreach (var cardNr in TrackedTreacheryCards[faction].Keys) Write(ref result, TrackedTreacheryCards[faction][cardNr]);

        //Traitors
        Write(ref result, _trackedTraitors.Keys.Count);
        foreach (var key in _trackedTraitors.Keys)
        {
            Write(ref result, key.Item1);
            Write(ref result, key.Item2);
            Write(ref result, _trackedTraitors[key]);
        }

        //Discarded traitors
        Write(ref result, _trackedDiscardedTraitors.Keys.Count);
        foreach (var key in _trackedDiscardedTraitors.Keys)
        {
            Write(ref result, key);
            Write(ref result, _trackedDiscardedTraitors[key]);
        }

        //Spice blow card
        Write(ref result, TrackedSpiceCard);

        //Discarded treachery cards
        Write(ref result, _discardedCards.Count);
        foreach (var card in _discardedCards) Write(ref result, card);

        return result;
    }

    public static GreenIntelligence Parse(IGameService client, string data)
    {
        var elements = data.Split(';');
        var elt = 0;

        try
        {
            var result = new GreenIntelligence(client);

            //Treachery cards
            foreach (var faction in result.TrackedTreacheryCards.Keys.ToList())
            foreach (var cardNumber in result.TrackedTreacheryCards[faction].Keys.ToList())
            {
                var cardId = int.Parse(elements[elt++]);
                result.TrackedTreacheryCards[faction][cardNumber] = cardId;
            }

            //Traitors
            var count = int.Parse(elements[elt++]);
            for (var i = 0; i < count; i++)
            {
                var faction = Enum.Parse<Faction>(elements[elt++]);
                var cardNumber = int.Parse(elements[elt++]);
                var traitorId = int.Parse(elements[elt++]);
                result.ChangeSelectedTraitor(faction, cardNumber, traitorId);
            }

            //Discarded traitors
            count = int.Parse(elements[elt++]);
            for (var i = 0; i < count; i++) result.ChangeDiscardedTraitor(int.Parse(elements[elt++]), int.Parse(elements[elt++]));

            //Spice blow card
            result.TrackedSpiceCard = elements[elt++];

            //Discarded treachery cards
            if (elt < elements.Length)
            {
                count = int.Parse(elements[elt++]);
                for (var i = 0; i < count; i++) result._discardedCards.Add(int.Parse(elements[elt++]));
            }

            return result;
        }
        catch (Exception)
        {
            //Do nothing
        }

        return new GreenIntelligence(client);
    }
}