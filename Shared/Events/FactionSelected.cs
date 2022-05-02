/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FactionSelected : GameEvent
    {
        public FactionSelected(Game game) : base(game)
        {
        }

        public FactionSelected()
        {
        }

        public string InitiatorPlayerName { get; set; }

        public Faction Faction { get; set; }

        public override Message Validate()
        {
            if (Faction != Faction.None && !Game.FactionsInPlay.Contains(Faction)) return "Faction not available";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(InitiatorPlayerName, " plays ", Faction);
        }

        public static IEnumerable<Faction> ValidFactions(Game g)
        {
            return g.FactionsInPlay.Where(f => !g.Players.Any(p => p.Faction == f));
        }

        [JsonIgnore]
        public override Player Player => Game.GetPlayer(InitiatorPlayerName);
    }
}
