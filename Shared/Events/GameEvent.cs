/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public abstract class GameEvent
    {
        #region Construction

        public GameEvent()
        {
            Game = null;
        }

        public GameEvent(Game game)
        {
            Game = game;
        }

        #endregion Construction

        #region Properties

        private const char IDSTRINGSEPARATOR = ';';

        public Faction Initiator { get; set; }

        public DateTime Time { get; set; }

        [JsonIgnore]
        public Game Game;

        [JsonIgnore]
        public virtual Player Player => Game.GetPlayer(Initiator);

        #endregion Properties

        #region Validation

        public abstract Message Validate();

        [JsonIgnore]
        public bool IsValid => Validate() == null;

        public virtual bool IsApplicable(bool isHost)
        {
            if (Game == null)
            {
                throw new ArgumentException("Cannot check applicability of a GameEvent without a Game.");
            }

            return Game.GetApplicableEvents(Player, isHost).Contains(GetType());
        }

        #endregion Validation

        #region Execution

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

                Message result = null;

                if (performValidation)
                {
                    if (!IsApplicable(isHost))
                    {
                        return Message.Express("Event '", GetMessage(), "' is not applicable");
                    }

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

        public virtual Message GetMessage() => Message.Express(GetType().Name, " by ", Initiator);

        public virtual Message GetShortMessage() => GetMessage();

        #endregion Execution

        #region Support

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

        public bool By(Faction f) => Initiator == f;

        public GameEvent Clone() => (GameEvent)MemberwiseClone();

        protected void Log() => Game.CurrentReport.Express(this);

        protected void Log(params object[] expression) => Game.CurrentReport.Express(expression);

        protected void LogIf(bool condition, params object[] expression)
        {
            if (condition)
            {
                Game.CurrentReport.Express(expression);
            }
        }

        protected void LogTo(Faction faction, params object[] expression) => Game.CurrentReport.ExpressTo(faction, expression);

        protected Player GetPlayer(Faction f) => Game.GetPlayer(f);

        protected bool IsPlaying(Faction f) => Game.IsPlaying(f);

        #endregion Support
    }
}