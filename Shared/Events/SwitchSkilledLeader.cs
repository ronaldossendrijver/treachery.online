/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class SwitchedSkilledLeader : GameEvent
    {
        #region Construction

        public SwitchedSkilledLeader(Game game) : base(game)
        {
        }

        public SwitchedSkilledLeader()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static Leader SwitchableLeader(Game game, Player player) => player.Leaders.FirstOrDefault(l => game.IsSkilled(l) && !game.CapturedLeaders.ContainsKey(l) && (player.Faction == Faction.Pink || l.HeroType != HeroType.Vidal));

        public static bool CanBePlayed(Game game, Player player)
        {
            return game.CurrentBattle.IsAggressorOrDefender(player) && game.CurrentBattle.PlanOf(player) == null && SwitchableLeader(game, player) != null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var leader = SwitchableLeader(Game, Player);
            Game.SetInFrontOfShield(leader, !Game.IsInFrontOfShield(leader));
            Log(Initiator, " place ", Game.Skill(leader), " ", leader, Game.IsInFrontOfShield(leader) ? " in front of" : " behind", " their shield");
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " switch their skilled leader");
        }

        #endregion Execution
    }
}
