/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownRemoveForce : GameEvent
    {
        public int _locationId;

        public Faction Target;

        public bool SpecialForce;

        public BrownRemoveForce(Game game) : base(game)
        {
        }

        public BrownRemoveForce()
        {
        }

        [JsonIgnore]
        public Location Location
        {
            get { return Game.Map.LocationLookup.Find(_locationId); }
            set { _locationId = Game.Map.LocationLookup.GetId(value); }
        }

        public override Message Validate()
        {
            if (!ValidLocations(Game).Contains(Location)) return Message.Express("Invalid location");
            if (!ValidFactions(Game, Player, Location).Contains(Target)) return Message.Express("Invalid faction");
            if (!ValidSpecialForceChoices(Game, Location, Target).Contains(SpecialForce)) return Message.Express("Invalid type of forces");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return g.ForcesOnPlanetExcludingEmptyLocations.Keys.Where(l => !g.IsInStorm(l)).Distinct();
        }

        public static IEnumerable<Faction> ValidFactions(Game g, Player p, Location l)
        {
            if (l != null)
            {
                return g.Players.Where(p => p.AnyForcesIn(l) > 0).Select(p => p.Faction);
            }
            else
            {
                return g.PlayersOtherThan(p);
            }
        }

        public static IEnumerable<bool> ValidSpecialForceChoices(Game g, Location l, Faction f)
        {
            var result = new List<bool>();
            var playerToCheck = g.GetPlayer(f);

            if (l == null || playerToCheck == null)
            {
                result.Add(false);
                result.Add(true);
            }
            else
            {
                if (playerToCheck.ForcesIn(l) > 0)
                {
                    result.Add(false);
                }

                if (playerToCheck.SpecialForcesIn(l) > 0)
                {
                    result.Add(true);
                }
            }

            return result;
        }

        public static bool CanBePlayedBy(Game g, Player p)
        {
            return p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null;
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_TRIPTOGAMONT);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            var targetPlayer = Game.GetPlayer(Target);
            return Message.Express(Initiator, " use a ", TreacheryCardType.Useless, " card to remove ", 1, SpecialForce ? (object)targetPlayer.SpecialForce : targetPlayer.Force, " from ", Location);
        }

        public TreacheryCard CardUsed() => CardToUse(Player);
    }
}
