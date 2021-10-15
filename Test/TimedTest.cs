using System;
using System.Timers;

namespace Treachery.Test
{
    internal class TimedTest : IDisposable
    {
        public event EventHandler<ElapsedEventArgs> Elapsed;
        private Timer _timer;
        private object _toTest;
        
        public TimedTest(object toTest, int interval)
        {
            _toTest = toTest;
            _timer = new Timer(interval);
            _timer.AutoReset = false;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(_toTest, e);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
