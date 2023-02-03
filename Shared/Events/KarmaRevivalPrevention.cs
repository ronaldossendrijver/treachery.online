/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

        public override Message Validate()
        {
            if (!GetValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<Faction> GetValidTargets(Game g, Player p)
        {
            return g.PlayersOtherThan(p);
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " prevent revival by ", Target);
        }
    }
}
