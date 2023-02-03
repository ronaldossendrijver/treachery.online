/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Treachery.Shared;

namespace Treachery.Client
{
    public static class Popup
    {
        public static string Get(string c)
        {
            return string.Format("<div style='background-color:white;border-color:black;border-width:1px;border-style:solid;color:black;'>{0}</div>", c);
        }

        public static string Get(ResourceCard c) => GetImageHoverHTML(Skin.Current.GetImageURL(c));

        public static string Get(TerrorType t) => Get(Skin.Current.Describe(t));

        public static string Get(LeaderSkill c) => GetImageHoverHTML(Skin.Current.GetImageURL(c));

        public static string Get(TechToken tt) => GetImageHoverHTML(Skin.Current.GetImageURL(tt));

        public static string GetNexusCard(Faction f) => GetImageHoverHTML(Skin.Current.GetNexusCardImageURL(f));

        public static string Get(Homeworld w, HomeworldStatus status)
        {
            return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><span style='color:{1};font-size:large;position:absolute;left:20px;top:20px;filter:drop-shadow(-1px 1px 1px black);'>{2}</span><img src='{3}' width=100 style='position:absolute;left:240px;top:120px;filter:drop-shadow(-3px 3px 2px black);'/></div>",
                        Skin.Current.GetHomeworldCardImageURL(w.World),
                        status.IsHigh ? "green" : "red",
                        status.IsHigh ? "High Threshold" : "Low Threshold",
                        Skin.Current.GetImageURL(status.Occupant));
        }

        public static string Get(TreacheryCard c)
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

        public static string Get(IHero h) => GetImageHoverHTML(Skin.Current.GetImageURL(h));

        public static string Get(IHero h, Game g)
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
                    if (h is Leader)
                    {
                        return GetImageHoverHTML(Skin.Current.GetImageURL(h));
                    }
                    else
                    {
                        return GetImageHoverHTML(Skin.Current.GetImageURL(h));
                    }
                }
                else
                {
                    return Get(h as Leader, skill);
                }
            }
        }

        public static string Get(Leader l, LeaderSkill s)
        {
            if (l == null)
            {
                return "";
            }
            else
            {
                return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=140 style='position:absolute;left:200px;top:120px;filter:drop-shadow(-2px 2px 2px black);'/></div>", Skin.Current.GetImageURL(s), Skin.Current.GetImageURL(l));
            }
        }

        public static string Get(Location l) => GetImageHoverHTML(Skin.Current.GetImageURL(l));

        public static string Get(Location l, Faction f)
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



        private static string GetImageHoverHTML(string imageURL) => string.Format("<img src='{0}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", imageURL);


    }
}
