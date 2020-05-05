using System;
using KnowYourLimits.Configuration;
using KnowYourLimits.Identity;
using KnowYourLimits.Strategies;
using KnowYourLimits.Strategies.LeakyBucket;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

// These are all used outside of this lib, and so are 'unused'
// ReSharper disable UnusedMember.Global

namespace KnowYourLimits.AspNetCore
{
    public static class RateLimiterMiddlewareExtensions
    {
        private static IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity> LeakyBucketConfigProvider
            = new LeakyBucketConfigurationProvider();
        
        public static void UseRateLimiting<TClientIdentity, TConfig, TStrategy>(
                this IApplicationBuilder applicationBuilder)
            where TClientIdentity : class, IClientIdentity, new()
            where TConfig : BaseRateLimitConfiguration
            where TStrategy : class, IRateLimitStrategy<TClientIdentity, TConfig>, new()
        {
            applicationBuilder.UseMiddleware<RateLimiterMiddleware<TClientIdentity, TConfig, TStrategy>>();
        }

        public static void UseLeakyBucketRateLimiting(this IApplicationBuilder applicationBuilder)
        {
            if(LeakyBucketConfigProvider.GetDefaultIdentityProvider() == null) {
              var httpContextAccessor = applicationBuilder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
              var idProvider = new IpClientIdentityProvider<LeakyBucketClientIdentity>(httpContextAccessor);
              LeakyBucketConfigProvider.AddDefaultIdentityProvider(idProvider);
            }

            applicationBuilder.UseRateLimiting<
                LeakyBucketClientIdentity,
                LeakyBucketConfiguration,
                LeakyBucketRateLimitStrategy>();
        }

        public static void AddLeakyBucketRateLimiting(this IServiceCollection _,
            Action<IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity>> configure)
        {
            configure(LeakyBucketConfigProvider);
            
            _.AddSingleton(LeakyBucketConfigProvider);
        }
    }
}
