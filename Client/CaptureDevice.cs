/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Client
{
    public class CaptureDevice
    {
        public string Label { get; internal set; }

        public string DeviceId { get; internal set; }

        public string Kind { get; internal set; }

        public string GroupId { get; internal set; }

        public override string ToString()
        {
            return DeviceId;
        }
    }
}
