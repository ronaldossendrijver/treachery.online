﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;

namespace Treachery.Shared
{
    public class HiddenMobileStronghold : AttachedLocation
    {
        public HiddenMobileStronghold(Territory t, int id) : base(id)
        {
            Territory = t;
        }
    }
}