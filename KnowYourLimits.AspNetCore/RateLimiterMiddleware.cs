using System;
using System.Threading.Tasks;
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
                rateLimitStrategy.IdentityProvider = new IpClientIdentityHttpContextProvider<TClientIdentity>();
            }

            applicationBuilder.Use(async (context, next) =>
            {
                if (rateLimitStrategy.IdentityProvider is IHttpContextProvider provider)
                {
                    provider.Context = context;
                }

                async Task OnHasRequestsRemaining() => await next.Invoke();
                async Task OnNoRequestsRemaining() => context.Response.StatusCode = 429;

                await rateLimitStrategy.OnRequest(OnHasRequestsRemaining, OnNoRequestsRemaining);
            });
        }
    }
}
