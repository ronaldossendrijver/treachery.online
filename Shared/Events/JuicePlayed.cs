/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class JuicePlayed : GameEvent
{
    #region Construction

    public JuicePlayed(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public JuicePlayed()
    {
    }

    #endregion Construction

    #region Properties

    public JuiceType Type { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static IEnumerable<JuiceType> ValidTypes(Game g, Player p)
    {
        var result = new List<JuiceType>();

        if (g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p) && g.BattleWinner == Faction.None) result.Add(JuiceType.Aggressor);

        if ((g.CurrentMainPhase == MainPhase.Bidding && !g.Bids.Any()) ||
            g.CurrentPhase == Phase.BeginningOfShipAndMove ||
            (g.CurrentMainPhase == MainPhase.Battle && g.CurrentBattle == null) ||
            g.CurrentPhase == Phase.Contemplate)
            result.Add(JuiceType.GoFirst);

        result.Add(JuiceType.GoLast);

        return result;
    }

    public static bool CanBePlayedBy(Game g, Player player)
    {
        var applicablePhase =

            g.CurrentMainPhase == MainPhase.Bidding ||

            g.CurrentPhase == Phase.BeginningOfShipAndMove ||
            g.CurrentPhase == Phase.OrangeShip ||
            g.CurrentPhase == Phase.NonOrangeShip ||

            g.CurrentMainPhase == MainPhase.Battle ||

            g.CurrentPhase == Phase.Contemplate;

        return applicablePhase && player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Juice);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();

        var aggressorBeforeJuiceIsPlayed = Game.CurrentBattle?.AggressivePlayer;

        Game.CurrentJuice = this;
        Game.Discard(Player, TreacheryCardType.Juice);

        if ((Type == JuiceType.GoFirst || Type == JuiceType.GoLast) && Game.Version <= 117)
        {
            switch (Game.CurrentMainPhase)
            {
                case MainPhase.Bidding: Game.BidSequence.CheckCurrentPlayer(); break;
                case MainPhase.ShipmentAndMove: Game.ShipmentAndMoveSequence.CheckCurrentPlayer(); break;
                case MainPhase.Battle: Game.BattleSequence.CheckCurrentPlayer(); break;
            }
        }
        else if (Game.CurrentBattle != null && Type == JuiceType.Aggressor && Game.CurrentBattle.AggressivePlayer != aggressorBeforeJuiceIsPlayed)
        {
            (Game.DefenderPlan, Game.AggressorPlan) = (Game.AggressorPlan, Game.DefenderPlan);
            (Game.DefenderTraitorAction, Game.AggressorTraitorAction) = (Game.AggressorTraitorAction, Game.DefenderTraitorAction);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " use ", TreacheryCardType.Juice, " to ", Type);
    }

    #endregion Execution


}