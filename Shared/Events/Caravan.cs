/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class Caravan : PlacementEvent
    {
        public Caravan(Game game) : base(game)
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

            if (Game.ContainsConflictingAlly(initiator, To))
            {
                Game.ChosenDestinationsWithAllies.Add(To.Territory);
            }

            Game.CurrentFlightUsed = null;
            Game.CurrentFlightDiscoveryUsed = null;

            if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.YellowExtraMove);
            if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreyCyborgExtraMove);

            if (Game.CheckIntrusion(this))
            {
                Game.PausedPhase = Game.CurrentPhase;
                Game.Enter(Game.LastBlueIntrusion != null, Phase.BlueIntrudedByCaravan, Game.LastTerrorTrigger != null, Phase.TerrorTriggeredByCaravan, Phase.AmbassadorTriggeredByCaravan);
            }
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " pass ", TreacheryCardType.Caravan);
            }
            else
            {
                return Message.Express(Initiator, " move to ", To, " by ", TreacheryCardType.Caravan);
            }
        }
    }
}
