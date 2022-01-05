/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            var p = Player;
            if (!Game.KilledHeroes(p).Contains(Hero)) return Skin.Current.Format("{0} can't be revived this way.", Hero);

            return "";
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} request {1} revival of a leader.", Initiator, Faction.Purple);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<IHero> ValidTargets(Game g, Player p)
        {
            return g.KilledHeroes(p);
        }
    }

}
