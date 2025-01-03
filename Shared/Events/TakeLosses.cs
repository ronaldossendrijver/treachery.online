/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public class TakeLosses : GameEvent
{
    #region Construction

    public TakeLosses(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public TakeLosses()
    {
    }

    #endregion Construction

    #region Properties

    public int ForceAmount { get; set; }

    public int SpecialForceAmount { get; set; }

    public bool UseUselessCard { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (UseUselessCard && !CanPreventLosses(Game, Player)) return Message.Express("You can't use a card to prevent force losses");
        if (UseUselessCard) return null;

        var valueToBeKilled = LossesToTake(Game).Amount;
        var specialForceBonus = Game.Version < 167 ? 2 : 1;
        if (ForceAmount + specialForceBonus * SpecialForceAmount < valueToBeKilled) return Message.Express("Select a total value of at least ", valueToBeKilled, " to be killed");

        if (Game.Version >= 120)
        {
            if (ForceAmount > ValidMaxForceAmount(Game, Player)) return Message.Express("Invalid amount of forces");
            if (SpecialForceAmount > ValidMaxSpecialForceAmount(Game, Player)) return Message.Express("Invalid amount of forces");
        }

        return null;
    }

    public static LossToTake LossesToTake(Game g)
    {
        if (g.StormLossesToTake.Count > 0)
            return g.StormLossesToTake[0];
        return new LossToTake();
    }

    public static int ValidMaxForceAmount(Game g, Player p)
    {
        return p.ForcesIn(LossesToTake(g).Location);
    }

    public static int ValidMaxSpecialForceAmount(Game g, Player p)
    {
        return p.SpecialForcesIn(LossesToTake(g).Location);
    }

    public static TreacheryCard ValidUselessCardToPreventLosses(Game g, Player p)
    {
        if (p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding)) return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_JUBBACLOAK);

        return null;
    }

    public static bool CanPreventLosses(Game g, Player p)
    {
        return p.Is(Faction.Brown) &&
               ((!g.Prevented(FactionAdvantage.BrownDiscarding) && ValidUselessCardToPreventLosses(g, p) != null) ||
                (NexusPlayed.CanUseCunning(p) && p.TreacheryCards.Any()));
    }

    public static int HalfOf(int AmountOfForces, int AmountOfSpecialForces)
    {
        return (int)Math.Ceiling(0.5 * (AmountOfForces + AmountOfSpecialForces));
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var player = GetPlayer(Initiator);
        var mustDiscard = false;

        if (UseUselessCard)
        {
            var card = ValidUselessCardToPreventLosses(Game, Player);
            if (card == null && NexusPlayed.CanUseCunning(player))
            {
                Game.PlayNexusCard(player, "Cunning", " prevent losing forces in ", Game.StormLossesToTake[0].Location);
                mustDiscard = true;
            }
            else
            {
                Game.Discard(Player, card);
                Log(Initiator, " prevent losing forces in ", Game.StormLossesToTake[0].Location);
            }

            Game.StormLossesToTake.RemoveAt(0);
            Game.Stone(Milestone.SpecialUselessPlayed);
        }
        else
        {
            player.KillForces(LossesToTake(Game).Location, ForceAmount, SpecialForceAmount, false);
            Game.StormLossesToTake.RemoveAt(0);
            Log();
        }

        if (Game.PhaseBeforeStormLoss == Phase.BlowA)
        {
            Game.Enter(Game.StormLossesToTake.Count > 0, Phase.StormLosses, Game.EndStormPhase);
        }
        else
        {
            Game.Enter(Game.PhaseBeforeStormLoss);
            Game.DetermineNextShipmentAndMoveSubPhase();
        }

        if (mustDiscard) Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
    }

    public override Message GetMessage()
    {
        var p = Player;
        return Message.Express(
            "The storm kills ",
            MessagePart.ExpressIf(ForceAmount > 0, ForceAmount, p.Force),
            MessagePart.ExpressIf(ForceAmount > 0 && SpecialForceAmount > 0, " and "),
            MessagePart.ExpressIf(SpecialForceAmount > 0, SpecialForceAmount, p.SpecialForce));
    }

    #endregion Execution
}