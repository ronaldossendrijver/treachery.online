/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared;

public class Deck<T>
{
    public List<T> Items { get; set; } = new();

    public Random Random { get; set; }

    public Deck(IEnumerable<T> items, Random random)
    {
        Items = new List<T>(items);
        Random = random;
    }

    public Deck(Random random)
    {
        Random = random;
    }

    public void PutOnTop(T item)
    {
        Items.Insert(0, item);
    }

    public void PutOnTop(IEnumerable<T> items)
    {
        foreach (var item in items) PutOnTop(item);
    }

    public void PutOnBottom(T item)
    {
        Items.Add(item);
    }

    public T Draw()
    {
        if (Items.Count > 0)
        {
            var result = Top;
            Items.RemoveAt(0);
            return result;
        }

        throw new InvalidOperationException("Cannot draw from an empty deck");
    }

    public void Shuffle()
    {
        Shuffle(Items, Random);
    }

    public static void Shuffle(List<T> items, Random random)
    {
        var n = items.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (items[n], items[k]) = (items[k], items[n]);
        }
    }

    public static List<T> Randomize(IEnumerable<T> toRandomize)
    {
        var deck = new Deck<T>(toRandomize, new Random());
        deck.Shuffle();
        return deck.Items;
    }

    public bool IsEmpty => Items.Count == 0;

    public T Top
    {
        get
        {
            if (Items.Count > 0)
                return Items[0];
            return default;
        }
    }

    public void Clear()
    {
        Items.Clear();
    }
}