/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class FaceDancerRevealed : PassableGameEvent
    {
        public FaceDancerRevealed(Game game) : base(game)
        {
        }

        public FaceDancerRevealed()
        {
        }

        public override Message Validate()
        {
            if (Passed) return null;

            if (!FaceDanced.MayCallFaceDancer(Game, Player)) return Message.Express("You can't reveal a face dancer");

            return null;
        }

        public bool FacedancerSucceeded(Game g) => !Passed && (Initiator != Faction.Purple || !g.FacedancerWasCancelled);

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " reveal a face dancer!");
            }
            else
            {
                return Message.Express(Initiator, " don't reveal a face dancer");
            }
        }
    }
}
