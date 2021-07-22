/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RockWasMelted : GameEvent
    {
        public RockWasMelted(Game game) : base(game)
        {
        }

        public RockWasMelted()
        {
        }

        public bool Kill { get; set; }

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
            if (Kill)
            {
                return new Message(Initiator, "{0} use {1} to kill both leaders.", Initiator, TreacheryCardType.Rockmelter);
            }
            else
            {
                return new Message(Initiator, "{0} use {1} to not count leader strength.", Initiator, TreacheryCardType.Rockmelter);
            }
        }

        public static Territory GetTerritory(Game g)
        {
            return g.LastShippedOrMovedTo.Territory;
        }
    }
}
