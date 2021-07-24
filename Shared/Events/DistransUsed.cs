/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DistransUsed : GameEvent
    {
        public DistransUsed(Game game) : base(game)
        {
        }

        public DistransUsed()
        {
        }

        public Faction Target { get; set; }

        public int _cardId = -1;
        
        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.Lookup.GetId(value);
            }
        }

        public override string Validate()
        {
            return "";
        }

        public static IEnumerable<TreacheryCard> ValidCards(Game g, Player p)
        {
            return p.TreacheryCards.Where(c => c.Type != TreacheryCardType.Distrans);
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(target => target != p && target.HasRoomForCards).Select(target => target.Faction);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use {1} to give a card to {2}.", Initiator, TreacheryCardType.Distrans, Target);
        }

        public static bool CanBePlayed(Game game, Player player)
        {
            return player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Distrans) && ValidCards(game, player).Any() && ValidTargets(game, player).Any();
        }
    }
}
