using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using Microsoft.AspNetCore.Builder;

namespace KnowYourLimits.AspNetCore
{
    public static class RateLimiterMiddleware
    {
        public static void UseRateLimiting<TClientIdentity>(this IApplicationBuilder applicationBuilder, IRateLimitStrategy<TClientIdentity> rateLimitStrategy)
            where TClientIdentity : IClientIdentity, new()
        {
            if (rateLimitStrategy.IdentityProvider == null)
            {
                rateLimitStrategy.IdentityProvider = new IpClientIdentityProvider<TClientIdentity>();
            }

            applicationBuilder.Use(async (context, next) =>
            {
                if (rateLimitStrategy.IdentityProvider is IpClientIdentityProvider<TClientIdentity> provider)
                {
                    provider.Context = context;
                }

                if (rateLimitStrategy.HasRemainingAllowance())
                {
                    rateLimitStrategy.ReduceAllowanceBy(1);
                    await next.Invoke();
                }
                else
                {
                    context.Response.StatusCode = 429;
                }
            });
        }
    }
}
