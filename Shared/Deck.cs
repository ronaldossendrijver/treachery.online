/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Deck<T>
    {
        public List<T> Items { get; set; } = new List<T>();

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
            int n = Items.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                var value = Items[k];
                Items[k] = Items[n];
                Items[n] = value;
            }
        }

        public static List<T> Randomize(IEnumerable<T> toRandomize)
        {
            var deck = new Deck<T>(toRandomize, new Random());
            deck.Shuffle();
            return deck.Items;
        }

        public bool IsEmpty
        {
            get
            {
                return Items.Count == 0;
            }
        }

        public T Top
        {
            get
            {
                if (Items.Count > 0)
                {
                    return Items[0];
                }
                else
                {
                    return default;
                }
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public override string ToString()
        {
            return string.Join(", ", Items);
        }
    }

    public enum ReshuffleOption : int
    {
        None = 0,
        UseEntireDiscardPile = 1,
        UseDiscardPileButLeaveTop = 2
    }
}
