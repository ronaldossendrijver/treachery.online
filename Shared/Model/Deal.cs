/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

namespace Treachery.Shared
{
    public class Deal
    {
        public Faction BoundFaction { get; set; }

        public Faction ConsumingFaction { get; set; }

        public DealType Type { get; set; }

        public string DealParameter1 { get; set; }

        public string DealParameter2 { get; set; }

        public string Text { get; set; }

        public int Benefit { get; set; }

        public Phase End { get; set; }

        public T GetParameter1<T>(Game g) => GetParameter1<T>(g, Type, DealParameter1);

        public static T GetParameter1<T>(Game g, DealType Type, string Parameter)
        {
            return Type switch
            {
                DealType.DontShipOrMoveTo => (T)(object)g.Map.TerritoryLookup.Find(int.Parse(Parameter)),
                _ => default,
            };
        }

        public Message DealContentsDescription(Game g) => DealContentsDescription(g, Type, Text, Benefit, End, DealParameter1);

        public static Message DealContentsDescription(Game g, DealType Type, string Text, int benefit, Phase End, string Parameter1)
        {
            if (Text != null && Text.Length > 0)
            {
                return Message.Express(
                    MessagePart.ExpressIf(benefit > 0, "Receive ", Payment.Of(benefit), " and "),
                    Text,
                    " until ",
                    End);
            }
            else
            {
                return Message.Express(
                    MessagePart.ExpressIf(benefit > 0, "Receive ", Payment.Of(benefit), " and "),
                    Express(Type, GetParameter1<object>(g, Type, Parameter1)),
                    " until ",
                    End);
            }
        }

        public static Message Express(DealType d, object parameter = null)
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
                _ => Message.Express("unknown deal type"),
            };
        }
    }
}
