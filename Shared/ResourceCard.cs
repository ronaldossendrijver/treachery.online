/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public bool IsShaiHulud
        {
            get
            {
                return Location == null && !IsSandTrout;
            }
        }

        public bool IsSpiceBlow
        {
            get
            {
                return Location != null;
            }
        }

        public override string ToString()
        {
            if (IsShaiHulud)
            {
                return Skin.Current.Describe(Concept.Monster);
            }
            else if (IsSandTrout)
            {
                return Skin.Current.Describe(Concept.BabyMonster);
            }
            else
            {
                return Location.Territory.Name;
            }
        }
    }
}