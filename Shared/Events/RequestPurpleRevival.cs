/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class RequestPurpleRevival : GameEvent
    {
        #region Construction

        public RequestPurpleRevival(Game game) : base(game)
        {
        }

        public RequestPurpleRevival()
        {
        }

        #endregion Construction

        #region Properties

        public int _heroId;

        [JsonIgnore]
        public IHero Hero
        {
            get => LeaderManager.HeroLookup.Find(_heroId);
            set => _heroId = LeaderManager.HeroLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTargets(Game, Player).Contains(Hero)) return Message.Express(Hero, " can't be revived this way");

            return null;
        }

        public static IEnumerable<IHero> ValidTargets(Game g, Player p)
        {
            var purple = g.GetPlayer(Faction.Purple);
            var gholas = purple != null ? purple.Leaders.Where(l => l.Faction == p.Faction) : Array.Empty<Leader>();
            return g.KilledHeroes(p).Union(gholas);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var existingRequest = Game.CurrentRevivalRequests.FirstOrDefault(r => r.Hero == Hero);
            if (existingRequest != null)
            {
                Game.CurrentRevivalRequests.Remove(existingRequest);
            }

            Log();
            Game.CurrentRevivalRequests.Add(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " request ", Faction.Purple, " revival of a leader");
        }

        #endregion Execution
    }
}
