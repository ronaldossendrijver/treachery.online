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

        public static string DealContentsDescription(Game g, DealType Type, string Text, int benefit, Phase End, string Parameter1)
        {
            string description;
            if (Text != null && Text.Length > 0)
            {
                description = Text;
            }
            else
            {
                description = Skin.Current.Format("{0} until {1}",
                    string.Format(Skin.Current.Describe(Type), GetParameter1<object>(g, Type, Parameter1), GetParameter2<object>()),
                    End);
            }

            if (benefit > 0)
            {
                description = "Receive " + benefit + " and " + description;
            }

            return description;
        }

        public string DealContentsDescription(Game g)
        {
            return DealContentsDescription(g, Type, Text, Benefit, End, DealParameter1);
        }

        public string ToString(Game g)
        {
            var description = DealContentsDescription(g, Type, Text, Benefit, End, DealParameter1);
            return Skin.Current.Format("{0} ⇔ {1}: {2}", BoundFaction, ConsumingFaction, description);
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
