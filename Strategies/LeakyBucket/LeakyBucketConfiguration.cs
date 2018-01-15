using System;
using System.Collections.Generic;
using System.Text;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketConfiguration
    {
        public int MaxRequests { get; set; }
        public TimeSpan LeakRate { get; set; }
        public IClientIdentityProvider<LeakyBucketClientIdentity> IdentityProvider { get; set; }
    }
}
