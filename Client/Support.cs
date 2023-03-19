/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace Treachery.Client
{
    public static class Support
    {
        public static void Log(object o)
        {
            Console.WriteLine(o);
        }

        public static void Log(string msg, params object[] o)
        {
            Console.WriteLine(msg, o);
        }

        public static string GetHash(string input)
        {
            if (input == null || input.Length == 0)
            {
                return "";
            }

            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < input.Length; i++)
            {
                hashedValue += input[i];
                hashedValue *= 3074457345618258799ul;
            }

            return hashedValue.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyHash(string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetHash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public static string TextBorder(int borderwidth, string bordercolor) =>
            string.Format("text-shadow: {0}px {0}px {0}px {1}, 0px {0}px {0}px {1}, -{0}px {0}px {0}px {1}, -{0}px 0px {0}px {1}, -{0}px -{0}px {0}px {1}, 0px -{0}px {0}px {1}, {0}px -{0}px {0}px {1}, {0}px 0px {0}px {1}, 0px 0px {0}px {1};", Round(0.5f * borderwidth), bordercolor);
        //$"-webkit-text-stroke: {Px(0.4f * borderwidth)} {bordercolor}";

        public static string Round(double x) => Math.Round(x, 3).ToString(CultureInfo.InvariantCulture);

        public static string RoundWithHalves(double x)
        {
            if (x == 0.5f)
            {
                return "½";
            }
            else if (x == -0.5f)
            {
                return "-½";
            }
            else if (x - 0.5f == (int)x)
            {
                return "" + (int)x + "½";
            }
            else
            {
                return Round(x);
            }
        }

        public static string Px(double x) => "" + Round(x) + "px";
    }
}
