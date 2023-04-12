/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TerrorRevealed : PassableGameEvent, ILocationEvent
    {
        public TerrorRevealed(Game game) : base(game)
        {
        }

        public TerrorRevealed()
        {
        }

        public bool AllianceOffered;

        public TerrorType Type { get; set; }

        public int ForcesInSneakAttack { get; set; }

        public int _sneakAttackTo;

        [JsonIgnore]
        public Location SneakAttackTo { get { return Game.Map.LocationLookup.Find(_sneakAttackTo); } set { _sneakAttackTo = Game.Map.LocationLookup.GetId(value); } }

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

        [JsonIgnore]
        public Location To => SneakAttackTo;

        public override Message Validate()
        {
            if (Passed && !MayPass(Game)) return Message.Express("You must reveal a terror token");
            if (AllianceOffered && !MayOfferAlliance(Game)) return Message.Express("You can't offer an alliance to this faction");

            if (Passed || AllianceOffered) return null;
            if (!GetTypes(Game).Contains(Type)) return Message.Express("You cannot use this token");
            if (Initiator != Faction.Cyan) return Message.Express("Your faction can't reveal terror tokens");

            if (Type == TerrorType.SneakAttack && SneakAttackTo == null && ForcesInSneakAttack > 0) return Message.Express("You cannot send forces there");
            if (Type == TerrorType.SneakAttack && SneakAttackTo != null && !ValidSneakAttackTargets(Game, Player).Contains(SneakAttackTo)) return Message.Express("Invalid location of sneak attack");
            if (Type == TerrorType.SneakAttack && ForcesInSneakAttack > MaxAmountOfForcesInSneakAttack(Player)) return Message.Express("Too many forces selected");


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
                return Message.Express(Initiator, " offer an alliance instead of terror");
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

        public static bool MayOfferAlliance(Game g)
        {
            var victim = g.GetPlayer(GetVictim(g));
            var cyan = g.GetPlayer(Faction.Cyan);

			return !g.AllianceByTerrorWasOffered && !g.Prevented(FactionAdvantage.CyanEnemyOfEnemy) && !victim.Is(Faction.Pink) && !victim.ForcesInLocations.Keys.Any(l => cyan.AnyForcesIn(l.Territory) > 0);
        }

        public static Territory GetTerritory(Game g) => g.LastTerrorTrigger?.Territory;

        public static Faction GetVictim(Game g) => g.LastTerrorTrigger != null ? g.LastTerrorTrigger.Initiator : Faction.None;

        public static IEnumerable<TerrorType> GetTypes(Game g) => g.LastTerrorTrigger != null ? g.TerrorIn(GetTerritory(g)) : Array.Empty<TerrorType>();

        public static int MaxAmountOfForcesInSneakAttack(Player p) => Math.Min(5, p.ForcesInReserve);

        public static IEnumerable<Location> ValidSneakAttackTargets(Game g, Player p) => GetTerritory(g).Locations.Where(l => OpenDespiteAllyAndStormAndOccupancy(g, p, l));

        private static bool OpenDespiteAllyAndStormAndOccupancy(Game g, Player p, Location l) =>
            g.IsNotFull(p, l) &&
            l.Sector != g.SectorInStorm &&
            (!p.HasAlly || p.AlliedPlayer.AnyForcesIn(l.Territory) == 0 || p.Ally == Faction.Blue && g.Applicable(Rule.AdvisorsDontConflictWithAlly) && p.AlliedPlayer.ForcesIn(l.Territory) == 0);

        [JsonIgnore]
        public int TotalAmountOfForcesAddedToLocation => ForcesInSneakAttack;
    }
}
