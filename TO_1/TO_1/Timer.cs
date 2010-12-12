using System;
using System.Diagnostics;

namespace TO_1
{
    /// <summary>
    /// Simple measurement unit to be used in using(..) statement.
    /// </summary>
    public class Timer : Stopwatch, IDisposable
    {
        public string AlgorithmName { get; set; }
        public Timer(string algorithmName)
        {
            AlgorithmName = algorithmName;
           Start();
        }
        public void Dispose()
        {
            Stop();
            Console.WriteLine("{0} -> Elapsed: {1}", AlgorithmName, ElapsedMilliseconds);
        }
    }
}
