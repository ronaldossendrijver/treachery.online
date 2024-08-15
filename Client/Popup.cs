/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Client;

public static class Popup
{
    public static string Get(string c)
    {
        return string.Format("<div style='background-color:white;border-color:black;border-width:1px;border-style:solid;color:black;padding:2px;'>{0}</div>", c);
    }

    public static string Get(ResourceCard c)
    {
        return GetImageHoverHTML(Skin.Current.GetImageURL(c));
    }

    public static string Get(TerrorType t)
    {
        return Get(Skin.Current.GetTerrorTypeDescription(t));
    }

    public static string Get(LeaderSkill c)
    {
        return GetImageHoverHTML(Skin.Current.GetImageURL(c));
    }

    public static string Get(TechToken tt)
    {
        return GetImageHoverHTML(Skin.Current.GetImageURL(tt));
    }

    public static string GetNexusCard(Nexus n)
    {
        return GetImageHoverHTML(Skin.Current.GetImageUrl(n));
    }

    public static string Get(Homeworld w)
    {
        return string.Format("<div style='position:relative'><img width=480 style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}'/></div>",
            Skin.Current.GetHomeworldCardImageURL(w.World));
    }

    public static string Get(TreacheryCard c)
    {
        if (c == null)
            return "";
        return string.Format("<div style='filter:drop-shadow(-3px 3px 2px black);'><img src='{0}' width=300 class='img-fluid'/></div><div class='bg-dark text-white text-center' style='width:300px'>{1}</div>", Skin.Current.GetImageURL(c), Skin.Current.GetTreacheryCardDescription(c));
    }

    public static string Get(IHero h)
    {
        return GetImageHoverHTML(Skin.Current.GetImageURL(h));
    }

    public static string Get(IHero h, Game g)
    {
        if (h == null) return "";

        var skill = g.Skill(h);

        if (skill == LeaderSkill.None)
        {
            if (h is Leader)
                return GetImageHoverHTML(Skin.Current.GetImageURL(h));
            return GetImageHoverHTML(Skin.Current.GetImageURL(h));
        }

        return Get(h as Leader, skill);
    }

    public static string Get(Leader l, LeaderSkill s)
    {
        if (l == null)
            return "";
        return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=140 style='position:absolute;left:200px;top:120px;filter:drop-shadow(-2px 2px 2px black);'/></div>", Skin.Current.GetImageURL(s), Skin.Current.GetImageURL(l));
    }

    public static string Get(StrongholdAdvantage adv)
    {
        return GetImageHoverHTML(Skin.Current.GetImageURL(adv));
    }

    public static string Get(StrongholdAdvantage adv, Faction f)
    {
        return string.Format("<div style='position:relative'><img style='position:relative;filter:drop-shadow(-3px 3px 2px black);' src='{0}' width=300/><img src='{1}' width=100 style='position:absolute;left:220px;top:40px;filter:drop-shadow(-3px 3px 2px black);'/></div>", Skin.Current.GetImageURL(adv), Skin.Current.GetImageURL(f));
    }

    private static string GetImageHoverHTML(string imageURL)
    {
        return string.Format(
            "<img src='{0}' width=300 class='img-fluid' style='filter:drop-shadow(-3px 3px 2px black);'/>", imageURL);
    }
}