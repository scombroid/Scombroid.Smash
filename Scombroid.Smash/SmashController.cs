using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Scombroid.Smash
{
    public class SmashController
    {
        private int _taskReadyCount;
        private System.Threading.ManualResetEvent _startEvent;
        private List<System.Threading.Thread> _threads;
        private ConcurrentDictionary<int, SmashResult> _threadResults;
        private string _result;
        private StringBuilder _summary = new StringBuilder();
        public SmashController()
        {
            _taskReadyCount = 0;
            TotalIterations = 0;
            TotalProcessed = 0;
            _startEvent = new System.Threading.ManualResetEvent(false);
            _threads = new List<System.Threading.Thread>();
            _threadResults = new ConcurrentDictionary<int, SmashResult>();
        }

        public int TotalIterations
        {
            get;
            private set;
        }

        public int TotalProcessed
        {
            get;
            private set;
        }

        public void Enqueue(int iterationPerThread, Func<int, int, bool> smashFunc)
        {
            var ctx = new SmashContext()
            {
                Manager = this,
                ThreadNo = this._threads.Count + 1,
                NumIterations = iterationPerThread,
                StartEvent = this._startEvent,
                SmashFunc = smashFunc
            };

            TotalIterations += iterationPerThread;
            _threads.Add(new Thread(() => DoWork(ctx)));
        }

        public bool Run()
        {
            // start all threads
            _threads.ForEach(t => t.Start());

            // wait for all threads to get ready
            while (_taskReadyCount != _threads.Count) // atomic read, hence not using Interlocked
            {
                System.Threading.Thread.Sleep(10);
            }

            SmashStopwatch sw = new SmashStopwatch(TotalIterations);

            // fire the event
            _startEvent.Set();

            // Wait for all threads to complete
            _threads.ForEach(t => t.Join());

            sw.Done();

            _result = sw.ToString();

            TotalProcessed = _threadResults.Values.Sum(t => t.SuccessfulIterations);
            SmashAssert.AreEqual(TotalIterations, TotalProcessed);            

            return TotalIterations == TotalProcessed;
        }

        private void SubmitResult(int tno, SmashResult result)
        {
            _threadResults[tno] = result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var r in _threadResults.OrderBy(k => k.Key))
            {
                sb.AppendLine(string.Format("Thread #{0} {1}", r.Key, r.Value.TimeTaken));
            }

            sb.AppendLine(string.Format("Result: {0}", _result));
            sb.AppendLine(string.Format("Total processed: {0}", _threadResults.Values.Sum(t => t.SuccessfulIterations)));
            sb.AppendLine(string.Format("Average time taken {0}ms", _threadResults.Values.Average(t => t.TimeTaken.TotalMilliseconds / t.SuccessfulIterations)));
            sb.AppendLine(string.Format("Min time taken {0}ms", _threadResults.Values.Min(t => t.TimeTaken.TotalMilliseconds / t.SuccessfulIterations)));
            sb.AppendLine(string.Format("Max time taken {0}ms", _threadResults.Values.Max(t => t.TimeTaken.TotalMilliseconds / t.SuccessfulIterations)));

            return sb.ToString();
        }

        private void DoWork(SmashContext tparam)
        {
            System.Threading.ManualResetEvent startEvent = tparam.StartEvent;
            System.Threading.Interlocked.Increment(ref _taskReadyCount);
            if (!startEvent.WaitOne())
            {
                throw new SmashException(string.Format("Thread #{0} fail to start", tparam.ThreadNo));
            }

            SmashStopwatch swTask = new SmashStopwatch(tparam.NumIterations);

            SmashResult result = new SmashResult();
            result.SuccessfulIterations = 0;
            for (int i = 0; i < tparam.NumIterations; ++i)
            {
                if (tparam.SmashFunc(tparam.ThreadNo, i))
                {
                    result.SuccessfulIterations++;
                }
            }
            swTask.Done();

            SmashAssert.AreEqual(tparam.NumIterations, result.SuccessfulIterations);
            result.TimeTaken = swTask.Elapsed;
            result.RatePerSec = swTask.RatePerSec;

            tparam.Manager.SubmitResult(tparam.ThreadNo, result);
        }
    }    
}
