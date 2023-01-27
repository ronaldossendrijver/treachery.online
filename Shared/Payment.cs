/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    public class Payment
    {
        public int Amount { get; set; }

        public Faction By { get; set; }

        public Faction To { get; set; }

        public GameEvent Reason { get; set; }

        public Payment()
        {

        }

        public Payment(int amount)
        {
            Amount = amount;
        }

        public Payment(int amount, Faction by)
        {
            Amount = amount;
            By = by;
        }

        public Payment(int amount, Faction by, Faction to, GameEvent reason)
        {
            Amount = amount;
            By = by;
            To = to;
            Reason = reason;
        }
    }

}
