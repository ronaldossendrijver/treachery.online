/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;

namespace Treachery.Shared;

public class Caravan : PlacementEvent
{
    public Caravan(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Caravan()
    {
    }

    public bool AsAdvisors { get; set; }

    public override Message Validate()
    {
        return ValidateMove(AsAdvisors);
    }

    protected override void ExecuteConcreteEvent()
    {
        Game.RecentMoves.Add(this);

        Game.StormLossesToTake.Clear();
        var initiator = GetPlayer(Initiator);
        var card = initiator.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Caravan);

        Game.Discard(initiator, TreacheryCardType.Caravan);
        Game.PerformMoveFromLocations(initiator, ForceLocations, this, Initiator != Faction.Blue || AsAdvisors, true);

        if (Game.ContainsConflictingAlly(initiator, To)) Game.ChosenDestinationsWithAllies.Add(To.Territory);

        Game.CurrentFlightUsed = null;
        Game.CurrentFlightDiscoveryUsed = null;

        if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.YellowExtraMove);
        if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreyCyborgExtraMove);

        if (Game.CheckIntrusion(this))
        {
            Game.PhaseBeforeCaravanCausedIntrusion = Game.CurrentPhase;
            Game.Enter(Game.LastBlueIntrusion != null, Phase.BlueIntrudedByCaravan, Game.LastTerrorTrigger != null, Phase.TerrorTriggeredByCaravan, Phase.AmbassadorTriggeredByCaravan);
        }
    }

    public override Message GetMessage()
    {
        if (Passed)
            return Message.Express(Initiator, " pass ", TreacheryCardType.Caravan);
        return Message.Express(Initiator, " move to ", To, " by ", TreacheryCardType.Caravan);
    }
}