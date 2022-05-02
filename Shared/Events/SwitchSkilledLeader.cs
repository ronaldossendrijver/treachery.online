/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class SwitchedSkilledLeader : GameEvent
    {
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
            return game.CurrentBattle.IsAggressorOrDefender(player) && player.Leaders.Any(l => game.Skilled(l) && !game.CapturedLeaders.ContainsKey(l)) && game.CurrentBattle.PlanOf(player) == null;
        }
    }
}
