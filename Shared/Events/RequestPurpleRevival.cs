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
        public int _heroId;

        public RequestPurpleRevival(Game game) : base(game)
        {
        }

        public RequestPurpleRevival()
        {
        }

        [JsonIgnore]
        public IHero Hero
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_heroId);
            }
            set
            {
                _heroId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (!ValidTargets(Game, Player).Contains(Hero)) return Message.Express(Hero, " can't be revived this way");

            return null;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " request ", Faction.Purple, " revival of a leader");
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<IHero> ValidTargets(Game g, Player p)
        {
            var purple = g.GetPlayer(Faction.Purple);
            var gholas = purple != null ? purple.Leaders.Where(l => l.Faction == p.Faction) : Array.Empty<Leader>();
            return g.KilledHeroes(p).Union(gholas);
        }
    }

}
