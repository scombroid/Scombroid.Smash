using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scombroid.Smash
{
    internal class SmashContext
    {
        public int ThreadNo { get; set; }
        public int NumIterations { get; set; }
        public ManualResetEvent StartEvent { get; set; }
        public Func<int, int, bool> SmashFunc { get; set; }
        public SmashController Manager { get; set; }        
    }

    internal struct SmashResult
    {
        public int SuccessfulIterations { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public double RatePerSec { get; set; }
    }
}
