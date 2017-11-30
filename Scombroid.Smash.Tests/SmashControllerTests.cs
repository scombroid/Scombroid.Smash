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
            SmashController sc = new SmashController();

            // Put jobs into the queue
            TestSmash ts1 = new TestSmash();
            sc.Enqueue(ts1, 1000);

            TestSmash ts2 = new TestSmash();
            sc.Enqueue(ts2, 1000);

            // Run
            Assert.True(sc.Run());

            // Check output
            Assert.Equal(1000, ts1.Counter);
            Assert.Equal(1000, ts2.Counter);

            // output result, check test output
            this.output.WriteLine(sc.ToString());
        }
    }

    public class TestSmash : ISmash
    {
        public int Counter { get; set; }

        public bool RunTest(int threadNo, int iteration)
        {
            Counter++;
            return true;            
        }
    }
}
