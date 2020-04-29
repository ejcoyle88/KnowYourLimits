using System;
using KnowYourLimits.Strategies.LeakyBucket;
using Microsoft.AspNetCore.Http;

namespace KnowYourLimits.Configuration
{
    public interface IConfigurationProvider<TConfig> where TConfig : class
    {
        IConfigurationProvider<TConfig> AddDefaultConfiguration(TConfig config);
        IConfigurationProvider<TConfig> AddConfiguration(Func<HttpContext, bool> useWhen, TConfig config);
        TConfig? GetConfiguration(HttpContext ctx);
    }
}