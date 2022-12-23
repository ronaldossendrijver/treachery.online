/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TerrorRevealed : GameEvent
    {
        public TerrorRevealed(Game game) : base(game)
        {
        }

        public TerrorRevealed()
        {
        }

        public bool AllianceOffered;

        public bool Passed { get; set; }

        public TerrorType Type { get; set; }

        public int ForcesInSneakAttack { get; set; }

        public bool RobberyTakesCard { get; set; }

        public int _cardToGiveInSabotageId;
                
        [JsonIgnore]
        public TreacheryCard CardToGiveInSabotage
        {
            get
            {
                return TreacheryCardManager.Get(_cardToGiveInSabotageId);
            }
            set
            {
                _cardToGiveInSabotageId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (Passed && !MayPass(Game)) return Message.Express("You must reveal a terror token");
            if (AllianceOffered && !MayOfferAlliance(Game)) return Message.Express("You can't offer an alliance to this faction");

            if (Passed || AllianceOffered) return null;

            if (Initiator != Faction.Cyan) return Message.Express("Your faction can't reveal terror tokens");
            if (Type == TerrorType.SneakAttack && ForcesInSneakAttack > 0 && !MayPlaceForcesInSabotage(Game, Player)) return Message.Express("You can't send forces due to storm or occupancy");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (AllianceOffered)
            {
                return Message.Express(Initiator, " offer an alliance to ", Type);
            }
            else if (Passed)
            {
                return Message.Express(Initiator, " don't terrorize");
            }
            else
            {
                return Message.Express(Initiator, " reveal a terror token");
            }
        }

        public static bool MayPass(Game g) => !g.AllianceByTerrorWasOffered;

        public static bool MayOfferAlliance(Game g) => !g.AllianceByTerrorWasOffered && GetVictim(g) != Faction.Pink;

        public static Territory GetTerritory(Game g) => g.LastTerrorTrigger != null ? g.LastTerrorTrigger.Territory : g.LatestIntrusion.Territory;

        public static Faction GetVictim(Game g) => g.LastTerrorTrigger != null ? g.LastTerrorTrigger.Initiator : g.LatestIntrusion.Initiator;

        public static IEnumerable<TerrorType> GetTypes(Game g) => g.LastTerrorTrigger != null ? g.TerrorIn(GetTerritory(g)) : Array.Empty<TerrorType>();

        public static bool MayPlaceForcesInSabotage(Game g, Player p) => OpenDespiteAllyAndStormAndOccupancy(g, p, GetTerritory(g).MiddleLocation);

        public static int MaxAmountOfForcesInSneakAttack(Game g, Player p) => MayPlaceForcesInSabotage(g, p) ? Math.Min(5, p.ForcesInReserve) : 0;

        public static bool OpenDespiteAllyAndStormAndOccupancy(Game g, Player p, Location l) =>
            g.IsNotFull(p, l.Territory) &&
            l.Sector != g.SectorInStorm &&
            (!p.HasAlly || p.AlliedPlayer.AnyForcesIn(l.Territory) == 0 || p.Ally == Faction.Blue && g.Applicable(Rule.AdvisorsDontConflictWithAlly) && p.AlliedPlayer.ForcesIn(l.Territory) == 0);

    }
}
