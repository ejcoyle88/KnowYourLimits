using System;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketInterval : IComparable<LeakyBucketInterval>, IComparable<DateTime>
    {
        public DateTime Start { get; }
        public int Requests { get; set; }

        public LeakyBucketInterval(DateTime start)
        {
            Start = start;
        }

        public int CompareTo(LeakyBucketInterval other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Start.CompareTo(other.Start);
        }

        public int CompareTo(DateTime other)
        {
            return Start.CompareTo(other);
        }
    }
}
