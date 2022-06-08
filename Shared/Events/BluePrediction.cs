/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BluePrediction : GameEvent
    {
        public BluePrediction(Game game) : base(game)
        {
        }

        public BluePrediction()
        {
        }

        public Faction ToWin { get; set; }
        public int Turn { get; set; }

        public override Message Validate()
        {
            if (!Game.IsPlaying(ToWin)) return Message.Express("Invalid target");
            if (Turn < 1 || Turn > Game.MaximumNumberOfTurns) return Message.Express("Invalid turn");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " predict who will win and when");
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p) => g.PlayersOtherThan(p);

        public static IEnumerable<int> ValidTurns(Game g)
        {
            return Enumerable.Range(1, g.MaximumNumberOfTurns);
        }
    }
}
