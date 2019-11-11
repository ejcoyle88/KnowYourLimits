using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using Microsoft.AspNetCore.Builder;

namespace KnowYourLimits.AspNetCore
{
    // ReSharper disable once UnusedMember.Global
    public static class RateLimiterMiddlewareExtensions
    {
        // ReSharper disable once UnusedMember.Global
        public static void UseRateLimiting<TClientIdentity>(this IApplicationBuilder applicationBuilder,
            IRateLimitStrategy<TClientIdentity> rateLimitStrategy)
            where TClientIdentity : IClientIdentity, new()
        {
            if (rateLimitStrategy.IdentityProvider == null)
            {
                rateLimitStrategy.IdentityProvider = new IpClientIdentityProvider<TClientIdentity>();
            }

            applicationBuilder.UseMiddleware<RateLimiterMiddleware<TClientIdentity>>(rateLimitStrategy);
        }
    }
}