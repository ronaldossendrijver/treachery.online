/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class JuicePlayed : GameEvent
    {
        #region Construction

        public JuicePlayed(Game game) : base(game)
        {
        }

        public JuicePlayed()
        {
        }

        #endregion Construction

        #region Properties

        public JuiceType Type { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<JuiceType> ValidTypes(Game g, Player p)
        {
            var result = new List<JuiceType>();

            if (g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p) && g.BattleWinner == Faction.None)
            {
                result.Add(JuiceType.Aggressor);
            }

            if (g.CurrentMainPhase == MainPhase.Bidding && !g.Bids.Any() ||
                g.CurrentPhase == Phase.BeginningOfShipAndMove ||
                (g.CurrentMainPhase == MainPhase.Battle && g.CurrentBattle == null) ||
                g.CurrentPhase == Phase.Contemplate)
            {
                result.Add(JuiceType.GoFirst);
            }

            result.Add(JuiceType.GoLast);

            return result;
        }

        public static bool CanBePlayedBy(Game g, Player player)
        {
            bool applicablePhase =

                g.CurrentMainPhase == MainPhase.Bidding ||

                g.CurrentPhase == Phase.BeginningOfShipAndMove ||
                g.CurrentPhase == Phase.OrangeShip ||
                g.CurrentPhase == Phase.NonOrangeShip ||

                g.CurrentMainPhase == MainPhase.Battle ||

                g.CurrentPhase == Phase.Contemplate;

            return applicablePhase && player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Juice);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();

            var aggressorBeforeJuiceIsPlayed = Game.CurrentBattle?.AggressivePlayer;

            Game.CurrentJuice = this;
            Game.Discard(Player, TreacheryCardType.Juice);

            if ((Type == JuiceType.GoFirst || Type == JuiceType.GoLast) && Game.Version <= 117)
            {
                switch (Game.CurrentMainPhase)
                {
                    case MainPhase.Bidding: Game.BidSequence.CheckCurrentPlayer(); break;
                    case MainPhase.ShipmentAndMove: Game.ShipmentAndMoveSequence.CheckCurrentPlayer(); break;
                    case MainPhase.Battle: Game.BattleSequence.CheckCurrentPlayer(); break;
                }
            }
            else if (Game.CurrentBattle != null && Type == JuiceType.Aggressor && Game.CurrentBattle.AggressivePlayer != aggressorBeforeJuiceIsPlayed)
            {
                (Game.DefenderBattleAction, Game.AggressorBattleAction) = (Game.AggressorBattleAction, Game.DefenderBattleAction);
                (Game.DefenderTraitorAction, Game.AggressorTraitorAction) = (Game.AggressorTraitorAction, Game.DefenderTraitorAction);
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use ", TreacheryCardType.Juice, " to ", Type);
        }

        #endregion Execution

        
    }
}
