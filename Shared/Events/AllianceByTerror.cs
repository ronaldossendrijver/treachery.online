/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class AllianceByTerror : PassableGameEvent
    {
        public AllianceByTerror(Game game) : base(game)
        {
        }

        public AllianceByTerror()
        {
        }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.Enter(Game.PausedTerrorPhase);

            if (!Passed)
            {
                if (Player.HasAlly)
                {
                    Log(Initiator, " and ", Player.Ally, " end their alliance");
                    Game.BreakAlliance(Initiator);
                }

                var cyan = Game.GetPlayer(Faction.Cyan);
                if (cyan.HasAlly)
                {
                    Log(Faction.Cyan, " and ", cyan.Ally, " end their alliance");
                    Game.BreakAlliance(Faction.Cyan);
                }

                Game.MakeAlliance(Initiator, Faction.Cyan);

                if (Game.HasActedOrPassed.Contains(Initiator) && Game.HasActedOrPassed.Contains(Faction.Cyan))
                {
                    Game.CheckIfForcesShouldBeDestroyedByAllyPresence(Player);
                }

                var territory = Game.LastTerrorTrigger.Territory;
                Log("Terror in ", territory, " is returned to supplies");
                foreach (var t in Game.TerrorIn(territory).ToList())
                {
                    Game.TerrorOnPlanet.Remove(t);
                    Game.UnplacedTerrorTokens.Add(t);
                }

                Game.AllianceByTerrorWasOffered = false;
                Game.DequeueIntrusion(IntrusionType.Terror);
                Game.DetermineNextShipmentAndMoveSubPhase();
            }
            else
            {
                Log(Initiator, " don't ally with ", Faction.Cyan);
            }

            Game.LetFactionsDiscardSurplusCards();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, !Passed ? "" : " don't", " agree to ally");
        }
    }
}
