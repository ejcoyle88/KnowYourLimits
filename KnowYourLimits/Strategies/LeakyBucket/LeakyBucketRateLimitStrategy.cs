using System;
using System.Linq;
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

        public int GetRemainingAllowance(IClientIdentity identity)
        {
            var leakyIdentity = CastIdentity(identity);
            Leak(leakyIdentity);
            return GetRemainingAllowance(leakyIdentity);
        }

        public int GetRemainingAllowance()
        {
            return GetRemainingAllowance(_configuration.IdentityProvider.GetIdentityForCurrentRequest());
        }

        public int ReduceAllowanceBy(IClientIdentity identity, int requests)
        {
            var leakyIdentity = CastIdentity(identity);
            return UpdateCurrentInterval(leakyIdentity, requests);
        }

        public int ReduceAllowanceBy(int requests)
        {
            return ReduceAllowanceBy(_configuration.IdentityProvider.GetIdentityForCurrentRequest(), requests);
        }

        public int IncreaseAllowanceBy(IClientIdentity identity, int requests)
        {
            var leakyIdentity = CastIdentity(identity);
            return UpdateCurrentInterval(leakyIdentity, -requests);
        }

        public int IncreaseAllowanceBy(int requests)
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

            if (identity.LastLeak == null)
            {
                identity.LastLeak = DateTime.UtcNow;
            }

            var nextLeak = identity.LastLeak + _configuration.LeakRate;

            if (DateTime.UtcNow <= nextLeak)
            {
                return;
            }

            identity.LastLeak = DateTime.UtcNow;
            identity.Intervals = identity.Intervals.Where(x => x.Start > identity.LastLeak).ToList();
        }

        private int GetRemainingAllowance(LeakyBucketClientIdentity identity)
        {
            return _configuration.MaxRequests - identity.Intervals.Sum(x => x.Requests);
        }

        private LeakyBucketInterval GetCurrentInterval(LeakyBucketClientIdentity identity)
        {
            var existingInterval = identity.Intervals.SingleOrDefault(x =>
                x.Start < DateTime.UtcNow && x.Start + _configuration.LeakRate > DateTime.UtcNow);

            if (existingInterval == null)
            {
                var newInterval = new LeakyBucketInterval(DateTime.UtcNow);
                identity.Intervals.Add(newInterval);
                return newInterval;
            }

            return existingInterval;
        }

        private int UpdateCurrentInterval(LeakyBucketClientIdentity identity, int requestModifier)
        {
            var currentInterval = GetCurrentInterval(identity);
            currentInterval.Requests += requestModifier;
            return currentInterval.Requests;
        }
    }
}