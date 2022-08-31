/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Treachery.Shared
{
    public class ClairVoyancePlayed : GameEvent
    {
        public ClairVoyancePlayed(Game game) : base(game)
        {
        }

        public ClairVoyancePlayed()
        {
        }

        public ClairvoyanceQuestion Question { get; set; }

        public string QuestionParameter1 { get; set; }

        public string QuestionParameter2 { get; set; }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Target == Faction.None)
            {
                return Message.Express(Initiator, " perform ", TreacheryCardType.Clairvoyance);
            }
            else
            {
                if (Question == ClairvoyanceQuestion.None)
                {
                    return Message.Express("By ", TreacheryCardType.Clairvoyance, ", ", Initiator, " ask ", Target, " a question");
                }
                else
                {
                    return Message.Express("By ", TreacheryCardType.Clairvoyance, ", ", Initiator, " ask ", Target, ": \"", GetQuestion(), "\"");
                }
            }
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.PlayersOtherThan(p);
        }

        public static IEnumerable<ClairvoyanceQuestion> ValidQuestions(Game g, Faction target)
        {
            var allValues = Enumerations.GetValues<ClairvoyanceQuestion>(typeof(ClairvoyanceQuestion));
            var targetPlayer = g.GetPlayer(target);
            if (targetPlayer == null || !(targetPlayer.IsBot))
            {
                return allValues;
            }
            else
            {
                return allValues.Where(v => AppliesToBot(v, g, targetPlayer));
            }
        }

        private static bool AppliesToBot(ClairvoyanceQuestion q, Game g, Player p)
        {
            switch (q)
            {
                case ClairvoyanceQuestion.CardTypeInBattle:
                case ClairvoyanceQuestion.CardTypeAsDefenseInBattle:
                case ClairvoyanceQuestion.CardTypeAsWeaponInBattle:
                case ClairvoyanceQuestion.LeaderInBattle:
                case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                    return g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p);

                default: return true;
            }
        }

        public bool IsAbout(TreacheryCardType type)
        {
            return Parameter1 is TreacheryCardType t && t == type;
        }

        [JsonIgnore]
        public object Parameter1
        {
            get
            {
                switch (Question)
                {
                    case ClairvoyanceQuestion.CardTypeInBattle:
                    case ClairvoyanceQuestion.CardTypeAsDefenseInBattle:
                    case ClairvoyanceQuestion.CardTypeAsWeaponInBattle:
                    case ClairvoyanceQuestion.HasCardTypeInHand:
                        return Enum.Parse<TreacheryCardType>(QuestionParameter1);

                    case ClairvoyanceQuestion.LeaderAsFacedancer:
                    case ClairvoyanceQuestion.LeaderAsTraitor:
                    case ClairvoyanceQuestion.LeaderInBattle:
                        return LeaderManager.HeroLookup.Find(int.Parse(QuestionParameter1));

                    case ClairvoyanceQuestion.Prediction:
                        return Enum.Parse<Faction>(QuestionParameter1);

                    case ClairvoyanceQuestion.WillAttackX:
                        return Game.Map.TerritoryLookup.Find(int.Parse(QuestionParameter1));

                    case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                        return float.Parse(QuestionParameter1, CultureInfo.InvariantCulture);
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    QuestionParameter1 = null;
                }
                else
                {
                    switch (Question)
                    {
                        case ClairvoyanceQuestion.DialOfMoreThanXInBattle:
                            QuestionParameter1 = ((float)value).ToString(CultureInfo.InvariantCulture);
                            break;

                        default:
                            QuestionParameter1 = value.ToString();
                            break;
                    }
                }
            }
        }

        [JsonIgnore]
        public object Parameter2
        {
            get
            {
                return Question switch
                {
                    ClairvoyanceQuestion.Prediction => int.Parse(QuestionParameter2),
                    _ => null,
                };
            }

            set
            {
                if (value == null)
                {
                    QuestionParameter2 = null;
                }
                else
                {
                    QuestionParameter2 = value.ToString();
                }
            }
        }

        public Message GetQuestion()
        {
            if (Question == ClairvoyanceQuestion.None)
            {
                return Message.Express("");
            }
            else
            {
                return Express(Question, Parameter1, Parameter2);
            }
        }

        public static Message Express(ClairvoyanceQuestion q, object parameter1 = null, object parameter2 = null)
        {
            var p1 = parameter1 ?? "...";
            var p2 = parameter2 ?? "...";

            return q switch
            {
                ClairvoyanceQuestion.None => Message.Express("Any question"),
                ClairvoyanceQuestion.CardTypeInBattle => Message.Express("In this battle, is one of your cards a ", p1),
                ClairvoyanceQuestion.CardTypeAsDefenseInBattle => Message.Express("In this battle, will you use ", p1, " as defense?"),
                ClairvoyanceQuestion.CardTypeAsWeaponInBattle => Message.Express("In this battle, will you use ", p1, " as weapon?"),
                ClairvoyanceQuestion.HasCardTypeInHand => Message.Express("Do you own a ", p1, "?"),
                ClairvoyanceQuestion.LeaderInBattle => Message.Express("In this battle, is your leader ", p1, "?"),
                ClairvoyanceQuestion.DialOfMoreThanXInBattle => Message.Express("In this battle, is your dial higher than ", p1, "?"),
                ClairvoyanceQuestion.LeaderAsFacedancer => Message.Express("Is ", p1, " one of your face dancers?"),
                ClairvoyanceQuestion.LeaderAsTraitor => Message.Express("Is ", p1, " one of your traitors?"),
                ClairvoyanceQuestion.Prediction => Message.Express("Did you predict a ", p1, " win in turn ", p2, "?"),
                ClairvoyanceQuestion.WillAttackX => Message.Express("Will you ship or move to ", p1, " this turn?"),
                _ => Message.Express("unknown question"),
            };
        }

        public static bool IsInScopeOf(bool asWeapon, TreacheryCardType cardType, TreacheryCardType inScopeOfQuestion)
        {
            if (cardType == inScopeOfQuestion)
            {
                return true;
            }
            else
            {
                switch (inScopeOfQuestion)
                {
                    case TreacheryCardType.PoisonDefense: return cardType == TreacheryCardType.Antidote || (!asWeapon && cardType == TreacheryCardType.Chemistry) || cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.Poison: return cardType == TreacheryCardType.PoisonTooth || cardType == TreacheryCardType.ProjectileAndPoison;
                    case TreacheryCardType.Shield: return cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.ProjectileDefense: return cardType == TreacheryCardType.Shield || cardType == TreacheryCardType.ShieldAndAntidote;
                    case TreacheryCardType.Projectile: return (asWeapon && cardType == TreacheryCardType.WeirdingWay) || cardType == TreacheryCardType.ProjectileAndPoison;
                }
            }

            return false;
        }
    }



    public enum ClairvoyanceQuestion : int
    {
        None = 0,

        Prediction = 10,
        LeaderAsTraitor = 20,
        LeaderAsFacedancer = 30,
        HasCardTypeInHand = 40,

        LeaderInBattle = 100,
        CardTypeInBattle = 110,
        CardTypeAsDefenseInBattle = 111,
        CardTypeAsWeaponInBattle = 112,
        DialOfMoreThanXInBattle = 120,

        WillAttackX = 200
    }
}
