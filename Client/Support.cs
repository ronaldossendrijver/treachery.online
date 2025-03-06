/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

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
        using SHA256 sha256Hash = SHA256.Create();
        // ComputeHash - returns byte array
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert byte array to a string
        StringBuilder builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }
        return builder.ToString();
    }

    public static string TextBorder(int borderWidth, string borderColor)
    {
        return
            $"-webkit-text-stroke: {Px(0.3f * borderWidth)} {borderColor}; text-shadow: 0 0 {2 * borderWidth}px {borderColor}";
    }

    public static string Round(double x)
    {
        return Math.Round(x, 3).ToString(CultureInfo.InvariantCulture);
    }

    public static string RoundWithHalves(double x)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
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