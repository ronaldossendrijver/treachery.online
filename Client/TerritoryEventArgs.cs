/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Treachery.Shared;

namespace Treachery.Client
{
    public class TerritoryEventArgs
    {
        public Territory Territory { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
    }
}
