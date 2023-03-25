/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ExtortionPrevented : GameEvent
    {
        public ExtortionPrevented(Game game) : base(game)
        {
        }

        public ExtortionPrevented()
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

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " pay ", Payment.Of(3), " to prevent ", Faction.Cyan, " from regaining ", TerrorType.Extortion);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return p.Faction != Faction.Cyan && p.Resources >= 3;
        }

    }
}
