/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
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
        #region Construction

        public ClairVoyancePlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ClairVoyancePlayed()
        {
        }

        #endregion Construction

        #region Properties

        public ClairvoyanceQuestion Question { get; set; }

        public Faction Target { get; set; }

        public bool IsAbout(TreacheryCardType type)
        {
            return Parameter1 is TreacheryCardType t && t == type;
        }

        public string QuestionParameter1 { get; set; }

        [JsonIgnore]
        public object Parameter1
        {
            get
            {
                return Question switch
                {
                    ClairvoyanceQuestion.CardTypeInBattle or
                    ClairvoyanceQuestion.CardTypeAsDefenseInBattle or
                    ClairvoyanceQuestion.CardTypeAsWeaponInBattle or
                    ClairvoyanceQuestion.HasCardTypeInHand => Enum.Parse<TreacheryCardType>(QuestionParameter1),

                    ClairvoyanceQuestion.LeaderAsFacedancer or
                    ClairvoyanceQuestion.LeaderAsTraitor or
                    ClairvoyanceQuestion.LeaderInBattle => LeaderManager.HeroLookup.Find(int.Parse(QuestionParameter1)),

                    ClairvoyanceQuestion.Prediction => Enum.Parse<Faction>(QuestionParameter1),

                    ClairvoyanceQuestion.WillAttackX => Game.Map.TerritoryLookup.Find(int.Parse(QuestionParameter1)),

                    ClairvoyanceQuestion.DialOfMoreThanXInBattle => float.Parse(QuestionParameter1, CultureInfo.InvariantCulture),

                    _ => null,
                };
            }

            set
            {
                if (value == null)
                {
                    QuestionParameter1 = null;
                }
                else
                {
                    QuestionParameter1 = Question switch
                    {
                        ClairvoyanceQuestion.DialOfMoreThanXInBattle => ((float)value).ToString(CultureInfo.InvariantCulture),

                        _ => value.ToString(),
                    };
                }
            }
        }

        public string QuestionParameter2 { get; set; }

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

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
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
            return q switch
            {
                ClairvoyanceQuestion.CardTypeInBattle or
                ClairvoyanceQuestion.CardTypeAsDefenseInBattle or
                ClairvoyanceQuestion.CardTypeAsWeaponInBattle or
                ClairvoyanceQuestion.LeaderInBattle or
                ClairvoyanceQuestion.DialOfMoreThanXInBattle => g.CurrentBattle != null && g.CurrentBattle.IsAggressorOrDefender(p),

                _ => true,
            };
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

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var card = Card;

            if (card != null)
            {
                Game.Discard(card);
                Log();
                Game.Stone(Milestone.Clairvoyance);
            }

            if (Target != Faction.None)
            {
                Game.LatestClairvoyance = this;
                Game.LatestClairvoyanceQandA = null;
                Game.LatestClairvoyanceBattle = Game.CurrentBattle;
                Game.PhasePausedByClairvoyance = Game.CurrentPhase;
                Game.Enter(Phase.Clairvoyance);
            }
        }

        private TreacheryCard Card
        {
            get
            {
                if (Player.Has(TreacheryCardType.Clairvoyance))
                {
                    return Player.TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Clairvoyance);
                }
                else if (Player.Occupies(Game.Map.Cistern))
                {
                    return Karma.ValidKarmaCards(Game, Player).FirstOrDefault(c => c.Type == TreacheryCardType.Karma);
                }

                return null;
            }
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

        public override Message GetShortMessage() => Message.Express(Initiator, " perform ", TreacheryCardType.Clairvoyance);

        #endregion Execution
    }
}
