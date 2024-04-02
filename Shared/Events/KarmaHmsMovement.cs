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

public class KarmaHmsMovement : GameEvent
{
    #region Construction

    public KarmaHmsMovement(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaHmsMovement()
    {
    }

    #endregion Construction

    #region Properties

    public bool Passed;

    public int _targetId;

    [JsonIgnore]
    public Location Target
    {
        get => Game.Map.LocationLookup.Find(_targetId);
        set => _targetId = Game.Map.LocationLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

        return null;
    }

    public static IEnumerable<Location> ValidLocations(Game g)
    {
        return Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm).Where(l => !l.IsStronghold);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var collectionRate = Player.AnyForcesIn(Game.Map.HiddenMobileStronghold) * 2;
        Log();

        if (!Player.SpecialKarmaPowerUsed)
        {
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Player.SpecialKarmaPowerUsed = true;
        }

        var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;
        Game.CollectSpiceFrom(Initiator, currentLocation, collectionRate);

        if (!Passed)
        {
            Game.Map.HiddenMobileStronghold.PointAt(Game, Target);
            Game.CollectSpiceFrom(Initiator, Target, collectionRate);
            Game.KarmaHmsMovesLeft--;
            Game.Stone(Milestone.HmsMovement);
        }

        if (Passed) Game.KarmaHmsMovesLeft = 0;
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " move the ", Game.Map.HiddenMobileStronghold, " to ", Target);
        return Message.Express(Initiator, " pass (further) movement of the ", Game.Map.HiddenMobileStronghold, " using ", TreacheryCardType.Karma);
    }

    #endregion Execution
}