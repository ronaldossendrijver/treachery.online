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

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(Passed, " pass"), " mulligan");
        }

        public static bool MayMulligan(Player p)
        {
            return p.Traitors.Where(l => l.Is(p.Faction)).Count() >= 2;
        }

    }
}
