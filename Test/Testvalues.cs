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
using Treachery.Shared;

namespace Treachery.Test;

public class Testvalues
{
    public Phase currentPhase;
    public int forcesinArrakeen;
    public int forcesinCarthag;
    public int forcesinTabr;
    public int forcesinHabbanya;
    public int forcesinTuek;
    public int nrofplayers;
    public Faction[] winners;
    public TestvaluesPerPlayer[] playervalues;

    public static string Difference { get; private set; } = "";

    public override bool Equals(object obj)
    {
        if (obj is not Testvalues) return false;

        var other = obj as Testvalues;

        if (!
                Check(other.currentPhase, currentPhase, "currentPhase") &&
            Check(other.forcesinArrakeen, forcesinArrakeen, "forcesinArrakeen") &&
            Check(other.forcesinCarthag, forcesinCarthag, "forcesinCarthag") &&
            Check(other.forcesinTabr, forcesinTabr, "forcesinTabr") &&
            Check(other.forcesinHabbanya, forcesinHabbanya, "forcesinHabbanya") &&
            Check(other.forcesinTuek, forcesinTuek, "forcesinTuek") &&
            Check(other.nrofplayers, nrofplayers, "nrofplayers") &&
            CheckEnum(other.winners, winners, "winners") &&
            Check(other.playervalues.Length, playervalues.Length, "playervalues.Count"))
            return false;

        for (var i = 0; i < playervalues.Length; i++)
            if (!other.playervalues[i].Equals(playervalues[i])) return false;

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool Check<T>(T a, T b, string compared)
    {
        if (a == null)
        {
            if (b == null) return true;

            Difference = compared + ": " + a + " (actual) != " + b + " (expected)";
            return false;
        }

        if (a.Equals(b))
        {
            return true;
        }

        Difference = compared + ": " + a + " (actual) != " + b + " (expected)";
        return false;
    }

    public static bool CheckEnum<T>(IEnumerable<T> a, IEnumerable<T> b, string compared)
    {
        if (a == null)
        {
            if (b == null) return true;

            Difference = compared + ": " + a + " (actual) != " + b + " (expected)";
            return false;
        }

        if (a.SequenceEqual(b))
        {
            return true;
        }

        Difference = compared + ": " + a + " (actual) != " + b + " (expected)";
        return false;
    }

    public override string ToString()
    {
        return "Testvalues";
    }
}

public class TestvaluesPerPlayer
{
    public Faction faction;
    public Faction ally;
    public int position;
    public int resources;
    public int bribes;
    public int forcesinreserve;
    public int specialforcesinreserve;
    public int totaldeathcount;
    public int cardcount;
    public int cardtypes;
    public int traitors;
    public int facedancers;
    public int totalforcesonplanet;
    public int totalspecialforcesonplanet;
    public int forceskilled;
    public int specialforceskilled;
    public int nroftechtokens;

    public override bool Equals(object obj)
    {
        if (obj is not TestvaluesPerPlayer) return false;

        var other = obj as TestvaluesPerPlayer;

        return
            Testvalues.Check(other.ally, ally, faction + "ally") &&
            Testvalues.Check(other.bribes, bribes, faction + "bribes") &&
            Testvalues.Check(other.cardcount, cardcount, faction + "cardcount") &&
            Testvalues.Check(other.cardtypes, cardtypes, faction + "cardtypes") &&
            Testvalues.Check(other.facedancers, facedancers, faction + "facedancers") &&
            Testvalues.Check(other.faction, faction, faction + "faction") &&
            Testvalues.Check(other.forcesinreserve, forcesinreserve, faction + "forcesinreserve") &&
            Testvalues.Check(other.forceskilled, forceskilled, faction + "forceskilled") &&
            Testvalues.Check(other.nroftechtokens, nroftechtokens, faction + "nroftechtokens") &&
            Testvalues.Check(other.position, position, faction + "position") &&
            Testvalues.Check(other.resources, resources, faction + "resources") &&
            Testvalues.Check(other.specialforcesinreserve, specialforcesinreserve, faction + "specialforcesinreserve") &&
            Testvalues.Check(other.specialforceskilled, specialforceskilled, faction + "specialforceskilled") &&
            Testvalues.Check(other.totaldeathcount, totaldeathcount, faction + "totaldeathcount") &&
            Testvalues.Check(other.totalforcesonplanet, totalforcesonplanet, faction + "totalforcesonplanet") &&
            Testvalues.Check(other.totalspecialforcesonplanet, totalspecialforcesonplanet, faction + "totalspecialforcesonplanet") &&
            Testvalues.Check(other.traitors, traitors, faction + "traitors");
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


}