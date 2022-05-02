/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
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

        public int _traitorToReplaceId = -1;

        [JsonIgnore]
        public IHero ReplacedTraitor
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_traitorToReplaceId);
            }
            set
            {
                _traitorToReplaceId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public int _newTraitorId = -1;

        [JsonIgnore]
        public IHero NewTraitor
        {
            get
            {
                return LeaderManager.HeroLookup.Find(_newTraitorId);
            }
            set
            {
                _newTraitorId = LeaderManager.HeroLookup.GetId(value);
            }
        }

        public CaptureDecision DecisionToCapture
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

        public override Message Validate()
        {
            var p = Player;
            if (SpecialForceLossesReplaced > 0 && Game.Prevented(FactionAdvantage.GreyReplacingSpecialForces)) return Message.Express(TreacheryCardType.Karma, " prevents replacing ", FactionSpecialForce.Grey, " losses");
            if (SpecialForceLossesReplaced > 0 && !ValidReplacementForceAmounts(Game, p).Contains(SpecialForceLossesReplaced)) return Message.Express("Invalid amount of replacement forces");
            if (NewTraitor != null && ReplacedTraitor == null) return Message.Express("Select a traitor to be replaced by ", NewTraitor);
            if (NewTraitor == null && ReplacedTraitor != null) return Message.Express("Select a traitor to replace ", ReplacedTraitor);

            return null;
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " conclude the battle");
        }

        public static IEnumerable<int> ValidReplacementForceAmounts(Game g, Player p)
        {
            if (g.GreySpecialForceLossesToTake == 0) return new int[] { 0 };

            int replacementForcesLeft = p.ForcesIn(g.CurrentBattle.Territory) - (g.WinnerBattleAction.Forces + g.WinnerBattleAction.ForcesAtHalfStrength);

            if (replacementForcesLeft <= 0) return new int[] { 0 };

            return Enumerable.Range(0, 1 + Math.Min(replacementForcesLeft, g.GreySpecialForceLossesToTake));
        }

        public static int ValidMaxReplacementForceAmount(Game g, Player p)
        {
            if (g.GreySpecialForceLossesToTake == 0) return 0;

            int replacementForcesLeft = p.ForcesIn(g.CurrentBattle.Territory) - (g.WinnerBattleAction.Forces + g.WinnerBattleAction.ForcesAtHalfStrength);

            if (replacementForcesLeft <= 0) return 0;

            return Math.Min(replacementForcesLeft, g.GreySpecialForceLossesToTake);
        }

        public static bool MayCaptureOrKill(Game g, Player p)
        {
            return p.Faction == Faction.Black && g.BattleWinner == Faction.Black && !g.Prevented(FactionAdvantage.BlackCaptureLeader) && g.Applicable(Rule.BlackCapturesOrKillsLeaders) && g.BlackVictim != null;
        }

        public static IEnumerable<CaptureDecision> ValidCaptureDecisions(Game g, Player p)
        {
            if (g.Version < 116)
            {
                return new CaptureDecision[] { CaptureDecision.Capture, CaptureDecision.Kill, CaptureDecision.DontCapture };
            }
            else
            {
                return new CaptureDecision[] { CaptureDecision.Capture, CaptureDecision.Kill };
            }
        }

        public static IEnumerable<IHero> ValidTraitorsToReplace(Game g, Player p)
        {
            return p.Traitors.Where(h => !p.RevealedTraitors.Contains(h));
        }

    }
}
