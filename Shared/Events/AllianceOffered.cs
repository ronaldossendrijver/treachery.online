/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class AllianceOffered : GameEvent
    {
        public AllianceOffered(Game game) : base(game)
        {
        }

        public AllianceOffered()
        {
        }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            var matchingOffer = Game.CurrentAllianceOffers.FirstOrDefault(x => x.Initiator == Target && x.Target == Initiator);
            if (matchingOffer != null)
            {
                Game.MakeAlliance(Initiator, Target);

                AllianceOffered invalidOffer;
                while ((invalidOffer = Game.CurrentAllianceOffers.FirstOrDefault(x => x.By(Initiator) || x.Initiator == Target)) != null)
                {
                    Game.CurrentAllianceOffers.Remove(invalidOffer);
                }

                if (Game.Version > 150)
                {
                    Game.HasActedOrPassed.Add(Initiator);
                    Game.HasActedOrPassed.Add(Target);
                }
            }
            else
            {
                Log();
                Game.CurrentAllianceOffers.Add(this);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " offer an alliance to ", Target);
        }
    }
}
