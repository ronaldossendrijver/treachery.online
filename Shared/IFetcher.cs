/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public interface IFetcher<T>
    {
        T Find(int id);

        int GetId(T obj);
    }
}
