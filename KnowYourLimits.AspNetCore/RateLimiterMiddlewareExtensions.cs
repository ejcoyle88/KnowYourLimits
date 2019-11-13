using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using KnowYourLimits.Strategies.LeakyBucket;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// These are all used outside of this lib, and so are 'unused'
// ReSharper disable UnusedMember.Global

namespace KnowYourLimits.AspNetCore
{
    public static class RateLimiterMiddlewareExtensions
    {
        public static void UseRateLimiting<TClientIdentity>(this IApplicationBuilder applicationBuilder)
            where TClientIdentity : IClientIdentity, new()
        {
            applicationBuilder.UseMiddleware<RateLimiterMiddleware<TClientIdentity>>();
        }

        public static void AddRateLimiting<TClientIdentity>(
            this IServiceCollection serviceCollection,
            IRateLimitStrategy<TClientIdentity> strategy)
            where TClientIdentity : IClientIdentity, new()
        {
            if (strategy.IdentityProvider == null)
            {
                strategy.IdentityProvider = new IpClientIdentityProvider<TClientIdentity>();
            }

            serviceCollection.AddSingleton(strategy);
        }

        public static void AddLeakyBucketRateLimiting(
            this IServiceCollection serviceCollection,
            LeakyBucketConfiguration config)
        {
            serviceCollection.AddRateLimiting(new LeakyBucketRateLimitStrategy(config));
        }
    }
}