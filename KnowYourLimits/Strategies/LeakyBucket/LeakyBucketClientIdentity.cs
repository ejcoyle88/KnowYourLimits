using System;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketClientIdentity : IClientIdentity
    {
        public bool Equals(IClientIdentity other) => other?.UniqueIdentifier == UniqueIdentifier;
        public string UniqueIdentifier { get; set; }
        public long RequestCount { get; set; }
        public DateTime? LastLeak { get; set; }
    }
}
