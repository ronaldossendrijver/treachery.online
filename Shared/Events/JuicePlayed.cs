/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use {1} to {2}.", Initiator, TreacheryCardType.Juice, Type);
        }

        public static IEnumerable<JuiceType> ValidTypes(Game g, Player p)
        {
            var result = new List<JuiceType>();

            if (g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p))
            {
                result.Add(JuiceType.Aggressor);
            }

            if (g.CurrentMainPhase == MainPhase.Bidding && !g.Bids.Any() ||
                g.CurrentPhase == Phase.BeginningOfShipAndMove ||
                (g.CurrentMainPhase == MainPhase.Battle) ||
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
                
                (g.CurrentMainPhase == MainPhase.Battle && g.CurrentBattle == null) || 
                
                g.CurrentPhase == Phase.Contemplate;

            return applicablePhase && player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Juice);
        }
    }
}
