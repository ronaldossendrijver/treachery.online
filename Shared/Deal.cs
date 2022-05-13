/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public T GetParameter1<T>(Game g)
        {
            return GetParameter1<T>(g, Type, DealParameter1);
        }

        public static T GetParameter1<T>(Game g, DealType Type, string Parameter)
        {
            return Type switch
            {
                DealType.DontShipOrMoveTo => (T)(object)g.Map.TerritoryLookup.Find(int.Parse(Parameter)),
                _ => default,
            };
        }

        public static T GetParameter2<T>()
        {
            return default;
        }

        public static Message DealContentsDescription(Game g, DealType Type, string Text, int benefit, Phase End, string Parameter1)
        {
            if (Text != null && Text.Length > 0)
            {
                return Message.Express(
                    MessagePart.ExpressIf(benefit > 0, "Receive ", new Payment(benefit), " and "),
                    Text,
                    " until ",
                    End);
            }
            else
            {
                return Message.Express(
                    MessagePart.ExpressIf(benefit > 0, "Receive ", new Payment(benefit), " and "),
                    Express(Type, GetParameter1<object>(g, Type, Parameter1)),
                    " until ",
                    End);
            }
        }

        public Message DealContentsDescription(Game g)
        {
            return DealContentsDescription(g, Type, Text, Benefit, End, DealParameter1);
        }

        public Message GetMessage(Game g)
        {
            var description = DealContentsDescription(g, Type, Text, Benefit, End, DealParameter1);
            return Message.Express(BoundFaction, " ⇔ ", ConsumingFaction, ": ", description);
        }

        private static MessagePart Express(DealType d, object parameter1)
        {
            return d switch
            {
                DealType.None => MessagePart.Express("Custom deal"),
                DealType.DontShipOrMoveTo => MessagePart.Express("Don't ship or move to ", parameter1),
                DealType.ShareBiddingPrescience => MessagePart.Express("Share treachery card prescience"),
                DealType.ShareResourceDeckPrescience => MessagePart.Express("Share prescience of the top ", Concept.Resource, "card"),
                DealType.ShareStormPrescience => MessagePart.Express("Share storm prescience"),
                DealType.ForfeitBattle => MessagePart.Express("Forfeit this battle (no weapons and defenses, lowest leader, zero dial)"),
                _ => MessagePart.Express("unknown deal type"),
            };
        }
    }

    public enum DealType
    {
        None = 0,
        DontShipOrMoveTo = 10,
        ShareBiddingPrescience = 30,
        ShareResourceDeckPrescience = 50,
        ShareStormPrescience = 60,
        ForfeitBattle = 70,
        TellDiscardedTraitors = 80
    }
}
