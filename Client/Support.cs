/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Treachery.Client;

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
        /*
        if (input == null || input.Length == 0) return "";

        var hashedValue = 3074457345618258791ul;
        for (var i = 0; i < input.Length; i++)
        {
            hashedValue += input[i];
            hashedValue *= 3074457345618258799ul;
        }

        return hashedValue.ToString();
        */

        using SHA256 sha256Hash = SHA256.Create();
        // ComputeHash - returns byte array
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert byte array to a string
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    // Verify a hash against a string.
    public static bool VerifyHash(string input, string hash)
    {
        // Hash the input.
        var hashOfInput = GetHash(input);

        // Create a StringComparer an compare the hashes.
        var comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(hashOfInput, hash) == 0;
    }

    public static string TextBorder(int borderwidth, string bordercolor)
    {
        return
            $"-webkit-text-stroke: {Px(0.3f * borderwidth)} {bordercolor}; text-shadow: 0 0 {2 * borderwidth}px {bordercolor}";
    }
    //string.Format("text-shadow: {0}px {0}px {0}px {1}, 0px {0}px {0}px {1}, -{0}px {0}px {0}px {1}, -{0}px 0px {0}px {1}, -{0}px -{0}px {0}px {1}, 0px -{0}px {0}px {1}, {0}px -{0}px {0}px {1}, {0}px 0px {0}px {1}, 0px 0px {0}px {1};", Round(0.5f * borderwidth), bordercolor);
    //$"-webkit-text-stroke: {Px(0.4f * borderwidth)} {bordercolor}";

    public static string Round(double x)
    {
        return Math.Round(x, 3).ToString(CultureInfo.InvariantCulture);
    }

    public static string RoundWithHalves(double x)
    {
        if (x == 0.5f)
            return "½";
        if (x == -0.5f)
            return "-½";
        if (x - 0.5f == (int)x)
            return "" + (int)x + "½";
        return Round(x);
    }

    public static string Px(double x)
    {
        return "" + Round(x) + "px";
    }
}