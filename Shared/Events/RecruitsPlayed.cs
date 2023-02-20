/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RecruitsPlayed : GameEvent
    {
        public RecruitsPlayed(Game game) : base(game)
        {
        }

        public RecruitsPlayed()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static bool IsApplicable(Game g)
        {
            return g.CurrentPhase == Phase.BeginningOfResurrection;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Recruits, " to double free revival rates and increase the revival limit to ", 7);
        }
    }
}
