using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Scombroid.Smash;
using Smash;
using Xunit;
using Xunit.Abstractions;

namespace Scombroid.Smash.Tests
{
    public class SmashControllerTests
    {
        private readonly ITestOutputHelper output;
        public SmashControllerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SmashController_ParallelTest()
        {
            int iteration = 100;
            var parallel = new SmashParallel<int>();

            for (int i = 0; i < iteration; ++i)
            {
                parallel.Enqueue(() =>
                {
                    System.Threading.Thread.Sleep(10);
                    return 1;
                });
            }

            parallel.Run(5);
            var threadResults = parallel.GetResults();
            int totalCounter = threadResults.Sum(o => o.Results.Sum(r => r.Result));
            Assert.Equal(iteration, totalCounter);
            this.output.WriteLine(JsonConvert.SerializeObject(parallel.GetSummary(), Formatting.Indented));
        }

        [Fact]
        public void SmashController_TaskTest()
        {
            SmashTask<int> smash = new SmashTask<int>();
            int iteration = 10;
            for (int i = 0; i < iteration; ++i)
            {
                smash.Enqueue(()=>RunTask());
            }

            System.Threading.Thread.Sleep(1000);
            this.output.WriteLine("ENQUEUE COMPLETED");

            smash.Run(5);
            var threadResults = smash.GetResults();
            int totalCounter = threadResults.Sum(o => o.Results.Sum(r => r.Result));
            Assert.Equal(iteration, totalCounter);
            this.output.WriteLine(JsonConvert.SerializeObject(smash.GetSummary(), Formatting.Indented));
        }

        public Task<int> RunTask()
        {
            System.Threading.Thread.Sleep(1000);
            int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            this.output.WriteLine($"{tid} - completed at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            return Task.FromResult(1);
        }
    }
}
