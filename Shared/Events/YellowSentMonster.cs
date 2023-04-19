/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class YellowSentMonster : GameEvent
    {
        #region Construction

        public YellowSentMonster(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public YellowSentMonster()
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
            if (Territory == null) return Message.Express("No territory selected");
            if (Territory.IsProtectedFromWorm) return Message.Express("Selected territory is protected");

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
            Log();
            var monster = new MonsterAppearence(Territory, false);
            Game.Monsters.Add(monster);
            Game.PerformMonster(monster);
            Game.Enter(Game.CurrentPhase == Phase.YellowSendingMonsterA, Phase.BlowA, Phase.BlowB);
            Game.DrawResourceCard();
            Game.LetFactionsDiscardSurplusCards();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " send ", Concept.Monster, " to ", Territory);
        }

        #endregion Execution
    }
}
