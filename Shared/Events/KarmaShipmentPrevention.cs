/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class KarmaShipmentPrevention : GameEvent
{
    #region Construction

    public KarmaShipmentPrevention(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaShipmentPrevention()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Game.Version >= 138 && !GetValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

        return null;
    }

    public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
    {
        return g.PlayersOtherThan(p).Where(f => f != Faction.Yellow);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.CurrentKarmaShipmentPrevention = this;
        Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
        Player.SpecialKarmaPowerUsed = true;
        Log();
        Game.Stone(Milestone.Karma);
    }

    public override Message GetMessage()
    {
        return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent shipment by ", Target);
    }

    #endregion Execution
}