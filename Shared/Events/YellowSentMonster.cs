/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class YellowSentMonster : GameEvent
    {
        public int _territoryId;

        public YellowSentMonster(Game game) : base(game)
        {
        }

        public YellowSentMonster()
        {
        }

        [JsonIgnore]
        public Territory Territory { get { return Game.Map.TerritoryLookup.Find(_territoryId); } set { _territoryId = Game.Map.TerritoryLookup.GetId(value); } }

        public override Message Validate()
        {
            if (Territory == null) return Message.Express("No territory selected");
            if (Territory.IsProtectedFromWorm) return Message.Express("Selected territory is protected");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " send ", Concept.Monster, " to ", Territory);
        }

        public static IEnumerable<Territory> ValidTargets(Game g)
        {
            return g.Map.Territories.Where(t => !t.IsProtectedFromWorm);
        }
    }
}
