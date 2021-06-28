/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
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
            return new Message(Initiator, "{0} selects {1}.", InitiatorPlayerName, Faction);
        }

        public static IEnumerable<Faction> ValidFactions(Game g)
        {
            return g.FactionsInPlay.Where(f => !g.Players.Any(p => p.Faction == f));
        }

        [JsonIgnore]
        public override Player Player => Game.GetPlayer(InitiatorPlayerName);
    }
}
