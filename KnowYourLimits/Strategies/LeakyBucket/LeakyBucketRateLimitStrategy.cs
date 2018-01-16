using System;
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
            leakyIdentity.RequestCount += requests;
            return leakyIdentity.RequestCount;
        }

        public int ReduceAllowanceBy(int requests)
        {
            return ReduceAllowanceBy(_configuration.IdentityProvider.GetIdentityForCurrentRequest(), requests);
        }

        public int IncreaseAllowanceBy(IClientIdentity identity, int requests)
        {
            var leakyIdentity = CastIdentity(identity);
            leakyIdentity.RequestCount -= requests;
            return leakyIdentity.RequestCount;
        }

        public async Task OnRequest(Func<Task> onHasRequestsRemaining, Func<Task> onNoRequestsRemaining)
        {
            if (HasRemainingAllowance())
            {
                ReduceAllowanceBy(1);
                await onHasRequestsRemaining();
            }
            else
            {
                await onNoRequestsRemaining();
            }
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

            if (identity.LastLeak == null) identity.LastLeak = DateTime.UtcNow;

            if (identity.LastLeak >= DateTime.UtcNow || 
                identity.LastLeak + _configuration.LeakRate > DateTime.UtcNow)
            {
                return;
            }
            
            identity.RequestCount -= _configuration.LeakAmount;
            identity.LastLeak = DateTime.UtcNow;
        }

        private int GetRemainingAllowance(LeakyBucketClientIdentity identity)
        {
            return _configuration.MaxRequests - identity.RequestCount;
        }
    }
}