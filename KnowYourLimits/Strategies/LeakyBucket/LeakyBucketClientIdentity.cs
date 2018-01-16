using System;
using System.Collections.Generic;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketClientIdentity : IClientIdentity
    {
        public bool Equals(IClientIdentity other) => other.UniqueIdentifier == UniqueIdentifier;
        public string UniqueIdentifier { get; set; }
        public int RequestCount { get; set; }
        public DateTime? LastLeak { get; set; }
    }
}
