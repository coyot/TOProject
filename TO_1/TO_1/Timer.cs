using System;
using System.Diagnostics;

namespace TO_1
{
    /// <summary>
    /// Simple measurement unit to be used in using(..) statement.
    /// </summary>
    public class Timer : Stopwatch, IDisposable
    {
        public Timer()
        {
           Start();
        }
        public void Dispose()
        {
            Stop();
            Console.WriteLine("Elapsed: {0}", ElapsedMilliseconds);
        }
    }
}
