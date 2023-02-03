/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
*/

using Treachery.Shared;

namespace Treachery.Client.GenericComponents
{
    public abstract partial class GameEventComponent<GameEventType> where GameEventType : GameEvent, new()
    {
        protected abstract GameEventType ConfirmedResult { get; }

        protected virtual GameEventType PassedResult { get; }

        protected virtual GameEventType OtherResult { get; }
    }
}
