/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class MulliganPerformed : GameEvent
    {
        public MulliganPerformed(Game game) : base(game)
        {
        }

        public MulliganPerformed()
        {
        }

        public bool Passed { get; set; }

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
            return new Message(Initiator, "{0}{1} mulligan.", Initiator, Passed ? " pass" : "");
        }

        public static bool MayMulligan(Player p)
        {
            return p.Traitors.Where(l => l.Is(p.Faction)).Count() >= 2;
        }

    }
}
