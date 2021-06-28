/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} answer: {1}", Initiator, ToString(Answer));
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
