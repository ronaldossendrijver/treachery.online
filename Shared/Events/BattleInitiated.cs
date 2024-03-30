/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class BattleInitiated : GameEvent
    {
        #region Construction

        public BattleInitiated(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public BattleInitiated()
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

        public Faction Target { get; set; }

        [JsonIgnore]
        public Faction ActualInitiator => Game.CurrentPinkOrAllyFighter != Faction.None && Initiator == Game.GetAlly(Game.CurrentPinkOrAllyFighter) ? Game.CurrentPinkOrAllyFighter : Initiator;

        [JsonIgnore]
        public Faction ActualTarget => Game.CurrentPinkOrAllyFighter != Faction.None && Target == Game.GetAlly(Game.CurrentPinkOrAllyFighter) ? Game.CurrentPinkOrAllyFighter : Target;

        [JsonIgnore]
        public Faction Defender => (ActualInitiator == Aggressor) ? ActualTarget : ActualInitiator;

        [JsonIgnore]
        public Player DefendingPlayer => Game.GetPlayer(Defender);

        [JsonIgnore]
        public Faction Aggressor
        {
            get
            {
                var target = ActualTarget;

                if (IsAggressorByJuice(Game, target))
                {
                    return target;
                }
                else if (IsInitiatorByJuice(Game, Player, Game.GetPlayer(target)))
                {
                    return target;
                }
                else if (IsTargetByJuice(Game, Player, Game.GetPlayer(target)))
                {
                    return target;
                }
                else
                {
                    return ActualInitiator;
                }
            }
        }

        [JsonIgnore]
        public Player AggressivePlayer => Game.GetPlayer(Aggressor);

        public static bool IsAggressorByJuice(Game g, Faction f) => g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.Aggressor && g.CurrentJuice.Initiator == f;

        public static bool IsInitiatorByJuice(Game g, Player initiator, Player target) => g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.GoFirst && g.CurrentJuice.Player == initiator && PlayerSequence.IsAfter(g, initiator, target);

        public static bool IsTargetByJuice(Game g, Player initiator, Player target) => g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.GoLast && g.CurrentJuice.Player == initiator && PlayerSequence.IsAfter(g, initiator, target);

        public bool IsInvolved(Player p)
        {
            return Initiator == p.Faction || Target == p.Faction || Initiator == p.Ally || Target == p.Ally;
        }

        public bool IsInvolved(Faction f)
        {
            return Initiator == f || Target == f || Player.Ally == f || Game.GetPlayer(Target).Ally == f;
        }

        public bool IsAggressorOrDefender(Player p)
        {
            return Aggressor == p.Faction || Defender == p.Faction;
        }

        public bool IsAggressorOrDefender(Faction f)
        {
            return Aggressor == f || Defender == f;
        }

        public Player OpponentOf(Player p)
        {
            if (p.Faction == Initiator || p.Ally == Initiator)
            {
                return Game.GetPlayer(ActualTarget);
            }
            else if (p.Faction == Target || p.Ally == Target)
            {
                return Game.GetPlayer(ActualInitiator);
            }

            return null;
        }

        public Player OpponentOf(Faction f) => OpponentOf(Game.GetPlayer(f));

        public Battle PlanOf(Player p)
        {
            if (p == null) return null;

            if (p.Faction == Aggressor)
            {
                return Game.AggressorPlan;
            }
            else if (p.Faction == Defender)
            {
                return Game.DefenderPlan;
            }
            else
            {
                return null;
            }
        }

        public Battle PlanOfOpponent(Player p) => PlanOf(OpponentOf(p));

        public Battle PlanOfOpponent(Faction f) => PlanOf(OpponentOf(f));

        public Battle PlanOf(Faction f)
        {
            if (f == Aggressor)
            {
                return Game.AggressorPlan;
            }
            else if (f == Defender)
            {
                return Game.DefenderPlan;
            }
            else
            {
                return null;
            }
        }

        public TreacheryCalled TreacheryOfOpponent(Player p)
        {
            return TreacheryOf(OpponentOf(p));
        }

        public TreacheryCalled TreacheryOf(Player p)
        {
            if (p == null) return null;

            if (Game.AggressorTraitorAction != null && (Game.AggressorTraitorAction.Initiator == p.Faction || Game.AggressorTraitorAction.Initiator == p.Ally))
            {
                return Game.AggressorTraitorAction;
            }
            else if (Game.DefenderTraitorAction != null && (Game.DefenderTraitorAction.Initiator == p.Faction || Game.DefenderTraitorAction.Initiator == p.Ally))
            {
                return Game.DefenderTraitorAction;
            }
            else
            {
                return null;
            }
        }

        public TreacheryCalled TreacheryOf(Faction f) => TreacheryOf(Game.GetPlayer(f));

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Territory == null) return Message.Express("Territory not selected");

            var p = Player;
            var target = Game.GetPlayer(Defender);
            if (!p.Occupies(Territory)) return Message.Express("You have no forces in this territory");
            if (!target.Occupies(Territory)) return Message.Express("Opponent has no forces in this territory");
            if (target == null) return Message.Express("Opponent not selected");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentReport = new Report(MainPhase.Battle);
            Log();
            Game.BattleAboutToStart = this;

            var pink = GetPlayer(Faction.Pink);
            Game.Enter(pink != null && IsInvolved(Faction.Pink) && pink.Occupies(Territory) && pink.HasAlly && pink.AlliedPlayer.Occupies(Territory), Phase.ClaimingBattle, Game.InitiateBattle);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " initiate battle against ", Target, Territory.IsHomeworld ? " on " : " in ", Territory);
        }

        #endregion Execution
    }
}
