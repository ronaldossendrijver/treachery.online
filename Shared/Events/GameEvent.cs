/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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
                return Validate() == "";
            }
        }

        public abstract string Validate();

        public virtual string ExecuteWithoutValidation()
        {
            return Execute(false, false);
        }

        public virtual string Execute(bool performValidation, bool isHost)
        {
            if (Game == null)
            {
                throw new ArgumentException("Cannot execute a GameEvent without a Game.");
            }

            try
            {
                if (performValidation)
                {
                    if (!IsApplicable(isHost))
                    {
                        return string.Format("Event '{0}' is not applicable.", GetMessage());
                    }

                    var result = Validate();
                    if (result == "")
                    {
                        Game.RecentMilestones.Clear();
                        ExecuteConcreteEvent();
                        Game.AddEvent(this);
                    }

                    return result;
                }
                else
                {
                    Game.RecentMilestones.Clear();
                    ExecuteConcreteEvent();
                    Game.AddEvent(this);
                    return "";
                }
            }
            catch (Exception e)
            {
                return string.Format("Game Error: {0}. Technical description: {1}.", e.Message, e);
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
            return new Message(Initiator, "{0} by {1}", this.GetType().Name, Initiator);
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
    }
}


