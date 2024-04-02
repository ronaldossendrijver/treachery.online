/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class NexusCardDrawn : PassableGameEvent
{
    #region Construction

    public NexusCardDrawn(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public NexusCardDrawn()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        if (!Passed && !MayDraw(Game, Player)) return Message.Express("You're not allowed to draw a Nexus Card");
        return null;
    }

    public static bool MayDraw(Game g, Player p)
    {
        return !g.FactionsThatDrewNexusCard.Contains(p.Faction) || p.Faction == p.Nexus;
    }

    public static bool Applicable(Game g, Player p)
    {
        return g.FactionsThatMayDrawNexusCard.Contains(p.Faction);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (!Passed)
        {
            DealNexusCard();
            Game.FactionsThatDrewNexusCard.Add(Initiator);
        }

        if (Passed) Game.FactionsThatMayDrawNexusCard.Remove(Initiator);

        if (!Game.FactionsThatMayDrawNexusCard.Any()) Game.EndBlowPhase();
    }


    private void DealNexusCard()
    {
        Game.DiscardNexusCard(Player);

        if (Game.NexusCardDeck.IsEmpty)
        {
            Game.NexusCardDeck.Items.AddRange(Game.NexusDiscardPile);
            Game.NexusDiscardPile.Clear();
            Game.NexusCardDeck.Shuffle();
            Game.Stone(Milestone.Shuffled);
            Log("The Nexus Card discard pile was shuffled into a new Nexus Card deck");
        }

        Log(Player.Faction, " draw a Nexus Card");
        Player.Nexus = Game.NexusCardDeck.Draw();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, MessagePart.ExpressIf(Passed, " don't"), " draw a Nexus card");
    }

    #endregion Execution

}