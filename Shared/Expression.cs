/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class Expression
    {
        public object[] Elements { get; private set; }

        public Expression(params object[] elements)
        {
            Elements = elements;
        }

        public Expression(List<object> elements)
        {
            Elements = elements.ToArray();
        }
    }
}
