/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Treachery.Shared;

namespace Treachery.Client
{
    public class LocationEventArgs
    {
        public Location Location { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
    }
}
