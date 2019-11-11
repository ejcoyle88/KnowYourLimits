using System.Threading.Tasks;
using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace KnowYourLimits.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // Instantiated as middleware
    public class RateLimiterMiddleware<TClientIdentity>
        where TClientIdentity : IClientIdentity, new()
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimitStrategy<IClientIdentity> _rateLimitStrategy;

        public RateLimiterMiddleware(RequestDelegate next, IRateLimitStrategy<IClientIdentity> rateLimitStrategy)
        {
            _next = next;
            _rateLimitStrategy = rateLimitStrategy;
        }

        // ReSharper disable once UnusedMember.Global
        // Called by the runtime
        public async Task Invoke(HttpContext context)
        {
            if (_rateLimitStrategy.IdentityProvider is IHttpContextIdentityProvider<TClientIdentity> provider)
            {
                provider.Context = context;
            }

            if (_rateLimitStrategy.ShouldAddHeaders())
            {
                var headers = _rateLimitStrategy.GetResponseHeaders();
                context.Response.OnStarting(() =>
                {
                    foreach (var header in headers)
                    {
                        context.Response.Headers.Add(header.Key, new StringValues(header.Value));
                    }

                    return Task.FromResult(0);
                });
            }

            async Task OnHasRequestsRemaining() => await _next(context);

            // ReSharper disable once ImplicitlyCapturedClosure
#pragma warning disable 1998
            async Task OnNoRequestsRemaining() => context.Response.StatusCode = 429;
#pragma warning restore 1998

            await _rateLimitStrategy.OnRequest(OnHasRequestsRemaining, OnNoRequestsRemaining);
        }
    }
}