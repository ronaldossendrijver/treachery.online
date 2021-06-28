/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BattleConcluded : GameEvent
    {
        public string _cardIds;

        public BattleConcluded(Game game) : base(game)
        {
        }

        public BattleConcluded()
        {
        }

        public bool Kill { get; set; }

        public bool Capture { get; set; } = true;

        public CaptureDecision CaptureDecision
        {
            get
            {
                if (!Capture)
                {
                    return CaptureDecision.DontCapture;
                }
                else
                {
                    if (Kill)
                    {
                        return CaptureDecision.Kill;
                    }
                    else
                    {
                        return CaptureDecision.Capture;
                    }
                }

            }

            set
            {
                if (value == CaptureDecision.DontCapture || value == CaptureDecision.None)
                {
                    Capture = false;
                }
                else
                {
                    Capture = true;
                    Kill = (value == CaptureDecision.Kill);
                }
            }
        }

        public int SpecialForceLossesReplaced { get; set; }

        [JsonIgnore]
        public IEnumerable<TreacheryCard> DiscardedCards
        {
            get
            {
                return IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
            }
            set
            {
                _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
            }
        }

        public TechToken StolenToken { get; set; }

        public override string Validate()
        {
            var p = Player;
            if (SpecialForceLossesReplaced > 0 && Game.Prevented(FactionAdvantage.GreyReplacingSpecialForces)) return Skin.Current.Format("{0} prevents replacing {1} losses", TreacheryCardType.Karma, FactionSpecialForce.Grey);
            if (SpecialForceLossesReplaced > 0 && !ValidReplacementForceAmounts(Game, p).Contains(SpecialForceLossesReplaced)) return "Invalid amount of replacement forces";

            return "";
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} conclude the battle.", Initiator);
        }

        public static IEnumerable<int> ValidReplacementForceAmounts(Game g, Player p)
        {
            if (g.GreySpecialForceLossesToTake == 0) return new int[] { 0 };

            int replacementForcesLeft = p.ForcesIn(g.CurrentBattle.Territory) - (g.WinnerBattleAction.Forces + g.WinnerBattleAction.ForcesAtHalfStrength);

            if (replacementForcesLeft <= 0) return new int[] { 0 };

            return Enumerable.Range(0, 1 + Math.Min(replacementForcesLeft, g.GreySpecialForceLossesToTake));
        }

        public static bool MayCaptureOrKill(Game g, Player p)
        {
            return p.Faction == Faction.Black && g.BattleWinner == Faction.Black && !g.Prevented(FactionAdvantage.BlackCaptureLeader) && g.Applicable(Rule.BlackCapturesOrKillsLeaders) && g.BlackVictim != null;
        }
    }

    public enum CaptureDecision
    {
        None = 0,
        DontCapture = 10,
        Capture = 20,
        Kill = 30
    }
}
