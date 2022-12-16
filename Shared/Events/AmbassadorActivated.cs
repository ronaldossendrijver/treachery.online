/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AmbassadorActivated : GameEvent
    {
        public AmbassadorActivated(Game game) : base(game)
        {
        }

        public AmbassadorActivated()
        {
        }

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

        public bool Passed { get; set; }

        public Faction BlueSelectedFaction { get; set; }

        public bool PinkOfferAlliance { get; set; }

        public bool PinkGiveVidalToAlly { get; set; }

        public bool PinkTakeVidal { get; set; }

        public override Message Validate()
        {
            if (Initiator != Faction.Pink) return Message.Express("Your faction can't use Ambassadors");
            if (PinkOfferAlliance && !AllianceCanBeOffered(Game, Player)) return Message.Express("You can't offer an alliance");
            if (PinkTakeVidal && !VidalCanBeTaken(Game, Player)) return Message.Express("You can't take ", Game.Vidal);

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

        public static bool VidalCanBeTaken(Game g, Player p) => g.VidalIsAlive && !g.VidalIsCapturedOrGhola;




    }

}
