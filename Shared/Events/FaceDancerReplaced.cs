/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class FaceDancerReplaced : PassableGameEvent
{
    public int dancerId;

    public FaceDancerReplaced(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public FaceDancerReplaced()
    {
    }

    [JsonIgnore]
    public IHero SelectedDancer { get => LeaderManager.HeroLookup.Find(dancerId);
        set => dancerId = LeaderManager.HeroLookup.GetId(value);
    }

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