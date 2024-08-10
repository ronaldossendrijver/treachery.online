/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

/// <summary>
/// When adding a new GameEvent 'XYZ' to the game:
/// - Add it to Game.GetApplicableEvents()
/// - Add a HandleEvent(XYZ e) method to the Game class
/// - Create a XYZComponent to show the event in Treachery.Client.GameEventComponents.
/// - Add the XYZComponent to Treachery.Client.OtherComponents.ActionPanel
/// - Add a method RequestXYZ(int hostID, XYZ e)<XYZ>(XYZ e) to Treachery.online.Server.GameHub
/// </summary>
/// 

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public abstract class GameEvent
{
    #region Construction

    protected GameEvent()
    {
    }

    protected GameEvent(Game game, int seat)
    {
        Initialize(game, seat);
    }
    
    protected GameEvent(Game game, Faction initiator)
    {
        Initialize(game, initiator);
    }

    #endregion Construction

    #region Properties

    private const char IdStringSeparator = ';';

    public Faction Initiator { get; set; }

    public DateTimeOffset Time { get; set; }

    [JsonIgnore]
    public Game Game { get; private set; }

    [JsonIgnore]
    public Player Player { get; private set; }

    #endregion Properties

    #region Validation

    public abstract Message Validate();

    [JsonIgnore]
    public bool IsValid => Validate() == null;

    public bool IsApplicable(bool isHost)
    {
        if (Game == null) throw new ArgumentException("Cannot check applicability of a GameEvent without a Game.");

        return Game.GetApplicableEvents(Player, isHost).Contains(GetType());
    }

    #endregion Validation

    #region Execution

    public void Initialize(Game game, Faction initiator)
    {
        Game = game;
        Initiator = initiator;
        Player = game.GetPlayer(initiator);
    }
    
    public void Initialize(Game game, int seat)
    {
        Game = game;
        Initiator = Faction.None;
        Player = game.GetPlayerBySeat(seat);
    }

    public void Initialize(Game game)
    {
        Initialize(game, Initiator);
    }

    public virtual Message Execute(bool performValidation, bool isHost)
    {
        if (Game == null) throw new ArgumentException("Cannot execute a GameEvent without a Game.");

        try
        {
            var momentJustBeforeEvent = Game.CurrentMoment;

            Message result = null;

            if (performValidation)
            {
                if (!IsApplicable(isHost)) 
                    return Message.Express("Event '", GetMessage(), "' is not applicable");

                result = Validate();
            }

            if (result == null)
            {
                Game.RecentMilestones.Clear();
                Game.PerformPreEventTasks(this);
                ExecuteConcreteEvent();
                Game.PerformPostEventTasks(this, momentJustBeforeEvent != MainPhaseMoment.Start && Game.CurrentMoment == MainPhaseMoment.Start);
            }

            return result;
        }
        catch (Exception e)
        {
            return Message.Express("Game Error: ", e.Message, ". Technical description: ", e, ".");
        }
    }

    protected abstract void ExecuteConcreteEvent();

    public virtual Message GetMessage()
    {
        return Message.Express(GetType().Name, " by ", Initiator);
    }

    public virtual Message GetShortMessage()
    {
        return GetMessage();
    }

    #endregion Execution

    #region Support

    public static List<T> IdStringToObjects<T>(string ids, IFetcher<T> lookup)
    {
        var result = new List<T>();

        if (ids != null && ids.Length > 0)
            foreach (var id in ids.Split(IdStringSeparator)) result.Add(lookup.Find(Convert.ToInt32(id)));

        return result;
    }

    public static string ObjectsToIdString<T>(IEnumerable<T> objs, IFetcher<T> lookup)
    {
        return string.Join(IdStringSeparator, objs.Select(pj => Convert.ToString(lookup.GetId(pj))));
    }

    public bool By(Faction f)
    {
        return Initiator == f;
    }

    public bool ByAllyOf(Faction f)
    {
        return Player.Ally == f;
    }

    public bool By(Player p)
    {
        return Player == p;
    }

    public GameEvent Clone()
    {
        return (GameEvent)MemberwiseClone();
    }

    protected void Log()
    {
        Game.CurrentReport.Express(GetMessage());
    }

    protected void Log(params object[] expression)
    {
        Game.Log(expression);
    }

    protected void LogIf(bool condition, params object[] expression)
    {
        Game.LogIf(condition, expression);
    }

    protected void LogTo(Faction faction, params object[] expression)
    {
        Game.LogTo(faction, expression);
    }

    protected Player GetPlayer(Faction f)
    {
        return Game.GetPlayer(f);
    }

    protected bool IsPlaying(Faction f)
    {
        return Game.IsPlaying(f);
    }

    #endregion Support
}