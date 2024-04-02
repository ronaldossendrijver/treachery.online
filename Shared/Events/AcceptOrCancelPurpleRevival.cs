/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace Treachery.Shared;

public class AcceptOrCancelPurpleRevival : GameEvent
{
    #region Construction

    public AcceptOrCancelPurpleRevival(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public AcceptOrCancelPurpleRevival()
    {
    }

    #endregion Construction

    #region Properties

    public bool Cancel { get; set; }

    public int Price { get; set; }

    public int _heroId;

    [JsonIgnore]
    public IHero Hero
    {
        get => LeaderManager.HeroLookup.Find(_heroId);
        set => _heroId = LeaderManager.HeroLookup.GetId(value);
    }

    [JsonIgnore]
    private bool IsDenial => Price == int.MaxValue;

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
        Log();

        if (Hero == null)
        {
            foreach (var r in Game.CurrentRevivalRequests) Game.EarlyRevivalsOffers.Add(r.Hero, int.MaxValue);

            Game.CurrentRevivalRequests.Clear();
        }
        else
        {
            Game.EarlyRevivalsOffers.Remove(Hero);

            if (!Cancel) Game.EarlyRevivalsOffers.Add(Hero, Price);

            var requestToRemove = Game.CurrentRevivalRequests.FirstOrDefault(r => r.Hero == Hero);
            if (requestToRemove != null) Game.CurrentRevivalRequests.Remove(requestToRemove);
        }
    }

    public override Message GetMessage()
    {
        if (Hero == null) return Message.Express(Initiator, " deny all outstanding requests for early revival");

        if (!Cancel)
        {
            if (IsDenial)
                return Message.Express(Initiator, " deny ", Hero.Faction, " early revival of a leader");
            return Message.Express(Initiator, " offer ", Hero.Faction, " revival of a leader for ", Payment.Of(Price));
        }

        return Message.Express(Initiator, " cancel their revival offer to ", Hero.Faction);
    }

    #endregion Execution
}