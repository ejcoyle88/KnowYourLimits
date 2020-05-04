using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    /// <inheritdoc cref="IRateLimitStrategy{TIdentityType, TConfig}" />
    /// <summary>
    ///     A rate limiting strategy using the leaky bucket algorithm.
    /// </summary>
    public class LeakyBucketRateLimitStrategy 
        : IRateLimitStrategy<LeakyBucketClientIdentity, LeakyBucketConfiguration>
    {
        public bool HasRemainingAllowance(LeakyBucketClientIdentity identity, LeakyBucketConfiguration config)
        {
            Leak(identity, config);
            return GetRemainingAllowance(identity, config) > 0;
        }

        public long ReduceAllowanceBy(LeakyBucketClientIdentity identity, LeakyBucketConfiguration config)
        {
            return ReduceAllowanceBy(identity, config.RequestCost);
        }

        public long ReduceAllowanceBy(LeakyBucketClientIdentity identity, long requests)
        {
            identity.RequestCount += requests;
            return identity.RequestCount;
        }

        public long IncreaseAllowanceBy(LeakyBucketClientIdentity identity, long requests)
        {
            identity.RequestCount -= requests;
            return identity.RequestCount;
        }

        public Dictionary<string, string> GetResponseHeaders(LeakyBucketClientIdentity identity, LeakyBucketConfiguration config)
        {
            string GetHeaderName(string hN) => $"{config.HeaderPrefix}{hN}";

            return new Dictionary<string, string>
            {
                { GetHeaderName("RateLimit-Remaining"), GetRemainingAllowance(identity, config).ToString() },
                { GetHeaderName("RateLimit-LeakRate"), config.LeakRate.ToString()  },
                { GetHeaderName("RateLimit-LeakAmount"), config.LeakAmount.ToString() },
                { GetHeaderName("RateLimit-BucketSize"), config.MaxRequests.ToString() },
                { GetHeaderName("RateLimit-Cost"), config.RequestCost.ToString() }
            };
        }

        public bool ShouldAddHeaders(LeakyBucketConfiguration config)
        {
            return config.EnableHeaders;
        }

        private void Leak(LeakyBucketClientIdentity identity, LeakyBucketConfiguration config)
        {
            if (identity == null || config == null)
            {
                return;
            }

            if (identity.LastLeak == null)
            {
                identity.LastLeak = DateTime.UtcNow;
            }

            if (identity.LastLeak >= DateTime.UtcNow ||
                identity.LastLeak + config.LeakRate > DateTime.UtcNow)
            {
                return;
            }

            var timeSinceLastLeak = DateTime.UtcNow - identity.LastLeak.Value;
            var leaksSinceLast = timeSinceLastLeak.Ticks / config.LeakRate.Ticks;
            var rawLeakTotal = config.LeakAmount * leaksSinceLast;
            identity.RequestCount -= rawLeakTotal >= identity.RequestCount ? identity.RequestCount : rawLeakTotal;
            identity.LastLeak = DateTime.UtcNow;
        }

        public long GetRemainingAllowance(LeakyBucketClientIdentity identity, LeakyBucketConfiguration config)
        {
            return config.MaxRequests - identity.RequestCount;
        }
    }
}
