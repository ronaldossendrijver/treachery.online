﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

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

        public static Payment Of(int amount)
        {
            return new Payment() { Amount = amount };
        }

        public static Payment Of(int amount, Faction by)
        {
            return new Payment() { Amount = amount, By = by };
        }
    }

}