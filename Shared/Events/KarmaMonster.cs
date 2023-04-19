/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class KarmaMonster : GameEvent
    {
        #region Construction

        public KarmaMonster(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public KarmaMonster()
        {
        }

        #endregion Construction

        #region Properties

        public int _territoryId;

        [JsonIgnore]
        public Territory Territory
        {
            get => Game.Map.TerritoryLookup.Find(_territoryId);
            set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<Territory> ValidTargets(Game g)
        {
            return g.Map.Territories(false).Where(t => !t.IsProtectedFromWorm);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());
            Player.SpecialKarmaPowerUsed = true;
            Log();
            Game.Stone(Milestone.Karma);
            Game.NumberOfMonsters++;
            Game.LetMonsterAppear(Territory, false);

            if (Game.CurrentPhase == Phase.BlowReport)
            {
                Game.Enter(Phase.AllianceB);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " send ", Concept.Monster, " to ", Territory);
        }

        #endregion Execution
    }
}
