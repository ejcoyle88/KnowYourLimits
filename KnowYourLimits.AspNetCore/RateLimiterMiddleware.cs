using System.Threading.Tasks;
using KnowYourLimits.Configuration;
using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace KnowYourLimits.AspNetCore
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // Instantiated as middleware
    public class RateLimiterMiddleware<TClientIdentity, TConfig, TStrategy>
        where TClientIdentity : class, IClientIdentity, new()
        where TConfig : BaseRateLimitConfiguration
        where TStrategy : class, IRateLimitStrategy<TClientIdentity, TConfig>, new()
    {
        private readonly RequestDelegate _next;
        private readonly IConfigurationProvider<TConfig, TClientIdentity> _configurationProvider;
        private readonly TStrategy _strategy = new TStrategy();

        public RateLimiterMiddleware(RequestDelegate next,
            IConfigurationProvider<TConfig, TClientIdentity> configurationProvider)
        {
            _next = next;
            _configurationProvider = configurationProvider;
        }

        // ReSharper disable once UnusedMember.Global
        // Called by the runtime
        public async Task Invoke(HttpContext context)
        {
            var config = _configurationProvider.GetConfiguration(context);
            // If there's no matching configuration, let the request through.
            if (config == null)
            {
                await _next(context);
                return;
            }

            var identityProvider = _configurationProvider.GetIdentityProvider(context);
            if(identityProvider == null) {
              await _next(context);
              return;
            }

            var identity = identityProvider.GetCurrentIdentity();
            
            if (_strategy.ShouldAddHeaders(config))
            {
                var headers = _strategy.GetResponseHeaders(identity, config);
                context.Response.OnStarting(() =>
                {
                    foreach (var header in headers)
                    {
                        context.Response.Headers.Add(header.Key, new StringValues(header.Value));
                    }

                    return Task.FromResult(0);
                });
            }

            if (_strategy.HasRemainingAllowance(identity, config))
            {
                _strategy.ReduceAllowanceBy(identity, config);
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 429;
            }
        }
    }
}
