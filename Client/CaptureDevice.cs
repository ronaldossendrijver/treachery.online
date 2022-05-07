/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Client
{
    public class CaptureDevice
    {
        public string Label;
        public string DeviceId;

        public string Kind { get; internal set; }
        public string GroupId { get; internal set; }

        public override string ToString()
        {
            return DeviceId;
        }
    }
}
