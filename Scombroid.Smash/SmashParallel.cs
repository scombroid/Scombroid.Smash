using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smash
{
    public abstract class BaseSmash<T>
    {
        protected System.Collections.Concurrent.ConcurrentDictionary<int, SmashThreadResult<T>> ThreadStatsMapping = new System.Collections.Concurrent.ConcurrentDictionary<int, SmashThreadResult<T>>();
        public TimeSpan TotalExecutionTime { get; protected set; }

        public ICollection<SmashThreadResult<T>> GetResults()
        {
            return ThreadStatsMapping.Values;
        }

        public SmashSummary GetSummary()
        {
            var results = GetResults();

            var total = results.Sum(o => o.Total);

            return new SmashSummary()
            {
                Total = total,
                MinExecutionTime = results.Min(o => o.MinTime),
                MaxExecutionTime = results.Max(o => o.MaxTime),
                TotalExecutionTime = TotalExecutionTime,
                RatePerSecond = total / TotalExecutionTime.TotalSeconds,
                NoOfThreadsUsed = ThreadStatsMapping.Keys.Count
            };
        }

    }

    public class SmashParallel<T> : BaseSmash<T>
    {
        private System.Collections.Generic.ICollection<Func<T>> Jobs = new System.Collections.Generic.List<Func<T>>();

        public SmashParallel()
        {
            TotalExecutionTime = TimeSpan.Zero;
        }

        public void Enqueue(Func<T> job)
        {
            Jobs.Add(job);
        }

        public void Run(int maxDegreeOfParallelism = -1)
        {
            Stopwatch overallStopWatch = Stopwatch.StartNew();

            Parallel.ForEach(
                Jobs,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism
                },
                (action) =>
                {
                    int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    var ts = ThreadStatsMapping.GetOrAdd(tid, (id) =>
                    {
                        return new SmashThreadResult<T>(id);
                    });

                    Stopwatch tsw = Stopwatch.StartNew();
                    T result = action();
                    tsw.Stop();
                    ts.Append(new SmashResult<T>(result, tsw.Elapsed));
                }
            );

            overallStopWatch.Stop();
            TotalExecutionTime = overallStopWatch.Elapsed;
        }
    }

    public class SmashTask<T> : BaseSmash<T>
    {
        private System.Collections.Generic.IList<Func<Task<T>>> TaskFuncs = new System.Collections.Generic.List<Func<Task<T>>>();

        public SmashTask()
        {
        }

        public void Enqueue(Func<Task<T>> task)
        {
            TaskFuncs.Add(task);
        }

        public void Run(int noOfTasks = 1)
        {
            Stopwatch overallStopWatch = Stopwatch.StartNew();
            using (var throttler = new SemaphoreSlim(noOfTasks))
            {
                Console.WriteLine($"Throttler: {throttler.CurrentCount}");
                int pos = 0;
                var tasks = new Task<T>[TaskFuncs.Count];
                foreach (var tf in TaskFuncs)
                {
                    throttler.Wait();
                    Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")} - Created Task #{pos + 1}");
                    var t = Task.Run(() => ExecuteTask(throttler, tf));
                    tasks[pos] = t;
                    ++pos;
                }
                Task.WaitAll(tasks);
            }

            overallStopWatch.Stop();
            TotalExecutionTime = overallStopWatch.Elapsed;
        }

        public async Task<T> ExecuteTask(SemaphoreSlim throttler, Func<Task<T>> createTaskFunc)
        {
            int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            var ts = ThreadStatsMapping.GetOrAdd(tid, (id) =>
            {
                return new SmashThreadResult<T>(id);
            });
            Stopwatch tsw = Stopwatch.StartNew();
            var result = await createTaskFunc();
            tsw.Stop();
            int r = throttler.Release();
            ts.Append(new SmashResult<T>(result, tsw.Elapsed));
            return result;
        }

    }
}
