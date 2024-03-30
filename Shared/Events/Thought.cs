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

public class Thought : GameEvent
{
    #region Construction

    public Thought(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Thought()
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
        if (!ValidCards(Game).Contains(Card)) return Message.Express("Invalid card");

        return null;
    }

    public static IEnumerable<TreacheryCard> ValidCards(Game g)
    {
        return TreacheryCardManager.GetCardsInPlay(g).Where(c => c.IsWeapon && c.Type != TreacheryCardType.Chemistry);
    }

    public static bool MayBeUsed(Game game, Player player)
    {
        return game.SkilledAs(player, LeaderSkill.Thinker) && game.CurrentBattle != null && game.CurrentThought == null && game.CurrentBattle.IsAggressorOrDefender(player);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.CurrentThought = this;
        var opponent = Game.CurrentBattle.OpponentOf(Initiator).Faction;
        Log(Initiator, " use their ", LeaderSkill.Thinker, " skill to ask ", opponent, " if they have a ", Card);
        Game.Stone(Milestone.Prescience);
        Game.Enter(Phase.Thought);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use their ", LeaderSkill.Thinker, " skill to ask a question");
    }

    #endregion Execution


}