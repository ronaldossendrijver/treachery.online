/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class AllianceOffered : GameEvent
{
    #region Construction

    public AllianceOffered(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AllianceOffered()
    {
    }

    #endregion Construction

    #region Properties

    public Faction Target { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidTargets(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

        return null;
    }

    public static IEnumerable<Faction> ValidTargets(Game g, Player p)
    {
        return g.Players.Where(other => p != other && !p.HaveForcesOnEachOthersHomeWorlds(other))
            .Select(other => other.Faction);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var matchingOffer = Game.CurrentAllianceOffers.FirstOrDefault(x => x.Initiator == Target && x.Target == Initiator);
        if (matchingOffer != null)
        {
            Game.MakeAlliance(Initiator, Target);

            AllianceOffered invalidOffer;
            while ((invalidOffer = Game.CurrentAllianceOffers.FirstOrDefault(x => x.By(Initiator) || x.Initiator == Target)) != null) Game.CurrentAllianceOffers.Remove(invalidOffer);

            if (Game.Version > 150)
            {
                Game.HasActedOrPassed.Add(Initiator);
                Game.HasActedOrPassed.Add(Target);
            }
        }
        else
        {
            Log();
            Game.CurrentAllianceOffers.Add(this);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " offer an alliance to ", Target);
    }

    #endregion Execution
}