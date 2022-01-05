/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaMonster : GameEvent
    {
        public int _territoryId;

        public KarmaMonster(Game game) : base(game)
        {
        }

        public KarmaMonster()
        {
        }

        [JsonIgnore]
        public Territory Territory { get { return Game.Map.TerritoryLookup.Find(_territoryId); } set { _territoryId = Game.Map.TerritoryLookup.GetId(value); } }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "Using {0}, {1} send {2} to {3}.", TreacheryCardType.Karma, Initiator, Concept.Monster, Territory);
        }

        public static IEnumerable<Territory> ValidTargets(Game g)
        {
            return g.Map.Territories.Where(t => !t.IsProtectedFromWorm);
        }
    }
}
