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

        public bool Cancel;

        public DealOffered(Game game) : base(game)
        {
        }

        public DealOffered()
        {
        }

        public override string Validate()
        {
            return "";
        }

        public override Message GetMessage()
        {
            if (!Cancel)
            {
                return new Message(Initiator, "{0} offer{1} for {2}: {3}",
                    Initiator,
                    TargetsToString(),
                    Price,
                    Deal.DealContentsDescription(Game, Type, Text, EndPhase, DealParameter1, DealParameter2));
            }
            else
            {
                return new Message(Initiator, "{0} cancel their offer", Initiator);
            }
        }

        public string GetDealDescription()
        {
            return Deal.DealContentsDescription(Game, Type, Text, EndPhase, DealParameter1, DealParameter2);
        }

        private string TargetsToString()
        {
            if (To.Length > 0)
            {
                return string.Format(" to {0}", Skin.Current.Join(To));
            }
            else
            {
                return "";
            }
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
