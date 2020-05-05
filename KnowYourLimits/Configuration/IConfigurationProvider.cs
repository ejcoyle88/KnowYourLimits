using System;
using KnowYourLimits.Strategies.LeakyBucket;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.Configuration
{
    public interface IConfigurationProvider<TConfig, TClientIdentity> 
      where TConfig : class
      where TClientIdentity : class, IClientIdentity
    {
        IConfigurationProvider<TConfig, TClientIdentity> AddDefaultIdentityProvider(IClientIdentityProvider<TClientIdentity> idProvider);
        IConfigurationProvider<TConfig, TClientIdentity> AddDefaultConfiguration(TConfig config);
        IConfigurationProvider<TConfig, TClientIdentity> AddConfiguration(Func<HttpContext, bool> useWhen, TConfig config, IClientIdentityProvider<TClientIdentity> idProvider);
        IConfigurationProvider<TConfig, TClientIdentity> AddConfiguration(Func<HttpContext, bool> useWhen, TConfig config);
        TConfig? GetConfiguration(HttpContext ctx);
        IClientIdentityProvider<TClientIdentity>? GetIdentityProvider(HttpContext ctx);
        TConfig? GetDefaultConfiguration();
        IClientIdentityProvider<TClientIdentity>? GetDefaultIdentityProvider();
    }
}
