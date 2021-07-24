/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class JuicePlayed : GameEvent
    {
        public JuicePlayed(Game game) : base(game)
        {
        }

        public JuicePlayed()
        {
        }

        public JuiceType Type { get; set; }

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
            return new Message(Initiator, "{0} use {1} to {2}.", Initiator, TreacheryCardType.Juice, Type);
        }
    }
}
