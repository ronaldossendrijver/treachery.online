/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ResourceCard
    {
        public bool IsSandTrout = false;

        public int SkinId { get; private set; }

        public Location Location { get; set; } = null;

        public ResourceCard(int skinId)
        {
            SkinId = skinId;
        }

        public bool IsShaiHulud => Location == null && !IsSandTrout;

        public bool IsSpiceBlow => Location != null;

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