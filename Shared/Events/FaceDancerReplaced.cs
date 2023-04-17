/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Treachery.Shared
{
    public class FaceDancerReplaced : PassableGameEvent
    {
        public int dancerId;

        public FaceDancerReplaced(Game game) : base(game)
        {
        }

        public FaceDancerReplaced()
        {
        }

        [JsonIgnore]
        public IHero SelectedDancer { get { return LeaderManager.HeroLookup.Find(dancerId); } set { dancerId = LeaderManager.HeroLookup.GetId(value); } }

        public override Message Validate()
        {
            if (!Passed)
            {
                var p = Player;
                if (p.RevealedDancers.Contains(SelectedDancer)) return Message.Express("You can't replace a revealed Face Dancer");
                if (!p.FaceDancers.Contains(SelectedDancer)) return Message.Express("Invalid Face Dancer");
            }

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                var player = GetPlayer(Initiator);
                player.FaceDancers.Remove(SelectedDancer);
                Game.TraitorDeck.PutOnTop(SelectedDancer);
                Game.TraitorDeck.Shuffle();
                Game.Stone(Milestone.Shuffled);
                var leader = Game.TraitorDeck.Draw();
                player.FaceDancers.Add(leader);
                if (!player.KnownNonTraitors.Contains(leader)) player.KnownNonTraitors.Add(leader);
            }

            Log();
            Game.Enter(Phase.TurnConcluded);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, MessagePart.ExpressIf(Passed, " don't"), " replace a Face Dancer");
        }

        public static IEnumerable<IHero> ValidFaceDancers(Player p)
        {
            return p.UnrevealedFaceDancers;
        }
    }
}
