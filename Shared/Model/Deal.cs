/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Deal
{
    public Faction BoundFaction { get; set; }

    public Faction ConsumingFaction { get; set; }

    public DealType Type { get; set; }

    public string? DealParameter1 { get; set; }

    public string? DealParameter2 { get; set; }

    public string? Text { get; set; }

    public int Benefit { get; set; }

    public Phase End { get; set; }

    public T? GetParameter1<T>(Game g)
    {
        return DealParameter1 is null 
            ? default 
            : GetParameter1<T>(g, Type, DealParameter1);
    }

    private static T? GetParameter1<T>(Game g, DealType type, string parameter)
    {
        return type switch
        {
            DealType.DontShipOrMoveTo => (T?)(object?)g.Map.TerritoryLookup.Find(int.Parse(parameter)),
            _ => default
        };
    }

    public Message DealContentsDescription(Game g)
    {
        return DealContentsDescription(g, Type, Text ?? string.Empty, Benefit, End, DealParameter1 ?? string.Empty);
    }

    public static Message DealContentsDescription(Game g, DealType type, string? text, int benefit, Phase end, string? parameter1)
    {
        if (!string.IsNullOrWhiteSpace(text))
            return Message.Express(
                MessagePart.ExpressIf(benefit > 0, "Receive ", Payment.Of(benefit), " and "),
                text,
                " until ",
                end);
        return Message.Express(
            MessagePart.ExpressIf(benefit > 0, "Receive ", Payment.Of(benefit), " and "),
            Express(type, GetParameter1<object>(g, type, parameter1 ?? "?")),
            " until ",
            end);
    }

    public static Message Express(DealType d, object? parameter = null)
    {
        var p = parameter ?? "...";

        return d switch
        {
            DealType.None => Message.Express("Custom deal"),
            DealType.DontShipOrMoveTo => Message.Express("Don't ship or move to ", p),
            DealType.ShareBiddingPrescience => Message.Express("Share treachery card prescience"),
            DealType.ShareResourceDeckPrescience => Message.Express("Share prescience of the top ", Concept.Resource, "card"),
            DealType.ShareStormPrescience => Message.Express("Share storm prescience"),
            DealType.ForfeitBattle => Message.Express("Forfeit this battle (no weapons and defenses, lowest leader, zero dial)"),
            _ => Message.Express("unknown deal type")
        };
    }
}