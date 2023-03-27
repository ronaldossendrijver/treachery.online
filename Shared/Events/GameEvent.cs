/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

/// <summary>
/// When adding a new GameEvent 'XYZ' to the game:
/// - Add it to Game.GetApplicableEvents()
/// - Add a HandleEvent(XYZ e) method to the Game class
/// - Create a XYZComponent to show the event in Treachery.online.Client.Pages. An event without parameters can be shown with a SimpleEventComponent
/// - Add the XYZComponent to Treachery.online.Client.Pages.Index.razor
/// - Add a method RequestXYZ(int hostID, XYZ e)<XYZ>(XYZ e) to Treachery.online.Server.GameHub
/// </summary>
/// 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public abstract class GameEvent
    {
        private const char IDSTRINGSEPARATOR = ';';

        [JsonIgnore]
        public Game Game;

        public Faction Initiator { get; set; }

        public DateTime Time { get; set; }

        public GameEvent()
        {
            Game = null;
        }

        public GameEvent(Game game)
        {
            Game = game;
        }

        [JsonIgnore]
        public string _validationErrors = "";

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                return Validate() == null;
            }
        }

        public abstract Message Validate();

        public virtual void ExecuteWithoutValidation()
        {
            Execute(false, false);
        }

        public virtual Message Execute(bool performValidation, bool isHost)
        {
            if (Game == null)
            {
                throw new ArgumentException("Cannot execute a GameEvent without a Game.");
            }

            try
            {
                var momentJustBeforeEvent = Game.CurrentMoment;

                if (performValidation)
                {
                    if (!IsApplicable(isHost))
                    {
                        return Message.Express("Event '", GetMessage(), "' is not applicable");
                    }

                    var result = Validate();
                    if (result == null)
                    {
                        Game.RecentMilestones.Clear();
                        Game.PerformPreEventTasks(this);
                        ExecuteConcreteEvent();
                        Game.PerformPostEventTasks(this, momentJustBeforeEvent != MainPhaseMoment.Start && Game.CurrentMoment == MainPhaseMoment.Start);
                    }

                    return result;
                }
                else
                {
                    Game.RecentMilestones.Clear();
                    Game.PerformPreEventTasks(this);
                    ExecuteConcreteEvent();
                    Game.PerformPostEventTasks(this, momentJustBeforeEvent != MainPhaseMoment.Start && Game.CurrentMoment == MainPhaseMoment.Start);
                    return null;
                }
            }
            catch (Exception e)
            {
                return Message.Express("Game Error: ", e.Message, ". Technical description: ", e, ".");
            }
        }

        protected abstract void ExecuteConcreteEvent();

        public static List<T> IdStringToObjects<T>(string ids, IFetcher<T> lookup)
        {
            var result = new List<T>();

            if (ids != null && ids.Length > 0)
            {
                foreach (var id in ids.Split(IDSTRINGSEPARATOR))
                {
                    result.Add(lookup.Find(Convert.ToInt32(id)));
                }
            }

            return result;
        }

        public static string ObjectsToIdString<T>(IEnumerable<T> objs, IFetcher<T> lookup)
        {
            return string.Join(IDSTRINGSEPARATOR, objs.Select(pj => Convert.ToString(lookup.GetId(pj))));
        }

        public virtual Message GetMessage()
        {
            return Message.Express(GetType().Name, " by ", Initiator);
        }

        [JsonIgnore]
        public virtual Player Player => Game.GetPlayer(Initiator);

        public virtual bool IsApplicable(bool isHost)
        {
            if (Game == null)
            {
                throw new ArgumentException("Cannot check applicability of a GameEvent without a Game.");
            }

            return Game.GetApplicableEvents(Player, isHost).Contains(this.GetType());
        }

        public bool By(Faction f)
        {
            return Initiator == f;
        }

        public GameEvent Clone()
        {
            return (GameEvent)MemberwiseClone();
        }

        protected void Log()
        {
            Game.CurrentReport.Express(this);
        }

        protected void Log(params object[] expression)
        {
            Game.CurrentReport.Express(expression);
        }

        protected void LogIf(bool condition, params object[] expression)
        {
            if (condition)
            {
                Game.CurrentReport.Express(expression);
            }
        }

        protected void LogTo(Faction faction, params object[] expression)
        {
            Game.CurrentReport.ExpressTo(faction, expression);
        }

        protected Player GetPlayer(Faction f) => Game.GetPlayer(f);

        protected void SetValue(IIdentifiable value, ref int idField)
        {
            if (value == null)
            {
                idField = -1;
            }
            else
            {
                idField = value.Id;
            }
        }
    }
}


