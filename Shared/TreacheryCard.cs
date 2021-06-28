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

        public bool IsPoisonWeapon
        {
            get
            {
                return Type == TreacheryCardType.Poison || Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.ProjectileAndPoison;
            }
        }

        public bool IsProjectileWeapon
        {
            get
            {
                return Type == TreacheryCardType.Projectile || Type == TreacheryCardType.WeirdingWay || Type == TreacheryCardType.ProjectileAndPoison;
            }
        }

        public bool IsPoisonDefense
        {
            get
            {
                return Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.Antidote || Type == TreacheryCardType.ShieldAndAntidote;
            }
        }

        public bool IsNonAntidotePoisonDefense
        {
            get
            {
                return Type == TreacheryCardType.Chemistry;
            }
        }

        public bool IsShield
        {
            get
            {
                return Type == TreacheryCardType.Shield || Type == TreacheryCardType.ShieldAndAntidote;
            }
        }

        public bool IsProjectileDefense
        {
            get
            {
                return Type == TreacheryCardType.Shield || Type == TreacheryCardType.WeirdingWay || Type == TreacheryCardType.ShieldAndAntidote;
            }
        }

        public bool IsLaser
        {
            get
            {
                return Type == TreacheryCardType.Laser;
            }
        }

        public bool IsPoisonTooth
        {
            get
            {
                return Type == TreacheryCardType.PoisonTooth;
            }
        }

        public bool IsArtillery
        {
            get
            {
                return Type == TreacheryCardType.ArtilleryStrike;
            }
        }

        public bool IsWeapon => IsLaser || IsPoisonWeapon || IsProjectileWeapon || IsPoisonTooth || IsArtillery;

        public bool IsDefense => IsPoisonDefense || IsProjectileDefense;
        public bool IsUseless => Type == TreacheryCardType.Useless;

        public bool CounteredBy(TreacheryCard c)
        {
            return
                Type == TreacheryCardType.PoisonTooth && c.IsNonAntidotePoisonDefense ||
                Type == TreacheryCardType.Poison && c.IsPoisonDefense ||
                Type == TreacheryCardType.Chemistry && c.IsPoisonDefense ||
                Type == TreacheryCardType.Projectile && c.IsProjectileDefense ||
                Type == TreacheryCardType.WeirdingWay && c.IsProjectileDefense ||
                Type == TreacheryCardType.ProjectileAndPoison && c.Type == TreacheryCardType.ShieldAndAntidote;
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