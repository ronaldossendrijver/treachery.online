/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public override string Validate()
        {
            if (Game.Version >= 77)
            {
                //if (Target == Faction.None) return "Invalid faction.";
            }

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Target == Faction.None)
            {
                return new Message(Initiator, "{0} perform {1}.", Initiator, TreacheryCardType.Clairvoyance);
            }
            else
            {
                if (Question == ClairvoyanceQuestion.None)
                {
                    return new Message(Initiator, "By {0}, {1} ask {2} a question.", TreacheryCardType.Clairvoyance, Initiator, Target);
                }
                else
                {
                    return new Message(Initiator, "By {0}, {1} ask {2}: \"{3}\"", TreacheryCardType.Clairvoyance, Initiator, Target, GetQuestion());
                }
            }
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.ValidTargets(p);
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
                    return g.CurrentBattle != null && (g.CurrentBattle.Initiator == p.Faction || g.CurrentBattle.Target == p.Faction);

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
                        return float.Parse(QuestionParameter1);
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
                    QuestionParameter1 = value.ToString();
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

        public string GetQuestion()
        {
            if (Question == ClairvoyanceQuestion.None)
            {
                return "";
            }
            else
            {
                return Skin.Current.Format(Skin.Current.Describe(Question), Parameter1, Parameter2);
            }
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
