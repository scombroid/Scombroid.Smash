namespace Scombroid.Smash
{
    public static class SmashAssert
    {
        public static void IsTrue(bool val)
        {
            if (!val)
                throw new SmashException("Not true");
        }

        public static void AreEqual(int v1, int v2)
        {
            if (v1 != v2)
                throw new SmashException(string.Format("{0} does not equal {1}", v1, v2));
        }
    }
}
