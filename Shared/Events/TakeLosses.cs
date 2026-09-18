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

    public int ForceAmountToRemain { get; set; }

    public int SpecialForceAmountToRemain { get; set; }

    public bool UseUselessCard { get; set; }

    #endregion Properties

    #region Validation

    public override Message? Validate()
    {
        var losses = LossesToTake(Game);
        if (UseUselessCard && (losses.IsBattleLoss || !CanPreventLosses(Game, Player))) return Message.Express("You can't use a card to prevent force losses");
        if (UseUselessCard) return null;
        if (ForceAmount < 0 || SpecialForceAmount < 0) return Message.Express("Invalid amount of forces");

        var valueToBeKilled = losses.Amount;
        var specialForceBonus = Game.Version < 167 ? 2 : 1;
        var selectedLossValue = ForceAmount + specialForceBonus * SpecialForceAmount;
        if (losses.IsBattleLoss && selectedLossValue != valueToBeKilled) return Message.Express("Select a total of exactly ", valueToBeKilled, " forces to be killed");
        if (!losses.IsBattleLoss && selectedLossValue < valueToBeKilled) return Message.Express("Select a total value of at least ", valueToBeKilled, " to be killed");

        if (Game.Version >= 120)
        {
            if (ForceAmount > ValidMaxForceAmount(Game, Player)) return Message.Express("Invalid amount of forces");
            if (SpecialForceAmount > ValidMaxSpecialForceAmount(Game, Player)) return Message.Express("Invalid amount of forces");
        }

        if (losses.IsBattleLoss)
        {
            if (ForceAmountToRemain < 0 || SpecialForceAmountToRemain < 0) return Message.Express("Invalid amount of forces to remain");
            if (ForceAmountToRemain + SpecialForceAmountToRemain != losses.ForcesToRemain) return Message.Express("Select exactly ", losses.ForcesToRemain, " forces to remain in the territory");
            if (ForceAmountToRemain > losses.MaximumForceAmount - ForceAmount) return Message.Express("Too many regular forces selected to remain");
            if (SpecialForceAmountToRemain > losses.MaximumSpecialForceAmount - SpecialForceAmount) return Message.Express("Too many special forces selected to remain");
        }

        return null;
    }

    public static LossToTake? LossesToTake(Game g)
    {
        if (g.LossesToTake.Count > 0)
            return g.LossesToTake[0];
        
        return null;
    }

    public static int ValidMaxForceAmount(Game g, Player p)
    {
        var losses = LossesToTake(g);
        if (losses == null) return 0;
        return losses.IsBattleLoss ? losses.MaximumForceAmount : p.ForcesIn(losses.Location);
    }

    public static int ValidMaxSpecialForceAmount(Game g, Player p)
    {
        var losses = LossesToTake(g);
        if (losses == null) return 0;
        return losses.IsBattleLoss ? losses.MaximumSpecialForceAmount : p.SpecialForcesIn(losses.Location);
    }

    public static TreacheryCard? ValidUselessCardToPreventLosses(Game g, Player p)
    {
        if (p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding)) 
            return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CardJubbaCloak);

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
        var losses = LossesToTake(Game);
        var mustDiscard = false;

        if (UseUselessCard)
        {
            var card = ValidUselessCardToPreventLosses(Game, Player);
            if (card == null && NexusPlayed.CanUseCunning(player))
            {
                Game.PlayNexusCard(player, "Cunning", " prevent losing forces in ", losses.Location);
                mustDiscard = true;
            }
            else
            {
                Game.Discard(Player, card);
                Log(Initiator, " prevent losing forces in ", losses.Location);
            }

            Game.LossesToTake.RemoveAt(0);
            Game.Stone(Milestone.SpecialUselessPlayed);
        }
        else if (losses.IsBattleLoss)
        {
            TakeBattleLosses(losses);
            Game.LossesToTake.RemoveAt(0);
            Log();
        }
        else
        {
            player.KillForces(losses.Location, ForceAmount, SpecialForceAmount, false);
            Game.LossesToTake.RemoveAt(0);
            Log();
        }

        if (losses.IsBattleLoss)
        {
            if (losses.ContinueBattleConclusion)
            {
                Game.Enter(Phase.BattleConclusion);
                Game.CompleteBattleConclusion();
            }
            else
                Game.ContinueAfterBattleLosses();
        }
        else if (Game.PhaseBeforeStormLoss == Phase.BlowA)
        {
            Game.Enter(Game.LossesToTake.Count > 0, Phase.StormLosses, Game.EndStormPhase);
        }
        else
        {
            Game.Enter(Game.PhaseBeforeStormLoss);
            Game.DetermineNextShipmentAndMoveSubPhase();
        }

        if (mustDiscard) Game.LetPlayerDiscardTreacheryCardOfChoice(Initiator);
    }

    private void TakeBattleLosses(LossToTake losses)
    {
        var forceOwner = GetPlayer(losses.ForceOwner);
        var territory = losses.Location.Territory;
        var savedForces = losses.MaximumForceAmount - ForceAmount;
        var savedSpecialForces = losses.MaximumSpecialForceAmount - SpecialForceAmount;
        var specialForcesToReserves = savedSpecialForces - SpecialForceAmountToRemain;
        var forcesToReserves = savedForces - ForceAmountToRemain;

        if (specialForcesToReserves > 0) forceOwner.ForcesToReserves(territory, specialForcesToReserves, true);
        if (forcesToReserves > 0) forceOwner.ForcesToReserves(territory, forcesToReserves, false);

        Log(
            LeaderSkill.Graduate,
            " rescues ",
            MessagePart.ExpressIf(ForceAmountToRemain > 0, ForceAmountToRemain, forceOwner.Force),
            MessagePart.ExpressIf(SpecialForceAmountToRemain > 0, SpecialForceAmountToRemain, forceOwner.SpecialForce),
            MessagePart.ExpressIf(ForceAmountToRemain > 0 || SpecialForceAmountToRemain > 0, " on site"),
            MessagePart.ExpressIf(forcesToReserves > 0 || specialForcesToReserves > 0, " and "),
            MessagePart.ExpressIf(forcesToReserves > 0, forcesToReserves, forceOwner.Force),
            MessagePart.ExpressIf(specialForcesToReserves > 0, specialForcesToReserves, forceOwner.SpecialForce),
            MessagePart.ExpressIf(forcesToReserves > 0 || specialForcesToReserves > 0, " to reserves"));

        Game.HandleForceLosses(territory, forceOwner, ForceAmount, SpecialForceAmount);
    }

    public override Message GetMessage()
    {
        var losses = LossesToTake(Game);
        var p = losses.IsBattleLoss ? GetPlayer(losses.ForceOwner) : Player;
        return Message.Express(
            losses.IsBattleLoss ? "Battle losses: " : "The storm kills ",
            MessagePart.ExpressIf(ForceAmount > 0, ForceAmount, p.Force),
            MessagePart.ExpressIf(ForceAmount > 0 && SpecialForceAmount > 0, " and "),
            MessagePart.ExpressIf(SpecialForceAmount > 0, SpecialForceAmount, p.SpecialForce));
    }

    #endregion Execution
}