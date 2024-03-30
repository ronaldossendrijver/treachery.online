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
    public class Messiah : IHero
    {
        public Messiah()
        {
        }

        public int Value => 2;

        public int ValueInCombatAgainst(IHero opposingHero) => Value;

        public Faction Faction => Faction.Green;

        public HeroType HeroType => HeroType.Messiah;

        public bool Is(Faction f) => Faction == f;

        public int CostToRevive => Value;

        public int Id { get; set; }

        public int SkinId { get; set; }

        public bool IsTraitor(IHero hero) => false;

        public bool IsFaceDancer(IHero hero) => false;

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