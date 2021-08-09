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
                return new Message(Initiator, "{0} don't apply Bureaucracy.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} apply Bureaucracy.", Initiator);
            }
        }

        public Message GetDynamicMessage()
        {
            if (Passed)
            {
                return new Message(Initiator, "{0} don't apply Bureaucracy.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} apply Bureaucracy; {1} lose 2.", Initiator, Game.TargetOfBureaucracy);
            }
        }
    }
}
