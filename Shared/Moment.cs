/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public struct Moment
    {
        public int Turn;
        public MainPhase Phase;

        public Moment(int turn, MainPhase phase)
        {
            Turn = turn;
            Phase = phase;
        }
    }
}
