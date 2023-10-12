/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TerrorPlanted : PassableGameEvent
    {
        #region Construction

        public TerrorPlanted(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public TerrorPlanted()
        {
        }

        #endregion Construction

        #region Properties

        public TerrorType Type { get; set; }

        public int _territoryId;

        [JsonIgnore]
        public Territory Stronghold
        {
            get => Game.Map.TerritoryLookup.Find(_territoryId);
            set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Stronghold == null)
            {
                if (!Game.TerrorOnPlanet.ContainsKey(Type)) return Message.Express("You can't remove a token that is not on the board");
            }
            else
            {
                if (!ValidStrongholds(Game, Player).Contains(Stronghold)) return Message.Express("Invalid Stronghold");
                if (!ValidTerrorTypes(Game, false).Contains(Type)) return Message.Express("Token not available");
            }

            return null;
        }

        public static IEnumerable<Territory> ValidStrongholds(Game g, Player p)
        {
            var ally = g.GetPlayer(p.Ally);

            return g.Map.Territories(false).Where(t =>
                t.IsVisible &&
                t.IsStronghold &&
                t != g.Map.HiddenMobileStronghold.Territory &&
                (!g.TerrorIn(t).Any() || MayPlaceAtExistingToken(p)));
        }

        public static bool MayRemoveTokens(Game g, Player p) => p.HasHighThreshold(World.Cyan) && g.TerrorOnPlanet.Any();

        public static bool MayPlaceAtExistingToken(Player p) => p.HasHighThreshold(World.Cyan) || p.Nexus == Faction.Cyan && NexusPlayed.CanUseCunning(p);

        public static IEnumerable<TerrorType> ValidTerrorTypes(Game g, bool toRemove)
        {
            if (toRemove)
            {
                return g.TerrorOnPlanet.Keys;
            }
            else
            {
                return g.UnplacedTerrorTokens.Union(g.TerrorOnPlanet.Keys);
            }
        }

        public static bool IsApplicable(Game g, Player p) =>
            g.CurrentPhase == Phase.Contemplate &&
            !g.CyanHasPlantedTerror &&
            !g.Prevented(FactionAdvantage.CyanPlantingTerror) &&
            ValidTerrorTypes(g, false).Any() &&
            ValidStrongholds(g, p).Any();

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log(GetVerboseMessage());

            if (!Passed)
            {
                Game.Stone(Milestone.TerrorPlanted);
                Game.CyanHasPlantedTerror = true;

                if (Stronghold == null)
                {
                    Game.TerrorOnPlanet.Remove(Type);
                    Game.UnplacedTerrorTokens.Add(Type);
                    Player.Resources += 4;
                }
                else
                {
                    if (Game.TerrorIn(Stronghold).Any() && !Player.HasHighThreshold(World.Cyan))
                    {
                        Game.PlayNexusCard(Player, "Cunning", "to plant additional Terror in ", Stronghold);
                    }

                    if (Game.UnplacedTerrorTokens.Contains(Type))
                    {
                        Game.TerrorOnPlanet.Add(Type, Stronghold);
                        Game.UnplacedTerrorTokens.Remove(Type);
                    }
                    else
                    {
                        Game.TerrorOnPlanet.Remove(Type);
                        Game.TerrorOnPlanet.Add(Type, Stronghold);
                    }
                }
            }
        }

        private Message GetVerboseMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't plant terror");
            }
            else if (Stronghold != null)
            {
                if (Game.TerrorOnPlanet.TryGetValue(Type, out var territory))
                {
                    return Message.Express(Initiator, " move terror from ", territory, " to ", Stronghold);
                }
                else
                {
                    return Message.Express(Initiator, " plant terror in ", Stronghold);
                }
            }
            else
            {
                return Message.Express(Initiator, " remove a terror token to get ", Payment.Of(4));
            }
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't plant terror");
            }
            else if (Stronghold != null)
            {
                return Message.Express(Initiator, " plant terror in ", Stronghold);
            }
            else
            {
                return Message.Express(Initiator, " remove a terror token to get ", Payment.Of(4));
            }
        }

        #endregion Execution
    }
}
