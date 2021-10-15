using System;
using System.Timers;
using Treachery.Shared;

namespace Treachery.Test
{
    internal class TimedTest : IDisposable
    {
        public event EventHandler<ElapsedEventArgs> Elapsed;
        private Timer _timer;
        private object _toTest;
        
        public TimedTest(object toTest, int seconds)
        {
            _toTest = toTest;
            _timer = new Timer(1000 * seconds);
            _timer.AutoReset = false;
            _timer.Elapsed += Timer_Elapsed;
            Console.WriteLine(DateTime.Now.ToLongTimeString() + ";Starting timer for game;" + ((Game)toTest).Seed);
            _timer.Start();
        }

        public void Stop()
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() + ";Stopping timer for game;" + ((Game)_toTest).Seed);
            _timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() + ";Elapsing timer for game;" + ((Game)_toTest).Seed);
            Elapsed?.Invoke(_toTest, e);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
