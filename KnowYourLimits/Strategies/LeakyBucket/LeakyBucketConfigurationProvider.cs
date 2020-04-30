﻿using System;
using System.Collections.Generic;
using System.Linq;
using KnowYourLimits.Configuration;
using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.Strategies.LeakyBucket
{
    public class LeakyBucketConfigurationProvider
        : IConfigurationProvider<LeakyBucketConfiguration>
    {
        private class ConfigTarget
        {
            public ConfigTarget(Func<HttpContext, bool> useWhen, LeakyBucketConfiguration config)
            {
                UseWhen = useWhen;
                Configuration = config;
            }
            
            public Func<HttpContext, bool> UseWhen { get; }
            public LeakyBucketConfiguration Configuration { get; }
        }

        private readonly ICollection<ConfigTarget> _configTargets = new List<ConfigTarget>();
        private LeakyBucketConfiguration? _defaultConfig;

        public IConfigurationProvider<LeakyBucketConfiguration> AddDefaultConfiguration(LeakyBucketConfiguration config)
        {
            _defaultConfig = config;
            return this;
        }

        public IConfigurationProvider<LeakyBucketConfiguration> AddConfiguration(
            Func<HttpContext, bool> useWhen, LeakyBucketConfiguration config)
        {
            _configTargets.Add(new ConfigTarget(useWhen, config));
            return this;
        }

        public LeakyBucketConfiguration? GetConfiguration(HttpContext ctx)
        {
            return _configTargets.SingleOrDefault(t => t.UseWhen(ctx))?.Configuration ?? _defaultConfig;
        }
    }
}