/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaRevivalPrevention : GameEvent
    {
        public KarmaRevivalPrevention(Game game) : base(game)
        {
        }

        public KarmaRevivalPrevention()
        {
        }

        public Faction Target { get; set; }

        public override string Validate()
        {
            if (!GetValidTargets(Game, Player).Contains(Target)) return "Invalid target";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
        {
            return g.ValidTargets(p);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent revival by ", Target);
        }
    }
}
