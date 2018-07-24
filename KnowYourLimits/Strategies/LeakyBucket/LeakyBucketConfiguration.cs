using System;
using System.Collections.Generic;
using System.Text;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketConfiguration : BaseRateLimitConfiguration
    {
        /// <summary>
        /// The maximum number of requests that can be made.
        /// </summary>
        public long MaxRequests { get; set; }
        /// <summary>
        /// The rate at which the bucket should leak.
        /// </summary>
        public TimeSpan LeakRate { get; set; }
        /// <summary>
        /// The number of requests to leak.
        /// </summary>
        public long LeakAmount { get; set; }
        /// <summary>
        /// The cost of a request. Defaults to 1.
        /// </summary>
        public long RequestCost { get; set; } = 1;
        /// <summary>
        /// A provider of unique identifiers.
        /// </summary>
        public IClientIdentityProvider<LeakyBucketClientIdentity> IdentityProvider { get; set; }
    }
}
