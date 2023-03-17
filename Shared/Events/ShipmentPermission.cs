/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;

namespace Treachery.Shared
{
    [Flags]
    public enum ShipmentPermission
    {
        None = 0,
        Cross = 1,
        ToHomeworld = 2,
        OrangeRate = 4
    }
}
