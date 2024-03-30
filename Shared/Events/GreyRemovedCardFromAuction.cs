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

public class GreyRemovedCardFromAuction : GameEvent
{
    #region Construction

    public GreyRemovedCardFromAuction(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public GreyRemovedCardFromAuction()
    {
    }

    #endregion Construction

    #region Properties

    public bool PutOnTop { get; set; }

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
        Game.CardsOnAuction.Items.Remove(Card);

        if (PutOnTop)
            Game.TreacheryDeck.PutOnTop(Card);
        else
            Game.TreacheryDeck.PutOnBottom(Card);

        Game.RegisterKnown(Faction.Grey, Card);
        Game.CardsOnAuction.Shuffle();
        Game.Stone(Milestone.Shuffled);
        Log();

        if (Game.GreyMaySwapCardOnBid)
        {
            if (Game.Version < 113 || !Game.Prevented(FactionAdvantage.GreySwappingCard))
            {
                Game.Enter(Phase.GreySwappingCard);
            }
            else
            {
                Game.LogPreventionByKarma(FactionAdvantage.GreySwappingCard);
                if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreySwappingCard);
                Game.StartBiddingRound();
            }
        }
        else
        {
            Game.StartBiddingRound();
        }
    }

    public override Message GetMessage()
    {
        if (PutOnTop)
            return Message.Express(Initiator, " put a card on top of the Treachery Card deck");
        return Message.Express(Initiator, " put a card at the bottom of the Treachery Card deck");
    }

    #endregion Execution
}