/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public class StormDialled : GameEvent
{
    #region Construction

    public StormDialled(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public StormDialled()
    {
    }

    #endregion Construction

    #region Properties

    public int Amount { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Amount < ValidMinAmount(Game) || Amount > ValidMaxAmount(Game)) return Message.Express("Invalid amount");

        return null;
    }

    public static int ValidMinAmount(Game g)
    {
        if (g.CurrentTurn == 1)
            return 0;
        return 1;
    }

    public static int ValidMaxAmount(Game g)
    {
        if (g.CurrentTurn == 1)
            return 20;
        return 3;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Dials.Add(Amount);
        Game.HasActedOrPassed.Add(Initiator);

        if (Game.HasBattleWheel.All(f => Game.HasActedOrPassed.Contains(f)))
        {
            Log("Storm dial: ", Game.Dials[0], Game.HasActedOrPassed[0], " + ", Game.Dials[1], Game.HasActedOrPassed[1], " = ", Game.Dials[0] + Game.Dials[1]);
            Game.NextStormMoves = Game.Dials.Sum();

            if (Game.CurrentTurn == 1)
                PositionFirstStorm();
            else
                Game.RevealStorm();
        }
    }

    private void PositionFirstStorm()
    {
        Game.SectorInStorm = Game.NextStormMoves % Map.NUMBER_OF_SECTORS;
        Log("The first storm moves ", Game.SectorInStorm, " sectors");
        Game.PerformStorm();

        if (Game.Applicable(Rule.TechTokens)) AssignTechTokens();

        if (Game.UseStormDeck) Game.NextStormMoves = Game.DetermineLaterStormWithStormDeck();

        Game.Enter(IsPlaying(Faction.Grey) || Game.Applicable(Rule.HMSwithoutGrey), Phase.HmsPlacement, Game.EndStormPhase);
    }

    private void AssignTechTokens()
    {
        var techTokensToBeDealt = new List<TechToken>();

        var yellow = GetPlayer(Faction.Yellow);
        if (yellow != null)
        {
            yellow.TechTokens.Add(TechToken.Resources);
            Log(Faction.Yellow, " receive ", TechToken.Resources);
        }
        else
        {
            techTokensToBeDealt.Add(TechToken.Resources);
        }

        var purple = GetPlayer(Faction.Purple);
        if (purple != null)
        {
            purple.TechTokens.Add(TechToken.Graveyard);
            Log(Faction.Purple, " receive ", TechToken.Graveyard);
        }
        else
        {
            techTokensToBeDealt.Add(TechToken.Graveyard);
        }

        var grey = GetPlayer(Faction.Grey);
        if (grey != null)
        {
            grey.TechTokens.Add(TechToken.Ships);
            Log(Faction.Grey, " receive ", TechToken.Ships);
        }
        else
        {
            techTokensToBeDealt.Add(TechToken.Ships);
        }

        var remainingTechTokens = new Deck<TechToken>(techTokensToBeDealt, Game.Random);
        remainingTechTokens.Shuffle();
        Game.Stone(Milestone.Shuffled);

        var techTokenSequence = new PlayerSequence(Game);

        while (!remainingTechTokens.IsEmpty && Game.Players.Any(p => p.TechTokens.Count == 0))
        {
            if (techTokenSequence.CurrentPlayer.TechTokens.Count == 0)
            {
                var token = remainingTechTokens.Draw();
                techTokenSequence.CurrentPlayer.TechTokens.Add(token);
                Log(techTokenSequence.CurrentPlayer.Faction, " receive ", token);
            }

            techTokenSequence.NextPlayer();
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " dial for the storm");
    }

    #endregion Execution
}