/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DealOffered : GameEvent
    {
        public Faction[] To = Array.Empty<Faction>();

        public DealType Type { get; set; }

        public string DealParameter1 { get; set; }

        public string DealParameter2 { get; set; }

        public string Text { get; set; }

        public Phase EndPhase { get; set; }

        public int Price;

        public int Benefit;

        public bool Cancel;

        public DealOffered(Game game) : base(game)
        {
        }

        public DealOffered()
        {
        }

        public override Message Validate()
        {
            return "";
        }

        public override Message GetMessage()
        {
            if (!Cancel)
            {
                return Message.Express(Initiator, " offer ", MessagePart.ExpressIf(To.Any(), ToObjects(To)), " for ", new Payment(Price), ": ", Deal.DealContentsDescription(Game, Type, Text, Benefit, EndPhase, DealParameter1));
            }
            else
            {
                return Message.Express(Initiator, " withdraw a deal offer");
            }
        }

        private object[] ToObjects(IEnumerable<Faction> factions)
        {
            var result = new List<object>();
            foreach (var faction in factions)
            {
                result.Add(faction);
            }
            result.Add(" ");
            return result.ToArray();
        }

        public string GetDealDescription()
        {
            return Deal.DealContentsDescription(Game, Type, Text, Benefit, EndPhase, DealParameter1);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public DealOffered Cancellation()
        {
            var result = (DealOffered)MemberwiseClone();
            result.Cancel = true;
            return result;
        }

        public DealAccepted Acceptance(Faction by)
        {
            return new DealAccepted(Game)
            {
                Initiator = by,
                BoundFaction = Initiator,
                Price = Price,
                Type = Type,
                DealParameter1 = DealParameter1,
                DealParameter2 = DealParameter2,
                Text = Text,
                Benefit = Benefit,
                End = EndPhase
            };
        }

        public bool Same(DealOffered offeredDeal)
        {
            return
                offeredDeal.Price == Price &&
                offeredDeal.Type == Type &&
                offeredDeal.DealParameter1 == DealParameter1 &&
                offeredDeal.DealParameter2 == DealParameter2 &&
                offeredDeal.Text == Text &&
                offeredDeal.Benefit == Benefit &&
                offeredDeal.EndPhase == EndPhase &&
                offeredDeal.To.SequenceEqual(To);
        }

        public bool IsAcceptedBy(DealAccepted acceptedDeal)
        {
            return
                acceptedDeal.BoundFaction == Initiator &&
                acceptedDeal.Price == Price &&
                acceptedDeal.Type == Type &&
                acceptedDeal.DealParameter1 == DealParameter1 &&
                acceptedDeal.DealParameter2 == DealParameter2 &&
                acceptedDeal.Text == Text &&
                acceptedDeal.Benefit == Benefit &&
                acceptedDeal.End == EndPhase &&
                (To.Length == 0 || To.Contains(acceptedDeal.Initiator));
        }

        public static IEnumerable<DealType> GetAllDealTypes()
        {
            return Enumerations.GetValues<DealType>(typeof(DealType));
        }

        public static IEnumerable<DealType> GetHumanDealTypes(Game g, Player p)
        {
            var result = new List<DealType>() {

                DealType.None
            };

            switch (p.Faction)
            {
                case Faction.Green:
                    if (g.HasBiddingPrescience(p)) result.Add(DealType.ShareBiddingPrescience);
                    if (g.HasResourceDeckPrescience(p)) result.Add(DealType.ShareResourceDeckPrescience);
                    break;

                case Faction.Yellow:
                    if (g.HasStormPrescience(p)) result.Add(DealType.ShareStormPrescience);
                    break;
            }

            return result;
        }
    }
}
