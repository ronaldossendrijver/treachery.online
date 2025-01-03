/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class DivideResourcesAccepted : PassableGameEvent
{
    #region Construction

    public DivideResourcesAccepted(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DivideResourcesAccepted()
    {
    }

    #endregion Construction

    #region Validation

    public override Message Validate()
    {
        return null;
    }

    public static bool IsApplicable(Game g, Player p)
    {
        return g.CurrentPhase == Phase.AcceptingResourceDivision &&
               GetResourcesToBeDivided(g).OtherFaction == p.Faction;
    }

    public static ResourcesToBeDivided GetResourcesToBeDivided(Game g)
    {
        return g.CollectedResourcesToBeDivided.FirstOrDefault();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.DivideResourcesFromCollection(!Passed);
        Game.Enter(Game.CollectedResourcesToBeDivided.Any(), Phase.DividingCollectedResources, Game.EndCollectionMainPhase);
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, !Passed ? "" : " don't", " agree with the proposed division");
    }

    #endregion Execution
}