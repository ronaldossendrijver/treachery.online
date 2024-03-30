/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BrownRemoveForce : GameEvent
    {
        #region Construction

        public BrownRemoveForce(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BrownRemoveForce()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target;

        public bool SpecialForce;

        public int _locationId;

        [JsonIgnore]
        public Location Location
        {
            get => Game.Map.LocationLookup.Find(_locationId);
            set => _locationId = Game.Map.LocationLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidLocations(Game).Contains(Location)) return Message.Express("Invalid location");
            if (!ValidFactions(Game, Player, Location).Contains(Target)) return Message.Express("Invalid faction");
            if (!ValidSpecialForceChoices(Game, Location, Target).Contains(SpecialForce)) return Message.Express("Invalid type of forces");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return g.ForcesOnPlanetExcludingEmptyLocations(false).Keys.Where(l => !g.IsInStorm(l)).Distinct();
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
            return p.Faction == Faction.Brown && (!g.Prevented(FactionAdvantage.BrownDiscarding) && CardToUse(p) != null || NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any());
        }

        public static TreacheryCard CardToUse(Player p)
        {
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_TRIPTOGAMONT);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            var card = CardToUse(Player);
            if (card == null && NexusPlayed.CanUseCunning(Player))
            {
                Game.PlayNexusCard(Player, "Cunning", "send a force back to reserves");
                Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
            }
            else
            {
                Game.Discard(card);
            }

            var target = GetPlayer(Target);

            if (SpecialForce)
            {
                target.SpecialForcesToReserves(Location, 1);
            }
            else
            {
                target.ForcesToReserves(Location, 1);
            }

            Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
            Game.Stone(Milestone.SpecialUselessPlayed);
        }

        public override Message GetMessage()
        {
            var targetPlayer = Game.GetPlayer(Target);
            return Message.Express(Initiator, " remove ", 1, SpecialForce ? targetPlayer.SpecialForce : targetPlayer.Force, " from ", Location);
        }

        #endregion Execution
    }
}
