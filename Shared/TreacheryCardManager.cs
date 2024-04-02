/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public static class TreacheryCardManager
{
    private static readonly List<TreacheryCard> Items = new();
    public static IFetcher<TreacheryCard> Lookup = new TreacheryCardFetcher();

    public const int CARD_BALISET = 28;
    public const int CARD_JUBBACLOAK = 29;
    public const int CARD_KULON = 30;
    public const int CARD_LALALA = 31;
    public const int CARD_TRIPTOGAMONT = 32;
    public const int CARD_KULLWAHAD = 46;

    static TreacheryCardManager()
    {
        Initialize();
    }

    public static void Initialize()
    {
        //Basic cards
        Items.Add(new TreacheryCard(0, 0, TreacheryCardType.Laser, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(1, 1, TreacheryCardType.Projectile, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(2, 2, TreacheryCardType.Projectile, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(3, 3, TreacheryCardType.Projectile, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(4, 4, TreacheryCardType.Projectile, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(5, 5, TreacheryCardType.Poison, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(6, 6, TreacheryCardType.Poison, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(7, 7, TreacheryCardType.Poison, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(8, 8, TreacheryCardType.Poison, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(9, 9, TreacheryCardType.Shield, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(10, 9, TreacheryCardType.Shield, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(11, 9, TreacheryCardType.Shield, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(12, 9, TreacheryCardType.Shield, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(13, 13, TreacheryCardType.Antidote, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(14, 13, TreacheryCardType.Antidote, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(15, 13, TreacheryCardType.Antidote, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(16, 13, TreacheryCardType.Antidote, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(17, 17, TreacheryCardType.Mercenary, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(18, 17, TreacheryCardType.Mercenary, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(19, 19, TreacheryCardType.Mercenary, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(20, 20, TreacheryCardType.RaiseDead, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(21, 21, TreacheryCardType.Metheor, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(22, 22, TreacheryCardType.Caravan, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(23, 23, TreacheryCardType.Karma, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(24, 23, TreacheryCardType.Karma, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(25, 25, TreacheryCardType.Clairvoyance, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(26, 25, TreacheryCardType.Clairvoyance, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(27, 27, TreacheryCardType.StormSpell, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(CARD_BALISET, CARD_BALISET, TreacheryCardType.Useless, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(CARD_JUBBACLOAK, CARD_JUBBACLOAK, TreacheryCardType.Useless, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(CARD_KULON, CARD_KULON, TreacheryCardType.Useless, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(CARD_LALALA, CARD_LALALA, TreacheryCardType.Useless, Rule.BasicTreacheryCards));
        Items.Add(new TreacheryCard(CARD_TRIPTOGAMONT, CARD_TRIPTOGAMONT, TreacheryCardType.Useless, Rule.BasicTreacheryCards));

        //Grey & Purple Expansion Treachery Cards
        Items.Add(new TreacheryCard(33, 33, TreacheryCardType.ProjectileAndPoison, Rule.ExpansionTreacheryCardsPBandSS));
        Items.Add(new TreacheryCard(34, 34, TreacheryCardType.Projectile, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(35, 35, TreacheryCardType.Poison, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(36, 36, TreacheryCardType.WeirdingWay, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(37, 37, TreacheryCardType.PoisonTooth, new[] { Rule.ExpansionTreacheryCardsExceptPBandSSandAmal, Rule.Expansion2TreacheryCards }));
        Items.Add(new TreacheryCard(38, 38, TreacheryCardType.ShieldAndAntidote, Rule.ExpansionTreacheryCardsPBandSS));
        Items.Add(new TreacheryCard(39, 9, TreacheryCardType.Shield, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(40, 13, TreacheryCardType.Antidote, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(41, 39, TreacheryCardType.Chemistry, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(42, 40, TreacheryCardType.ArtilleryStrike, new[] { Rule.ExpansionTreacheryCardsExceptPBandSSandAmal, Rule.Expansion2TreacheryCards }));
        Items.Add(new TreacheryCard(43, 41, TreacheryCardType.Harvester, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(44, 42, TreacheryCardType.Thumper, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));
        Items.Add(new TreacheryCard(45, 43, TreacheryCardType.Amal, Rule.ExpansionTreacheryCardsAmal));
        Items.Add(new TreacheryCard(CARD_KULLWAHAD, 44, TreacheryCardType.Useless, Rule.ExpansionTreacheryCardsExceptPBandSSandAmal));

        //White Treachery Cards
        Items.Add(new TreacheryCard(47, 45, TreacheryCardType.Distrans, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(48, 46, TreacheryCardType.Juice, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(49, 47, TreacheryCardType.MirrorWeapon, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(50, 48, TreacheryCardType.PortableAntidote, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(51, 49, TreacheryCardType.Flight, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(52, 50, TreacheryCardType.SearchDiscarded, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(53, 51, TreacheryCardType.TakeDiscarded, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(54, 52, TreacheryCardType.Residual, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(55, 53, TreacheryCardType.Rockmelter, Rule.WhiteTreacheryCards));
        Items.Add(new TreacheryCard(56, 54, TreacheryCardType.Karma, Rule.WhiteTreacheryCards));

        //Expansion 3 Treachery Cards
        Items.Add(new TreacheryCard(57, 55, TreacheryCardType.Recruits, Rule.Expansion3TreacheryCards));
        Items.Add(new TreacheryCard(58, 56, TreacheryCardType.Reinforcements, Rule.Expansion3TreacheryCards));
        Items.Add(new TreacheryCard(59, 57, TreacheryCardType.HarassAndWithdraw, Rule.Expansion3TreacheryCards));

        //3 extra karma cards
        Items.Add(new TreacheryCard(100, 23, TreacheryCardType.Karma, Rule.ExtraKaramaCards));
        Items.Add(new TreacheryCard(101, 23, TreacheryCardType.Karma, Rule.ExtraKaramaCards));
        Items.Add(new TreacheryCard(102, 23, TreacheryCardType.Karma, Rule.ExtraKaramaCards));
    }

    public static Deck<TreacheryCard> CreateTreacheryDeck(Game g, Random random)
    {
        return new Deck<TreacheryCard>(GetCardsInPlay(g), random);
    }

    public static List<TreacheryCard> GetWhiteCards()
    {
        return GetCardsInPlay(Rule.WhiteTreacheryCards).ToList();
    }

    public static IEnumerable<TreacheryCard> GetCardsInPlay(Game g)
    {
        return Items.Where(c =>

            c.Rules.Any(rule => g.Applicable(rule)) ||

            //Amal used to be included in the basic set of expansion cards
            (g.Version <= 104 && c.Type == TreacheryCardType.Amal && g.Applicable(Rule.ExpansionTreacheryCardsExceptPBandSSandAmal)));
    }

    public static IEnumerable<TreacheryCard> GetCardsInPlay(Rule rule)
    {
        return Items.Where(c => c.Rules.Contains(rule));
    }

    public static IEnumerable<TreacheryCard> GetCardsInAndOutsidePlay()
    {
        return Items;
    }

    public static TreacheryCard Get(int id)
    {
        return Items.SingleOrDefault(i => i.Id == id);
    }

    public static int GetId(TreacheryCard value)
    {
        if (value != null)
            return value.Id;
        return -1;
    }

    public class TreacheryCardFetcher : IFetcher<TreacheryCard>
    {
        public TreacheryCard Find(int id)
        {
            if (id < -1)
                return null;
            return Items.SingleOrDefault(t => t.Id == id);
        }

        public int GetId(TreacheryCard obj)
        {
            if (obj == null)
                return -1;
            return obj.Id;
        }
    }
}