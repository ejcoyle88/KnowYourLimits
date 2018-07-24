﻿using System;
using System.Threading.Tasks;
using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

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
                if (rateLimitStrategy.IdentityProvider is IHttpContextIdentityProvider<TClientIdentity> provider)
                {
                    provider.Context = context;
                }

                async Task OnHasRequestsRemaining() => await next.Invoke();
                async Task OnNoRequestsRemaining() => context.Response.StatusCode = 429;

                await rateLimitStrategy.OnRequest(OnHasRequestsRemaining, OnNoRequestsRemaining);

                if (rateLimitStrategy.ShouldAddHeaders())
                {
                    var headers = rateLimitStrategy.GetResponseHeaders();
                    foreach (var header in headers)
                    {
                        context.Response.Headers.Add(header.Key, new StringValues(header.Value));
                    }
                }
            });
        }
    }
}
