/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class DealOffered : GameEvent
{
    #region Construction

    public DealOffered(Game game, Faction initiator) : base(game, initiator)
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

    public DealOffered Cancellation()
    {
        var result = (DealOffered)MemberwiseClone();
        result.Cancel = true;
        return result;
    }

    public DealAccepted Acceptance(Faction by)
    {
        return new DealAccepted(Game, by)
        {
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
        var result = new List<DealType>
        {

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

    public override Message GetMessage()
    {
        if (!Cancel)
            return Message.Express(Initiator, " offer ", MessagePart.ExpressIf(To != null && To.Any(), To, " "), "for ", Payment.Of(Price), ": ", Deal.DealContentsDescription(Game, Type, Text, Benefit, EndPhase, DealParameter1));
        return Message.Express(Initiator, " withdraw a deal offer");
    }

    public override Message GetShortMessage()
    {
        if (!Cancel)
            return Message.Express(Initiator, " offer a deal");
        return Message.Express(Initiator, " withdraw a deal offer");
    }

    #endregion Execution
}