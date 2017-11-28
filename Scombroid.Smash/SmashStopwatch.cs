using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scombroid.Smash
{
    public class SmashStopwatch
    {
        private Stopwatch _sw;
        private long _quantity;
        
        public SmashStopwatch(long quantity)
            : this( quantity, true)
        {
            Elapsed = TimeSpan.Zero;
        }

        public SmashStopwatch(long quantity, bool autoStart)
        {
            _quantity = quantity;
            _sw = null;
            if (autoStart)
            {
                Start();
            }
        }

        public void Start()
        {
            if (_sw == null)
            {
                _sw = Stopwatch.StartNew();
            }
        }

        public void Done()
        {
            if (_sw != null)
            {
                _sw.Stop();

                Elapsed = _sw.Elapsed;
                
                

                _sw = null;
            }
        }

        public TimeSpan Elapsed
        {
            get;
            private set;
        }

        public double RatePerSec
        {
            get
            {
                if (Elapsed == TimeSpan.Zero)
                {
                    return 0;
                }
                return (double)_quantity / Elapsed.TotalSeconds;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} per sec (time taken: {1:c})", RatePerSec, Elapsed);
        }
    }
}
