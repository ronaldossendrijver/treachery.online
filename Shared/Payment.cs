/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Payment
    {
        public int Amount { get; set; }

        public Faction By { get; set; }

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
    }

}
