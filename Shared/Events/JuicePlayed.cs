/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class JuicePlayed : GameEvent
    {
        public JuicePlayed(Game game) : base(game)
        {
        }

        public JuicePlayed()
        {
        }

        public JuiceType Type { get; set; }

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
            return Message.Express(Initiator, " use ", TreacheryCardType.Juice, " to ", Type);
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
    }
}
