/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */
using System;

namespace Treachery.Shared
{
    public class TreacheryCard : IHero, IIdentifiable, IComparable<TreacheryCard>
    {
        public const int NONE = -2;
        public const int UNKNOWN = -1;

        public TreacheryCard(int id, int skinId, TreacheryCardType type, Rule rule)
        {
            Id = id;
            Type = type;
            Rule = rule;
            SkinId = skinId;
        }

        public int Id { get; private set; }

        public int SkinId { get; private set; }

        public int Value
        {
            get
            {
                return 0;
            }
        }

        public bool Is(Faction f)
        {
            return Faction == f;
        }

        public int ValueInCombatAgainst(IHero opposingHero)
        {
            return Value;
        }

        public Faction Faction
        {
            get
            {
                return Faction.None;
            }
        }

        public int CostToRevive
        {
            get
            {
                return Value;
            }
        }

        public bool IsTraitor(IHero hero)
        {
            return hero != null && hero is TreacheryCard;
        }

        public bool IsFaceDancer(IHero hero)
        {
            return hero != null && hero is TreacheryCard;
        }

        public Rule Rule { get; private set; }

        public TreacheryCardType Type { get; private set; }

        public HeroType HeroType => HeroType.Mercenary;

        public virtual string Name
        {
            get
            {
                if (Id == -1)
                {
                    return "Unknown";
                }
                else if (Id == -2)
                {
                    return "None";
                }
                else
                {
                    return Skin.Current.GetTreacheryCardName(this);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is TreacheryCard c && c.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool IsPoisonWeapon => Type == TreacheryCardType.Poison || Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.ProjectileAndPoison;

        public bool IsProjectileWeapon => Type == TreacheryCardType.Projectile || Type == TreacheryCardType.WeirdingWay || Type == TreacheryCardType.ProjectileAndPoison;

        public bool IsPoisonDefense => Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.Antidote || Type == TreacheryCardType.ShieldAndAntidote;

        public bool IsNonAntidotePoisonDefense => Type == TreacheryCardType.Chemistry;

        public bool IsShield => Type == TreacheryCardType.Shield || Type == TreacheryCardType.ShieldAndAntidote;

        public bool IsProjectileDefense => Type == TreacheryCardType.Shield || Type == TreacheryCardType.WeirdingWay || Type == TreacheryCardType.ShieldAndAntidote;

        public bool IsLaser => Type == TreacheryCardType.Laser;

        public bool IsPoisonTooth => Type == TreacheryCardType.PoisonTooth;

        public bool IsArtillery => Type == TreacheryCardType.ArtilleryStrike;

        public bool IsRockmelter => Type == TreacheryCardType.Rockmelter;

        public bool IsMirrorWeapon => Type == TreacheryCardType.MirrorWeapon;

        public bool IsPortableAntidote => Type == TreacheryCardType.PortableAntidote;

        public bool IsWeapon => IsLaser || IsPoisonWeapon || IsProjectileWeapon || IsPoisonTooth || IsArtillery || IsMirrorWeapon || IsRockmelter;

        public bool IsDefense => IsPoisonDefense || IsProjectileDefense;

        public bool IsUseless => Type == TreacheryCardType.Useless;

        public bool CounteredBy(TreacheryCard defense, TreacheryCard combinedWithWeapon)
        {
            if (Type == TreacheryCardType.MirrorWeapon)
            {
                return combinedWithWeapon == null || combinedWithWeapon.CounteredBy(defense, combinedWithWeapon);
            }
            else
            {
                return
                    Type == TreacheryCardType.PoisonTooth && defense.IsNonAntidotePoisonDefense ||
                    Type == TreacheryCardType.Poison && defense.IsPoisonDefense ||
                    Type == TreacheryCardType.Chemistry && defense.IsPoisonDefense ||
                    Type == TreacheryCardType.Projectile && defense.IsProjectileDefense ||
                    Type == TreacheryCardType.WeirdingWay && defense.IsProjectileDefense ||
                    Type == TreacheryCardType.ProjectileAndPoison && defense.Type == TreacheryCardType.ShieldAndAntidote;
            }
        }

        public int CompareTo(TreacheryCard other)
        {
            if (other == null)
            {
                return 1;
            }
            else
            {
                return Name.CompareTo(other.Name);
            }
        }
    }

}