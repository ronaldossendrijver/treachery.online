/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface IDescriber
    {
        public string Describe(object obj);

        public string Format(string m, params object[] list);
    }
}
