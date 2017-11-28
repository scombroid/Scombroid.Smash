using System;

namespace Scombroid.Smash
{
    public class SmashException : Exception
    {
        public SmashException()
        {
        }

        public SmashException(string message)
            : base(message)
        { }

    }
}
