/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class KarmaHandSwap : GameEvent
{
    #region Construction

    public KarmaHandSwap(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaHandSwap()
    {
    }

    #endregion Construction

    #region Properties

    public string _cardIds;

    [JsonIgnore]
    public IEnumerable<TreacheryCard> ReturnedCards
    {
        get => IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
        set => _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (ReturnedCards.Count() != Game.KarmaHandSwapNumberOfCards) return Message.Express("Select ", Game.KarmaHandSwapNumberOfCards, " cards to return");

        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var victim = GetPlayer(Game.KarmaHandSwapTarget);

        foreach (var p in Game.Players.Where(p => p != Player && p != victim))
        {
            Game.UnregisterKnown(p, Player.TreacheryCards);
            Game.UnregisterKnown(p, victim.TreacheryCards);
        }

        foreach (var returned in ReturnedCards)
        {
            victim.TreacheryCards.Add(returned);
            Player.TreacheryCards.Remove(returned);
        }

        foreach (var returned in ReturnedCards) Game.RegisterKnown(Player, returned);

        Log();
        Game.Enter(Game.KarmaHandSwapPausedPhase, false);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " return ", ReturnedCards.Count(), " cards");
    }

    #endregion Execution
}