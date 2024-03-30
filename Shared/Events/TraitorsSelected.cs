/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Linq;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class TraitorsSelected : GameEvent
{
    #region Construction

    public TraitorsSelected(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public TraitorsSelected()
    {
    }

    #endregion Construction

    #region Properties

    public int _traitorId;

    [JsonIgnore]
    public IHero SelectedTraitor
    {
        get => LeaderManager.HeroLookup.Find(_traitorId);
        set => _traitorId = LeaderManager.HeroLookup.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!Player.Traitors.Contains(SelectedTraitor)) return Message.Express("Invalid traitor");
        return null;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        var toRemove = Player.Traitors.Where(l => !l.Equals(SelectedTraitor)).ToList();

        foreach (var l in toRemove)
        {
            Game.TraitorDeck.Items.Add(l);
            Player.Traitors.Remove(l);
            Player.DiscardedTraitors.Add(l);
            Player.KnownNonTraitors.Add(l);
        }

        Game.HasActedOrPassed.Add(Initiator);
        Log();

        if (Game.EveryoneActedOrPassed) Game.AssignFaceDancers();
    }

    public override Message GetMessage()
    {
        return Message.Express(Initiator, " pick their traitor");
    }

    #endregion Execution
}