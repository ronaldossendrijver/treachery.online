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

public class DistransUsed : GameEvent
{
    #region Construction

    public DistransUsed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DistransUsed()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    public int _cardId = -1;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Lookup.Find(_cardId);
        set => _cardId = TreacheryCardManager.Lookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidCards(Player).Contains(Card)) return Message.Express("Invalid card");
        if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Player p)
    {
        return p.TreacheryCards.Where(c => c.Type != TreacheryCardType.Distrans);
    }

    public static IEnumerable<Faction> ValidTargets(Game g, Player p)
    {
        return g.Players.Where(target => target != p && target.HasRoomForCards).Select(target => target.Faction);
    }

    public static bool CanBePlayed(Game game, Player player)
    {
        return player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Distrans) && ValidCards(player).Any() && ValidTargets(game, player).Any();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var target = GetPlayer(Target);

        var targetHadRoomForCards = target.HasRoomForCards;

        Game.Discard(Player, TreacheryCardType.Distrans);

        Player.TreacheryCards.Remove(Card);
        Game.RegisterKnown(Player, Card);
        target.TreacheryCards.Add(Card);

        if (Player.TreacheryCards.Any())
            foreach (var p in Game.Players.Where(p => !p.Is(Initiator) && p != target))
            {
                Game.UnregisterKnown(p, Player.TreacheryCards);
                Game.UnregisterKnown(p, target.TreacheryCards);
            }

        Log();

        CheckIfBiddingForPlayerShouldBeSkipped(target, targetHadRoomForCards);
    }

    private void CheckIfBiddingForPlayerShouldBeSkipped(Player player, bool hadRoomForCards)
    {
        if (Game.CurrentPhase == Phase.BlackMarketBidding && hadRoomForCards && !player.HasRoomForCards && BlackMarketBid.MayBePlayed(Game, player))
            new Bid(Game, player.Faction) { Passed = true }.Execute(false, false);
        else if (Game.CurrentPhase == Phase.Bidding && hadRoomForCards && !player.HasRoomForCards && Bid.MayBePlayed(Game, player)) new Bid(Game, player.Faction) { Passed = true }.Execute(false, false);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use ", TreacheryCardType.Distrans, " to give a card to ", Target);
    }

    #endregion Execution
}