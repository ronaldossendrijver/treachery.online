/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class LoserConcluded : GameEvent
    {
        public LoserConcluded(Game game) : base(game)
        {
        }

        public LoserConcluded()
        {
        }

        public bool Assassinate { get; set; }

        public int _keptCardId;

        [JsonIgnore]
        public TreacheryCard KeptCard
        {
            get
            {
                return TreacheryCardManager.Get(_keptCardId);
            }
            set
            {
                _keptCardId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (Assassinate && AssassinationTarget(Game, Player) == null) return Message.Express("You can't assassinate");
            if (KeptCard != null && !CardsLoserMayKeep(Game).Contains(KeptCard)) return Message.Express("You can't keep ", KeptCard);

            return null;
        }


        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static Leader AssassinationTarget(Game g, Player p)
        {
            var winner = g.GetPlayer(g.BattleWinner);
            if (!g.LoserMayTryToAssassinate || winner == null)
            {
                return null;
            }
            else
            {
                return p.Traitors.FirstOrDefault(l => l is Leader && l != g.WinnerHero && winner.Leaders.Contains(l) && !p.RevealedTraitors.Contains(l)) as Leader;
            }
        }

        public static IEnumerable<TreacheryCard> CardsLoserMayKeep(Game g) => g.CardsToBeDiscardedByLoserAfterBattle;

        public static bool IsApplicable(Game g, Player p) => p.Faction == g.BattleLoser && (CardsLoserMayKeep(g).Any() || g.LoserMayTryToAssassinate);

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " conclude the battle");
        }
    }
}
