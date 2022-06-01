/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class ClairVoyanceAnswered : GameEvent
    {
        public ClairVoyanceAnswered(Game game) : base(game)
        {
        }

        public ClairVoyanceAnswered()
        {
        }

        public ClairVoyanceAnswer Answer { get; set; }

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
            return Message.Express(Initiator, " answer: ", ToString(Answer));
        }

        private string ToString(ClairVoyanceAnswer answer)
        {
            return answer switch
            {
                ClairVoyanceAnswer.No => "No.",
                ClairVoyanceAnswer.Yes => "Yes.",
                ClairVoyanceAnswer.Unknown => "I don't know...",
                _ => "?",
            };
        }

        public bool IsNo()
        {
            return Answer == ClairVoyanceAnswer.No;
        }

        public bool IsYes()
        {
            return Answer == ClairVoyanceAnswer.Yes;
        }

        public static bool IsQuestionedBy(bool asWeapon, TreacheryCardType cardType, TreacheryCardType asked)
        {
            if (cardType == asked)
            {
                return true;
            }
            else
            {
                switch (asked)
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

    public class ClairVoyanceQandA
    {
        public ClairVoyancePlayed Question { get; }
        public ClairVoyanceAnswered Answer { get; }

        public ClairVoyanceQandA(ClairVoyancePlayed question, ClairVoyanceAnswered answer)
        {
            Question = question;
            Answer = answer;
        }
    }

    public enum ClairVoyanceAnswer : int
    {
        None = 0,
        Yes = 10,
        No = 20,
        Unknown = 30
    }
}
