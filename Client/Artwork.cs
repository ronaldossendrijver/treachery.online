/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Treachery.Shared;

namespace Treachery.Client
{
    public class Artwork
    {
        public static ElementReference MapImage { get; set; }
        public static ElementReference EyeImage { get; set; }
        public static ElementReference EyeSlashImage { get; set; }
        public static ElementReference Monster { get; set; }
        public static ElementReference ResourceCardBackImage { get; set; }
        public static ElementReference TreacheryCardBackImage { get; set; }
        public static ElementReference BattleScreenImage { get; set; }
        public static ElementReference MessiahImage { get; set; }
        public static ElementReference HarvesterImage { get; set; }
        public static ElementReference ResourceImage { get; set; }
        public static ElementReference HiddenMobileStrongholdImage { get; set; }

        public static readonly Dictionary<Leader, Art> LeaderTokens = new Dictionary<Leader, Art>();
        public static readonly Dictionary<TreacheryCard, Art> TreacheryCards = new Dictionary<TreacheryCard, Art>();
        public static readonly Dictionary<int, Art> ResourceCards = new Dictionary<int, Art>();
        public static readonly Dictionary<Faction, Art> FactionTokens = new Dictionary<Faction, Art>();
        public static readonly Dictionary<Faction, Art> FactionTableTokens = new Dictionary<Faction, Art>();
        public static readonly Dictionary<Faction, Art> FactionFacedownTokens = new Dictionary<Faction, Art>();
        public static readonly Dictionary<Faction, Art> ForceTokens = new Dictionary<Faction, Art>();
        public static readonly Dictionary<Faction, Art> SpecialForceTokens = new Dictionary<Faction, Art>();
        public static readonly Dictionary<TechToken, Art> TechTokens = new Dictionary<TechToken, Art>();
        public static readonly Art[] Arrows = new Art[200];

        static Artwork()
        {
            foreach (var l in LeaderManager.Leaders)
            {
                LeaderTokens.Add(l, new Art());
            }

            foreach (var c in TreacheryCardManager.GetCardsInAndOutsidePlay())
            {
                TreacheryCards.Add(c, new Art());
            }

            foreach (var c in Enumerations.GetValuesExceptDefault(typeof(Faction), Faction.None))
            {
                FactionTokens.Add(c, new Art());
                FactionTableTokens.Add(c, new Art());
                FactionFacedownTokens.Add(c, new Art());
                ForceTokens.Add(c, new Art());
                SpecialForceTokens.Add(c, new Art());
            }

            foreach (var c in Enumerations.GetValuesExceptDefault(typeof(TechToken), TechToken.None))
            {
                TechTokens.Add(c, new Art());
            }

            for (int i = 0; i < Arrows.Length; i++)
            {
                Arrows[i] = new Art();
            }
        }

        public static ElementReference SpiceCardBack
        {
            get
            {
                return ResourceCardBackImage;
            }
        }

        public static ElementReference TreacheryCardBack
        {
            get
            {
                return TreacheryCardBackImage;
            }
        }

        public static ElementReference Wheel
        {
            get
            {
                return BattleScreenImage;
            }
        }

        public static ElementReference Messiah
        {
            get
            {
                return MessiahImage;
            }
        }

        public static ElementReference GetLeaderToken(Leader l)
        {
            return LeaderTokens[l].Value;
        }

        public static ElementReference GetTreacheryCard(TreacheryCard card)
        {
            return TreacheryCards[card].Value;
        }

        public static Art GetResourceCard(ResourceCard card)
        {
            return GetResourceCard(card.SkinId);
        }

        public static Art GetResourceCard(int skinId)
        {
            if (!ResourceCards.ContainsKey(skinId))
            {
                ResourceCards.Add(skinId, new Art());
            }

            return ResourceCards[skinId];
        }

        public static ElementReference Eye
        {
            get
            {
                return EyeImage;
            }
        }

        public static ElementReference EyeSlash
        {
            get
            {
                return EyeSlashImage;
            }
        }

        public static ElementReference Map
        {
            get
            {
                return MapImage;
            }
        }

        public static ElementReference HiddenMobileStronghold
        {
            get
            {
                return HiddenMobileStrongholdImage;
            }
        }
    }

    public class Art
    {
        public bool Available = false;

        private ElementReference _value;
        public ElementReference Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Available = true;
            }
        }

    }
}
