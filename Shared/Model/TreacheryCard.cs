/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class TreacheryCard(int id, int skinId, TreacheryCardType type, Rule[] rules)
    : IHero
{
    public const int None = -2;
    public const int Unknown = -1;

    public TreacheryCard(int id, int skinId, TreacheryCardType type, Rule rule) : this(id, skinId, type, [rule])
    {
    }

    public int Id { get; } = id;

    public int SkinId { get; } = skinId;

    public int Value => 0;

    public bool Is(Faction f) => false;

    public int ValueInCombatAgainst(IHero opposingHero) => 0;

    public Faction Faction => Faction.None;

    public int CostToRevive => 0;

    public bool IsTraitor(IHero hero) => hero is TreacheryCard;

    public bool IsFaceDancer(IHero hero) => hero is TreacheryCard;

    public Rule[] Rules { get; private set; } = rules;

    public TreacheryCardType Type { get; } = type;

    public HeroType HeroType => HeroType.Mercenary;

    public override bool Equals(object obj) => obj is TreacheryCard c && c.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();

    public bool IsPoisonWeapon => Type is TreacheryCardType.Poison or TreacheryCardType.Chemistry or TreacheryCardType.ProjectileAndPoison;

    public bool IsProjectileWeapon => Type is TreacheryCardType.Projectile or TreacheryCardType.WeirdingWay or TreacheryCardType.ProjectileAndPoison;

    public bool IsPoisonDefense => Type is TreacheryCardType.Chemistry or TreacheryCardType.Antidote or TreacheryCardType.ShieldAndAntidote or TreacheryCardType.PortableAntidote;

    public bool IsNonAntidotePoisonDefense => Type is TreacheryCardType.Chemistry;

    public bool IsShield => Type is TreacheryCardType.Shield or TreacheryCardType.ShieldAndAntidote;

    public bool IsProjectileDefense => Type is TreacheryCardType.Shield or TreacheryCardType.WeirdingWay or TreacheryCardType.ShieldAndAntidote;

    public bool IsLaser => Type is TreacheryCardType.Laser;

    public bool IsPoisonTooth => Type is TreacheryCardType.PoisonTooth;

    public bool IsArtillery => Type is TreacheryCardType.ArtilleryStrike;

    public bool IsRockMelter => Type is TreacheryCardType.Rockmelter;

    public bool IsMirrorWeapon => Type is TreacheryCardType.MirrorWeapon;

    public bool IsPortableAntidote => Type is TreacheryCardType.PortableAntidote;

    public bool IsWeapon => IsPoisonWeapon || IsProjectileWeapon || IsLaser || IsPoisonTooth || IsArtillery || IsMirrorWeapon || IsRockMelter;

    public bool IsDefense => IsPoisonDefense || IsProjectileDefense;

    public bool IsUseless => Type is TreacheryCardType.Useless;

    public bool IsMercenary => Type is TreacheryCardType.Mercenary;

    public bool IsGreen => !(IsWeapon || IsDefense || IsUseless || IsMercenary);

    public bool CounteredBy(TreacheryCard defense, TreacheryCard combinedWithWeapon)
    {
        if (Type == TreacheryCardType.MirrorWeapon)
            return combinedWithWeapon == null || combinedWithWeapon.CounteredBy(defense, combinedWithWeapon);
        
        return
            (Type is TreacheryCardType.PoisonTooth && defense.IsNonAntidotePoisonDefense) ||
            (Type is TreacheryCardType.Poison && defense.IsPoisonDefense) ||
            (Type is TreacheryCardType.Chemistry && defense.IsPoisonDefense) ||
            (Type is TreacheryCardType.Projectile && defense.IsProjectileDefense) ||
            (Type is TreacheryCardType.WeirdingWay && defense.IsProjectileDefense) ||
            (Type is TreacheryCardType.ProjectileAndPoison && defense.Type is TreacheryCardType.ShieldAndAntidote);
    }

    public override string ToString()
    {
        if (Message.DefaultDescriber != null)
            return Message.DefaultDescriber.Describe(this) + "*";
        
        return base.ToString();
    }
}