/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class CardTraded : GameEvent
{
    #region Construction

    public CardTraded(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public CardTraded()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target;

    public int _cardId;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Get(_cardId);
        set => _cardId = TreacheryCardManager.GetId(value);
    }

    public int _requestedCardId;

    [JsonIgnore]
    public TreacheryCard RequestedCard
    {
        get => TreacheryCardManager.Get(_requestedCardId);
        set => _requestedCardId = TreacheryCardManager.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");
        if (!Player.AlliedPlayer.TreacheryCards.Any()) Message.Express("Your ally does not have cards to trade");

        var targetPlayer = Game.GetPlayer(Target);
        if (targetPlayer == null) return Message.Express("Invalid target player");

        if (RequestedCard != null && !Game.IsBot(Player.AlliedPlayer)) return Message.Express("You can only select a card from a Bot ally");
        if (RequestedCard != null && !ValidCards(Game.GetPlayer(Target)).Contains(RequestedCard)) return Message.Express("Invalid requested card");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Player p)
    {
        return p.TreacheryCards;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (Game.CurrentCardTradeOffer == null)
        {
            Log();
            Game.CurrentCardTradeOffer = this;
            Game.PhaseBeforeCardTrade = Game.CurrentPhase;
            Game.Enter(Phase.TradingCards);
        }
        else
        {
            Log(Initiator, " and ", Game.CurrentCardTradeOffer.Initiator, " exchange a card");
            var otherPlayer = Game.CurrentCardTradeOffer.Player;

            if (otherPlayer.TreacheryCards.Count > 1 || Player.TreacheryCards.Count > 1)
                foreach (var p in Game.Players.Where(pl => pl != otherPlayer && pl != Player))
                {
                    Game.UnregisterKnown(p, otherPlayer.TreacheryCards);
                    Game.UnregisterKnown(p, Player.TreacheryCards);
                }

            otherPlayer.TreacheryCards.Add(Card);
            Player.TreacheryCards.Remove(Card);
            Player.TreacheryCards.Add(Game.CurrentCardTradeOffer.Card);
            otherPlayer.TreacheryCards.Remove(Game.CurrentCardTradeOffer.Card);
            Game.CurrentCardTradeOffer = null;
            Game.Stone(Milestone.CardTraded);
            Game.LastTurnCardWasTraded = Game.CurrentTurn;
            Game.Enter(Game.PhaseBeforeCardTrade);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " offer their ally a card trade");
    }

    #endregion Execution
}