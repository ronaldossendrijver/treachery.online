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
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;

namespace Treachery.Shared
{
    public abstract class GameEvent
    {
        #region Construction

        public GameEvent()
        {
        }

        public GameEvent(Game game, Faction initiator)
        {
            Initialize(game, initiator);
        }

        public GameEvent(Game game, string playername)
        {
            Initialize(game, playername);
        }

        #endregion Construction

        #region Properties

        private const char IDSTRINGSEPARATOR = ';';

        public Faction Initiator { get; set; }

        public DateTime Time { get; set; }

        [JsonIgnore]
        public Game Game { get; private set; }

        [JsonIgnore]
        public Player Player { get; private set; }

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

        public void Initialize(Game game, Faction initiator)
        {
            Game = game;
            Initiator = initiator;
            Player = game.GetPlayer(initiator);
        }

        public void Initialize(Game game, string playername)
        {
            Game = game;
            Player = game.GetPlayer(playername);
        }

        public void Initialize(Game game)
        {
            Initialize(game, Initiator);
        }

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

        public bool By(Player p) => Player == p;

        public GameEvent Clone() => (GameEvent)MemberwiseClone();

        protected void Log() => Game.CurrentReport.Express(GetMessage());

        protected void Log(params object[] expression) => Game.Log(expression);

        protected void LogIf(bool condition, params object[] expression) => Game.LogIf(condition, expression);

        protected void LogTo(Faction faction, params object[] expression) => Game.LogTo(faction, expression);

        protected Player GetPlayer(Faction f) => Game.GetPlayer(f);

        protected bool IsPlaying(Faction f) => Game.IsPlaying(f);

        #endregion Support
    }
}