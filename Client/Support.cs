/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Web;
using Treachery.Shared;
using System.Globalization;

namespace Treachery.Client
{
    public static class Support
    {
        public static string HTMLEncode(string toEncode)
        {
            return HttpUtility.HtmlEncode(toEncode);
        }

        public static string GetCardImage(TreacheryCardType type)
        {
            return Skin.Current.GetImageURL(
                TreacheryCardManager.GetCardsInAndOutsidePlay().FirstOrDefault(c => c.Type == type));
        }

        public static string GetCardTitle(TreacheryCardType t)
        {
            return string.Format("Use {0}?", Skin.Current.Describe(t));
        }

        public static string GetTreacheryCardHoverHTML(TreacheryCard c)
        {
            if (c == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div><img src='{0}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/></div><div class='bg-dark text-white text-center' style='width:300px'>{1}</div>", Skin.Current.GetImageURL(c), Skin.Current.GetTreacheryCardDescription(c));
            }
        }

        public static string GetTreacheryCardHoverHTMLSmall(TreacheryCard c)
        {
            if (c == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div><img src='{0}' width=200 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/></div><div class='bg-dark text-white text-center small' style='width:200px'>{1}</div>", Skin.Current.GetImageURL(c), Skin.Current.GetTreacheryCardDescription(c));
            }
        }

        public static string GetResourceCardHoverHTML(ResourceCard c)
        {
            if (c == null)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(c));
            }
        }

        public static string GetSkillCardHoverHTML(LeaderSkill c)
        {
            if (c == LeaderSkill.None)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(c));
            }
        }

        public static string GetResourceCardHoverHTMLSmall(ResourceCard c)
        {
            if (c == null)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=200 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(c));
            }
        }

        public static string GetHeroHoverHTML(IHero h) => GetHeroHoverHTML(h, LeaderSkill.None);

        public static string GetHeroHoverHTML(IHero h, Game g) => GetHeroHoverHTML(h, g.Skill(h));

        public static string GetHeroHoverHTML(IHero h, LeaderSkill skill)
        {
            if (h == null)
            {
                return "";
            }
            else
            {
                if (skill == LeaderSkill.None)
                {
                    return string.Format("<img src='{0}' width=200 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(h));
                }
                else
                {
                    return GetSkilledLeaderHTML(h as Leader, skill);
                }
            }
        }

        public static string GetLeaderHTML(Leader l)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<img class='img-fluid' src='{0}' width=80 style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(l));
            }
        }

        public static string GetSkilledLeaderHTML(Leader l, LeaderSkill s)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=200 style='position:absolute;left:200px;top:80px;filter:drop-shadow(-3px 3px 2px black);'/></div>", Skin.Current.GetImageURL(s), Skin.Current.GetImageURL(l));
            }
        }

        public static string GetStrongholdHTML(Location l)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=300 style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(l));
            }
        }

        public static string GetOwnedStrongholdHTML(Location l, Faction f)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=100 style='position:absolute;left:220px;top:40px;filter:drop-shadow(-3px 3px 2px black);'/></div>", Skin.Current.GetImageURL(l), Skin.Current.GetImageURL(f));
            }
        }

        public static string GetTechTokenHTML(TechToken tt)
        {
            if (tt == TechToken.None)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=200 style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(tt));
            }
        }

        public static string Color(Faction f)
        {
            return string.Format("background-color:{0}", Skin.Current.GetFactionColor(f));
        }

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

            /*
            using SHA256 sha256Hash = SHA256.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();*/

            UInt64 hashedValue = 3074457345618258791ul;
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

        public static Skin LoadSkin(string skinData)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var textReader = new StringReader(skinData);
            var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<Skin>(jsonReader);
        }

        public static string TextBorder(int borderwidth, string bordercolor) => TextBorder(borderwidth, borderwidth, bordercolor);

        public static string TextBorder(int borderwidth, int blur, string bordercolor) => string.Format("text-shadow: {0}px {0}px {1}px {2}, 0px {0}px {1}px {2}, -{0}px {0}px {1}px {2}, -{0}px 0px {1}px {2}, -{0}px -{0}px {1}px {2}, 0px -{0}px {1}px {2}, {0}px -{0}px {1}px {2}, {0}px 0px {1}px {2}, 0px 0px {1}px {2};", Round(0.5f * borderwidth), Round(0.5f * blur), bordercolor);

        public static string Round(double x) => Math.Round(x, 3).ToString(CultureInfo.InvariantCulture);
    }
}
