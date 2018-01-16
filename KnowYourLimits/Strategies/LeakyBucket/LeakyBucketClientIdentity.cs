using System;
using System.Collections.Generic;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketClientIdentity : IClientIdentity
    {
        public bool Equals(IClientIdentity other) => other.UniqueIdentifier == UniqueIdentifier;
        public string UniqueIdentifier { get; set; }
        public List<LeakyBucketInterval> Intervals = new List<LeakyBucketInterval>();
    }
}
