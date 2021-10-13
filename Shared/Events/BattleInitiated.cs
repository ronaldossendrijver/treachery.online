/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class BattleInitiated : GameEvent
    {
        public int _territoryId;

        public BattleInitiated(Game game) : base(game)
        {
        }

        public BattleInitiated()
        {
        }

        public Faction Target { get; set; }

        [JsonIgnore]
        public Faction Defender
        {
            get
            {
                if (Initiator == Aggressor)
                {
                    return Target;
                }
                else
                {
                    return Initiator;
                }
            }
        }

        [JsonIgnore]
        public Player DefendingPlayer => Game.GetPlayer(Defender);

        [JsonIgnore]
        public Faction Aggressor
        {
            get
            {
                if (IsAggressorByJuice(Game, Target))
                {
                    return Target;
                }
                else if (IsInitiatorByJuice(Game, Player, Game.GetPlayer(Target)))
                {
                    return Target;
                }
                else if (IsTargetByJuice(Game, Player, Game.GetPlayer(Target)))
                {
                    return Target;
                }
                else
                {
                    return Initiator;
                }
            }
        }

        [JsonIgnore]
        public Player AggressivePlayer => Game.GetPlayer(Aggressor);

        public static bool IsAggressorByJuice(Game g, Faction f)
        {
            return g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.Aggressor && g.CurrentJuice.Initiator == f;
        }

        public static bool IsInitiatorByJuice(Game g, Player initiator, Player target)
        {
            return g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.GoFirst && g.CurrentJuice.Player == initiator && PlayerSequence.IsAfter(g, initiator, target);
        }

        public static bool IsTargetByJuice(Game g, Player initiator, Player target)
        {
            return g.CurrentJuice != null && g.CurrentJuice.Type == JuiceType.GoLast && g.CurrentJuice.Player == initiator && PlayerSequence.IsAfter(g, initiator, target);
        }

        [JsonIgnore]
        public Territory Territory { get { return Game.Map.TerritoryLookup.Find(_territoryId); } set { _territoryId = Game.Map.TerritoryLookup.GetId(value); } }

        public override string Validate()
        {
            if (Territory == null) return "Territory not selected.";

            var p = Player;
            var target = Game.GetPlayer(Defender);
            if (!p.Occupies(Territory)) return "You have no forces in this territory.";
            if (!target.Occupies(Territory)) return "Opponent has no forces in this territory.";
            if (target == null) return "Opponent not selected.";

            return "";
        }

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

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} initiate battle with {1} in {2}.", Initiator, Target, Territory);
        }

        public Player OpponentOf(Player p)
        {
            if (p.Faction == Initiator || p.Ally == Initiator)
            {
                return Game.GetPlayer(Target);
            }
            else if (p.Faction == Target || p.Ally == Target)
            {
                return Game.GetPlayer(Initiator);
            }

            return null;
        }

        public Player OpponentOf(Faction f)
        {
            var p = Game.GetPlayer(f);

            if (p == null)
            {
                return null;
            }
            else if (p.Faction == Initiator || p.Ally == Initiator)
            {
                return Game.GetPlayer(Target);
            }
            else if (p.Faction == Target || p.Ally == Target)
            {
                return Game.GetPlayer(Initiator);
            }

            return null;
        }

        public Battle PlanOf(Player p)
        {
            if (p == null) return null;

            if (p.Faction == Aggressor)
            {
                return Game.AggressorBattleAction;
            }
            else if (p.Faction == Defender)
            {
                return Game.DefenderBattleAction;
            }
            else
            {
                return null;
            }
        }

        public Battle PlanOfOpponent(Player p)
        {
            return PlanOf(OpponentOf(p));
        }

        public Battle PlanOf(Faction f)
        {
            if (f == Aggressor)
            {
                return Game.AggressorBattleAction;
            }
            else if (f == Defender)
            {
                return Game.DefenderBattleAction;
            }
            else
            {
                return null;
            }
        }

        [JsonIgnore]
        public Battle AggressorAction => Game.AggressorBattleAction;

        [JsonIgnore]
        public Battle DefenderAction => Game.DefenderBattleAction;
    }
}
