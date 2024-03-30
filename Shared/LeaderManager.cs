/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public static class LeaderManager
{
    public const int FIRST_ID = 1000;
    public static List<Leader> Leaders { get; private set; }
    public static Messiah Messiah { get; } = new() { Id = FIRST_ID, SkinId = 1098 };
    public static LeaderFetcher LeaderLookup { get; private set; } = new();
    public static HeroFetcher HeroLookup { get; private set; } = new();

    static LeaderManager()
    {
        Initialize();
    }

    public static void Initialize()
    {
        var id = FIRST_ID + 1;

        Leaders = new List<Leader>
        {
            new(id++) { Value = 5, Faction = Faction.Green, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Green, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Green, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Green, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Green, HeroType = HeroType.Normal },

            new(id++) { Value = 5, Faction = Faction.Blue, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Blue, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Blue, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Blue, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Blue, HeroType = HeroType.Normal },

            new(id++) { Value = 6, Faction = Faction.Red, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Red, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Red, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Red, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Red, HeroType = HeroType.Normal },

            new(id++) { Value = 7, Faction = Faction.Yellow, HeroType = HeroType.Normal },
            new(id++) { Value = 6, Faction = Faction.Yellow, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Yellow, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Yellow, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Yellow, HeroType = HeroType.Normal },

            new(id++) { Value = 5, Faction = Faction.Orange, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Orange, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Orange, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Orange, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Orange, HeroType = HeroType.Normal },

            new(id++) { Value = 6, Faction = Faction.Black, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Black, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Black, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Black, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Black, HeroType = HeroType.Normal },

            new(id++) { Value = 4, Faction = Faction.Grey, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Grey, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.Grey, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Grey, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Grey, HeroType = HeroType.Normal },

            new(id++) { Faction = Faction.Purple, HeroType = HeroType.VariableValue },
            new(id++) { Value = 4, Faction = Faction.Purple, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Purple, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Purple, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Purple, HeroType = HeroType.Normal },

            new(id++) { Value = 2, Faction = Faction.Brown, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Brown, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Brown, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Brown, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Brown, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Brown, HeroType = HeroType.Auditor },

            new(id++) { Value = 2, Faction = Faction.White, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.White, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.White, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.White, HeroType = HeroType.Normal },
            new(id++) { Value = 5, Faction = Faction.White, HeroType = HeroType.Normal },

            new(id++) { Value = 4, Faction = Faction.Pink, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Pink, HeroType = HeroType.Normal },
            new(id++) { Value = 3, Faction = Faction.Pink, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Pink, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Pink, HeroType = HeroType.Normal },
            new(id++) { Value = 6, Faction = Faction.Pink, HeroType = HeroType.Vidal },

            new(id++) { Value = 5, Faction = Faction.Cyan, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Cyan, HeroType = HeroType.Normal },
            new(id++) { Value = 2, Faction = Faction.Cyan, HeroType = HeroType.Normal },
            new(id++) { Value = 4, Faction = Faction.Cyan, HeroType = HeroType.Normal },
            new(id++) { Value = 1, Faction = Faction.Cyan, HeroType = HeroType.Normal }

        };
    }

    public static IEnumerable<Leader> GetLeaders(Faction faction)
    {
        return Leaders.Where(l => l.Faction == faction);
    }

    public class LeaderFetcher : IFetcher<Leader>
    {
        public Leader Find(int id)
        {
            if (id == -1)
                return null;
            return Leaders.SingleOrDefault(t => t.Id == id);
        }

        public int GetId(Leader obj)
        {
            if (obj == null)
                return -1;
            return obj.Id;
        }
    }

    public class HeroFetcher : IFetcher<IHero>
    {
        public IHero Find(int id)
        {
            if (id < 0)
                return null;
            if (id == Messiah.Id)
                return Messiah;
            if (id >= FIRST_ID)
                return Leaders.SingleOrDefault(t => t.Id == id);
            return TreacheryCardManager.Lookup.Find(id);
        }

        public int GetId(IHero obj)
        {
            if (obj == null)
                return -1;
            return obj.Id;
        }
    }
}