/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy");
            }
        }

        public Message GetDynamicMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy. ", Game.TargetOfBureaucracy, " lose ", new Payment(2));
            }
        }
    }
}
