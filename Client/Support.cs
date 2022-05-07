/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Treachery.Shared;

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

        public static string GetHoverHTML(string c)
        {
            return string.Format("<div style='background-color:white;border-color:black;border-width:1px;border-style:solid;color:black;'>{0}</div>", c);
        }

        public static string GetHoverHTML(TreacheryCard c)
        {
            if (c == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div style='filter:drop-shadow(-3px 3px 2px black);'><img src='{0}' width=300 class='img-fluid'/></div><div class='bg-dark text-white text-center' style='width:300px'>{1}</div>", Skin.Current.GetImageURL(c), Skin.Current.GetTreacheryCardDescription(c));
            }
        }

        public static string GetHoverHTML(ResourceCard c)
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

        public static string GetHoverHTML(LeaderSkill c)
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

        public static string GetHoverHTML(IHero h, Game g)
        {
            if (h == null)
            {
                return "";
            }
            else
            {
                var skill = g.Skill(h);

                if (skill == LeaderSkill.None)
                {
                    return string.Format("<img src='{0}' width=200 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(h));
                }
                else
                {
                    return GetHoverHTML(h as Leader, skill);
                }
            }
        }

        public static string GetHoverHTML(IHero h)
        {
            if (h == null)
            {
                return "";
            }
            else
            {
                return string.Format("<img src='{0}' width=200 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", Skin.Current.GetImageURL(h));
            }
        }

        public static string GetHoverHTML(Leader l, LeaderSkill s)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=140 style='position:absolute;left:180px;top:120px;filter:drop-shadow(-2px 2px 2px black);'/></div>", Skin.Current.GetImageURL(s), Skin.Current.GetImageURL(l));
            }
        }

        public static string GetHoverHTML(Location l)
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

        public static string GetHoverHTML(Location l, Faction f)
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

        public static string GetHoverHTML(TechToken tt)
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

        public static Skin LoadSkin(string skinData)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var textReader = new StringReader(skinData);
            var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<Skin>(jsonReader);
        }

        public static string TextBorder(int borderwidth, string bordercolor) => string.Format("text-shadow: {0}px {0}px {1}px {2}, 0px {0}px {1}px {2}, -{0}px {0}px {1}px {2}, -{0}px 0px {1}px {2}, -{0}px -{0}px {1}px {2}, 0px -{0}px {1}px {2}, {0}px -{0}px {1}px {2}, {0}px 0px {1}px {2}, 0px 0px {1}px {2};", Round(0.5f * borderwidth), Round(0.5f * borderwidth), bordercolor);

        public static string Round(double x) => Math.Round(x, 3).ToString(CultureInfo.InvariantCulture);

        public static string Px(double x) => "" + Round(x) + "px";
    }
}
