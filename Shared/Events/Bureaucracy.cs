/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Bureaucracy : PassableGameEvent
    {
        public Bureaucracy(Game game) : base(game)
        {
        }

        public Bureaucracy()
        {
        }

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
            if (Passed)
            {
                return new Message(Initiator, "{0} don't excercise Bureaucracy.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} excercise Bureaucracy.", Game.TargetOfBureaucracy);
            }
        }

        public Message GetDynamicMessage()
        {
            if (Passed)
            {
                return new Message(Initiator, "{0} don't excercise Bureaucracy.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} excercise Bureaucracy; {1} lose 2.", Game.TargetOfBureaucracy);
            }
        }
    }
}
