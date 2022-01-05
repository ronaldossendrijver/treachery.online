/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using Treachery.Shared;

namespace Treachery.Client
{
    public class ThinkTimer
    {
        public Dictionary<MainPhase, TimeSpan> Times = new Dictionary<MainPhase, TimeSpan>();

        public void AddTime(MainPhase phase, TimeSpan duration)
        {
            if (Times.ContainsKey(phase))
            {
                var currentTime = Times[phase];
                Times.Remove(phase);
                Times.Add(phase, currentTime.Add(duration));
            }
            else
            {
                Times.Add(phase, duration);
            }
        }

        public TimeSpan GetTime(MainPhase phase)
        {
            if (Times.ContainsKey(phase))
            {
                return Times[phase];
            }
            else
            {
                return default;
            }
        }
    }

}
