using System;
using System.Collections.Generic;
using System.Text;

namespace Smash
{
    public class SmashThreadResult<T>
    {
        public int Id { get; private set; }
        public int Total { get; private set; }
        public TimeSpan TotalTime { get; private set; }
        public TimeSpan MinTime { get; private set; }
        public TimeSpan MaxTime { get; private set; }
        public IList<SmashResult<T>> Results { get; private set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }

        public SmashThreadResult(int id)
        {
            Id = id;
            MinTime = TimeSpan.MaxValue;
            MaxTime = TimeSpan.MinValue;
            Results = new List<SmashResult<T>>();
            StartTime = DateTimeOffset.Now;
        }

        public void Append(SmashResult<T> result)
        {
            EndTime = DateTimeOffset.Now;
            Total++;
            TotalTime += result.ExecutionTime;
            if (MinTime > result.ExecutionTime)
            {
                MinTime = result.ExecutionTime;
            }
            if (MaxTime < result.ExecutionTime)
            {
                MaxTime = result.ExecutionTime;
            }
            Results.Add(result);
        }
    }
}
