/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AmbassadorActivated : GameEvent, ILocationEvent
    {
        public AmbassadorActivated(Game game) : base(game)
        {
        }

        public AmbassadorActivated()
        {
        }

        public bool Passed { get; set; }

        public Faction BlueSelectedFaction { get; set; }

        public string _brownCardIds;

        [JsonIgnore]
        public IEnumerable<TreacheryCard> BrownCards
        {
            get
            {
                return IdStringToObjects(_brownCardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _brownCardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public bool PinkOfferAlliance { get; set; }

        public bool PinkGiveVidalToAlly { get; set; }

        public bool PinkTakeVidal { get; set; }

        public int _yellowOrOrangeToId;

        [JsonIgnore]
        public Location YellowOrOrangeTo { get { return Game.Map.LocationLookup.Find(_yellowOrOrangeToId); } set { _yellowOrOrangeToId = Game.Map.LocationLookup.GetId(value); } }

        [JsonIgnore]
        public Location To => YellowOrOrangeTo;

        public string _yellowForceLocations = "";

        [JsonIgnore]
        public Dictionary<Location, Battalion> YellowForceLocations
        {
            get
            {
                return PlacementEvent.ParseForceLocations(Game, Player.Faction, _yellowForceLocations);
            }
            set
            {
                _yellowForceLocations = PlacementEvent.ForceLocationsString(Game, value);
            }
        }

        public override Message Validate()
        {
            if (!Passed)
            {
                var player = Player;
                var faction = GetFaction(Game);
                var victim = GetVictim(Game);
                var victimPlayer = Game.GetPlayer(victim);

                if (Initiator != Faction.Pink) return Message.Express("Your faction can't activate Ambassadors");

                switch (faction)
                {

                    case Faction.Blue:
                        if (!GetValidBlueFactions(Game).Contains(BlueSelectedFaction)) return Message.Express("Invalid Ambassador selected");
                        break;

                    case Faction.Brown:
                        if (BrownCards.Any(c => !GetValidBrownCards(player).Contains(c))) return Message.Express("Invalid card selected");
                        break;

                    case Faction.Pink:
                        if (PinkOfferAlliance && !AllianceCanBeOffered(Game, player)) return Message.Express("You can't offer an alliance");
                        if (PinkTakeVidal && !VidalCanBeTaken(Game)) return Message.Express("You can't take ", Game.Vidal);
                        if (PinkGiveVidalToAlly && !VidalCanBeGivenTo(Game, victimPlayer)) return Message.Express("You can't give ", Game.Vidal, " to ", victim);
                        break;

                    case Faction.Yellow:
                        if (YellowForceLocations.Any(kvp => Game.IsInStorm(kvp.Key))) return Message.Express("Can't move from storm");
                        if (YellowForceLocations.Any(kvp => player.ForcesIn(kvp.Key) < kvp.Value.AmountOfForces)) return Message.Express("Invalid amount of ", Player.Force);
                        if (YellowForceLocations.Any(kvp => player.SpecialForcesIn(kvp.Key) < kvp.Value.AmountOfSpecialForces)) return Message.Express("Invalid amount of ", Player.SpecialForce);
                        if (!ValidYellowTargets(Game, player, YellowForceLocations).Contains(YellowOrOrangeTo)) return Message.Express("Invalid target location");
                        break;
                }
            }

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, Passed ? " don't" : "", " activate an Ambassador");
        }

        public static Territory GetTerritory(Game g) => g.LastAmbassadorTrigger.To.Territory;

        public static Faction GetVictim(Game g) => g.LastAmbassadorTrigger.Initiator;

        public static Faction GetFaction(Game g) => g.AmbassadorIn(GetTerritory(g));

        public static bool AllianceCanBeOffered(Game g, Player p) => !p.HasAlly && !g.GetPlayer(GetVictim(g)).HasAlly;

        public static bool VidalCanBeTaken(Game g) => g.VidalIsAlive && !g.VidalIsCapturedOrGhola;

        public static bool VidalCanBeGivenTo(Game g, Player p) => true; // g.HasRoomForLeaders(p);

        public static bool VidalCanBeGivenTo(Game g, Faction f) => true; // g.HasRoomForLeaders(g.GetPlayer(f));

        public static IEnumerable<Faction> GetValidBlueFactions(Game g) => g.UnassignedAmbassadors.Items;

        public static IEnumerable<TreacheryCard> GetValidBrownCards(Player p) => p.TreacheryCards;


        public static IEnumerable<Territory> ValidYellowSources(Game g, Player p) => PlacementEvent.TerritoriesWithAnyForcesNotInStorm(g, p);

        public static IEnumerable<Location> ValidYellowTargets(Game g, Player p, Dictionary<Location, Battalion> forces)
        {

            if (forces.Sum(kvp => kvp.Value.TotalAmountOfForces) > 0)
            {
                return PlacementEvent.ValidTargets(g, p, forces);
            }
            else
            {
                return Array.Empty<Location>();
            }
        }
    }

}
