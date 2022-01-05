/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
*/

using Treachery.Shared;

namespace Treachery.Client.GenericComponents
{
    public abstract partial class GameEventComponent<GameEventType> where GameEventType : GameEvent, new()
    {

    }
}
