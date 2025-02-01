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

public class BattleConcluded : GameEvent
{
    #region Properties

    public bool Kill { get; set; }

    public bool Capture { get; set; } = true;

    [JsonIgnore]
    public CaptureDecision DecisionToCapture
    {
        get
        {
            if (!Capture) return CaptureDecision.DontCapture;

            if (Kill)
                return CaptureDecision.Kill;
            return CaptureDecision.Capture;

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
                Kill = value == CaptureDecision.Kill;
            }
        }
    }

    public int _traitorToReplaceId = -1;

    [JsonIgnore]
    public IHero TraitorToReplace
    {
        get => LeaderManager.HeroLookup.Find(_traitorToReplaceId);
        set => _traitorToReplaceId = LeaderManager.HeroLookup.GetId(value);
    }

    public int _newTraitorId = -1;

    [JsonIgnore]
    public IHero NewTraitor
    {
        get => LeaderManager.HeroLookup.Find(_newTraitorId);
        set => _newTraitorId = LeaderManager.HeroLookup.GetId(value);
    }

    public int SpecialForceLossesReplaced { get; set; }

    public string _cardIds;

    [JsonIgnore]
    public IEnumerable<TreacheryCard> DiscardedCards
    {
        get => IdStringToObjects(_cardIds, TreacheryCardManager.Lookup);
        set => _cardIds = ObjectsToIdString(value, TreacheryCardManager.Lookup);
    }

    public TechToken StolenToken { get; set; }

    public bool AddExtraForce { get; set; }

    #endregion Properties

    #region Construction

    public BattleConcluded(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BattleConcluded()
    {
    }

    public override Message Validate()
    {
        var p = Player;
        if (SpecialForceLossesReplaced > 0 && Game.Prevented(FactionAdvantage.GreyReplacingSpecialForces)) return Message.Express(TreacheryCardType.Karma, " prevents replacing ", FactionSpecialForce.Grey, " losses");
        if (SpecialForceLossesReplaced > 0 && !ValidReplacementForceAmounts(Game, p).Contains(SpecialForceLossesReplaced)) return Message.Express("Invalid amount of replacement forces");

        if (Game.DeciphererMayReplaceTraitor)
        {
            if (NewTraitor != null && TraitorToReplace == null) return Message.Express("Select a traitor to be replaced by ", NewTraitor);
            if (NewTraitor == null && TraitorToReplace != null) return Message.Express("Select a traitor to replace ", TraitorToReplace);
        }

        if (!MayChooseToDiscardCards(Game) && DiscardedCards.Any()) return Message.Express("You are not allowed to choose which cards to discard");

        if (AddExtraForce && !MayAddExtraForce(Game, Player)) return Message.Express("You cannot add a force");

        return null;
    }

    #endregion Construction

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.BattleWasConcludedByWinner = true;

        if (AddExtraForce && Game.Version >= 161)
        {
            var to = Game.CurrentBattle.Territory.Locations.FirstOrDefault(l => Player.AnyForcesIn(l) > 0);
            Player.ShipForces(to, 1);
            Log(Initiator, " place ", 1, " extra force in ", Game.CurrentBattle.Territory);
        }

        foreach (var c in DiscardedCards)
        {
            Log(Initiator, " discard ", c);
            Player.TreacheryCards.Remove(c);
            Game.TreacheryDiscardPile.PutOnTop(c);
        }

        if (Game.TraitorsDeciphererCanLookAt.Count > 0) Log(Initiator, " look at ", Game.TraitorsDeciphererCanLookAt.Count, " leaders in the traitor deck");

        if (TraitorToReplace != null && NewTraitor != null && Game.DeciphererMayReplaceTraitor) DeciphererReplacesTraitors();

        DecideFateOfCapturedLeader();
        TakeTechToken();
        ProcessGreyForceLossesAndSubstitutions();

        if (!LoserConcluded.IsApplicable(Game, GetPlayer(Game.BattleLoser))) Game.Enter(!Game.IsPlaying(Faction.Purple) || Game.BattleWinner == Faction.Purple, Game.FinishBattle, Game.Version <= 150, Phase.Facedancing, Phase.RevealingFacedancer);
    }

    private void DeciphererReplacesTraitors()
    {
        var toReplace = Initiator == Faction.Purple ? "facedancer " : "traitor ";

        Log(Initiator, " replaced ", toReplace, TraitorToReplace, " with another leader from the traitor deck");

        var currentlyHeld = Initiator == Faction.Purple ? Player.FaceDancers : Player.Traitors;

        currentlyHeld.Add(NewTraitor);
        Game.TraitorsDeciphererCanLookAt.Remove(NewTraitor);

        currentlyHeld.Remove(TraitorToReplace);
        Game.TraitorDeck.PutOnTop(TraitorToReplace);

        Game.Stone(Milestone.Shuffled);
        Game.TraitorDeck.Shuffle();
    }

    private void DecideFateOfCapturedLeader()
    {
        if (By(Faction.Black) && Game.Applicable(Rule.BlackCapturesOrKillsLeaders) && Game.BlackVictim != null)
        {
            if (Game.Version > 125 && Game.Prevented(FactionAdvantage.BlackCaptureLeader))
                Game.LogPreventionByKarma(FactionAdvantage.BlackCaptureLeader);
            else
                CaptureOrAssassinateLeader(Player, Game.CurrentBattle.OpponentOf(Player));
        }
    }

    private void TakeTechToken()
    {
        if (StolenToken != TechToken.None)
        {
            var loser = GetPlayer(Game.BattleLoser);
            if (loser.TechTokens.Contains(StolenToken))
            {
                loser.TechTokens.Remove(StolenToken);
                Player.TechTokens.Add(StolenToken);
                Log(Initiator, " steal ", StolenToken, " from ", Game.BattleLoser);
            }
        }
    }

    private void ProcessGreyForceLossesAndSubstitutions()
    {
        if (Game.GreySpecialForceLossesToTake > 0)
        {
            var plan = Game.CurrentBattle.PlanOf(Player);
            var territory = Game.CurrentBattle.Territory;

            var winnerGambit = Game.WinnerBattleAction;
            var forcesToLose = winnerGambit.Forces + winnerGambit.ForcesAtHalfStrength + SpecialForceLossesReplaced;
            var specialForcesToLose = winnerGambit.SpecialForces + winnerGambit.SpecialForcesAtHalfStrength - SpecialForceLossesReplaced;

            var forceSupplierOfWinner = Battle.DetermineForceSupplier(Game, Player);

            Log(Player.Faction, " substitute ", SpecialForceLossesReplaced, forceSupplierOfWinner.SpecialForce, " losses by ", forceSupplierOfWinner.Force, " losses");

            var specialForcesToSaveToReserves = 0;
            var forcesToSaveToReserves = 0;
            var specialForcesToSaveInTerritory = 0;
            var forcesToSaveInTerritory = 0;

            if (Game.SkilledAs(plan.Hero, LeaderSkill.Graduate))
            {
                specialForcesToSaveInTerritory = Math.Min(specialForcesToLose, 1);
                forcesToSaveInTerritory = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveInTerritory));

                specialForcesToSaveToReserves = Math.Max(0, Math.Min(specialForcesToLose - specialForcesToSaveInTerritory - forcesToSaveInTerritory, 2));
                forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose - forcesToSaveInTerritory, 2 - specialForcesToSaveToReserves));
            }
            else if (Game.SkilledAs(Player, LeaderSkill.Graduate))
            {
                specialForcesToSaveToReserves = Math.Min(specialForcesToLose, 1);
                forcesToSaveToReserves = Math.Max(0, Math.Min(forcesToLose, 1 - specialForcesToSaveToReserves));
            }

            if (specialForcesToSaveInTerritory + forcesToSaveInTerritory + specialForcesToSaveToReserves + forcesToSaveToReserves > 0)
            {
                if (specialForcesToSaveToReserves > 0) forceSupplierOfWinner.ForcesToReserves(territory, specialForcesToSaveToReserves, true);

                if (forcesToSaveToReserves > 0) forceSupplierOfWinner.ForcesToReserves(territory, forcesToSaveToReserves, false);

                Log(
                    LeaderSkill.Graduate,
                    " saves ",
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0, forcesToSaveInTerritory, forceSupplierOfWinner.Force),
                    MessagePart.ExpressIf(specialForcesToSaveInTerritory > 0, specialForcesToSaveInTerritory, forceSupplierOfWinner.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveInTerritory > 0 || specialForcesToSaveInTerritory > 0, " in ", territory),

                    MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " and "),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0, forcesToSaveToReserves, forceSupplierOfWinner.Force),
                    MessagePart.ExpressIf(specialForcesToSaveToReserves > 0, specialForcesToSaveToReserves, forceSupplierOfWinner.SpecialForce),
                    MessagePart.ExpressIf(forcesToSaveToReserves > 0 || specialForcesToSaveToReserves > 0, " to reserves"));
            }

            Game.HandleForceLosses(territory, forceSupplierOfWinner,
                forcesToLose - forcesToSaveToReserves - forcesToSaveInTerritory,
                specialForcesToLose - specialForcesToSaveToReserves - specialForcesToSaveInTerritory);
        }
    }

    private void CaptureOrAssassinateLeader(Player black, Player target)
    {
        if (DecisionToCapture == CaptureDecision.Capture)
        {
            Log(Faction.Black, " capture a leader!");
            black.Leaders.Add(Game.BlackVictim);
            target.Leaders.Remove(Game.BlackVictim);
            Game.SetInFrontOfShield(Game.BlackVictim, false);
            Game.CapturedLeaders.Add(Game.BlackVictim, target.Faction);
        }
        else if (DecisionToCapture == CaptureDecision.Kill)
        {
            Log(Faction.Black, " kill a leader for ", Payment.Of(2));
            Game.AssassinateLeader(Game.BlackVictim);
            black.Resources += 2;
        }
        else if (DecisionToCapture == CaptureDecision.DontCapture)
        {
            Log(Faction.Black, " decide not to capture or kill a leader");
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " conclude the battle");
    }

    #endregion Execution

    #region Validation

    public static IEnumerable<int> ValidReplacementForceAmounts(Game g, Player p)
    {
        if (g.GreySpecialForceLossesToTake == 0) return new[] { 0 };

        var replacementForcesLeft = p.ForcesIn(g.CurrentBattle.Territory) - (g.WinnerBattleAction.Forces + g.WinnerBattleAction.ForcesAtHalfStrength);

        if (replacementForcesLeft <= 0) return new[] { 0 };

        return Enumerable.Range(0, 1 + Math.Min(replacementForcesLeft, g.GreySpecialForceLossesToTake));
    }

    public static int ValidMaxReplacementForceAmount(Game g, Player p)
    {
        if (g.GreySpecialForceLossesToTake == 0) return 0;

        var replacementForcesLeft = p.ForcesIn(g.CurrentBattle.Territory) - (g.WinnerBattleAction.Forces + g.WinnerBattleAction.ForcesAtHalfStrength);

        if (replacementForcesLeft <= 0) return 0;

        return Math.Min(replacementForcesLeft, g.GreySpecialForceLossesToTake);
    }

    public static bool MayCaptureOrKill(Game g, Player p)
    {
        return p.Faction == Faction.Black && g.BattleWinner == Faction.Black && !g.Prevented(FactionAdvantage.BlackCaptureLeader) && g.Applicable(Rule.BlackCapturesOrKillsLeaders) && g.BlackVictim != null;
    }

    public static IEnumerable<CaptureDecision> ValidCaptureDecisions(Game g)
    {
        if (g.Version < 116)
            return new[] { CaptureDecision.Capture, CaptureDecision.Kill, CaptureDecision.DontCapture };
        return new[] { CaptureDecision.Capture, CaptureDecision.Kill };
    }

    public static IEnumerable<IHero> ValidTraitorsToReplace(Player p)
    {
        if (p.Faction == Faction.Purple)
            return p.FaceDancers.Where(h => !p.RevealedDancers.Contains(h));
        return p.Traitors.Where(h => !p.RevealedTraitors.Contains(h));
    }

    public static bool MayChooseToDiscardCards(Game g)
    {
        return g.BattleWinnerMayChooseToDiscard;
    }

    public static bool MayAddExtraForce(Game g, Player p)
    {
        return p.Is(Faction.Green) && p.HasHighThreshold() && p.ForcesInReserve >= 1 &&
               p.AnyForcesIn(g.CurrentBattle.Territory) > 0;
    }

    #endregion Validation
}