/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DealOffered : GameEvent
    {
        #region Construction

        public DealOffered(Game game) : base(game)
        {
        }

        public DealOffered()
        {
        }

        #endregion Construction

        #region Properties

        public Faction[] To { get; set; } = Array.Empty<Faction>();

        public DealType Type { get; set; }

        public string DealParameter1 { get; set; }

        public string DealParameter2 { get; set; }

        public string Text { get; set; }

        public Phase EndPhase { get; set; }

        public int Price { get; set; }

        public int Benefit { get; set; }

        public bool Cancel { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public override Message GetMessage()
        {
            if (!Cancel)
            {
                return Message.Express(Initiator, " offer ", MessagePart.ExpressIf(To != null && To.Any(), To, " "), "for ", Payment.Of(Price), ": ", Deal.DealContentsDescription(Game, Type, Text, Benefit, EndPhase, DealParameter1));
            }
            else
            {
                return Message.Express(Initiator, " withdraw a deal offer");
            }
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

        public static IEnumerable<DealType> GetStandardDealTypes(Game g, Player p)
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

        #endregion Validation

        #region Execution

        public Message GetDealDescription()
        {
            return Deal.DealContentsDescription(Game, Type, Text, Benefit, EndPhase, DealParameter1);
        }

        protected override void ExecuteConcreteEvent()
        {
            if (Cancel)
            {
                var sameOffer = Game.DealOffers.FirstOrDefault(o => o.Same(this));
                Game.DealOffers.Remove(sameOffer);
            }
            else
            {
                Game.DealOffers.Add(this);
            }
        }

        #endregion Execution

        
    }
}
