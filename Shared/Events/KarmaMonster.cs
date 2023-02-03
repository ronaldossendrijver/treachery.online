/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " send ", Concept.Monster, " to ", Territory);
        }

        public static IEnumerable<Territory> ValidTargets(Game g)
        {
            return g.Map.Territories().Where(t => !t.IsProtectedFromWorm);
        }
    }
}
