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
            int Counter1 = 0;
            sc.Enqueue(iteration, (t, i) =>
            {
                System.Threading.Thread.Sleep(10);
                Counter1++;
                return true;
            });

            int Counter2 = 0;
            sc.Enqueue(iteration, (t, i) =>
            {
                System.Threading.Thread.Sleep(10);
                Counter2++;
                return true;
            });

            // Run
            Assert.True(sc.Run());

            // Check output
            Assert.Equal(iteration, Counter1);
            Assert.Equal(iteration, Counter2);

            // output result, check test output
            this.output.WriteLine(sc.ToString());
        }

        [Fact]
        public void SmashController_SlowThreadTest()
        {
            int iteration = 1;
            SmashController sc = new SmashController();

            // Put jobs into the queue
            int Counter1 = 0;
            sc.Enqueue(iteration, (t, i) => {
                System.Threading.Thread.Sleep(3000);
                Counter1++;
                return true;
            });

            int Counter2 = 0;
            sc.Enqueue(iteration, (t, i) => {
                System.Threading.Thread.Sleep(3000);
                Counter2++;
                return true;
            });

            // Run
            Assert.True(sc.Run());

            // Check output
            Assert.Equal(iteration, Counter1);
            Assert.Equal(iteration, Counter2);

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
                sc.Enqueue(iteration, (t, it) => {
                    System.Threading.Thread.Sleep(3000);
                    return true;
                });
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
