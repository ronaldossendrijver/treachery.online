/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
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
}
