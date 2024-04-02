/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class DivideResources : PassableGameEvent
{
    #region Construction

    public DivideResources(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DivideResources()
    {
    }

    #endregion Construction

    #region Properties

    public int PortionToFirstPlayer { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;

        var toBeDivided = GetResourcesToBeDivided(Game);
        if (toBeDivided == null) return Message.Express("No collections to be divided");
        if (PortionToFirstPlayer > GetResourcesToBeDivided(Game).Amount) return Message.Express("Too much assigned to ", toBeDivided.FirstFaction);

        return null;
    }

    public static ResourcesToBeDivided GetResourcesToBeDivided(Game g)
    {
        return g.CollectedResourcesToBeDivided.FirstOrDefault();
    }

    public static bool IsApplicable(Game g, Player p)
    {
        return g.CurrentPhase == Phase.DividingCollectedResources &&
               GetResourcesToBeDivided(g).FirstFaction == p.Faction;
    }

    public static int GainedByOtherFaction(ResourcesToBeDivided tbd, bool agreed, int portionToFirstPlayer)
    {
        return tbd.Amount - GainedByFirstFaction(tbd, agreed, portionToFirstPlayer);
    }

    public static int GainedByFirstFaction(ResourcesToBeDivided tbd, bool agreed, int portionToFirstPlayer)
    {
        return agreed ? portionToFirstPlayer : (int)(0.5f * tbd.Amount);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.CurrentDivisionProposal = this;

        if (Passed)
        {
            Game.DivideResourcesFromCollection(false);
            Game.Enter(Game.CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, Game.EndCollectionMainPhase);
        }
        else
        {
            var toBeDivided = GetResourcesToBeDivided(Game);
            var gainedByOtherFaction = GainedByOtherFaction(toBeDivided, true, PortionToFirstPlayer);
            Log(Initiator, " propose that they take ", Payment.Of(PortionToFirstPlayer), " and ", toBeDivided.OtherFaction, " take ", Payment.Of(gainedByOtherFaction));
            Game.Enter(Phase.AcceptingResourceDivision);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express("Collected resources were divided");
    }

    #endregion Execution
}