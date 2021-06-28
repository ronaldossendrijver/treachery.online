/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ObserverJoined
    {
        public string HashedPassword { get; set; }

        public string Name { get; set; }

        public static string ValidName(string name)
        {
            if (name == null || name.Length == 0) return "Please enter a name.";
            if (name.Contains('&') || name.Contains('#') || name.Contains('\"') || name.Contains('<') || name.Contains('>')) return "Name cannot contain &, #, \", < or >";

            return "";
        }
    }

    public class ObserverRejoined
    {
        public string HashedPassword { get; set; }

        public string Name { get; set; }
    }
}
