/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class BluePrediction : GameEvent
{
    #region Construction

    public BluePrediction(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BluePrediction()
    {
    }

    #endregion Construction

    #region Properties

    public Faction ToWin { get; set; }

    public int Turn { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!Game.IsPlaying(ToWin)) return Message.Express("Invalid target");
        if (Turn < 1 || Turn > Game.MaximumNumberOfTurns) return Message.Express("Invalid turn");

        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g, Player p)
    {
        return g.PlayersOtherThan(p);
    }

    public static IEnumerable<int> ValidTurns(Game g)
    {
        return Enumerable.Range(1, g.MaximumNumberOfTurns);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Player.PredictedFaction = ToWin;
        Player.PredictedTurn = Turn;
        Log();
        Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.DealStartingTreacheryCards, Game.DealTraitors);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " predict who will win and when");
    }

    #endregion Execution
}