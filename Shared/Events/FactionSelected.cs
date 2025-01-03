/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class FactionSelected : GameEvent
{
    #region Construction

    public FactionSelected(Game game, int seatId) : base(game, seatId)
    {
    }

    public FactionSelected()
    {
    }

    #endregion Construction

    #region Properties

    public string InitiatorPlayerName { get; set; }
    
    public int Seat { get; set; }

    public Faction Faction { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Faction != Faction.None && !ValidFactions(Game).Contains(Faction)) return Message.Express("Faction not available");

        return null;
    }

    public static IEnumerable<Faction> ValidFactions(Game g)
    {
        return g.FactionsInPlay.Where(f => !g.Players.Any(p => p.Faction == f));
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var initiator = Game.Version < 170 ? 
            Game.Players.FirstOrDefault(p => p.Name == InitiatorPlayerName) :
            Game.GetPlayerBySeat(Seat);
        
        if (initiator != null && Game.FactionsInPlay.Contains(Faction))
        {
            initiator.Faction = Faction;
            Game.FactionsInPlay.Remove(Faction);
            Log();
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(InitiatorPlayerName, " plays ", Faction);
    }

    #endregion Execution
}