using System;
using System.Collections.Generic;
using System.Text;

namespace Smash
{
    public class SmashSummary
    {
        public int Total { get; set; }
        public TimeSpan MinExecutionTime { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public double RatePerSecond { get; set; }
        public int NoOfThreadsUsed { get; set; }
    }

}
