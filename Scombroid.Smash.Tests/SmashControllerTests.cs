using System;
using Scombroid.Smash;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Scombroid.Smash.Tests
{
    [TestClass]
    public class SmashControllerTests
    {
        [TestMethod]
        public void SmashController_BasicTest()
        {
            SmashController sc = new SmashController();

            // Put jobs into the queue
            TestSmash ts1 = new TestSmash();
            sc.Enqueue(ts1, 1000);

            TestSmash ts2 = new TestSmash();
            sc.Enqueue(ts2, 1000);

            // Run
            Assert.IsTrue(sc.Run());

            // Check output
            Assert.AreEqual(1000, ts1.Counter);
            Assert.AreEqual(1000, ts2.Counter);

            // output result, check test output
            Console.WriteLine(sc);
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
