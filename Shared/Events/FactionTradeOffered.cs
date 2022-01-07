/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class FactionTradeOffered : GameEvent
    {
        public FactionTradeOffered(Game game) : base(game)
        {
        }

        public FactionTradeOffered()
        {
        }

        public Faction Target { get; set; }

        public override string Validate()
        {
            if (!Game.IsPlaying(Target)) return "Invalid target";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " offer to trade factions with ", Target);
        }
    }

}
