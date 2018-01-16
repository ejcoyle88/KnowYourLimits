using System;
using System.Collections.Generic;
using System.Text;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketConfiguration
    {
        /// <summary>
        /// The maximum number of requests that can be made.
        /// </summary>
        public int MaxRequests { get; set; }
        /// <summary>
        /// The rate at which the bucket should leak.
        /// </summary>
        public TimeSpan LeakRate { get; set; }
        /// <summary>
        /// The number of requests to leak.
        /// </summary>
        public int LeakAmount { get; set; }
        /// <summary>
        /// A provider of unique identifiers.
        /// </summary>
        public IClientIdentityProvider<LeakyBucketClientIdentity> IdentityProvider { get; set; }
    }
}
