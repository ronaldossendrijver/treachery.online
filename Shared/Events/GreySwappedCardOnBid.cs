/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class GreySwappedCardOnBid : PassableGameEvent
{
    #region Construction

    public GreySwappedCardOnBid(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public GreySwappedCardOnBid()
    {
    }

    #endregion Construction

    #region Properties

    public int _cardId;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Get(_cardId);
        set => _cardId = TreacheryCardManager.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (!Passed)
        {
            Game.GreySwappedCardOnBid = true;
            Player.TreacheryCards.Remove(Card);
            Player.TreacheryCards.Add(Game.CardsOnAuction.Draw());

            foreach (var p in Game.Players.Where(p => !Game.HasBiddingPrescience(p))) Game.UnregisterKnown(p, Player.TreacheryCards);

            Game.CardsOnAuction.PutOnTop(Card);
            Game.RegisterKnown(Faction.Grey, Card);
            Game.Stone(Milestone.CardOnBidSwapped);
            Log();
        }

        if (!Game.BiddingRoundWasStarted)
            Game.StartBiddingRound();
        else
            Game.Enter(IsPlaying(Faction.Green), Phase.WaitingForNextBiddingRound, Game.PutNextCardOnAuction);
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express(Initiator, " swap a card from hand with the next card on bid");
        return Message.Express(Initiator, " don't swap a card from hand with the next card on bid");
    }

    #endregion Execution
}