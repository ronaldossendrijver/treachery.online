/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class SwitchedSkilledLeader : GameEvent
    {

        public int _leaderId = -1;

        [JsonIgnore]
        public Leader Leader
        {
            get
            {
                return LeaderManager.LeaderLookup.Find(_leaderId);
            }

            set
            {
                _leaderId = LeaderManager.LeaderLookup.GetId(value);
            }
        }

        public SwitchedSkilledLeader(Game game) : base(game)
        {
        }

        public SwitchedSkilledLeader()
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
            return Message.Express(Initiator, " switch their skilled leader");
        }

        public static bool CanBePlayed(Game game, Player player)
        {
            return game.CurrentBattle.IsAggressorOrDefender(player) && player.Leaders.Any(l => game.IsSkilled(l)) && game.CurrentBattle.PlanOf(player) == null;
        }
    }
}
