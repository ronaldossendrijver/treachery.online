/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class KarmaHandSwapInitiated : GameEvent
{
    #region Construction

    public KarmaHandSwapInitiated(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public KarmaHandSwapInitiated()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.KarmaHandSwapPausedPhase = Game.CurrentPhase;
        Game.Enter(Phase.PerformingKarmaHandSwap);

        var victim = GetPlayer(Target);

        Player.SpecialKarmaPowerUsed = true;
        Game.Discard(Player, Karma.ValidKarmaCards(Game, Player).FirstOrDefault());

        Game.KarmaHandSwapNumberOfCards = victim.TreacheryCards.Count;
        Game.KarmaHandSwapTarget = Target;

        var cardsToDrawFrom = new Deck<TreacheryCard>(victim.TreacheryCards, Game.Random);
        Game.Stone(Milestone.Shuffled);
        cardsToDrawFrom.Shuffle();
        for (var i = 0; i < Game.KarmaHandSwapNumberOfCards; i++)
        {
            var card = cardsToDrawFrom.Draw();
            Game.RegisterKnown(Player, card);
            victim.TreacheryCards.Remove(card);
            Player.TreacheryCards.Add(card);
        }

        Log();
        Game.Stone(Milestone.Karma);
    }

    public override Message GetMessage()
    {
        return Message.Express("Using ", TreacheryCardType.Karma, ", ", Initiator, " swap cards with ", Target);
    }

    #endregion Execution
}