/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class TreacheryCard : IHero, IIdentifiable
    {
        public const int NONE = -2;
        public const int UNKNOWN = -1;

        public TreacheryCard(int id, int skinId, TreacheryCardType type, Rule[] rules)
        {
            Id = id;
            Type = type;
            Rules = rules;
            SkinId = skinId;
        }

        public TreacheryCard(int id, int skinId, TreacheryCardType type, Rule rule) : this(id, skinId, type, new Rule[] { rule })
        {
        }

        public int Id { get; private set; }

        public int SkinId { get; private set; }

        public int Value => 0;

        public bool Is(Faction f) => Faction == f;

        public int ValueInCombatAgainst(IHero opposingHero) => Value;

        public Faction Faction => Faction.None;

        public int CostToRevive => Value;

        public bool IsTraitor(IHero hero) => hero != null && hero is TreacheryCard;

        public bool IsFaceDancer(IHero hero) => hero != null && hero is TreacheryCard;

        public Rule[] Rules { get; private set; }

        public TreacheryCardType Type { get; private set; }

        public HeroType HeroType => HeroType.Mercenary;

        public override bool Equals(object obj) => obj is TreacheryCard c && c.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public bool IsPoisonWeapon => Type == TreacheryCardType.Poison || Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.ProjectileAndPoison;

        public bool IsProjectileWeapon => Type == TreacheryCardType.Projectile || Type == TreacheryCardType.WeirdingWay || Type == TreacheryCardType.ProjectileAndPoison;

        public bool IsPoisonDefense => Type == TreacheryCardType.Chemistry || Type == TreacheryCardType.Antidote || Type == TreacheryCardType.ShieldAndAntidote || Type == TreacheryCardType.PortableAntidote;

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

        public bool IsMercenary => Type == TreacheryCardType.Mercenary;

        public bool IsGreen => !(IsWeapon || IsDefense || IsUseless || IsMercenary);

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