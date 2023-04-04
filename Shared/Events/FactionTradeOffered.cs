/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FactionTradeOffered : GameEvent
    {
        #region Construction

        public FactionTradeOffered(Game game) : base(game)
        {
        }

        public FactionTradeOffered()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (Game.Version >= 132)
            {
                if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid target");
            }
            else
            {
                if (!Game.IsPlaying(Target)) return Message.Express("Invalid target");
            }


            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.FactionsInPlay.Union(g.Players.Select(p => p.Faction)).Where(f => f != p.Faction);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Game.IsPlaying(Target))
            {
                Log(Initiator, " switch to ", Target);
                Game.FactionsInPlay.Add(Initiator);
                Player.Faction = Target;
            }
            else
            {
                var match = Game.CurrentTradeOffers.SingleOrDefault(matchingOffer => matchingOffer.Initiator == Target && matchingOffer.Target == Initiator);
                if (match != null)
                {
                    Log(Initiator, " and ", match.Initiator, " traded factions");
                    var initiator = GetPlayer(Initiator);
                    var target = GetPlayer(Target);
                    (target.Faction, initiator.Faction) = (initiator.Faction, target.Faction);
                    FactionTradeOffered invalidOffer;
                    while ((invalidOffer = Game.CurrentTradeOffers.FirstOrDefault(x => x.Initiator == Initiator || x.Initiator == Target)) != null)
                    {
                        Game.CurrentTradeOffers.Remove(invalidOffer);
                    }
                }
                else
                {
                    Log(GetMessage());
                    if (!Game.CurrentTradeOffers.Any(o => o.Initiator == Initiator && o.Target == Target))
                    {
                        Game.CurrentTradeOffers.Add(this);
                    }
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " offer to trade factions with ", Target);
        }

        #endregion Execution

        
    }

}
