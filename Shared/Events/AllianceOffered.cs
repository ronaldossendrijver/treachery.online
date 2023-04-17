/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AllianceOffered : GameEvent
    {
        #region Construction

        public AllianceOffered(Game game) : base(game)
        {
        }

        public AllianceOffered()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p) => g.Players.Where(other => p != other && !p.HaveForcesOnEachOthersHomeworlds(other)).Select(other => other.Faction);

        #endregion Validation

        #region Execution

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

        #endregion Execution
    }
}
