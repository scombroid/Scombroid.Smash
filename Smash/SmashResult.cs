using System;
using System.Collections.Generic;
using System.Text;

namespace Smash
{
    public class SmashResult<T>
    {
        public T Result { get; private set; }
        public TimeSpan ExecutionTime { get; private set; }

        public SmashResult(T result, TimeSpan executionTime)
        {
            Result = result;
            ExecutionTime = executionTime;
        }
    }

}
