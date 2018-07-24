using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KnowYourLimits.Identity;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    /// <inheritdoc cref="IRateLimitStrategy" />
    /// <summary>
    ///     A rate limiting strategy using the leaky bucket algorithm.
    /// </summary>
    public class LeakyBucketRateLimitStrategy : IRateLimitStrategy<LeakyBucketClientIdentity>
    {
        private readonly LeakyBucketConfiguration _configuration;

        public IClientIdentityProvider<LeakyBucketClientIdentity> IdentityProvider
        {
            get => _configuration.IdentityProvider;
            set => _configuration.IdentityProvider = value;
        }

        public LeakyBucketRateLimitStrategy(LeakyBucketConfiguration configuration)
        {
            _configuration = configuration;
            IdentityProvider = _configuration.IdentityProvider;
        }

        public bool HasRemainingAllowance(IClientIdentity identity)
        {
            var leakyIdentity = CastIdentity(identity);
            Leak(leakyIdentity);
            return GetRemainingAllowance(leakyIdentity) > 0;
        }

        public bool HasRemainingAllowance()
        {
            return HasRemainingAllowance(_configuration.IdentityProvider.GetIdentityForCurrentRequest());
        }

        public long GetRemainingAllowance(IClientIdentity identity)
        {
            var leakyIdentity = CastIdentity(identity);
            Leak(leakyIdentity);
            return GetRemainingAllowance(leakyIdentity);
        }

        public long GetRemainingAllowance()
        {
            return GetRemainingAllowance(_configuration.IdentityProvider.GetIdentityForCurrentRequest());
        }

        public long ReduceAllowanceBy(IClientIdentity identity, long requests)
        {
            var leakyIdentity = CastIdentity(identity);
            leakyIdentity.RequestCount += requests;
            return leakyIdentity.RequestCount;
        }

        public long ReduceAllowanceBy(long requests)
        {
            return ReduceAllowanceBy(_configuration.IdentityProvider.GetIdentityForCurrentRequest(), requests);
        }

        public long IncreaseAllowanceBy(IClientIdentity identity, long requests)
        {
            var leakyIdentity = CastIdentity(identity);
            leakyIdentity.RequestCount -= requests;
            return leakyIdentity.RequestCount;
        }

        public async Task OnRequest(Func<Task> onHasRequestsRemaining, Func<Task> onNoRequestsRemaining)
        {
            if (HasRemainingAllowance())
            {
                ReduceAllowanceBy(_configuration.RequestCost);
                await onHasRequestsRemaining();
            }
            else
            {
                await onNoRequestsRemaining();
            }
        }

        public Dictionary<string, string> GetResponseHeaders()
        {
            return GetResponseHeaders(_configuration.IdentityProvider.GetIdentityForCurrentRequest());
        }

        public Dictionary<string, string> GetResponseHeaders(IClientIdentity identity)
        {
            string GetHeaderName(string hN) => $"{_configuration.HeaderPrefix}{hN}";

            return new Dictionary<string, string>
            {
                { GetHeaderName("RateLimit-Remaining"), GetRemainingAllowance(identity).ToString() },
                { GetHeaderName("RateLimit-LeakRate"), _configuration.LeakRate.ToString() },
                { GetHeaderName("RateLimit-LeakAmount"), _configuration.LeakAmount.ToString() },
                { GetHeaderName("RateLimit-BucketSize"), _configuration.MaxRequests.ToString() },
                { GetHeaderName("RateLimit-Cost"), _configuration.RequestCost.ToString() }
            };
        }

        public bool ShouldAddHeaders()
        {
            return _configuration.EnableHeaders;
        }

        public long IncreaseAllowanceBy(long requests)
        {
            return IncreaseAllowanceBy(_configuration.IdentityProvider.GetIdentityForCurrentRequest(), requests);
        }

        private LeakyBucketClientIdentity CastIdentity(IClientIdentity identity)
        {
            if (!(identity is LeakyBucketClientIdentity))
            {
                throw new ArgumentException(nameof(identity));
            }
            return (LeakyBucketClientIdentity) identity;
        }

        private void Leak(LeakyBucketClientIdentity identity)
        {
            if (identity == null)
            {
                return;
            }

            if (identity.LastLeak == null) identity.LastLeak = DateTime.UtcNow;

            if (identity.LastLeak >= DateTime.UtcNow || 
                identity.LastLeak + _configuration.LeakRate > DateTime.UtcNow)
            {
                return;
            }

            var timeSinceLastLeak = DateTime.UtcNow - identity.LastLeak.Value;
            var leaksSinceLast = timeSinceLastLeak.Ticks / _configuration.LeakRate.Ticks;
            var rawLeakTotal = _configuration.LeakAmount * leaksSinceLast;
            identity.RequestCount -= rawLeakTotal >= identity.RequestCount ? identity.RequestCount : rawLeakTotal;
            identity.LastLeak = DateTime.UtcNow;
        }

        private long GetRemainingAllowance(LeakyBucketClientIdentity identity)
        {
            return _configuration.MaxRequests - identity.RequestCount;
        }
    }
}