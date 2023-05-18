﻿/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ResourceCard
    {
        public bool IsSandTrout { get; set; } = false;
        public bool IsGreatMaker { get; set; } = false;

        public int SkinId { get; private set; }

        public Location Location { get; set; } = null;

        public Location DiscoveryLocation { get; set; } = null;

        public ResourceCard(int skinId)
        {
            SkinId = skinId;
        }

        public bool IsShaiHulud => Location == null && !IsSandTrout && !IsGreatMaker;

        public bool IsSpiceBlow => Location != null;

        public bool IsDiscovery => DiscoveryLocation != null;

        public Territory Territory => Location?.Territory;

        public override string ToString()
        {
            if (Message.DefaultDescriber != null)
            {
                return Message.DefaultDescriber.Describe(this) + "*";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}