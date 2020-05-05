using System;
using System.Collections.Generic;
using System.Linq;
using KnowYourLimits.Configuration;
using KnowYourLimits.Identity;
using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketConfigurationProvider
      : IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity>
    {
        private class ConfigTarget
        {
            public ConfigTarget(Func<HttpContext, bool> useWhen, LeakyBucketConfiguration? config,
                IClientIdentityProvider<LeakyBucketClientIdentity>? idProvider = null)
            {
                UseWhen = useWhen;
                Configuration = config;
                IdentityProvider = idProvider;
            }
            
            public Func<HttpContext, bool> UseWhen { get; }
            public LeakyBucketConfiguration? Configuration { get; }
            public IClientIdentityProvider<LeakyBucketClientIdentity>? IdentityProvider { get; set; }
        }

        private readonly ICollection<ConfigTarget> _configTargets = new List<ConfigTarget>();
        private LeakyBucketConfiguration? _defaultConfig;
        private IClientIdentityProvider<LeakyBucketClientIdentity>? _defaultIdentityProvider;

        public IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity> AddDefaultConfiguration(LeakyBucketConfiguration config)
        {
            _defaultConfig = config;
            return this;
        }

        public IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity> AddDefaultIdentityProvider(IClientIdentityProvider<LeakyBucketClientIdentity> idProvider)
        {
            _defaultIdentityProvider = idProvider;
            return this;
        }

        public IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity> AddConfiguration(
            Func<HttpContext, bool> useWhen,
            LeakyBucketConfiguration config,
            IClientIdentityProvider<LeakyBucketClientIdentity> idProvider)
        {
            _configTargets.Add(new ConfigTarget(useWhen, config, idProvider));
            return this;
        }

        public IConfigurationProvider<LeakyBucketConfiguration, LeakyBucketClientIdentity> AddConfiguration(
            Func<HttpContext, bool> useWhen,
            LeakyBucketConfiguration config)
        {
            _configTargets.Add(new ConfigTarget(useWhen, config));
            return this;
        }

        public LeakyBucketConfiguration? GetConfiguration(HttpContext ctx)
        {
            var configurations = _configTargets.Where(t => t.UseWhen(ctx));
            if(configurations.Count() > 1) {
                throw new InvalidOperationException("Multiple rate limiting configurations found matching this request.");
            }

            return configurations.SingleOrDefault()?.Configuration ?? _defaultConfig;
        }

        public LeakyBucketConfiguration? GetDefaultConfiguration()
        {
          return _defaultConfig;
        }

        public IClientIdentityProvider<LeakyBucketClientIdentity>? GetIdentityProvider(HttpContext ctx)
        {
            var configurations = _configTargets.Where(t => t.UseWhen(ctx));
            if(configurations.Count() > 1) {
                throw new InvalidOperationException("Multiple rate limiting configurations found matching this request.");
            }

            return configurations.SingleOrDefault()?.IdentityProvider ?? _defaultIdentityProvider;
        }

        public IClientIdentityProvider<LeakyBucketClientIdentity>? GetDefaultIdentityProvider()
        {
          return _defaultIdentityProvider;
        }
    }
}
