/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public static class LeaderManager
    {
        public const int FIRST_ID = 1000;
        public static List<Leader> Leaders;
        public static Messiah Messiah = new Messiah() { Id = FIRST_ID, SkinId = 1098 };
        public static LeaderFetcher LeaderLookup = new LeaderFetcher();
        public static HeroFetcher HeroLookup = new HeroFetcher();

        static LeaderManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            int id = FIRST_ID + 1;

            Leaders = new List<Leader>
            {
                new Leader(id++) { Value = 5, Faction = Faction.Green },
                new Leader(id++) { Value = 5, Faction = Faction.Green },
                new Leader(id++) { Value = 4, Faction = Faction.Green },
                new Leader(id++) { Value = 2, Faction = Faction.Green },
                new Leader(id++) { Value = 1, Faction = Faction.Green },

                new Leader(id++) { Value = 5, Faction = Faction.Blue },
                new Leader(id++) { Value = 5, Faction = Faction.Blue },
                new Leader(id++) { Value = 5, Faction = Faction.Blue },
                new Leader(id++) { Value = 5, Faction = Faction.Blue },
                new Leader(id++) { Value = 5, Faction = Faction.Blue },

                new Leader(id++) { Value = 6, Faction = Faction.Red },
                new Leader(id++) { Value = 5, Faction = Faction.Red },
                new Leader(id++) { Value = 3, Faction = Faction.Red },
                new Leader(id++) { Value = 3, Faction = Faction.Red },
                new Leader(id++) { Value = 2, Faction = Faction.Red },

                new Leader(id++) { Value = 7, Faction = Faction.Yellow },
                new Leader(id++) { Value = 6, Faction = Faction.Yellow },
                new Leader(id++) { Value = 5, Faction = Faction.Yellow },
                new Leader(id++) { Value = 3, Faction = Faction.Yellow },
                new Leader(id++) { Value = 2, Faction = Faction.Yellow },

                new Leader(id++) { Value = 5, Faction = Faction.Orange },
                new Leader(id++) { Value = 3, Faction = Faction.Orange },
                new Leader(id++) { Value = 3, Faction = Faction.Orange },
                new Leader(id++) { Value = 2, Faction = Faction.Orange },
                new Leader(id++) { Value = 1, Faction = Faction.Orange },

                new Leader(id++) { Value = 6, Faction = Faction.Black },
                new Leader(id++) { Value = 4, Faction = Faction.Black },
                new Leader(id++) { Value = 3, Faction = Faction.Black },
                new Leader(id++) { Value = 2, Faction = Faction.Black },
                new Leader(id++) { Value = 1, Faction = Faction.Black },

                new Leader(id++) { Value = 4, Faction = Faction.Grey },
                new Leader(id++) { Value = 5, Faction = Faction.Grey },
                new Leader(id++) { Value = 5, Faction = Faction.Grey },
                new Leader(id++) { Value = 2, Faction = Faction.Grey },
                new Leader(id++) { Value = 1, Faction = Faction.Grey },

                new Leader(id++) { Value = Leader.VARIABLEVALUE, Faction = Faction.Purple },
                new Leader(id++) { Value = 4, Faction = Faction.Purple },
                new Leader(id++) { Value = 3, Faction = Faction.Purple },
                new Leader(id++) { Value = 2, Faction = Faction.Purple },
                new Leader(id++) { Value = 1, Faction = Faction.Purple }
            };
        }

        public static IEnumerable<Leader> GetLeaders(Faction faction)
        {
            return Leaders.Where(l => l.Faction == faction);
        }


        public static Leader Find(string leaderName)
        {
            return Leaders.Where(l => l.Name == leaderName).SingleOrDefault();
        }

        public class LeaderFetcher : IFetcher<Leader>
        {
            public Leader Find(int id)
            {
                if (id == -1)
                {
                    return null;
                }
                else
                {
                    return Leaders.SingleOrDefault(t => t.Id == id);
                }
            }

            public int GetId(Leader obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }

        public class HeroFetcher : IFetcher<IHero>
        {
            public IHero Find(int id)
            {
                if (id < 0)
                {
                    return null;
                }
                else if (id == Messiah.Id)
                {
                    return Messiah;
                }
                else if (id >= FIRST_ID)
                {
                    return Leaders.SingleOrDefault(t => t.Id == id);
                }
                else
                {
                    return TreacheryCardManager.Lookup.Find(id);
                }
            }

            public int GetId(IHero obj)
            {
                if (obj == null)
                {
                    return -1;
                }
                else
                {
                    return obj.Id;
                }
            }
        }
    }
}
