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
                int pos = 0;
                var tasks = new Task<T>[TaskFuncs.Count];
                foreach (var tf in TaskFuncs)
                {
                    var t = ExecuteTask(throttler, tf);
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

            throttler.Wait();
            Stopwatch tsw = Stopwatch.StartNew();
            var result = await createTaskFunc();
            tsw.Stop();
            throttler.Release();

            ts.Append(new SmashResult<T>(result, tsw.Elapsed));
            return result;
        }

        public static void StartAndWaitAllThrottled(IEnumerable<Task> tasksToRun, int maxTasksToRunInParallel, int timeoutInMilliseconds, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert to a list of tasks so that we don&#39;t enumerate over it multiple times needlessly.
            var tasks = tasksToRun.ToList();

            using (var throttler = new SemaphoreSlim(maxTasksToRunInParallel))
            {
                var postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                tasks.ForEach(t => postTaskTasks.Add(t.ContinueWith(tsk => throttler.Release())));

                // Start running each task.
                foreach (var task in tasks)
                {
                    // Increment the number of tasks currently running and wait if too many are running.
                    throttler.Wait(timeoutInMilliseconds, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                    task.Start();
                }

                // Wait for all of the provided tasks to complete.
                // We wait on the list of "post" tasks instead of the original tasks, otherwise there is a potential race condition where the throttler&#39;s using block is exited before some Tasks have had their "post" action completed, which references the throttler, resulting in an exception due to accessing a disposed object.
                Task.WaitAll(postTaskTasks.ToArray(), cancellationToken);
            }
        }
    }
}
