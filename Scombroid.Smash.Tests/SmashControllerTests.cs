using System;
using Scombroid.Smash;
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
        public void SmashController_BasicTest()
        {
            int iteration = 100;
            SmashController sc = new SmashController();

            // Put jobs into the queue
            TestSmash ts1 = new TestSmash();
            sc.Enqueue(ts1, iteration);

            TestSmash ts2 = new TestSmash();
            sc.Enqueue(ts2, iteration);

            // Run
            Assert.True(sc.Run());

            // Check output
            Assert.Equal(iteration, ts1.Counter);
            Assert.Equal(iteration, ts2.Counter);

            // output result, check test output
            this.output.WriteLine(sc.ToString());
        }

        [Fact]
        public void SmashController_SlowThreadTest()
        {
            int iteration = 1;
            SmashController sc = new SmashController();

            // Put jobs into the queue
            TestSlowSmash ts1 = new TestSlowSmash();
            sc.Enqueue(ts1, iteration);

            TestSlowSmash ts2 = new TestSlowSmash();
            sc.Enqueue(ts2, iteration);

            // Run
            Assert.True(sc.Run());

            // Check output
            Assert.Equal(iteration, ts1.Counter);
            Assert.Equal(iteration, ts2.Counter);

            // output result, check test output
            this.output.WriteLine(sc.ToString());
        }


        [Fact]
        public void SmashController_LotsOfThreadTest()
        {
            int threads   = 250;
            int iteration = 1;
            SmashController sc = new SmashController();

            for (int i = 0; i < threads; ++i)
            {
                // Put jobs into the queue
                TestSlowSmash ts = new TestSlowSmash();
                sc.Enqueue(ts, iteration);
            }

            // Run
            Assert.True(sc.Run());

            // output result, check test output
            this.output.WriteLine(sc.ToString());
        }
    }

    public class TestSmash : ISmash
    {
        public int Counter { get; set; }

        public bool RunTest(int threadNo, int iteration)
        {
            System.Threading.Thread.Sleep(10);
            Counter++;
            return true;            
        }
    }

    public class TestSlowSmash : ISmash
    {
        public int Counter { get; set; }

        public bool RunTest(int threadNo, int iteration)
        {
            System.Threading.Thread.Sleep(3000);
            Counter++;
            return true;
        }
    }
}
