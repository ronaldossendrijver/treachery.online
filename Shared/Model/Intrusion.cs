/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Intrusion
    {
        public ILocationEvent TriggeringEvent { get; set; }

        public IntrusionType Type { get; set; }

        public Territory Territory => TriggeringEvent.To.Territory;

        public Faction Initiator => TriggeringEvent.Initiator;

        public Intrusion(ILocationEvent triggeringEvent, IntrusionType type)
        {
            TriggeringEvent = triggeringEvent;
            Type = type;
        }
    }
}
